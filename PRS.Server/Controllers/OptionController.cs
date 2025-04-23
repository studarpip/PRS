using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRS.Model.Entities;
using PRS.Model.Enums;
using PRS.Model.Responses;
using PRS.Server.Helpers;

namespace PRS.Server.Controllers
{
    namespace PRS.Server.Controllers
    {
        [ApiController]
        [Route("api/options")]
        public class OptionController : ControllerBase
        {
            [Authorize]
            [HttpGet("orderBy")]
            public ActionResult<ServerResponse<List<EnumOption>>> GetOrderBy()
            {
                var orderByOptions = Enum.GetValues<ProductOrderBy>()
                    .Where(c => !c.ShouldSkipRelationship())
                    .Select(c => new EnumOption { Value = (int)c, Label = c.ToString() })
                    .ToList();

                return Ok(ServerResponse<List<EnumOption>>.Ok(orderByOptions));
            }

            [Authorize]
            [HttpGet("categories")]
            public ActionResult<ServerResponse<List<EnumOption>>> GetCategories()
            {
                var categories = Enum.GetValues<Model.Enums.Category>()
                    .Where(c => !c.ShouldSkipRelationship())
                    .Select(c => new EnumOption { Value = (int)c, Label = c.ToString() })
                    .ToList();

                return Ok(ServerResponse<List<EnumOption>>.Ok(categories));
            }

            [HttpGet("registrationOptions")]
            public ActionResult<ServerResponse<RegistrationOptionsResponse>> GetRegistrationOptions()
            {
                var genders = Enum.GetValues<Gender>()
                    .Where(g => !g.ShouldSkipRelationship())
                    .Select(g => new EnumOption { Value = (int)g, Label = g.ToString() })
                    .ToList();

                var countries = Enum.GetValues<Country>()
                    .Where(c => !c.ShouldSkipRelationship())
                    .Select(c => new EnumOption { Value = (int)c, Label = c.ToString() })
                    .ToList();

                var response = new RegistrationOptionsResponse
                {
                    Genders = genders,
                    Countries = countries
                };

                return Ok(ServerResponse<RegistrationOptionsResponse>.Ok(response));
            }
        }
    }

}
