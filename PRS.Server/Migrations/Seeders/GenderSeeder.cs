using Neo4j.Driver;
using PRS.Model.Enums;

namespace PRS.Server.Migrations.Seeders;

public class GenderSeeder : IDatabaseSeeder
{
    public async Task SeedAsync(IDriver driver)
    {
        var session = driver.AsyncSession();

        try
        {
            var existing = new HashSet<string>();

            var result = await session.RunAsync("MATCH (g:Gender) RETURN g.name AS name");

            await result.ForEachAsync(record =>
            {
                var name = record["name"].As<string>();
                existing.Add(name);
            });

            var allEnumValues = Enum.GetValues<Gender>();

            foreach (var gender in allEnumValues)
            {
                var genderName = gender.ToString();
                if (existing.Contains(genderName)) 
                    continue;

                var cypher = "MERGE (:Gender { name: $name })";
                var parameters = new Dictionary<string, object> { { "name", genderName } };

                await session.RunAsync(cypher, parameters);
            }
        }
        finally
        {
            await session.CloseAsync();
        }
    }
}
