
using AppStudio.Framework.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UnifyNow.Forest.RulesEngine;
using UnifyNow.Forest.Services;

namespace UnifyNow.Forest.Controllers.APIController
{
    [Route("api/[controller]")]
    [ApiController]
    public class InferenceController : ControllerBase
    {
        private readonly InferenceServices _peopleService;
        public InferenceController(InferenceServices peopleServices)
        {
            _peopleService = peopleServices;
        }

        [HttpPost("InferenceData")]
        public async Task<Result> InferenceDataAsync(CameraDataModel cameraDataModel)
        {
            await _peopleService.InferenceDataAsync(cameraDataModel);

            return new Result();
        }
    }
}
