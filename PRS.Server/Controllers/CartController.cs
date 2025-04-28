using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRS.Model.Entities;
using PRS.Model.Requests;
using PRS.Model.Responses;
using PRS.Server.Services.Interfaces;
using System.Security.Claims;

namespace PRS.Server.Controllers
{
    [ApiController]
    [Route("api/cart")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpPost]
        public async Task<ActionResult<ServerResponse>> AddToCart([FromBody] CartRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return Unauthorized();

            var response = await _cartService.UpdateItemsAsync(userId, request);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpGet]
        public async Task<ActionResult<ServerResponse<List<CartProduct>>>> GetCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return Unauthorized();

            var response = await _cartService.GetCartAsync(userId);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPost("buy")]
        public async Task<ActionResult<ServerResponse>> BuyCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return Unauthorized();

            var response = await _cartService.BuyCartAsync(userId);
            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
}
