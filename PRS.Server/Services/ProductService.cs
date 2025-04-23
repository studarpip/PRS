using PRS.Model.Entities;
using PRS.Model.Exceptions;
using PRS.Model.Requests;
using PRS.Model.Responses;
using PRS.Server.Repositories.Interfaces;
using PRS.Server.Services.Interfaces;

namespace PRS.Server.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<ServerResponse<Product>> GetByIdAsync(Guid id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
                throw new ProductNotFoundException();

            return ServerResponse<Product>.Ok(product);
        }

        public async Task<ServerResponse<List<Product>>> SearchAsync(ProductSearchRequest request)
        {
            var products = await _repository.SearchAsync(request);
            return ServerResponse<List<Product>>.Ok(products);
        }

        public async Task<ServerResponse<List<Product>>> GetAllAsync()
        {
            var products = await _repository.GetAllAsync();
            return ServerResponse<List<Product>>.Ok(products);
        }

        public async Task<ServerResponse> CreateAsync(ProductCreateEditRequest request)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Image = request.Image,
                Categories = request.Categories,
                Price = request.Price,
            };

            await _repository.CreateAsync(product);
            return ServerResponse.Ok();
        }

        public async Task<ServerResponse> UpdateAsync(Guid id, ProductCreateEditRequest request)
        {
            var product = new Product
            {
                Id = id,
                Name = request.Name,
                Description = request.Description,
                Image = request.Image,
                Categories = request.Categories,
                Price= request.Price,
            };

            await _repository.UpdateAsync(product);
            return ServerResponse.Ok();
        }

        public async Task<ServerResponse> DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
            return ServerResponse.Ok();
        }
    }
}
