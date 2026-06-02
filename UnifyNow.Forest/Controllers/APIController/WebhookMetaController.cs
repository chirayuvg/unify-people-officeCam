using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace UnifyNow.Forest.Controllers.APIController
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookMetaController : ControllerBase
    {
        private readonly string accessToken = "YOUR_META_GRAPH_API_TOKEN";
        private readonly string verifyToken = "DTC";
        private readonly string _apiUrl = "https://graph.facebook.com/v22.0/587681384428298/messages";


        // ✅ 1. VERIFY WEBHOOK (For Meta)
        [HttpGet("receive-message")]
        public IActionResult VerifyWebhook([FromQuery] string hub_mode, [FromQuery] string hub_challenge, [FromQuery] string hub_verify_token)
        {
            if (hub_mode == "subscribe" && hub_verify_token == verifyToken)
            {
                return Ok(hub_challenge);
            }
            return Unauthorized("Verification failed.");
        }


        private async Task<bool> SendMessageToUser(string number, string message)
        {
            using var _httpClient = new HttpClient();

            var payload = new
            {
                messaging_product = "whatsapp",
                to = number,
                type = "text",
                text = new { body = message }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl)
            {
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessageToUser([FromBody] MessageRequest request)
        {
            if (string.IsNullOrEmpty(request.Number) || string.IsNullOrEmpty(request.Message))
            {
                return BadRequest(new { error = "Number and Message are required." });
            }

            var payload = new
            {
                messaging_product = "whatsapp",
                to = request.Number,
                type = "text",
                text = new { body = request.Message }
            };

            var response = await APIHttpPostCall(_apiUrl, payload);

            if (response.IsSuccessStatusCode)
            {
                return Ok(new { success = "Message sent successfully!" });
            }

            var errorResponse = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, new { error = "Failed to send message", details = errorResponse });
        }
        private async Task<HttpResponseMessage> APIHttpPostCall(string url, object data)
        {
            using var _httpClient = new HttpClient();
            var jsonContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync(url, jsonContent);
        }

        private async Task<HttpResponseMessage> APIHttpGetCall(string url)
        {
            using var _httpClient = new HttpClient();
            return await _httpClient.GetAsync(url);
        }


        [HttpPost("receive-message")]
        public async Task<IActionResult> ReceiveMessage([FromBody] JsonElement payload)
        {
            if (payload.ToString().Contains("messages"))
            {
                var messages = payload.GetProperty("entry")[0].GetProperty("changes")[0].GetProperty("value").GetProperty("messages");
                foreach (var message in messages.EnumerateArray())
                {
                    var sender = message.GetProperty("from").GetString();
                    var text = message.GetProperty("text").GetProperty("body").GetString();

                    Console.WriteLine($"Received from {sender}: {text}");

                    // ✅ AUTO-REPLY TO SENDER
                    await SendMessageToUser(sender, "Thanks for your message! We'll get back to you soon.");
                }
            }
            return Ok();
        }
    }

    public class MessageRequest
    {
        public string Number { get; set; }
        public string Message { get; set; }
    }

}
