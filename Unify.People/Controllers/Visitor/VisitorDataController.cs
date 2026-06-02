using Microsoft.AspNetCore.Mvc;
using Unify.Data.SQL;
using Unify.People.Services;

namespace Unify.People.Controllers.Visitors
{
    [Route("api/[controller]")]
    [ApiController]
    public class VisitorDataController : ControllerBase
    {
        private readonly ILogger<VisitorDataController> _logger;
        private readonly VisitorService _visitorServices;

        public VisitorDataController(VisitorService visitorService,
            ILogger<VisitorDataController> logger)
        {
            _visitorServices = visitorService;
            _logger = logger;
        }

        [HttpGet("GetVisitorById")]
        [ProducesResponseType(typeof(Visitor), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IResult> GetVisitorByIdAsync(Guid TenantId,Guid visitorId)
        {
            try
            {
                var data = await _visitorServices.GetVisitorByIdAsync(TenantId,visitorId);
                return Results.Ok(data);
            }
            catch (Exception ex)
            {
                var ex1 = new ServiceException("Something went wrong", ex);
                _logger.LogError(ex1, null);
                return Results.Problem(ex1.GetProblemDetails());
            }
        }
    }
}
