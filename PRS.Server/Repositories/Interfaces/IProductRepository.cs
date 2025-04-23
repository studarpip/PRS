using PRS.Model.Entities;
using PRS.Model.Requests;

namespace PRS.Server.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(Guid id);
        Task<List<Product>> SearchAsync(ProductSearchRequest request);
        Task CreateAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(Guid id);
    }
}
