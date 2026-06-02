using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UnifyNow.Forest.Services;

namespace UnifyNow.Forest.Controllers.APIController
{
    [Route("api/[controller]")]
    [ApiController]
    public class SMSController : ControllerBase
    {
        private readonly SMSServices _sMSServices;
        public SMSController(SMSServices sMSServices)
        {
            _sMSServices = sMSServices;
        }

        [HttpPost("SendSMS")]
        public async Task<IActionResult> SendSMSAsync([FromBody] SmsRequestModel smsRequest)
        {
            var result = await _sMSServices.SendMessage(smsRequest);
            return Ok(result);
        }   
    }
}
