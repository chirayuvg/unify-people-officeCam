using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Unify.CCC.Data;
using Unify.CCC.Services;

namespace Unify.CCC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShiftDataController : ControllerBase
    {
        private readonly ILogger<ShiftDataController> _logger;
        private readonly ShiftService _shiftServices;

        public ShiftDataController(ShiftService shiftService,
            ILogger<ShiftDataController> logger)
        {
            _logger = logger;
            _shiftServices = shiftService;
        }

        [HttpGet("GetShiftByTime")]
        [ProducesResponseType(typeof(Employee), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IResult> GeShiftByTimeAsync(Guid customerId, TimeOnly time)
        {
            try
            {
                var data = await _shiftServices.GeShiftByTimeAsync(customerId, time);
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
