using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using UnifyNow.Forest.Services;

namespace UnifyNow.Forest.Controllers.APIController
{
    

   
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly ILogger<LogController> _logger;
        private readonly LogServices _logServices;

        public LogController(LogServices logServices)
        {
            _logServices = logServices;
        }


        [HttpGet("GetLiveLog")]
        [ProducesResponseType(typeof(List<LogModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IResult> GetLiveDetectionAsync()
        {
            try
            {
                var lst = await _logServices.GetLiveDetectionAsync();
                return Results.Ok(lst);
                //if (lst != null && lst.Count > 0)
                //{
                //    return Results.Ok(lst);
                //}
                //return Results.NotFound($"doesn't contain any Person Data");
            }
            catch (Exception ex)
            {
                throw;
                //var ex1 = new ServiceException("Unable to retrive the Person Data", ex);
                //_logger.LogError(ex1, null);
                //return Results.Problem(ex1.GetProblemDetails());
            }
        }

        [HttpGet("GetKPILog")]
        [ProducesResponseType(typeof(List<LogModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IResult> GetKPILogAsync()
        {
            try
            {
                var lst = await _logServices.GetKPILogsAsync();
                return Results.Ok(lst);
                //if (lst != null && lst.Count > 0)
                //{
                //    return Results.Ok(lst);
                //}
                //return Results.NotFound($"doesn't contain any Person Data");
            }
            catch (Exception ex)
            {
                throw;
                //var ex1 = new ServiceException("Unable to retrive the Person Data", ex);
                //_logger.LogError(ex1, null);
                //return Results.Problem(ex1.GetProblemDetails());
            }
        }
    }
}
