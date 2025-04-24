using Neo4j.Driver;
using PRS.Model.Responses;
using PRS.Server.Repositories.Interfaces;

namespace PRS.Server.Repositories
{
    public class RatingRepository : IRatingRepository
    {
        private readonly IDriver _driver;

        public RatingRepository(IDriver driver)
        {
            _driver = driver;
        }

        public async Task<RatingCheckResponse> CheckIfUserCanRateAsync(string userId, Guid productId)
        {
            await using var session = _driver.AsyncSession();

            var result = await session.RunAsync(@"
                MATCH (u:User { id: $userId })-[b:BOUGHT]->(p:Product { id: $productId })
                OPTIONAL MATCH (u)-[r:RATED]->(p)
                RETURN b IS NOT NULL AS bought, r.rating AS rating
            ", new
            {
                userId,
                productId = productId.ToString()
            });

            IRecord? record = null;

            var records = await result.ToListAsync();
            if (records.Count != 0)
                record = records.First();

            if (record == null || !record["bought"].As<bool>())
                return new()
                {
                    CanRate = false,
                    PreviousRating = null
                };

            return new()
            {
                CanRate = true,
                PreviousRating = record["rating"].As<int?>()
            };
        }

        public async Task SubmitRatingAsync(string userId, Guid productId, int rating)
        {
            await using var session = _driver.AsyncSession();

            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(@"
                    MATCH (:User { id: $userId })-[r:RATED]->(:Product { id: $productId })
                    DELETE r
                ", new
                {
                    userId,
                    productId = productId.ToString()
                });

                await tx.RunAsync(@"
                    MATCH (u:User { id: $userId }), (p:Product { id: $productId })
                    MERGE (u)-[r:RATED]->(p)
                    SET r.rating = $rating,
                        r.timestamp = datetime()
                ", new
                {
                    userId,
                    productId = productId.ToString(),
                    rating
                });

                var avgResult = await tx.RunAsync(@"
                    MATCH (:User)-[r:RATED]->(p:Product { id: $productId })
                    RETURN avg(r.rating) AS average
                ", new
                {
                    productId = productId.ToString()
                });

                var avg = await avgResult.SingleAsync();
                var average = avg["average"].As<double>();

                await tx.RunAsync(@"
                    MATCH (p:Product { id: $productId })
                    SET p.averageRating = $average
                ", new
                {
                    productId = productId.ToString(),
                    average
                });
            });
        }
    }
}
