using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Unify.People.Services;

namespace Unify.People.Controllers.Attendance
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    [AllowAnonymous]
    public class AttendanceRegisterController : ControllerBase
    {
        private readonly AttendanceServices _attendanceServices;

        public AttendanceRegisterController(AttendanceServices attendanceServices)
        {
            _attendanceServices = attendanceServices;
        }

        [HttpPost("PostAttendance")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PostAttendanceAsync([FromBody] MauiAttModel request)
        {
            try
            {
                if (request.Embedding == null || request.Embedding.Length == 0)
                    return BadRequest(new { Error = "Embedding is required." });

                if (string.IsNullOrWhiteSpace(request.ImageBase64String))
                    return BadRequest(new { Error = "Base64Image is required." });

                AttModel model = new AttModel();

                model.Latitude = 0;
                model.Longitude = 0;
                model.Image = await GetImageFormFile(request.ImageBase64String);
                model.Action = request.Action;
                model.TenantId = request.TenantId;
                model.Embedding = request.Embedding;

                await _attendanceServices.LoginAsync(model);

                return Ok(new { Message = "Attendance registered successfully" });
            }
            catch (Exception ex)
            {
                var response = new { Error = ex.Message };
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        private async Task<IFormFile> GetImageFormFile(string ImageBase64String)
        {
            byte[] fileBytes = Convert.FromBase64String(ImageBase64String);
            var stream = new MemoryStream(fileBytes);

            IFormFile image = new FormFile(stream, 0, fileBytes.Length, "Image", "capture.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentDisposition = "form-data; name=\"Image\"; filename=\"capture.jpg\"",
                ContentType = "image/jpeg"
            };

            return image;
        }
    }

    public class MauiAttModel
    {
        public Guid TenantId { get; set; }

        public string Action { get; set; }

        public float[] Embedding { get; set; }

        public string ImageBase64String { get; set; }
    }
}
