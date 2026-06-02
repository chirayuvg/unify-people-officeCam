using AppStudio.Framework.Core;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Elastic.Clients.Elasticsearch.Requests;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using System;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using UnifyNow.Forest.Data.SQL;
using Microsoft.AspNetCore.Http;
using DevExpress.XtraPrinting.Native;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using static MassTransit.Monitoring.Performance.BuiltInCounters;
using Newtonsoft.Json;
using System.Net.Http;
using static UnifyNow.Forest.Controllers.APIController.WhatsAppMediaController;
using System.Buffers.Text;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace UnifyNow.Forest.Controllers
{
    public class WhatsAppController : PageController
    {
        private HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string accessToken = "YOUR_META_GRAPH_API_TOKEN";
        private readonly string _apiUrl = "https://graph.facebook.com/v22.0/587681384428298/messages";

        public override void Init()
        {
            this.Page.Title = "Notifications";


            var formView = this.AddFormView<WhatsAppMessage>();

            var gridView = this.AddGridView<WhatsAppMessage>()
    .SetFormView(formView)
    .ConfigueUI(
        grid => grid
            .ColumnWidth(Mode.Auto)
            .FilterRow(filterRow => filterRow
                .ApplyFilter(GridApplyFilterMode.Auto)
                .Visible(false)
            )
            .HeaderFilter(headerFilter => headerFilter.Visible(false))
            .RowAlternationEnabled(true)
            .SearchPanel(s => s.Visible(false))
            .ShowBorders(true)
            .ShowColumnLines(false)
            .ShowRowLines(false)
            .WordWrapEnabled(true).Width("100%")
    );
            var gridView2 = this.AddGridView<MessageModel>()
                .ConfigueUI(
                    grid => grid
                        .ColumnWidth(Mode.Auto)
                        .FilterRow(filterRow => filterRow
                            .ApplyFilter(GridApplyFilterMode.Auto)
                            .Visible(false)
                        )
                        .HeaderFilter(headerFilter => headerFilter.Visible(false))
                        .RowAlternationEnabled(true)
                        .SearchPanel(s => s.Visible(false))
                        .ShowBorders(true)
                        .ShowColumnLines(false)
                        .ShowRowLines(false)
                        .WordWrapEnabled(true).Width("100%")
                );
            formView.OnAfterInsert += FormView_OnAfterInsert;
            formView.OnAfterUpdate += FormView_OnAfterUpdate;
            formView.OnBeforeDelete += FormView_OnBeforeDelete;
            gridView2.OnLoadData += GridView2_OnLoadData;
        }

        private async Task<Result> FormView_OnBeforeDelete(object id)
        {
            string connectionString = "Server=192.168.0.125,1433;Database=ForestDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;Encrypt=False;";

            // Validate ID type

            int rowsAffected = 0;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string query1 = "DELETE FROM Messages WHERE Id = @Id";
                        using (SqlCommand cmd = new SqlCommand(query1, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);
                            rowsAffected += await cmd.ExecuteNonQueryAsync();
                        }

                        string query2 = "DELETE FROM WhatsAppMessages WHERE Id = @Id";
                        using (SqlCommand cmd = new SqlCommand(query2, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);
                            rowsAffected += await cmd.ExecuteNonQueryAsync();
                        }

                        transaction.Commit();

                        // Return success or failure based on rows affected
                        return new Result
                        {
                            IsSuccess = rowsAffected > 0,
                            Action = rowsAffected > 0 ? "Records deleted successfully." : "No records found."
                        };
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return new Result
                        {
                            IsSuccess = false,
                            Action = "Error occurred during deletion",
                            Errors = new List<Error> { new Error("Exception", ex.Message) }
                        };
                    }
                }
            }
        }


        private async Task FormView_OnAfterUpdate(WhatsAppMessage entity, IFormFileCollection files, string parentId)
        {
            string connectionString = "Server=192.168.0.125,1433;Database=ForestDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;Encrypt=False;";

            string query1 = "UPDATE Messages SET Sender = @Sender, Receiver = @Receiver, MessageText = @MessageText, Timestamp = GETDATE() WHERE Id = @Id";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query1, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", entity.Id); // Use Id for update
                    cmd.Parameters.AddWithValue("@Sender", "587681384428298");
                    cmd.Parameters.AddWithValue("@Receiver", entity.RecipientNumber);
                    cmd.Parameters.AddWithValue("@MessageText", entity.MessageText);

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            string query2 = "UPDATE WhatsAppMessages SET Name = @Name, RecipientNumber = @RecipientNumber, Latitude = @Latitude, Longitude = @Longitude, [File] = @File, MessageText = @MessageText, Address = @Address WHERE Id = @Id";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query2, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", entity.Id);
                    cmd.Parameters.AddWithValue("@Name", entity.Name ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RecipientNumber", entity.RecipientNumber);
                    cmd.Parameters.AddWithValue("@Latitude", (object?)entity.Latitude ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Longitude", (object?)entity.Longitude ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@File", entity.file ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@MessageText", entity.MessageText);
                    cmd.Parameters.AddWithValue("@Address", entity.Address ?? (object)DBNull.Value);

                    await cmd.ExecuteNonQueryAsync();
                }
            }

        }

        private string GetMediaType(string contentType)
        {
            return contentType switch
            {
                "image/jpeg" or "image/png" or "image/gif" => "image",
                "video/mp4" or "video/3gpp" => "video",
                "audio/mpeg" or "audio/ogg" => "audio",
                "application/pdf" => "document",
                _ => "document" // Default to document if unknown
            };
        }


        private async Task<string> GetAddressFromCoordinatesAsync(double? latitude, double? longitude)
        {
            using var httpClient = new HttpClient();
            var url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={latitude}&lon={longitude}";

            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(responseBody);
                if (jsonDoc.RootElement.TryGetProperty("display_name", out var addressElement))
                {
                    return addressElement.GetString() ?? "Unknown Address";
                }
            }
            return "Unknown Address"; // Default fallback if API fails
        }
        private async Task FormView_OnAfterInsert(WhatsAppMessage entity, IFormFileCollection files, string parentId)
        {
            try
            {
                using var httpClient = new HttpClient();
                var httpContextAccessor = new Microsoft.AspNetCore.Http.HttpContextAccessor();
                var httpContext = httpContextAccessor.HttpContext;
                string connectionString = "Server=192.168.0.125,1433;Database=ForestDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;Encrypt=False;";

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                // ✅ Step 1: Send a text message
                if (!string.IsNullOrEmpty(entity.RecipientNumber) && !string.IsNullOrEmpty(entity.MessageText))
                {
                    var sendMessagePayload = new
                    {
                        messaging_product = "whatsapp",
                        to = entity.RecipientNumber,
                        type = "text",
                        text = new { body = entity.MessageText }
                    };

                    var sendMessageResponse = await httpClient.PostAsync(
                        "https://graph.facebook.com/v22.0/587681384428298/messages",
                        new StringContent(JsonConvert.SerializeObject(sendMessagePayload), Encoding.UTF8, "application/json")
                    );

                    string query1 = "INSERT INTO Messages (Sender, Receiver, MessageText) VALUES (@Sender, @Receiver, @MessageText)";

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        await conn.OpenAsync();
                        using (SqlCommand cmd = new SqlCommand(query1, conn))
                        {
                            cmd.Parameters.AddWithValue("@Sender", string.IsNullOrEmpty("919958048761") ? "" : "587681384428298");
                            cmd.Parameters.AddWithValue("@Receiver", string.IsNullOrEmpty(entity.RecipientNumber) ? "" : entity.RecipientNumber);
                            cmd.Parameters.AddWithValue("@MessageText", string.IsNullOrEmpty(entity.MessageText) ? "" : entity.MessageText);

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }

                    string query2 = "INSERT INTO WhatsAppMessages (Id, Name, RecipientNumber, Latitude, Longitude, [File], MessageText, Address) " +
                   "VALUES (@Id, @Name, @RecipientNumber, @Latitude, @Longitude, @File, @MessageText, @Address)";

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        await conn.OpenAsync();
                        using (SqlCommand cmd = new SqlCommand(query2, conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", Guid.NewGuid());
                            cmd.Parameters.AddWithValue("@Name", string.IsNullOrEmpty(entity.Name) ? "" : entity.Name);
                            cmd.Parameters.AddWithValue("@RecipientNumber", string.IsNullOrEmpty(entity.RecipientNumber) ? "" : entity.RecipientNumber);
                            cmd.Parameters.AddWithValue("@Latitude", entity.Latitude ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Longitude", entity.Longitude ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@File", string.IsNullOrEmpty(entity.file) ? "" : entity.file);
                            cmd.Parameters.AddWithValue("@MessageText", string.IsNullOrEmpty(entity.MessageText) ? "" : entity.MessageText);
                            cmd.Parameters.AddWithValue("@Address", string.IsNullOrEmpty(entity.Address) ? "" : entity.Address);

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }

                    if (!sendMessageResponse.IsSuccessStatusCode)
                    {
                        var errorMessage = await sendMessageResponse.Content.ReadAsStringAsync();
                        Console.WriteLine($"Error sending message: {errorMessage}");
                    }
                }

                // ✅ Step 2: Upload media (if files are attached)
                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        using var content = new MultipartFormDataContent();
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                        using var fileStream = file.OpenReadStream();
                        var fileContent = new StreamContent(fileStream);
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");

                        content.Add(fileContent, "file", file.FileName);
                        content.Add(new StringContent(file.ContentType ?? "application/octet-stream"), "type");
                        content.Add(new StringContent("whatsapp"), "messaging_product");

                        var uploadResponse = await httpClient.PostAsync(
                            "https://graph.facebook.com/v22.0/587681384428298/media",
                            content
                        );

                        if (uploadResponse.IsSuccessStatusCode)
                        {
                            var responseContent = await uploadResponse.Content.ReadAsStringAsync();
                            var mediaResponse = JsonConvert.DeserializeObject<MediaUploadResponse>(responseContent);
                            string mediaId = mediaResponse?.Id ?? string.Empty;
                            string mediaType = GetMediaType(file.ContentType); // Get dynamic media type

                            var mediaKey = mediaType.ToLower(); // Ensure it matches API's expected format

                            var mediaPayload = new Dictionary<string, object>
{
    { "id", mediaId }
};
                            string jsonPayload;
                            var sendMediaPayload = new Dictionary<string, object>();
                            // ✅ Step 3: Send media message if upload was successful
                            if (!string.IsNullOrEmpty(mediaId))
                            {
                                sendMediaPayload = new Dictionary<string, object>
    {
        { "messaging_product", "whatsapp" },
        { "recipient_type", "individual" },
        { "to", entity.RecipientNumber },
        { "type", mediaKey }, // Dynamic media type
        { mediaKey, new Dictionary<string, object> { { "id", mediaId } } } // Dynamically add media key
    };
                                string query1 = "INSERT INTO Messages (Sender, Receiver, MessageText) VALUES (@Sender, @Receiver, @MessageText)";

                                using (SqlConnection conn = new SqlConnection(connectionString))
                                {
                                    await conn.OpenAsync();
                                    using (SqlCommand cmd = new SqlCommand(query1, conn))
                                    {
                                        cmd.Parameters.AddWithValue("@Sender", string.IsNullOrEmpty("919958048761") ? "" : "587681384428298");
                                        cmd.Parameters.AddWithValue("@Receiver", string.IsNullOrEmpty(entity.RecipientNumber) ? "" : entity.RecipientNumber);
                                        cmd.Parameters.AddWithValue("@MessageText", string.IsNullOrEmpty(entity.MessageText) ? "" : entity.MessageText);

                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                }

                                string query2 = "INSERT INTO WhatsAppMessages (Id, Name, RecipientNumber, Latitude, Longitude, [File], MessageText, Address) " +
                               "VALUES (@Id, @Name, @RecipientNumber, @Latitude, @Longitude, @File, @MessageText, @Address)";

                                using (SqlConnection conn = new SqlConnection(connectionString))
                                {
                                    await conn.OpenAsync();
                                    using (SqlCommand cmd = new SqlCommand(query2, conn))
                                    {
                                        cmd.Parameters.AddWithValue("@Id", Guid.NewGuid());
                                        cmd.Parameters.AddWithValue("@Name", string.IsNullOrEmpty(entity.Name) ? "" : entity.Name);
                                        cmd.Parameters.AddWithValue("@RecipientNumber", string.IsNullOrEmpty(entity.RecipientNumber) ? "" : entity.RecipientNumber);
                                        cmd.Parameters.AddWithValue("@Latitude", entity.Latitude ?? (object)DBNull.Value);
                                        cmd.Parameters.AddWithValue("@Longitude", entity.Longitude ?? (object)DBNull.Value);
                                        cmd.Parameters.AddWithValue("@File", string.IsNullOrEmpty(entity.file) ? "" : entity.file);
                                        cmd.Parameters.AddWithValue("@MessageText", string.IsNullOrEmpty(entity.MessageText) ? "" : entity.MessageText);
                                        cmd.Parameters.AddWithValue("@Address", string.IsNullOrEmpty(entity.Address) ? "" : entity.Address);

                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                }
                                jsonPayload = JsonConvert.SerializeObject(sendMediaPayload);


                            }
                            ;

                                var sendMediaResponse = await httpClient.PostAsync(
                                    "https://graph.facebook.com/v22.0/587681384428298/messages",
                                    new StringContent(JsonConvert.SerializeObject(sendMediaPayload), Encoding.UTF8, "application/json")
                                );

                                if (!sendMediaResponse.IsSuccessStatusCode)
                                {
                                    var mediaErrorMessage = await sendMediaResponse.Content.ReadAsStringAsync();
                                    Console.WriteLine($"Error sending media message: {mediaErrorMessage}");
                                }
                            }
                        
                        else
                        {
                            var uploadErrorMessage = await uploadResponse.Content.ReadAsStringAsync();
                            Console.WriteLine($"Error uploading media: {uploadErrorMessage}");
                        }
                    }
                }

                HttpContent jsonContent = null;

                if (entity.Latitude != null && entity.Longitude != null && entity.Name != "")
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    string googleMapsUrl = $"https://www.google.com/maps?q={entity.Latitude},{entity.Longitude}";

                    string Address = await GetAddressFromCoordinatesAsync(entity.Latitude, entity.Longitude);

                    var payload = new
                    {
                        messaging_product = "whatsapp",
                        to = entity.RecipientNumber,
                        type = "location",
                        location = new
                        {
                            longitude = entity.Longitude,
                            latitude = entity.Latitude,
                            name = entity.Name,
                            address = $"{entity.Latitude},{entity.Longitude}"
                        }
                    };

                    jsonContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync(_apiUrl, jsonContent);

                    string query1 = "INSERT INTO Messages (Sender, Receiver, MessageText) VALUES (@Sender, @Receiver, @MessageText)";

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        await conn.OpenAsync();
                        using (SqlCommand cmd = new SqlCommand(query1, conn))
                        {
                            cmd.Parameters.AddWithValue("@Sender", string.IsNullOrEmpty("919958048761") ? "" : "587681384428298");
                            cmd.Parameters.AddWithValue("@Receiver", string.IsNullOrEmpty(entity.RecipientNumber) ? "" : entity.RecipientNumber);
                            cmd.Parameters.AddWithValue("@MessageText", string.IsNullOrEmpty(entity.MessageText) ? "" : entity.MessageText);

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }

                    string query2 = "INSERT INTO WhatsAppMessages (Id, Name, RecipientNumber, Latitude, Longitude, [File], MessageText, Address) " +
                   "VALUES (@Id, @Name, @RecipientNumber, @Latitude, @Longitude, @File, @MessageText, @Address)";

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        await conn.OpenAsync();
                        using (SqlCommand cmd = new SqlCommand(query2, conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", Guid.NewGuid());
                            cmd.Parameters.AddWithValue("@Name", string.IsNullOrEmpty(entity.Name) ? "" : entity.Name);
                            cmd.Parameters.AddWithValue("@RecipientNumber", string.IsNullOrEmpty(entity.RecipientNumber) ? "" : entity.RecipientNumber);
                            cmd.Parameters.AddWithValue("@Latitude", entity.Latitude ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Longitude", entity.Longitude ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@File", string.IsNullOrEmpty(entity.file) ? "" : entity.file);
                            cmd.Parameters.AddWithValue("@MessageText", string.IsNullOrEmpty(entity.MessageText) ? "" : entity.MessageText);
                            cmd.Parameters.AddWithValue("@Address", string.IsNullOrEmpty(entity.Address) ? "" : entity.Address);

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        throw new Exception($"Failed to send WhatsApp message. Details: {errorResponse}");
                    }

                }


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in FormView_OnAfterInsert: {ex.Message}");
            }
        }


        //private async Task FormView_OnAfterInsert(WhatsAppMessage entity, IFormFileCollection files, string parentId)
        //{

        //    try
        //    {
        //        using var httpClient = new HttpClient();
        //        var httpContextAccessor = new Microsoft.AspNetCore.Http.HttpContextAccessor();
        //        var httpContext = httpContextAccessor.HttpContext;

        //        if (httpContext != null)
        //        {
        //            var forwardedScheme = httpContext.Request.Headers["X-Forwarded-Proto"].FirstOrDefault();
        //            string scheme = !string.IsNullOrEmpty(forwardedScheme) ? forwardedScheme : httpContext.Request.Scheme;

        //            string baseUrl = $"{scheme}://{httpContext.Request.Host}{httpContext.Request.PathBase}";

        //            //string baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.PathBase}";

                    //            string apiUrl = $"{baseUrl}/api/node";//"http://localhost:3000/api/node"; // Update with your actual API URL

                    //            // Send a text message if the message is present

                    //            if (entity.Latitude >= -90 && entity.Latitude <= 90 && entity.Longitude >= -180 && entity.Longitude <= 180)
                    //            {
                    //                // Create request body
                    //                var locationData = new
                    //                {
                    //                    number = entity.RecipientNumber,
                    //                    latitude = entity.Latitude,
                    //                    longitude = entity.Longitude
                    //                };

                    //                //var jsonContent = new StringContent(JsonConvert.SerializeObject(locationData), Encoding.UTF8, "application/json");

                    //                // Send request to /send-current-location
                    //                var response = await httpClient.PostAsJsonAsync($"{apiUrl}/send-current-location", locationData);

                    //                if (!response.IsSuccessStatusCode)
                    //                {
                    //                    Console.WriteLine(" Failed to send location.");
                    //                }
                    //                else
                    //                {
                    //                    Console.WriteLine(" Location sent successfully.");
                    //                }
                    //            }
                    //            else
                    //            {
                    //                Console.WriteLine(" Invalid latitude or longitude values.");
                    //            }

                    //            // Send media if files are attached
                    //            if (files != null && files.Count > 0)
                    //            {
                    //                using var content = new MultipartFormDataContent();
                    //                content.Add(new StringContent(entity.RecipientNumber), "number");
                    //                content.Add(new StringContent(entity.MessageText), "caption");

                    //                foreach (var file in files)
                    //                {
                    //                    var fileStream = file.OpenReadStream(); // Do not use 'using' here
                    //                    var streamContent = new StreamContent(fileStream);
                    //                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                    //                    content.Add(streamContent, "media", file.FileName);
                    //                }

                    //                var mediaResponse = await httpClient.PostAsync($"{apiUrl}/send-media", content);

                    //                if (!mediaResponse.IsSuccessStatusCode)
                    //                {
                    //                    throw new Exception($"Failed to send media: {await mediaResponse.Content.ReadAsStringAsync()}");
                    //                }
                    //            }
                    //            else
                    //            {
                    //                var textPayload = new
                    //                {
                    //                    number = entity.RecipientNumber,
                    //                    message = entity.MessageText
                    //                };

                    //                var textResponse = await httpClient.PostAsJsonAsync($"{apiUrl}/send-text", textPayload);
                    //                if (!textResponse.IsSuccessStatusCode)
                    //                {
                    //                    throw new Exception($"Failed to send text: {await textResponse.Content.ReadAsStringAsync()}");
                    //                }
                    //            }


                    //            Console.WriteLine("Message and Media sent successfully!");
                    //        }
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        Console.WriteLine($"Error: {ex.Message}");
                    //    }
                    //}

        private async Task<LoadResult> GridView2_OnLoadData(DataSourceLoadOptions loadOptions, string? parentId)
        {
            string connectionString = "Server=192.168.0.125,1433;Database=ForestDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;Encrypt=False;";
            //results = results.OrderByDescending(m => m.CountLateLogin).ToList();
            List<MessageModel> results = new();

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
                            results.Add(new MessageModel
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

            return DevExtreme.AspNet.Data.DataSourceLoader.Load(results, loadOptions);
        }
    }

    public class MessageModel
    {
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string MessageText { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

    //public class WhatsAppPage : PageModel
    //{
    //    public GridView<MessageModel> GridView2 { get; set; } = new();

    //    public GridView<WhatsAppMessage> GridView { get; set; } = new();

    //    public FormView<WhatsAppMessage> FormView { get; set; } = new();

    //    private HttpClient _httpClient;
    //    private readonly IHttpContextAccessor _httpContextAccessor;

    //    private string _apiUrl = "https://graph.facebook.com/v22.0/587681384428298/messages"; // Replace with your Phone Number ID
    //    private string _authToken = "YOUR_META_GRAPH_API_TOKEN"; // Replace with your Meta access token
    //    private string _whatsappBusinessId = "546729227820326"; // Replace with your business ID

    //    public WhatsAppPage()
    //    {

    //        this.FormView.OnAfterInsert += FormView_OnAfterInsert;

    //        this.GridView2.OnLoadData += GridView_OnLoadData;
    //    }

    //    private async Task<DevExtreme.AspNet.Data.ResponseModel.LoadResult> GridView_OnLoadData(DevExtreme.AspNet.Mvc.DataSourceLoadOptions loadOptions, string? parentId)
    //    {
    //        string connectionString = "Server=192.168.0.125,1433;Database=ForestDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;Encrypt=False;";
    //        //results = results.OrderByDescending(m => m.CountLateLogin).ToList();
    //        List<MessageModel> results = new();

    //        using (SqlConnection conn = new SqlConnection(connectionString))
    //        {
    //            await conn.OpenAsync();

    //            string query = "SELECT Sender, Receiver, MessageText, Timestamp FROM Messages ORDER BY Timestamp DESC";

    //            using (SqlCommand cmd = new SqlCommand(query, conn))
    //            {
    //                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
    //                {
    //                    while (await reader.ReadAsync())
    //                    {
    //                        results.Add(new MessageModel
    //                        {
    //                            Sender = reader["Sender"].ToString(),
    //                            Receiver = reader["Receiver"].ToString(),
    //                            MessageText = reader["MessageText"].ToString(),
    //                            Timestamp = Convert.ToDateTime(reader["Timestamp"])
    //                        });
    //                    }
    //                }
    //            }
    //        }

    //        return DevExtreme.AspNet.Data.DataSourceLoader.Load(results, loadOptions);

    //    }

    //    private async Task FormView_OnAfterInsert(WhatsAppMessage entity, IFormFileCollection files, string parentId)
    //    {
    //        try
    //        {
    //            using var httpClient = new HttpClient();
    //            var httpContextAccessor = new Microsoft.AspNetCore.Http.HttpContextAccessor();
    //            var httpContext = httpContextAccessor.HttpContext;

    //            if (httpContext != null)
    //            {
    //                var forwardedScheme = httpContext.Request.Headers["X-Forwarded-Proto"].FirstOrDefault();
    //                string scheme = !string.IsNullOrEmpty(forwardedScheme) ? forwardedScheme : httpContext.Request.Scheme;

    //                string baseUrl = $"{scheme}://{httpContext.Request.Host}{httpContext.Request.PathBase}";

    //                //string baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.PathBase}";

    //                string apiUrl = $"{baseUrl}/api/node";//"http://localhost:3000/api/node"; // Update with your actual API URL

    //                // Send a text message if the message is present

    //                if (entity.Latitude >= -90 && entity.Latitude <= 90 && entity.Longitude >= -180 && entity.Longitude <= 180)
    //                {
    //                    // Create request body
    //                    var locationData = new
    //                    {
    //                        number = entity.RecipientNumber,
    //                        latitude = entity.Latitude,
    //                        longitude = entity.Longitude
    //                    };

    //                    //var jsonContent = new StringContent(JsonConvert.SerializeObject(locationData), Encoding.UTF8, "application/json");

    //                    // Send request to /send-current-location
    //                    var response = await httpClient.PostAsJsonAsync($"{apiUrl}/send-current-location", locationData);

    //                    if (!response.IsSuccessStatusCode)
    //                    {
    //                        Console.WriteLine(" Failed to send location.");
    //                    }
    //                    else
    //                    {
    //                        Console.WriteLine(" Location sent successfully.");
    //                    }
    //                }
    //                else
    //                {
    //                    Console.WriteLine(" Invalid latitude or longitude values.");
    //                }

    //                // Send media if files are attached
    //                if (files != null && files.Count > 0)
    //                {
    //                    using var content = new MultipartFormDataContent();
    //                    content.Add(new StringContent(entity.RecipientNumber), "number");
    //                    content.Add(new StringContent(entity.MessageText), "caption");

    //                    foreach (var file in files)
    //                    {
    //                        var fileStream = file.OpenReadStream(); // Do not use 'using' here
    //                        var streamContent = new StreamContent(fileStream);
    //                        streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
    //                        content.Add(streamContent, "media", file.FileName);
    //                    }

    //                    var mediaResponse = await httpClient.PostAsync($"{apiUrl}/send-media", content);

    //                    if (!mediaResponse.IsSuccessStatusCode)
    //                    {
    //                        throw new Exception($"Failed to send media: {await mediaResponse.Content.ReadAsStringAsync()}");
    //                    }
    //                }
    //                else
    //                {
    //                    var textPayload = new
    //                    {
    //                        number = entity.RecipientNumber,
    //                        message = entity.MessageText
    //                    };

    //                    var textResponse = await httpClient.PostAsJsonAsync($"{apiUrl}/send-text", textPayload);
    //                    if (!textResponse.IsSuccessStatusCode)
    //                    {
    //                        throw new Exception($"Failed to send text: {await textResponse.Content.ReadAsStringAsync()}");
    //                    }
    //                }


    //                Console.WriteLine("Message and Media sent successfully!");
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine($"Error: {ex.Message}");
    //        }

    //    }

    //    public class MessageModel
    //    {
    //        public string Sender { get; set; }
    //        public string Receiver { get; set; }
    //        public string MessageText { get; set; }
    //        public DateTime Timestamp { get; set; }
    //    }

    //    private async Task FormView_OnAfterInsert2(WhatsAppMessage entity, IFormFileCollection files, string parentId)
    //    {
    //        try
    //        {



    //            using (HttpClient client = new HttpClient())
    //            {
    //                // Prepare the request payload

    //                var requestBody = new
    //                {
    //                    messaging_product = "whatsapp",
    //                    to = entity.RecipientNumber,  // WhatsApp recipient number
    //                    type = "text",
    //                    text = new
    //                    {
    //                        body = "Hello, world!"
    //                    }
    //                };

    //                string jsonPayload = System.Text.Json.JsonSerializer.Serialize(requestBody);

    //                // Configure HttpRequest
    //                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _apiUrl)
    //                {
    //                    Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
    //                };

    //                // Add Authorization and Content-Type headers
    //                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);

    //                // Send the request
    //                HttpResponseMessage response = await client.SendAsync(request);

    //                //var requestBody = new
    //                //{
    //                //    messaging_product = "whatsapp",
    //                //    to = entity.RecipientNumber,  // WhatsApp recipient number
    //                //    type = "template",
    //                //    template = new
    //                //    {
    //                //        name = "hello_world", // Template name (must be pre-approved in WhatsApp API)
    //                //        language = new
    //                //        {
    //                //            code = "en_US" // Language code
    //                //        }
    //                //    }
    //                //};

    //                //string jsonPayload = JsonSerializer.Serialize(requestBody);

    //                //// Configure HttpRequest
    //                //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _apiUrl)
    //                //{
    //                //    Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
    //                //};

    //                //// Add Authorization and Content-Type headers
    //                //request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);

    //                //// Send the request
    //                //HttpResponseMessage response = await client.SendAsync(request);

    //                // Send the request
    //                //HttpResponseMessage response = await client.SendAsync(request);

    //            }
    //        }

    //        catch (Exception ex)
    //        {
    //            var error = ex.Message; ;
    //        }
            
    //    }


    //    }
   // }


