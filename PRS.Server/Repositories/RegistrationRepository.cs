using Neo4j.Driver;
using PRS.Model.Entities;
using PRS.Server.Helpers;
using PRS.Server.Repositories.Interfaces;

namespace PRS.Server.Repositories
{
    public class RegistrationRepository : IRegistrationRepository
    {
        private readonly IDriver _driver;

        public RegistrationRepository(IDriver driver)
        {
            _driver = driver;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            await using var session = _driver.AsyncSession();
            var result = await session.RunAsync(
                "MATCH (u:User { username: $username }) RETURN count(u) > 0 AS exists",
                new { username }
            );
            return (await result.SingleAsync())["exists"].As<bool>();
        }

        public async Task<bool> EmailExistsAsync(string hashedEmail)
        {
            await using var session = _driver.AsyncSession();
            var result = await session.RunAsync(
                "MATCH (u:User { emailhash: $email }) RETURN count(u) > 0 AS exists",
                new { email = hashedEmail }
            );
            return (await result.SingleAsync())["exists"].As<bool>();
        }

        public async Task CreateAsync(User user)
        {
            await using var session = _driver.AsyncSession();
            var tx = await session.BeginTransactionAsync();

            try
            {
                await tx.RunAsync(@"
                    CREATE (u:User {
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

                if (!user.Country.ShouldSkipRelationship())
                {
                    await tx.RunAsync(@"
                        MATCH (u:User { id: $id })
                        MATCH (c:Country { name: $country })
                        MERGE (u)-[:FROM_COUNTRY]->(c)
                    ", new
                    {
                        id = user.Id.ToString(),
                        country = user.Country.ToString()
                    });
                }

                if (!user.Gender.ShouldSkipRelationship())
                {
                    await tx.RunAsync(@"
                        MATCH (u:User { id: $id })
                        MATCH (g:Gender { name: $gender })
                        MERGE (u)-[:HAS_GENDER]->(g)
                    ", new
                    {
                        id = user.Id.ToString(),
                        gender = user.Gender.ToString()
                    });
                }

                var defaultSettings = new RecommendationSettings
                {
                    UseContent = true,
                    UseCollaborative = true,
                    CategoryWeight = 0.8,
                    PriceWeight = 0.2,
                    BrowseWeight = 0.1,
                    ViewWeight = 0.2,
                    CartWeight = 0.3,
                    PurchaseWeight = 0.3,
                    RatingWeight = 0.1
                };

                await tx.RunAsync(@"
                    MERGE (s:Setting {userId: $userId})
                    SET s.useContent = $useContent,
                        s.useCollaborative = $useCollaborative,
                        s.categoryWeight = $categoryWeight,
                        s.priceWeight = $priceWeight,
                        s.browseWeight = $browseWeight,
                        s.viewWeight = $viewWeight,
                        s.cartWeight = $cartWeight,
                        s.purchaseWeight = $purchaseWeight,
                        s.ratingWeight = $ratingWeight
                ", new
                {
                    userId = user.Id.ToString(),
                    useContent = defaultSettings.UseContent,
                    useCollaborative = defaultSettings.UseCollaborative,
                    categoryWeight = defaultSettings.CategoryWeight,
                    priceWeight = defaultSettings.PriceWeight,
                    browseWeight = defaultSettings.BrowseWeight,
                    viewWeight = defaultSettings.ViewWeight,
                    cartWeight = defaultSettings.CartWeight,
                    purchaseWeight = defaultSettings.PurchaseWeight,
                    ratingWeight = defaultSettings.RatingWeight
                });

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
}
