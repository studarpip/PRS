using Neo4j.Driver;
using PRS.Model.Enums;
using PRS.Server.Helpers;
using PRS.Server.Migrations.Seeders.Interfaces;

namespace PRS.Server.Migrations.Seeders
{
    public class CountrySeeder : IDatabaseSeeder
    {
        public async Task SeedAsync(IAsyncTransaction tx)
        {
            var existing = new HashSet<string>();

            var result = await tx.RunAsync("MATCH (c:Country) RETURN c.name AS name");

            await result.ForEachAsync(record =>
            {
                existing.Add(record["name"].As<string>());
            });

            foreach (var country in Enum.GetValues<Country>())
            {
                var name = country.ToString();

                if (country.ShouldSkipRelationship() || existing.Contains(name))
                    continue;

                await tx.RunAsync("MERGE (:Country { name: $name })", new { name });
            }
        }
    }
}
