using Neo4j.Driver;
using PRS.Server.Migrations.Seeders.Interfaces;

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
        await using var session = _driver.AsyncSession();
        var tx = await session.BeginTransactionAsync();

        try
        {
            foreach (var seeder in _seeders)
            {
                await seeder.SeedAsync(tx);
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
}
