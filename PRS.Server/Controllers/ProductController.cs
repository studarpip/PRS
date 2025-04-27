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
    [Route("api/products")]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServerResponse<Product>>> GetById(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return Unauthorized();

            var response = await _productService.GetByIdAsync(id, userId);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        [HttpPost("search")]
        public async Task<ActionResult<ServerResponse<List<Product>>>> Search([FromBody] ProductSearchRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return Unauthorized();

            var response = await _productService.SearchAsync(request, userId);
            return Ok(response);
        }
    }
}
