using Neo4j.Driver;

namespace PRS.Server.Migrations.Seeders.Interfaces;

public interface IDatabaseSeeder
{
    Task SeedAsync(IAsyncTransaction tx);
}
