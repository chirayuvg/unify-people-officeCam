using AppStudio.Framework.Core;
using Microsoft.AspNetCore.Mvc;
using Unify.People.Services;

namespace Unify.People.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InferenceController : ControllerBase
    {
        private readonly InferenceServices _inferenceServices;
        public InferenceController(InferenceServices inferenceServices)
        {
            _inferenceServices = inferenceServices;    
        }

        [HttpPost("InferenceData")]
        public async Task<Result> InferenceDataAsync(CameraDataModel cameraDataModel)
        {
            await _inferenceServices.InferenceDataAsync(cameraDataModel);

            return new Result();
        }
    }
}
