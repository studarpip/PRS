using Neo4j.Driver;
using PRS.Model.Entities;
using PRS.Server.Repositories.Interfaces;

namespace PRS.Server.Repositories
{
    public class RecommendationsRepository : IRecommendationsRepository
    {
        private readonly IDriver _driver;
        private readonly IProductRepository _productRepository;

        public RecommendationsRepository(IDriver driver, IProductRepository productRepository)
        {
            _driver = driver;
            _productRepository = productRepository;
        }

        public async Task<List<Product>> GetRecommendationsAsync(string userId, string context, RecommendationSettings settings)
        {
            const int maxRecommendations = 15;
            var seen = new HashSet<Guid>();
            var final = new List<Product>();

            var contentResults = settings.UseContent
                ? await GetContentBasedRecommendations(userId, context, settings)
                : new List<Product>();

            var collaborativeResults = settings.UseCollaborative
                ? await GetCollaborativeRecommendations(userId, context, settings)
                : new List<Product>();

            IEnumerable<Product> PickUnique(IEnumerable<Product> source, int count)
            {
                return source
                    .Where(p => seen.Add(p.Id))
                    .Take(count);
            }

            if (settings.UseContent && settings.UseCollaborative)
            {
                final.AddRange(PickUnique(contentResults, 7));
                final.AddRange(PickUnique(collaborativeResults, maxRecommendations - final.Count));
            }
            else if (settings.UseContent)
            {
                final.AddRange(PickUnique(contentResults, 12));
            }
            else if (settings.UseCollaborative)
            {
                final.AddRange(PickUnique(collaborativeResults, 12));
            }

            if (final.Count < maxRecommendations)
            {
                var fallback = await GetPopularFallback(userId, maxRecommendations);
                final.AddRange(PickUnique(fallback, maxRecommendations - final.Count));
            }

            return final.OrderBy(x => Guid.NewGuid()).ToList();
        }

