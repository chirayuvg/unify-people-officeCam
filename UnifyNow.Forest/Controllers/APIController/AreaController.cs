using AppStudio.Framework.Identity.Models;
using AppStudio.Framework.MVC;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using UnifyNow.Forest.Data.SQL;
using UnifyNow.Forest.Services;
using static UnifyNow.Forest.Services.AreaServices;

namespace UnifyNow.Forest.Controllers.APIController
{
    [Route("api/[controller]")]
    [ApiController]
    public class AreaController : ControllerBase
    {
        private readonly ILogger<AreaController> _logger;
        private readonly AreaServices _areaServices;

        public AreaController(AreaServices areaServices)
        {
            _areaServices = areaServices;
        }


        [HttpGet("GetArea")]
        [ProducesResponseType(typeof(List<LogModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IResult> GetAOIAsync()
        {
            try
            {
                var lst = await _areaServices.GetArea();
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
        [HttpGet("GetAreaById")]
        [ProducesResponseType(typeof(List<LogModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IResult> GetAreaByIdAsync([Required] string CameraId)
        {
            try
            {
                var lst = await _areaServices.GetAreaById(CameraId);
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
        [HttpPost("PostAreas")]
        [ProducesResponseType(typeof(List<LogModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IResult> PostAOIAsync(AreaRequest areaRequest)
        {
            try
            {
                var lst = await _areaServices.AddArea(areaRequest);
                return Results.Ok();
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
        [HttpPost("UpdateAreas")]
        [ProducesResponseType(typeof(List<LogModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IResult> UpdateAreasAsync(AreaRequest areaRequest)
        {
            try
            {
                var lst = await _areaServices.UpdateArea(areaRequest);
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