using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Unify.CCC.Data;
using Unify.CCC.Services;

namespace Unify.CCC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeDataController : ControllerBase
    {
        private readonly ILogger<EmployeeDataController> _logger;
        private readonly EmployeeService _employeeServices;

        public EmployeeDataController(EmployeeService employeeService,
            ILogger<EmployeeDataController> logger)
        {
            _employeeServices = employeeService;
            _logger = logger;
        }

        [HttpGet("GetEmployeeById")]
        [ProducesResponseType(typeof(Employee), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IResult> GetEmployeeByIdAsync(Guid customerId, Guid employeeId)
        {
            try
            {
                var data = await _employeeServices.GetEmployeeByIdAsync(customerId, employeeId);
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
