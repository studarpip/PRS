using Neo4j.Driver;
using PRS.Server.Migrations.Seeders;

namespace PRS.Server.Migrations;

public class SeederRunner
{
    private readonly IEnumerable<IDatabaseSeeder> _seeders;
    private readonly IDriver _driver;

    public SeederRunner(IEnumerable<IDatabaseSeeder> seeders, IDriver driver)
    {
        _seeders = seeders;
        _driver = driver;
    }

    public async Task RunAllAsync()
    {
        foreach (var seeder in _seeders)
        {
            await seeder.SeedAsync(_driver);
        }
    }
}
