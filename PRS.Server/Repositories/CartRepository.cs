using Neo4j.Driver;
using PRS.Model.Entities;
using PRS.Server.Repositories.Interfaces;

namespace PRS.Server.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly IDriver _driver;

        public CartRepository(IDriver driver)
        {
            _driver = driver;
        }

        public async Task BuyCartAsync(string userId, List<Guid> productIds)
        {
            await using var session = _driver.AsyncSession();
            var saleId = Guid.NewGuid().ToString();

            await session.ExecuteWriteAsync(async tx =>
            {
                foreach (var productId in productIds)
                {
                    await tx.RunAsync(@"
                        MATCH (u:User { id: $userId })-[r:IN_CART]->(p:Product { id: $productId })
                        DELETE r
                        MERGE (u)-[:BOUGHT {
                            saleId: $saleId,
                            timestamp: datetime()
                        }]->(p)
                    ", new
                    {
                        userId,
                        productId = productId.ToString(),
                        saleId
                    });
                }
            });
        }

        public async Task AddOrUpdateCartItemAsync(string userId, Guid productId, int count)
        {
            await using var session = _driver.AsyncSession();

            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(@"
                    MATCH (u:User { id: $userId }), (p:Product { id: $productId })
                    MERGE (u)-[r:IN_CART]->(p)
                    SET r.count = $count,
                        r.timestamp = datetime()
                ", new { userId, productId = productId.ToString(), count });
            });
        }

        public async Task<List<CartProduct>> GetCartItemsAsync(string userId)
        {
            await using var session = _driver.AsyncSession();

            var result = await session.RunAsync(@"
                MATCH (u:User { id: $userId })-[r:IN_CART]->(p:Product)
                RETURN p, r.count AS count
            ", new { userId });

            var cart = new List<CartProduct>();

            await result.ForEachAsync(record =>
            {
                var node = record["p"].As<INode>();
                cart.Add(new CartProduct
                {
                    ProductId = Guid.Parse(node.Properties["id"].As<string>()),
                    Name = node.Properties["name"].As<string>(),
                    Image = node.Properties.ContainsKey("image") ? node.Properties["image"].As<byte[]>() : null,
                    Price = node.Properties["price"].As<decimal>(),
                    Count = record["count"].As<int>()
                });
            });

            return cart;
        }

        public async Task RemoveCartItemAsync(string userId, Guid productId)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(@"
                    MATCH (u:User { id: $userId })-[r:IN_CART]->(p:Product { id: $productId })
                    DELETE r
                ", new { userId, productId = productId.ToString() });
            });
        }
    }
}
