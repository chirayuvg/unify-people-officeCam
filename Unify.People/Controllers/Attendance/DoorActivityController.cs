using AppStudio.Framework.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Unify.Data;
using Unify.Data.SQL;

namespace Unify.People.Controllers.Attendance
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    [AllowAnonymous]
    public class DoorActivityController : ControllerBase
    {
        [HttpGet("GetDoorStatus")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetDoorStatusAsync()
        {
            try
            {
                var peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
                DoorStatus doorStatus = peopleDb.DoorStatuses.FirstOrDefault();

                var response = new { Relay = doorStatus.Status };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new { Error = ex.Message };
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpPost("PostDoorStatusClose")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PostDoorStatusCloseAsync()
        {
            try
            {
                var peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
                DoorStatus doorStatus = peopleDb.DoorStatuses.FirstOrDefault();
                doorStatus.Status = 0;
                await Entity.UpdateAsync(doorStatus.Id, doorStatus);

                return Ok();
            }
            catch (Exception ex)
            {
                var response = new { Error = ex.Message };
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
    }
}
