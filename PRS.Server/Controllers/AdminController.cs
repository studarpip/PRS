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

        [HttpGet]
        public async Task<ActionResult<ServerResponse<List<Product>>>> GetAll()
        {
            var response = await _productService.GetAllAsync();
            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<ServerResponse>> Create([FromBody] ProductCreateRequest request)
        {
            var response = await _productService.CreateAsync(request);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ServerResponse>> Edit(Guid id, [FromBody] ProductEditRequest request)
        {
            var response = await _productService.UpdateAsync(id, request);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult<ServerResponse>> Delete(Guid id)
        {
            var response = await _productService.DeleteAsync(id);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
    }
}
