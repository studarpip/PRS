using PRS.Model.Entities;
using PRS.Model.Requests;
using PRS.Model.Responses;

namespace PRS.Server.Services.Interfaces
{
    public interface ICartService
    {
        Task<ServerResponse<List<CartProduct>>> GetCartAsync(string userId);
        Task<ServerResponse> UpdateItemsAsync(string userId, CartRequest request);
        Task<ServerResponse> BuyCartAsync(string userId);
    }
}
