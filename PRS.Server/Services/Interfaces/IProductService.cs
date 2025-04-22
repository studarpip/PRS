using PRS.Model.Entities;
using PRS.Model.Requests;
using PRS.Model.Responses;

namespace PRS.Server.Services.Interfaces
{
    public interface IProductService
    {
        Task<ServerResponse<List<Product>>> GetAllAsync();
        Task<ServerResponse> CreateAsync(ProductCreateRequest request);
        Task<ServerResponse> UpdateAsync(Guid id, ProductEditRequest request);
        Task<ServerResponse> DeleteAsync(Guid id);
    }
}