        private async Task<List<Product>> GetContentBasedRecommendations(string userId, string context, RecommendationSettings settings)
        {
            await using var session = _driver.AsyncSession();

            var (interactionTypes, exclusionTypes) = GetRecommendationFilters(context);

            var cypher = $@"
                CALL {{
                    MATCH (u:User {{id: $userId}})-[r{interactionTypes}]->(p1:Product)
                    WHERE r.timestamp IS NOT NULL
                    WITH u, p1,
                         CASE 
                           WHEN duration.inSeconds(r.timestamp, datetime()).seconds > 3600 
                           THEN 3600 
                           ELSE duration.inSeconds(r.timestamp, datetime()).seconds 
                         END AS secondsAgo,
                         CASE type(r)
                           WHEN 'BOUGHT' THEN $purchaseWeight
                           WHEN 'IN_CART' THEN $cartWeight
                           WHEN 'RATED' THEN $ratingWeight
                           WHEN 'VIEWED' THEN $viewWeight
                           WHEN 'BROWSED' THEN $browseWeight
                           ELSE 0.1
                         END AS interactionWeight
                    WITH u, p1,
                         interactionWeight,
                         secondsAgo,
                         (interactionWeight * (1.0 / (1.0 + secondsAgo))) AS recencyWeight
                    WITH u, collect(p1) AS userProducts, collect(recencyWeight) AS weights
                    WITH u,
                         reduce(total = 0.0, i IN range(0, size(userProducts)-1) |
                            total + (userProducts[i].price * weights[i])) AS weightedPriceSum,
                         reduce(total = 0.0, w IN weights | total + w) AS totalWeight
                    RETURN u.id AS uid, 
                           (weightedPriceSum / CASE WHEN totalWeight = 0 THEN 1 ELSE totalWeight END) AS avgUserPrice
                }}
                WITH avgUserPrice

                MATCH (u:User {{id: $userId}})-[r{interactionTypes}]->(p1:Product)
                WHERE r.timestamp IS NOT NULL
                WITH u, p1, avgUserPrice,
                     CASE 
                       WHEN duration.inSeconds(r.timestamp, datetime()).seconds > 3600 
                       THEN 3600 
                       ELSE duration.inSeconds(r.timestamp, datetime()).seconds 
                     END AS secondsAgo,
                     CASE type(r)
                       WHEN 'BOUGHT' THEN $purchaseWeight
                       WHEN 'IN_CART' THEN $cartWeight
                       WHEN 'RATED' THEN $ratingWeight
                       WHEN 'VIEWED' THEN $viewWeight
                       WHEN 'BROWSED' THEN $browseWeight
                       ELSE 0.1
                     END AS interactionWeight
                WITH u, p1, avgUserPrice,
                     (interactionWeight * (1.0 / (1.0 + secondsAgo))) AS recencyWeight

                MATCH (p1)-[:IN_CATEGORY]->(c:Category)<-[:IN_CATEGORY]-(p2:Product)
                WHERE NOT (u)-[{exclusionTypes}]->(p2)
                  AND coalesce(p2.isDeleted, false) = false

                OPTIONAL MATCH (p1)-[:IN_CATEGORY]->(c1:Category)
                OPTIONAL MATCH (p2)-[:IN_CATEGORY]->(c2:Category)

                WITH p2, avgUserPrice,
                     SUM(recencyWeight) AS totalRecencyWeight,
                     COUNT(DISTINCT c) AS sharedCategories,
                     COUNT(DISTINCT c1) AS p1Categories,
                     abs(p2.price - avgUserPrice) AS priceDiff

                OPTIONAL MATCH (:User)-[r2:RATED]->(p2)

                WITH p2, totalRecencyWeight,
                     CASE WHEN p1Categories = 0 THEN 0.0 ELSE (toFloat(sharedCategories) / p1Categories) END AS categoryMatchRatio,
                     priceDiff,
                     AVG(r2.rating) AS avgRating

                RETURN p2.id AS id,
                       (categoryMatchRatio * totalRecencyWeight * $categoryWeight) +
                       ((1.0 / (1.0 + priceDiff)) * totalRecencyWeight * $priceWeight) +
                       (coalesce(avgRating, 0) * $avgRatingWeight) AS score
                ORDER BY score DESC
                LIMIT 15
            ";

            var result = await session.RunAsync(cypher, new
            {
                userId,
                categoryWeight = settings.CategoryWeight,
                priceWeight = settings.PriceWeight,
                avgRatingWeight = settings.AvgRatingWeight,
                browseWeight = settings.BrowseWeight,
                viewWeight = settings.ViewWeight,
                cartWeight = settings.CartWeight,
                purchaseWeight = settings.PurchaseWeight,
                ratingWeight = settings.RatingWeight
            });

            var ids = await result.ToListAsync();
            var products = new List<Product>();

            foreach (var record in ids)
            {
                var id = Guid.Parse(record["id"].As<string>());
                var product = await _productRepository.GetByIdAsync(id, null);
                if (product != null)
                    products.Add(product);
            }

            return products;
        }

