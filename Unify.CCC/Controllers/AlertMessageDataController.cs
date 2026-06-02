using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Unify.CCC.Data;
using Unify.CCC.Services;

namespace Unify.CCC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlertMessageDataController : ControllerBase
    {
        private readonly ILogger<AlertMessageDataController> _logger;
        private readonly AlertMessageService _alertMessageService;

        public AlertMessageDataController(AlertMessageService alertMessageService, ILogger<AlertMessageDataController> logger)
        {
            _logger = logger;
            _alertMessageService = alertMessageService;
        }

        [HttpPost("AddAlertMessage")]
        [ProducesResponseType(typeof(List<AlertMessage>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IResult> AddAlertMessageAsync(AlertMessage alertMessage)
        {
            try
            {
                var data = await _alertMessageService.AddAlertMessageAsync(alertMessage);
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
