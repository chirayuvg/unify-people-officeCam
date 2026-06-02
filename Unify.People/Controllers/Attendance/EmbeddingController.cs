using DTC.Unify.HttpClientService;
using Microsoft.AspNetCore.Mvc;

namespace Unify.People.Controllers.Attendance
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmbeddingController : ControllerBase
    {
        private readonly FaceService _faceService;
        public EmbeddingController(FaceService faceService)
        {
            _faceService = faceService;
        }
        //[EnableCors("AllowLocalhost")]
        [HttpGet("GetEmbedding")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IResult> GetEmbeddingAsync([FromForm]IFormFile formFile)
        {
            try
            {
                var lst = await _faceService.GetEmbeddings(formFile);
                return Results.Ok(lst);

            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
