using Neo4j.Driver;

namespace PRS.Server.Migrations.Seeders;

public interface IDatabaseSeeder
{
    Task SeedAsync(IDriver driver);
}
