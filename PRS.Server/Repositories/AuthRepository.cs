using Neo4j.Driver;
using PRS.Model.Entities;
using PRS.Model.Enums;
using PRS.Server.Repositories.Interfaces;

namespace PRS.Server.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly IDriver _driver;

        public AuthRepository(IDriver driver)
        {
            _driver = driver;
        }

        public async Task<UserBasic?> GetByUsernameAsync(string username)
        {
            await using var session = _driver.AsyncSession();
            var result = await session.RunAsync(@"
                MATCH (u:User { username: $username })
                RETURN u.id AS id,
                       u.username AS username,
                       u.password AS password,
                       u.role AS role
            ", new { username });

            var records = await result.ToListAsync();
            if (records.Count == 0)
                return null;

            var record = records.First();

            return new UserBasic
            {
                Id = Guid.Parse(record["id"].As<string>()),
                Username = record["username"].As<string>(),
                Password = record["password"].As<string>(),
                Role = Enum.Parse<Role>(record["role"].As<string>())
            };
        }
    }
}
