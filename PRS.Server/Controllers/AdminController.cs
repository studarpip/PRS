using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRS.Model.Entities;
using PRS.Model.Requests;
using PRS.Model.Responses;
using PRS.Server.Services.Interfaces;

namespace PRS.Server.Controllers
{
    [ApiController]
    [Route("api/admin/products")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IProductService _productService;

        public AdminController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost("search")]
        public async Task<ActionResult<ServerResponse<List<Product>>>> Search([FromBody] ProductSearchRequest request)
        {
            var response = await _productService.SearchAsync(request, null);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPost]
        public async Task<ActionResult<ServerResponse>> Create([FromBody] ProductCreateEditRequest request)
        {
            var response = await _productService.CreateAsync(request);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ServerResponse>> Edit(Guid id, [FromBody] ProductCreateEditRequest request)
        {
            var response = await _productService.UpdateAsync(id, request);
            return response.Success ? Ok(response) : BadRequest(response);
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult<ServerResponse>> Delete(Guid id)
        {
            var response = await _productService.DeleteAsync(id);
            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
}
