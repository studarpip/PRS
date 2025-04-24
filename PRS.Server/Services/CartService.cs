using PRS.Model.Entities;
using PRS.Model.Exceptions;
using PRS.Model.Requests;
using PRS.Model.Responses;
using PRS.Server.Repositories.Interfaces;
using PRS.Server.Services.Interfaces;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;

    public CartService(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<ServerResponse> BuyCartAsync(string userId)
    {
        var items = await _cartRepository.GetCartItemsAsync(userId);

        if (items.Count == 0)
            throw new CartIsEmptyException();

        var saleId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;

        await _cartRepository.BuyCartAsync(userId, items.Select(i => i.ProductId).ToList());
        return ServerResponse.Ok();
    }

    public async Task<ServerResponse<List<CartProduct>>> GetCartAsync(string userId)
    {
        var items = await _cartRepository.GetCartItemsAsync(userId);
        return ServerResponse<List<CartProduct>>.Ok(items);
    }

    public async Task<ServerResponse> UpdateItemsAsync(string userId, CartRequest request)
    {
        if (request.Count == 0)
            await _cartRepository.RemoveCartItemAsync(userId, request.ProductId);
        else
            await _cartRepository.AddOrUpdateCartItemAsync(userId, request.ProductId, request.Count);

        return ServerResponse.Ok();
    }
}
