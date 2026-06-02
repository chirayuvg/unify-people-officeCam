using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace UnifyNow.Forest.Services
{
    public class SMSServices
    {

        private readonly HttpClient _httpClient;
        private const string AuthId = "PLIVO_AUTH_ID_PLACEHOLDER";
        private const string AuthToken = "PLIVO_AUTH_TOKEN_PLACEHOLDER";
        private readonly string apiUrl = $"https://api.plivo.com/v1/Account/{AuthId}/Message/";

        public SMSServices(HttpClient httpClient)
        {
            _httpClient = httpClient;
            var byteArray = Encoding.ASCII.GetBytes($"{AuthId}:{AuthToken}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        }

        public async Task<string> SendTextMessage(string src, string dst, string message)
        {
            try
            {
                var payload = new
                {
                    src = src,
                    dst = dst,
                    text = message
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(apiUrl, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    return "Message sent successfully via Plivo";
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed response: {error}");
                    return "Failed to send message via Plivo";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return "Error occurred while sending message";
            }
        }
        public async Task<string> SendMessage(SmsRequestModel sms)
        {
            try
            {
                SmsRequest smsRequest = new();
                smsRequest.Phone = sms.Phone;
                // Convert SmsRequest to a collection of key-value pairs
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("user", smsRequest.User),
                    new KeyValuePair<string, string>("pass", smsRequest.Pass),
                    new KeyValuePair<string, string>("sender", smsRequest.Sender),
                    new KeyValuePair<string, string>("phone", smsRequest.Phone),
                    new KeyValuePair<string, string>("text", smsRequest.Text),
                    new KeyValuePair<string, string>("priority", smsRequest.Priority),
                    new KeyValuePair<string, string>("stype", smsRequest.Stype)
                });

                var response = await _httpClient.PostAsync("http://bhashsms.com/api/sendmsg.php", content);

                if (response.IsSuccessStatusCode)
                {
                    return "Message sent successfully via BhashSMS";
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed response: {error}");
                    return "Failed to send message via BhashSMS";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return "Error occurred while sending message";
            }
        }
    }

    public class SmsRequest
    {
        public string User { get; set; } = "UnifySMS";        // e.g., "UnifySMS"
        public string Pass { get; set; } = "123456";        // e.g., "123456"
        public string Sender { get; set; } = "UNOWAI";      // e.g., "UNOWAI"
        public string Phone { get; set; }           // e.g., "9535877810"
        public string Text { get; set; } = "हिंदी தமிழ் ಕನ್ನಡ తెలుగు - UNIFYNOW AI LABS";        // e.g., "हिंदी தமிழ் ಕನ್ನಡ తెలుగు - UNIFYNOW AI LABS"
        public string Priority { get; set; } = "ndnd";     // e.g., "ndnd"
        public string Stype { get; set; } = "normal";         // e.g., "normal"
    }
    public class SmsRequestModel
    {
     
        public string Phone { get; set; }           // e.g., "9535877810"

    }
}
