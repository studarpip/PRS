using Neo4j.Driver;
using PRS.Server.Helpers;
using PRS.Server.Migrations.Seeders.Interfaces;

namespace PRS.Server.Migrations.Seeders
{
    public class CategorySeeder : IDatabaseSeeder
    {
        public async Task SeedAsync(IAsyncTransaction tx)
        {
            var existing = new HashSet<string>();

            var result = await tx.RunAsync("MATCH (c:Category) RETURN c.name AS name");

            await result.ForEachAsync(record =>
            {
                existing.Add(record["name"].As<string>());
            });

            foreach (var category in Enum.GetValues<Model.Enums.Category>())
            {
                var name = category.ToString();

                if (category.ShouldSkipRelationship() || existing.Contains(name))
                    continue;

                await tx.RunAsync("MERGE (:Category { name: $name })", new { name });
            }
        }
    }

}
