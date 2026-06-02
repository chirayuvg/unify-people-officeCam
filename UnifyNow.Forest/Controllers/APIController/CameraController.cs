using AppStudio.Framework.Identity.Models;
using AppStudio.Framework.MVC;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UnifyNow.Forest.Data.SQL;

namespace UnifyNow.Forest.Controllers.APIController
{
    [Route("api/[controller]")]
    [ApiController]
    public class CameraController : ApiController<Camera, Camera, Camera>
    {
    }
}
