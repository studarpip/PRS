using Neo4j.Driver;
using PRS.Model.Enums;
using PRS.Server.Helpers;
using PRS.Server.Migrations.Seeders.Interfaces;

namespace PRS.Server.Migrations.Seeders
{
    public class GenderSeeder : IDatabaseSeeder
    {
        public async Task SeedAsync(IAsyncTransaction tx)
        {
            var existing = new HashSet<string>();

            var result = await tx.RunAsync("MATCH (g:Gender) RETURN g.name AS name");

            await result.ForEachAsync(record =>
            {
                existing.Add(record["name"].As<string>());
            });

            foreach (var gender in Enum.GetValues<Gender>())
            {
                var genderName = gender.ToString();

                if (gender.ShouldSkipRelationship() || existing.Contains(genderName))
                    continue;

                await tx.RunAsync("MERGE (:Gender { name: $name })", new { name = genderName });
            }
        }
    }
}
