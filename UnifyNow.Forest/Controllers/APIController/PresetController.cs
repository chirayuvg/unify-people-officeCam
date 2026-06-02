using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using UnifyNow.Forest.Services;
using static UnifyNow.Forest.Services.AreaServices;

namespace UnifyNow.Forest.Controllers.APIController
{
    [Route("api/[controller]")]
    [ApiController]
    public class PresetController : ControllerBase
    {
        private readonly ILogger<PresetController> _logger;
        private readonly PresetServices _presetServices;

        public PresetController(PresetServices presetServices)
        {
            _presetServices = presetServices;
        }
        [HttpGet("GetPreset")]
        [ProducesResponseType(typeof(List<LogModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IResult> GetPresetAsync()
        {
            try
            {
                var lst = await _presetServices.GetPreset();
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

        [HttpGet("GetPresetById")]
        [ProducesResponseType(typeof(List<LogModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IResult> GetPresetByAsync([Required] string AreaId)
        {
            try
            {
                var lst = await _presetServices.GetPresetById(AreaId);
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
        [HttpGet("GetPresetByCameraId")]
        [ProducesResponseType(typeof(List<LogModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IResult> GetPresetByCameraIdAsync([Required] string CameraId)
        {
            try
            {
                var lst = await _presetServices.GetPresetByCameraId(CameraId);
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

        [EnableCors("AllowLocalhost")]
        [HttpPost("PostPreset")]
        [ProducesResponseType(typeof(List<LogModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IResult> PostPresetAsync(PresetCreate presetCreate)
        {
            try
            {
                var lst = await _presetServices.AddPreset(presetCreate);
                return Results.Ok();
                
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}