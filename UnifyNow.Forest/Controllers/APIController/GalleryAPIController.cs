using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using UnifyNow.Forest.CoreServices;
using UnifyNow.Forest.Services;

namespace UnifyNow.Forest.Controllers.APIController
{
    [Route("api/[controller]")]
    [ApiController]
    public class GalleryAPIController : ControllerBase
    {
        private readonly ILogger<PresetController> _logger;
        private readonly GalleryServices _galleryServices;
        private readonly LogServices _logServices;
        private readonly LocalStorageSettings _localStorageSettings;
        private readonly IStorageServices _localStorageServices;

        public GalleryAPIController(GalleryServices galleryServices,LocalStorageSettings localStorageSettings, IStorageServices localStorageServices, LogServices logServices)
        {
            _galleryServices = galleryServices;
            _localStorageSettings = localStorageSettings;
            _localStorageServices = localStorageServices;
            _logServices = logServices;
        }
        [HttpGet("Download")]
        [ProducesResponseType(typeof(ProjectImageModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Download([Required] string path)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //var image = await _localStorageServices.GetFile(path);
                    //Path.Combine(_localStorageSettings.FileRootFolder, "projects", item.ImageUrl);
                    var imgPath = Path.Combine(path);
                    var image = await _localStorageServices.GetFile(imgPath);
                    if (image != null)
                    {
                        var extension = Path.GetExtension(path);
                        var contentType = extension.Substring(1);
                        //return Results.Ok(File(image, "image/" + contentType));
                        return File(image, "image/" + contentType);
                    }
                    //return Results.Problem($"image at path={path} does not exist");
                    return StatusCode(StatusCodes.Status400BadRequest, "Image does not exist");
                }
                else
                {
                    //return Results.BadRequest(ModelState.GetValidationProblemDetails());
                    return StatusCode(StatusCodes.Status404NotFound, "Invalid image details");
                }
            }
            catch (Exception ex)
            {
               return StatusCode(StatusCodes.Status500InternalServerError, "Unable to download Image");
            }
        }

        [HttpGet("GetGalleryImages")]
        [ProducesResponseType(typeof(List<LogModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IResult> GetPresetAsync()
        {
            try
            {
                var lst = await _logServices.GetLiveGallaryAsync();
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
    public class ProjectImageModel
    {
        //public Guid? Id { get; set; }
        public string? Id { get; set; }

        [Required]
        [MaxLength(36)]
        public string ProjectId { get; set; }

        [Required]
        public List<IFormFile> Images { get; set; }


    }
}
