using PRS.Model.Entities;
using PRS.Model.Requests;
using PRS.Model.Responses;

namespace PRS.Server.Services.Interfaces
{
    public interface IProductService
    {
        Task<ServerResponse<Product>> GetByIdAsync(Guid itemId, string userId);
        Task<ServerResponse<List<Product>>> SearchAsync(ProductSearchRequest request, string? userId);
        Task<ServerResponse> CreateAsync(ProductCreateEditRequest request);
        Task<ServerResponse> UpdateAsync(Guid id, ProductCreateEditRequest request);
        Task<ServerResponse> DeleteAsync(Guid id);
    }
}
