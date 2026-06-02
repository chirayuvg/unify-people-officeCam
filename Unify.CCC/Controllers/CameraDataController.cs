using Microsoft.AspNetCore.Mvc;
using Unify.CCC.Services;
using Unify.CCC.Models;
using Unify.CCC.Data;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace Unify.CCC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CameraDataController : ControllerBase
    {
        private readonly ILogger<CameraDataController> _logger;
        private readonly CameraServices _cameraServices;


        public CameraDataController(CameraServices cameraServices , ILogger<CameraDataController> logger)
        {
            _cameraServices = cameraServices;
            _logger = logger;
        }

        [HttpGet("GetCameraConfigurations")]
        [ProducesResponseType(typeof(List<CameraModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IResult> GetCameraConfigurationsAsync()
        {
            try
            {
                var data = await _cameraServices.GetCameraConfigurationsAsync();
                return Results.Ok(data);
                
            }
            catch (Exception ex)
            {
                var ex1 = new ServiceException("Something went wrong", ex);
                _logger.LogError(ex1, null);
                return Results.Problem(ex1.GetProblemDetails());
            }
        }

        [HttpGet("GetCameraById")]
        [ProducesResponseType(typeof(List<Camera>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IResult> GetCameraByIdAsync(Guid id)
        {
            try
            {
                var data = await _cameraServices.GetCameraByIdAsync(id);
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
