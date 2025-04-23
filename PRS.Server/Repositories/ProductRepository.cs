using Neo4j.Driver;
using PRS.Model.Entities;
using PRS.Model.Enums;
using PRS.Model.Requests;
using PRS.Server.Repositories.Interfaces;
using Category = PRS.Model.Enums.Category;

namespace PRS.Server.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IDriver _driver;

        public ProductRepository(IDriver driver)
        {
            _driver = driver;
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            await using var session = _driver.AsyncSession();

            var result = await session.RunAsync(@"
                MATCH (p:Product { id: $id })
                OPTIONAL MATCH (p)-[:IN_CATEGORY]->(c:Category)
                RETURN p, collect(c.name) AS categories
            ", new { id = id.ToString() });

            var records = await result.ToListAsync();
            if (records.Count == 0)
                return null;

            var record = records.First();

            var node = record["p"].As<INode>();
            var categoryStrings = record["categories"].As<List<string>>();

            var categories = new List<Category>();
            foreach (var c in categoryStrings)
            {
                if (Enum.TryParse<Category>(c, out var parsed))
                    categories.Add(parsed);
            }

            return new Product
            {
                Id = Guid.Parse(node.Properties["id"].As<string>()),
                Name = node.Properties["name"].As<string>(),
                Description = node.Properties.ContainsKey("description") ? node.Properties["description"].As<string>() : null,
                Image = node.Properties.ContainsKey("image") ? node.Properties["image"].As<byte[]>() : null,
                Categories = categories,
                Price = node.Properties["price"].As<decimal>(),
                Rating = node.Properties.ContainsKey("averageRating") ? node.Properties["averageRating"].As<decimal>() : null
            };
        }

        public async Task<List<Product>> SearchAsync(ProductSearchRequest request)
        {
            await using var session = _driver.AsyncSession();

            var filters = new List<string>();

            if (!string.IsNullOrWhiteSpace(request.Input))
                filters.Add("p.name CONTAINS $input OR p.description CONTAINS $input");

            if (request.PriceFrom.HasValue)
                filters.Add("p.price >= $priceFrom");

            if (request.PriceTo.HasValue)
                filters.Add("p.price <= $priceTo");

            if (request.RatingFrom.HasValue)
                filters.Add("p.averageRating >= $ratingFrom");

            if (request.RatingTo.HasValue)
                filters.Add("p.averageRating <= $ratingTo");

            if (request.Categories != null && request.Categories.Any())
                filters.Add("any(cat IN $categories WHERE cat IN categories)");

            string? whereClause = filters.Any() ? $"WHERE {string.Join(" AND ", filters)}" : null;

            string? orderByClause = request.OrderBy switch
            {
                ProductOrderBy.PriceAsc => "ORDER BY p.price ASC",
                ProductOrderBy.PriceDesc => "ORDER BY p.price DESC",
                ProductOrderBy.RatingAsc => "ORDER BY coalesce(p.averageRating, 0) ASC",
                ProductOrderBy.RatingDesc => "ORDER BY coalesce(p.averageRating, 0) DESC",
                _ => null
            };

            string query = $@"
                MATCH (p:Product)
                OPTIONAL MATCH (p)-[:IN_CATEGORY]->(c:Category)
                WITH p, collect(c.name) AS categories
                {whereClause}
                {orderByClause}
                RETURN p, categories
                SKIP $skip
                LIMIT $limit
            ";

            var result = await session.RunAsync(query, new
            {
                input = request.Input ?? "",
                priceFrom = request.PriceFrom,
                priceTo = request.PriceTo,
                ratingFrom = request.RatingFrom,
                ratingTo = request.RatingTo,
                categories = request.Categories?.Select(c => c.ToString()).ToList() ?? new List<string>(),
                skip = (request.Page - 1) * request.PageSize,
                limit = request.PageSize
            });

            var products = new List<Product>();

            await result.ForEachAsync(record =>
            {
                var node = record["p"].As<INode>();
                var categoryStrings = record["categories"].As<List<string>>();

                var categories = new List<Category>();
                foreach (var c in categoryStrings)
                {
                    if (Enum.TryParse<Category>(c, out var parsed))
                        categories.Add(parsed);
                }

                products.Add(new Product
                {
                    Id = Guid.Parse(node.Properties["id"].As<string>()),
                    Name = node.Properties["name"].As<string>(),
                    Description = node.Properties.ContainsKey("description") ? node.Properties["description"].As<string>() : null,
                    Image = node.Properties.ContainsKey("image") ? node.Properties["image"].As<byte[]>() : null,
                    Categories = categories,
                    Price = node.Properties["price"].As<decimal>(),
                    Rating = node.Properties.ContainsKey("averageRating") ? node.Properties["averageRating"].As<decimal>() : null
                });
            });

            return products;
        }


        public async Task<List<Product>> GetAllAsync()
        {
            await using var session = _driver.AsyncSession();

            var result = await session.RunAsync(@"
                MATCH (p:Product)
                OPTIONAL MATCH (p)-[:IN_CATEGORY]->(c:Category)
                RETURN p, collect(c.name) AS categories
            ");

            var products = new List<Product>();

            await result.ForEachAsync(record =>
            {
                var node = record["p"].As<INode>();
                var categoryStrings = record["categories"].As<List<string>>();

                var categories = new List<Category>();
                foreach (var c in categoryStrings)
                {
                    if (Enum.TryParse<Category>(c, out var parsed))
                        categories.Add(parsed);
                }


                products.Add(new Product
                {
                    Id = Guid.Parse(node.Properties["id"].As<string>()),
                    Name = node.Properties["name"].As<string>(),
                    Description = node.Properties.ContainsKey("description") ? node.Properties["description"].As<string>() : null,
                    Image = node.Properties.ContainsKey("image") ? node.Properties["image"].As<byte[]>() : null,
                    Categories = categories,
                    Price = node.Properties["price"].As<decimal>(),
                    Rating = node.Properties.ContainsKey("averageRating") ? node.Properties["averageRating"].As<decimal>() : null,
                });
            });

            return products;
        }

        public async Task CreateAsync(Product product)
        {
            await using var session = _driver.AsyncSession();
            var tx = await session.BeginTransactionAsync();

            try
            {
                await tx.RunAsync(@"
                    CREATE (p:Product {
                        id: $id,
                        name: $name,
                        description: $description,
                        image: $image,
                        price: $price
                    })
                ", new
                {
                    id = product.Id.ToString(),
                    name = product.Name,
                    description = product.Description ?? "",
                    image = product.Image ?? Array.Empty<byte>(),
                    price = product.Price,
                });

                foreach (var category in product.Categories)
                {
                    await tx.RunAsync(@"
                        MATCH (p:Product { id: $id })
                        MATCH (c:Category { name: $category })
                        MERGE (p)-[:IN_CATEGORY]->(c)
                    ", new
                    {
                        id = product.Id.ToString(),
                        category = category.ToString()
                    });
                }

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task UpdateAsync(Product product)
        {
            await using var session = _driver.AsyncSession();
            var tx = await session.BeginTransactionAsync();

            try
            {
                await tx.RunAsync(@"
                    MATCH (p:Product { id: $id })
                    SET p.name = $name,
                        p.description = $description,
                        p.image = $image,
                        p.price = $price
                ", new
                {
                    id = product.Id.ToString(),
                    name = product.Name,
                    description = product.Description ?? "",
                    image = product.Image ?? Array.Empty<byte>(),
                    price = product.Price,
                });

                await tx.RunAsync(@"
                    MATCH (p:Product { id: $id })-[r:IN_CATEGORY]->()
                    DELETE r
                ", new { id = product.Id.ToString() });

                foreach (var category in product.Categories)
                {
                    await tx.RunAsync(@"
                        MATCH (p:Product { id: $id })
                        MATCH (c:Category { name: $category })
                        MERGE (p)-[:IN_CATEGORY]->(c)
                    ", new
                    {
                        id = product.Id.ToString(),
                        category = category.ToString()
                    });
                }

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(@"
                    MATCH (p:Product { id: $id })
                    DETACH DELETE p
                ", new { id = id.ToString() });
            });
        }
    }
}
