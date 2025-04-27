using Neo4j.Driver;
using PRS.Model.Entities;
using PRS.Model.Requests;
using PRS.Server.Repositories.Interfaces;

namespace PRS.Server.Repositories
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly IDriver _driver;

        public SettingsRepository(IDriver driver)
        {
            _driver = driver;
        }

        public async Task<RecommendationSettings> GetSettingsAsync(string userId)
        {
            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(@"
                    MATCH (s:Setting {userId: $userId})
                    RETURN 
                        s.useContent AS useContent,
                        s.useCollaborative AS useCollaborative,
                        s.categoryWeight AS categoryWeight,
                        s.priceWeight AS priceWeight,
                        s.browseWeight AS browseWeight,
                        s.viewWeight AS viewWeight,
                        s.cartWeight AS cartWeight,
                        s.purchaseWeight AS purchaseWeight,
                        s.ratingWeight AS ratingWeight
                ", new { userId });

                var records = await result.ToListAsync();
                var record = records.First();

                return new RecommendationSettings
                {
                    UseContent = record["useContent"].As<bool>(),
                    UseCollaborative = record["useCollaborative"].As<bool>(),
                    CategoryWeight = record["categoryWeight"].As<double>(),
                    PriceWeight = record["priceWeight"].As<double>(),
                    BrowseWeight = record["browseWeight"].As<double>(),
                    ViewWeight = record["viewWeight"].As<double>(),
                    CartWeight = record["cartWeight"].As<double>(),
                    PurchaseWeight = record["purchaseWeight"].As<double>(),
                    RatingWeight = record["ratingWeight"].As<double>(),
                    UserId = Guid.Parse(userId)
                };
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task SaveSettingsAsync(string userId, RecommendationSettingRequest settings)
        {
            var session = _driver.AsyncSession();
            try
            {
                await session.ExecuteWriteAsync(async tx =>
                {
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
                        userId,
                        useContent = settings.UseContent,
                        useCollaborative = settings.UseCollaborative,
                        categoryWeight = settings.CategoryWeight,
                        priceWeight = settings.PriceWeight,
                        browseWeight = settings.BrowseWeight,
                        viewWeight = settings.ViewWeight,
                        cartWeight = settings.CartWeight,
                        purchaseWeight = settings.PurchaseWeight,
                        ratingWeight = settings.RatingWeight
                    });
                });
            }
            finally
            {
                await session.CloseAsync();
            }
        }
    }
}
