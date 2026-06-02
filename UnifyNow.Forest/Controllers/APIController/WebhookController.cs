using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace UnifyNow.Forest.Controllers.APIController
{
    [ApiController]
    [Route("webhook")]
    public class WebhookController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(IConfiguration configuration, ILogger<WebhookController> logger)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            _logger = logger;
        }

        /// <summary>
        /// Webhook verification (GET request).
        /// </summary>
        [HttpGet]
        public IActionResult VerifyWebhook([FromQuery] string hub_mode, [FromQuery] string hub_verify_token, [FromQuery] string hub_challenge)
        {
            string verifyToken = _configuration["WebhookVerifyToken"];

            if (hub_mode == "subscribe" && hub_verify_token == verifyToken)
            {
                _logger.LogInformation("Webhook verified successfully!");
                return Ok(hub_challenge); // Return challenge token for verification
            }

            _logger.LogWarning("Webhook verification failed.");
            return Forbid();
        }

        /// <summary>
        /// Handle incoming webhook messages (POST request).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ReceiveWebhook([FromBody] JsonElement requestBody)
        {
            _logger.LogInformation("Received webhook: {0}", requestBody.ToString());

            try
            {
                var entry = requestBody.GetProperty("entry")[0];
                var changes = entry.GetProperty("changes")[0];
                var value = changes.GetProperty("value");
                var messages = value.GetProperty("messages")[0];

                string sender = messages.GetProperty("from").GetString();
                string messageBody = messages.GetProperty("text").GetProperty("body").GetString();
                string messageId = messages.GetProperty("id").GetString();

                _logger.LogInformation($"Received message from {sender}: {messageBody}");

                // Get Business Phone Number ID from config or value
                string businessPhoneNumberId = _configuration["BusinessPhoneNumberId"];

                // Send reply message
                await SendWhatsAppMessage(sender, messageBody, messageId, businessPhoneNumberId);

                // Mark message as read
                await MarkMessageAsRead(messageId, businessPhoneNumberId);
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error processing webhook: {ex.Message}");
                return BadRequest();
            }

            return Ok();
        }

        private async Task SendWhatsAppMessage(string recipient, string messageText, string messageId, string businessPhoneNumberId)
        {
            string accessToken = _configuration["GraphApiToken"];

            var messageData = new
            {
                messaging_product = "whatsapp",
                to = recipient,
                text = new { body = "Echo: " + messageText },
                context = new { message_id = messageId }
            };

            string jsonPayload = JsonSerializer.Serialize(messageData);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.PostAsync($"https://graph.facebook.com/v22.0/{businessPhoneNumberId}/messages", content);
            response.EnsureSuccessStatusCode();
        }

        private async Task MarkMessageAsRead(string messageId, string businessPhoneNumberId)
        {
            string accessToken = _configuration["GraphApiToken"];

            var markReadData = new
            {
                messaging_product = "whatsapp",
                status = "read",
                message_id = messageId
            };

            string jsonPayload = JsonSerializer.Serialize(markReadData);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.PostAsync($"https://graph.facebook.com/v22.0/{businessPhoneNumberId}/messages", content);
            response.EnsureSuccessStatusCode();
        }
    }
}
