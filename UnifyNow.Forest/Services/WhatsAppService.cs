using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Net;

namespace UnifyNow.Forest.Services
{
    public class WhatsappService
    {
        private readonly HttpClient _httpClient;
        private string apiUrl = "http://192.168.3.100:4000";

        public WhatsappService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private static string GetLocalIPAddress()
        {
            string ipAddress = "192.168.0.56"; // Default to localhost in case no valid IP is found

            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                ipAddress = host.AddressList
                    .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?
                    .ToString() ?? "192.168.0.56";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting IP Address: " + ex.Message);
            }

            return ipAddress;
        }


        public async Task<string> SendTextMessage(string number, string message,string ticketId)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{apiUrl}/send-message", new
                {
                    number = number,
                    message = message,
                    ticketId = ticketId
                });

                return response.IsSuccessStatusCode ? "Message sent successfully" : "Failed to send message";
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error" + ex);
                return "Failed to send media";
            }
        }


        // ✅ Send Media File
        public async Task<string> SendMedia(string number, string caption, IFormFile mediaFile, string ticketId)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(number), "number");
                content.Add(new StringContent(caption ?? ""), "caption");
                content.Add(new StringContent(ticketId), "ticketId");

                using var fileStream = mediaFile.OpenReadStream();
                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mediaFile.ContentType);
                content.Add(streamContent, "media", mediaFile.FileName);

                var response = await _httpClient.PostAsync($"{apiUrl}/send-media", content);
                return response.IsSuccessStatusCode ? "Media sent successfully" : "Failed to send media";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error" + ex);
                return "Failed to send media";
            }
        }

        // ✅ Fetch Stored Messages from messages.json
        //public async Task<string> GetMessages()
        //{
        //    var response = await _httpClient.GetAsync($"{apiUrl}/messages");

        //    if (response.IsSuccessStatusCode)
        //    {
        //        return await response.Content.ReadAsStringAsync();
        //    }
        //    else
        //    {
        //        return "Failed to fetch messages";
        //    }
        //}


        [Obsolete]
        public async Task<string> GetMessages()
        {
            var messages = new List<MessageModel>();

            // Define the connection string
            string connectionString = "Server=157.90.14.249,1433;Database=ForestDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;Encrypt=False;";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    string query = "SELECT Sender, Receiver, MessageText, Timestamp FROM Messages ORDER BY Timestamp DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                messages.Add(new MessageModel
                                {
                                    Sender = reader["Sender"].ToString(),
                                    Receiver = reader["Receiver"].ToString(),
                                    MessageText = reader["MessageText"].ToString(),
                                    Timestamp = Convert.ToDateTime(reader["Timestamp"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching messages: {ex.Message}");
            }

            return JsonConvert.SerializeObject(messages, Newtonsoft.Json.Formatting.Indented); // Convert list to JSON string
        }
        //private readonly HttpClient _httpClient;
        //private const string apiUrl = "http://localhost:3000";

        //public WhatsappService(HttpClient httpClient)
        //{
        //    _httpClient = httpClient;
        //}

        //public async Task<string> SendTextMessage(string number, string message)
        //{
        //    var response = await _httpClient.PostAsJsonAsync($"{apiUrl}/send-message", new
        //    {
        //        number = number,
        //        message = message
        //    });

        //    return response.IsSuccessStatusCode ? "Message sent successfully" : "Failed to send message";
        //}


        //// ✅ Send Media File
        //public async Task<string> SendMedia(string number, string caption, IFormFile mediaFile)
        //{
        //    using var content = new MultipartFormDataContent();
        //    content.Add(new StringContent(number), "number");
        //    content.Add(new StringContent(caption ?? ""), "caption");

        //    using var fileStream = mediaFile.OpenReadStream();
        //    var streamContent = new StreamContent(fileStream);
        //    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mediaFile.ContentType);
        //    content.Add(streamContent, "media", mediaFile.FileName);

        //    var response = await _httpClient.PostAsync($"{apiUrl}/send-media", content);
        //    return response.IsSuccessStatusCode ? "Media sent successfully" : "Failed to send media";
        //}

        //// ✅ Fetch Stored Messages from messages.json
        //public async Task<string> GetMessages()
        //{
        //    var response = await _httpClient.GetAsync($"{apiUrl}/messages");

        //    if (response.IsSuccessStatusCode)
        //    {
        //        return await response.Content.ReadAsStringAsync();
        //    }
        //    else
        //    {
        //        return "Failed to fetch messages";
        //    }
        //}

    }
    public class MessageModel
    {
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string MessageText { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
