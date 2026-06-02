using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Unify.CCC.Data;
using Unify.CCC.Services;

namespace Unify.CCC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LatePolicyDataController : ControllerBase
    {
        private readonly ILogger<LatePolicyDataController> _logger;
        private readonly LatePolicyService _latePolicyServices;

        public LatePolicyDataController(LatePolicyService latePolicyServices,
            ILogger<LatePolicyDataController> logger)
        {
            _logger = logger;
            _latePolicyServices = latePolicyServices;
        }

        [HttpGet("GetEmployeeLatePolicy")]
        [ProducesResponseType(typeof(LatePolicy), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IResult> GetEmployeeLatePolicyAsync(Guid customerId, Guid employeeId)
        {
            try
            {
                var data = await _latePolicyServices.GetEmployeeLatePolicyAsync(customerId, employeeId);
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
