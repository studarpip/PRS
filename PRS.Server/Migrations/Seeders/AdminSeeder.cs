using Neo4j.Driver;
using PRS.Model.Entities;
using PRS.Model.Enums;
using PRS.Server.Helpers;
using PRS.Server.Helpers.Interfaces;
using PRS.Server.Migrations.Seeders.Interfaces;

namespace PRS.Server.Migrations.Seeders
{
    public class AdminSeeder : IDatabaseSeeder
    {
        private readonly IEncryptionHelper _encryption;

        public AdminSeeder(IEncryptionHelper encryption)
        {
            _encryption = encryption;
        }

        public async Task SeedAsync(IAsyncTransaction tx)
        {
            var result = await tx.RunAsync(@"
                MATCH (u:User { username: $username })
                RETURN count(u) > 0 AS exists
            ", new { username = "admin" });

            var exists = (await result.SingleAsync())["exists"].As<bool>();
            if (exists) return;

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                Email = _encryption.Encrypt("admin@example.com"),
                EmailHash = "admin@example.com".HashString(),
                Password = "admin".HashString(),
                Role = Role.Admin
            };

            await tx.RunAsync(@"
                CREATE (:User {
                    id: $id,
                    username: $username,
                    email: $email,
                    emailhash: $emailhash,
                    password: $password,
                    role: $role
                })
            ", new
            {
                id = user.Id.ToString(),
                username = user.Username,
                email = user.Email,
                emailhash = user.EmailHash,
                password = user.Password,
                role = user.Role.ToString()
            });
        }
    }
}