        private async Task<List<Product>> GetCollaborativeRecommendations(string userId, string context, RecommendationSettings settings)
        {
            await using var session = _driver.AsyncSession();

            var (interactionTypes, exclusionTypes) = GetRecommendationFilters(context);

            var query = $@"
                MATCH (u:User {{id: $userId}})-[r1{interactionTypes}]->(p:Product)<-[r2{interactionTypes}]-(other:User)
                WHERE other.id <> $userId AND r1.timestamp IS NOT NULL AND r2.timestamp IS NOT NULL

                WITH u, other,
                     CASE type(r1)
                         WHEN 'BOUGHT' THEN $purchaseWeight
                         WHEN 'IN_CART' THEN $cartWeight
                         WHEN 'RATED' THEN $ratingWeight
                         WHEN 'VIEWED' THEN $viewWeight
                         WHEN 'BROWSED' THEN $browseWeight
                         ELSE 0.1
                     END AS w1,
                     CASE type(r2)
                         WHEN 'BOUGHT' THEN $purchaseWeight
                         WHEN 'IN_CART' THEN $cartWeight
                         WHEN 'RATED' THEN $ratingWeight
                         WHEN 'VIEWED' THEN $viewWeight
                         WHEN 'BROWSED' THEN $browseWeight
                         ELSE 0.1
                     END AS w2,
                     duration.inSeconds(r1.timestamp, datetime()).seconds AS s1,
                     duration.inSeconds(r2.timestamp, datetime()).seconds AS s2

                WITH u, other,
                     SUM((w1 / (1 + s1)) * (w2 / (1 + s2))) AS similarityScore
                WHERE similarityScore > 0.001

                MATCH (other)-[r3{interactionTypes}]->(rec:Product)
                WHERE r3.timestamp IS NOT NULL
                  AND coalesce(rec.isDeleted, false) = false
                  AND NOT (u)-[{exclusionTypes}]->(rec)

                WITH rec, similarityScore,
                     CASE type(r3)
                         WHEN 'BOUGHT' THEN $purchaseWeight
                         WHEN 'IN_CART' THEN $cartWeight
                         WHEN 'RATED' THEN $ratingWeight
                         WHEN 'VIEWED' THEN $viewWeight
                         WHEN 'BROWSED' THEN $browseWeight
                         ELSE 0.1
                     END AS rw,
                     duration.inSeconds(r3.timestamp, datetime()).seconds AS s3

                WITH rec, SUM(similarityScore * (rw / (1 + s3))) AS totalScore
                RETURN rec.id AS id
                ORDER BY totalScore DESC
                LIMIT 15
            ";

            var result = await session.RunAsync(query, new
            {
                userId,
                browseWeight = settings.BrowseWeight,
                viewWeight = settings.ViewWeight,
                cartWeight = settings.CartWeight,
                purchaseWeight = settings.PurchaseWeight,
                ratingWeight = settings.RatingWeight
            });

            var ids = await result.ToListAsync();
            var products = new List<Product>();

            foreach (var record in ids)
            {
                var id = Guid.Parse(record["id"].As<string>());
                var product = await _productRepository.GetByIdAsync(id, null);
                if (product != null)
                    products.Add(product);
            }

            return products;
        }

