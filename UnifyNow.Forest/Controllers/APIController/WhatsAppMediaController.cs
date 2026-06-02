using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace UnifyNow.Forest.Controllers.APIController
{
    [Route("api/[controller]")]
    [ApiController]
    public class WhatsAppMediaController : ControllerBase
    {
        private readonly string accessToken = "YOUR_META_GRAPH_API_TOKEN";  // Replace with your actual access token
        private readonly string phoneNumberId = "587681384428298"; // Replace with your actual phone number ID
        private readonly string _apiUrl = "https://graph.facebook.com/v22.0/587681384428298/messages";

        public string mediaId;
        /// <summary>
        /// Uploads media (image, video, audio, document, sticker) to WhatsApp API
        /// </summary>
        [HttpPost("upload-media")]
        public async Task<IActionResult> UploadMedia([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "Media file is required" });

            using var client = new HttpClient();
            using var content = new MultipartFormDataContent();

            // Add the Authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Read the file into a stream and create a StreamContent
            using var fileStream = file.OpenReadStream();
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");

            // Add the file to the form data
            content.Add(fileContent, "file", file.FileName);

            // Add additional parameters
            content.Add(new StringContent(file.ContentType ?? "application/octet-stream"), "type");
            content.Add(new StringContent("whatsapp"), "messaging_product");

            // Make the POST request
            var response = await client.PostAsync($"https://graph.facebook.com/v22.0/587681384428298/media", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<MediaUploadResponse>(responseContent);
                string mediaId = responseData?.Id ?? string.Empty;

                return Ok(new { success = "Media uploaded successfully!", mediaId });
            }


            return StatusCode(500, new { error = "Failed to upload media", details = await response.Content.ReadAsStringAsync() });
        }

        /// <summary>
        /// Sends uploaded media to a WhatsApp user
        /// </summary>
        [HttpPost("send-media")]
        public async Task<IActionResult> SendMedia([FromBody] MediaMessageRequest request)
        {
            if (string.IsNullOrEmpty(request.To) || string.IsNullOrEmpty(request.Type) || string.IsNullOrEmpty(request.MediaId))
                return BadRequest(new { error = "To, Type, and MediaId are required" });

            using var client = new HttpClient();

            // Set Authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var requestUrl = $"https://graph.facebook.com/v22.0/587681384428298/messages";

            // Create dynamic media object
            var mediaObject = new Dictionary<string, object>
    {
        { "messaging_product", "whatsapp" },
        { "recipient_type", "individual" },
        { "to", request.To },
        { "type", request.Type }, // Example: "image", "video", "audio", etc.
        { request.Type, new { id = request.MediaId } } // Dynamically adds { "image": { "id": "MEDIA_ID" } }
    };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(mediaObject), Encoding.UTF8, "application/json");

            // Send the request
            var response = await client.PostAsync(requestUrl, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                return Ok(new { success = "Media message sent successfully!" });
            }

            return StatusCode(500, new { error = "Failed to send media message", details = await response.Content.ReadAsStringAsync() });
        }

        [HttpPost("send-location")]
        public async Task<IActionResult> SendLocation([FromBody] LocationMessageRequest request)
        {
            using var _httpClient = new HttpClient();
            if (string.IsNullOrEmpty(request.To) || request.Latitude == 0 || request.Longitude == 0)
                return BadRequest(new { error = "To, Latitude, and Longitude are required." });

            var payload = new
            {
                messaging_product = "whatsapp",
                to = request.To,
                type = "location",
                location = new
                {
                    longitude = request.Longitude,
                    latitude = request.Latitude,
                    name = request.Name,
                    address = request.Address
                }
            };

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var jsonContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_apiUrl, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                return Ok(new { success = "Location message sent successfully!" });
            }

            var errorResponse = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, new { error = "Failed to send location message", details = errorResponse });
        }


        [HttpPost("send-contact")]
        public async Task<IActionResult> SendContact([FromBody] ContactMessageRequest request)
        {
            using var _httpClient = new HttpClient();

            if (string.IsNullOrEmpty(request.To) || request.Contacts == null || request.Contacts.Count == 0)
                return BadRequest(new { error = "To and at least one contact are required." });

            var payload = new
            {
                messaging_product = "whatsapp",
                to = request.To,
                type = "contacts",
                contacts = request.Contacts
            };

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var jsonContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_apiUrl, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                return Ok(new { success = "Contact message sent successfully!" });
            }

            var errorResponse = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, new { error = "Failed to send contact message", details = errorResponse });
        }


        /// <summary>
        /// Request model for media upload
        /// </summary>
        public class MediaUploadResponse
        {
            [JsonProperty("id")]
            public string Id { get; set; }
        }

        /// <summary>
        /// Request model for sending media messages
        /// </summary>
        public class MediaMessageRequest
        {
            public string To { get; set; }
            public string Type { get; set; }
            public string MediaId { get; set; }
        }

        public class LocationMessageRequest
        {
            public string To { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public string Name { get; set; }
            public string Address { get; set; }
        }

        public class ContactMessageRequest
        {
            public string To { get; set; }
            public List<Contact> Contacts { get; set; }
        }

        public class Contact
        {
            public List<Address> Addresses { get; set; }
            public string Birthday { get; set; }
            public List<Email> Emails { get; set; }
            public Name Name { get; set; }
            public Organization Org { get; set; }
            public List<Phone> Phones { get; set; }
            public List<Url> Urls { get; set; }
        }

        public class Address
        {
            public string Street { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string Zip { get; set; }
            public string Country { get; set; }
            public string CountryCode { get; set; }
            public string Type { get; set; }
        }

        public class Email
        {
            public string EmailAddress { get; set; }
            public string Type { get; set; }
        }

        public class Name
        {
            public string FormattedName { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string MiddleName { get; set; }
            public string Suffix { get; set; }
            public string Prefix { get; set; }
        }

        public class Organization
        {
            public string Company { get; set; }
            public string Department { get; set; }
            public string Title { get; set; }
        }

        public class Phone
        {
            public string PhoneNumber { get; set; }
            public string Type { get; set; }
            public string WaId { get; set; } // WhatsApp ID if applicable
        }

        public class Url
        {
            public string UrlLink { get; set; }
            public string Type { get; set; }
        }
    }
}