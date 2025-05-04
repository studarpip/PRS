using PRS.Model.Entities;
using PRS.Model.Requests;

namespace PRS.Server.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(Guid itemId, string? userId);
        Task<List<Product>> SearchAsync(ProductSearchRequest request, string? userId);
        Task CreateAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(Guid id);
    }
}
