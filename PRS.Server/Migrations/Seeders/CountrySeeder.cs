using Neo4j.Driver;
using PRS.Model.Enums;

namespace PRS.Server.Migrations.Seeders;

public class CountrySeeder : IDatabaseSeeder
{
    public async Task SeedAsync(IDriver driver)
    {
        var session = driver.AsyncSession();

        try
        {
            var existing = new HashSet<string>();

            var result = await session.RunAsync("MATCH (c:Country) RETURN c.name AS name");

            await result.ForEachAsync(record =>
            {
                var name = record["name"].As<string>();
                existing.Add(name);
            });

            var allEnumValues = Enum.GetValues<Country>();

            foreach (var country in allEnumValues)
            {
                var countryName = country.ToString();
                if (existing.Contains(countryName)) 
                    continue;

                var cypher = "MERGE (:Country { name: $name })";
                var parameters = new Dictionary<string, object> { { "name", countryName } };

                await session.RunAsync(cypher, parameters);
            }
        }
        finally
        {
            await session.CloseAsync();
        }
    }
}
