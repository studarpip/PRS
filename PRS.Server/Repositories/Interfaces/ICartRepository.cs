using PRS.Model.Entities;

namespace PRS.Server.Repositories.Interfaces
{
    public interface ICartRepository
    {
        Task<List<CartProduct>> GetCartItemsAsync(string userId);
        Task AddOrUpdateCartItemAsync(string userId, Guid productId, int count);
        Task RemoveCartItemAsync(string userId, Guid productId);
        Task BuyCartAsync(string userId, List<Guid> productIds);
    }
}
