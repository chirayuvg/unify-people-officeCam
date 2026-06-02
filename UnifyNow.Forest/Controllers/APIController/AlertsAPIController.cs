using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UnifyNow.Forest.Data.SQL;
using UnifyNow.Forest.Services;

namespace UnifyNow.Forest.Controllers.APIController
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlertsAPIController : ControllerBase
    {
        private readonly AlertServices _alertService;

        public AlertsAPIController(AlertServices alertService)
        {
            _alertService = alertService;
        }

        [HttpGet("GetAlerts")]
        [ProducesResponseType(typeof(List<AlertRule>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IResult> GetAlertRulesAsync()
        {
            try
            {
                var rules = await _alertService.GetDummyAlertsAsync();
                return Results.Ok(rules);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving alert rules: {ex.Message}");
            }
        }
    }
}
