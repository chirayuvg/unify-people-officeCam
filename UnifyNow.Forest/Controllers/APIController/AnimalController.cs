using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AppStudio.Framework.Identity.Models;
using AppStudio.Framework.MVC;
using AppStudio.Model;
using UnifyNow.Forest.Data.SQL;
using AppStudio.Framework.MVC.Controllers;

namespace UnifyNow.Forest.Controllers.APIController
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnimalController : ApiController<Animal, Animal, Animal>
    {

    }
}
