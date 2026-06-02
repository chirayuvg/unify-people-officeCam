using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using UnifyNow.Forest.Services;

namespace UnifyNow.Forest.Controllers.APIController
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly ILogger<PresetController> _logger;
        private readonly TicketServices _ticketServices;

        public TicketController(TicketServices ticketServices)
        {
            _ticketServices = ticketServices;
        }

        [EnableCors("AllowLocalhost")]
        [HttpPost("AddTicket")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(List<LogModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IResult> AddTicketAsync([FromForm]TicketModel ticketModel)
        {
            try
            {
                var lst = await _ticketServices.AddTicket(ticketModel);
                return Results.Ok(lst);

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [EnableCors("AllowLocalhost")]
        [HttpPost("AddMessage")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(List<LogModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IResult> AddMessageAsync([FromForm]WhatsAppMsgRequest messageRequest)
        {
            try
            {
                var lst = await _ticketServices.AddMessage(messageRequest);
                return Results.Ok(lst);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [EnableCors("AllowLocalhost")]
        [HttpPost("PostStatus")]
        [ProducesResponseType(typeof(List<LogModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IResult> PostStatusAsync(StatusModel statusModel)
        {
            try
            {
                var lst = await _ticketServices.PostStatus(statusModel);
                return Results.Ok("Success");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet("GetAllTickets")]
        [ProducesResponseType(typeof(List<LogModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<List<TicketModelRespose>> GetAllTicketsAsync()
        {
            try
            {
                var lst = await _ticketServices.GetAllTicketsAsync();
                return lst;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet("GetAllMessages")]
        [ProducesResponseType(typeof(List<LogModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<List<MsgResponse>> GetAllMessagesAsync([Required] string Id)
        {
            try
            {
                var lst = await _ticketServices.GetAllMessagesAsync(Id);
                return lst;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