        private async Task<List<Product>> GetPopularFallback(string? userId, int needed)
        {
            await using var session = _driver.AsyncSession();

            string? gender = null;
            string? country = null;

            if (!string.IsNullOrWhiteSpace(userId))
            {
                var userInfo = await session.RunAsync(@"
                    MATCH (u:User {id: $userId})
                    OPTIONAL MATCH (u)-[:HAS_GENDER]->(g:Gender)
                    OPTIONAL MATCH (u)-[:FROM_COUNTRY]->(c:Country)
                    RETURN g.name AS gender, c.name AS country
                ", new { userId });

                var records = await userInfo.ToListAsync();
                var record = records.First();
                if (record != null)
                {
                    gender = record["gender"].As<string?>();
                    country = record["country"].As<string?>();
                }
            }

            List<Guid> ids = new();

            if (!string.IsNullOrWhiteSpace(gender) && !string.IsNullOrWhiteSpace(country))
            {
                var res = await session.RunAsync(@"
                    MATCH (p:Product)<-[:BOUGHT]-(u:User)
                    WHERE coalesce(p.isDeleted, false) = false
                      AND (u)-[:HAS_GENDER]->(:Gender {name: $gender})
                      AND (u)-[:FROM_COUNTRY]->(:Country {name: $country})
                    RETURN p.id AS id, COUNT(*) AS popularity
                    ORDER BY popularity DESC
                    LIMIT $limit
                ", new
                {
                    gender,
                    country,
                    limit = needed
                });

                ids.AddRange((await res.ToListAsync()).Select(r => Guid.Parse(r["id"].As<string>())));
            }

            if (ids.Count < needed && !string.IsNullOrWhiteSpace(gender))
            {
                var res = await session.RunAsync(@"
                    MATCH (p:Product)<-[:BOUGHT]-(u:User)-[:HAS_GENDER]->(:Gender {name: $gender})
                    WHERE coalesce(p.isDeleted, false) = false AND NOT p.id IN $exclude
                    RETURN p.id AS id, COUNT(*) AS popularity
                    ORDER BY popularity DESC
                    LIMIT $limit
                ", new
                {
                    gender,
                    exclude = ids.Select(id => id.ToString()).ToList(),
                    limit = needed - ids.Count
                });

                ids.AddRange((await res.ToListAsync()).Select(r => Guid.Parse(r["id"].As<string>())));
            }

            if (ids.Count < needed && !string.IsNullOrWhiteSpace(country))
            {
                var res = await session.RunAsync(@"
                    MATCH (p:Product)<-[:BOUGHT]-(u:User)-[:FROM_COUNTRY]->(:Country {name: $country})
                    WHERE coalesce(p.isDeleted, false) = false AND NOT p.id IN $exclude
                    RETURN p.id AS id, COUNT(*) AS popularity
                    ORDER BY popularity DESC
                    LIMIT $limit
                ", new
                {
                    country,
                    exclude = ids.Select(id => id.ToString()).ToList(),
                    limit = needed - ids.Count
                });

                ids.AddRange((await res.ToListAsync()).Select(r => Guid.Parse(r["id"].As<string>())));
            }

            if (ids.Count < needed)
            {
                var res = await session.RunAsync(@"
                    MATCH (p:Product)<-[:BOUGHT]-()
                    WHERE coalesce(p.isDeleted, false) = false AND NOT p.id IN $exclude
                    RETURN p.id AS id, COUNT(*) AS popularity
                    ORDER BY popularity DESC
                    LIMIT $limit
                ", new
                {
                    exclude = ids.Select(id => id.ToString()).ToList(),
                    limit = needed - ids.Count
                });

                ids.AddRange((await res.ToListAsync()).Select(r => Guid.Parse(r["id"].As<string>())));
            }

            if (ids.Count < needed)
            {
                var res = await session.RunAsync(@"
                    MATCH (p:Product)
                    WHERE coalesce(p.isDeleted, false) = false AND NOT p.id IN $exclude
                    RETURN p.id AS id
                    ORDER BY id(p) DESC
                    LIMIT $limit
                ", new
                {
                    exclude = ids.Select(id => id.ToString()).ToList(),
                    limit = needed - ids.Count
                });

                ids.AddRange((await res.ToListAsync()).Select(r => Guid.Parse(r["id"].As<string>())));
            }

            var products = new List<Product>();
            foreach (var id in ids)
            {
                var product = await _productRepository.GetByIdAsync(id, null);
                if (product != null)
                    products.Add(product);
            }

            return products;
        }

        private (string interactionTypes, string exclusionTypes) GetRecommendationFilters(string context)
        {
            var interactionTypes = context switch
            {
                "cart" => ":IN_CART|VIEWED|BOUGHT",
                "product" => ":VIEWED|RATED",
                "home" => ":BOUGHT|IN_CART|RATED|VIEWED|BROWSED",
                _ => ":BOUGHT|IN_CART|RATED|VIEWED|BROWSED"
            };

            var exclusionTypes = context switch
            {
                "cart" => ":BOUGHT|IN_CART",
                "product" => ":BOUGHT|IN_CART|RATED",
                "home" => ":BOUGHT|IN_CART",
                _ => ":BOUGHT|IN_CART"
            };

            return (interactionTypes, exclusionTypes);
        }

    }
}
