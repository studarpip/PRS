using Neo4j.Driver;
using PRS.Model.Entities;
using PRS.Model.Enums;
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
                    Categories = categories
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
                        image: $image
                    })
                ", new
                {
                    id = product.Id.ToString(),
                    name = product.Name,
                    description = product.Description ?? "",
                    image = product.Image ?? Array.Empty<byte>()
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
                        p.image = $image
                ", new
                {
                    id = product.Id.ToString(),
                    name = product.Name,
                    description = product.Description ?? "",
                    image = product.Image ?? Array.Empty<byte>()
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
