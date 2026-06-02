using AppStudio.Framework.MVC.Controllers;
using Microsoft.AspNetCore.Mvc;
using UnifyNow.Forest.Data.SQL;

namespace UnifyNow.Forest.Controllers.APIController
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemebersController : ApiController<Member, Member, Member>
    {

    }
}