using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System.Text;
using Unify.People.Services;

namespace DTC.Unify.HttpClientService
{
    public class AIPersonService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string AIPersonAddFace;
        private readonly string AIPersonDeleteFace;
        private readonly string AIStartAddFace;
        private readonly string AIMonitorUrl;
        private readonly HttpClient _httpClient;
        public AIPersonService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _httpClient = _httpClientFactory.CreateClient();
            AIPersonAddFace = _configuration.GetValue<string>("AIPersonAddFace") ?? string.Empty;
            AIPersonDeleteFace = _configuration.GetValue<string>("AIPersonDeleteFace") ?? string.Empty;
            AIStartAddFace = _configuration.GetValue<string>("AIStartAddFace") ?? string.Empty;
            AIMonitorUrl = _configuration.GetValue<string>("AIMonitorUrl") ?? string.Empty;
        }
        public virtual async Task<List<LiveMonitorStatus>> StartMonitorAsync(List<LiveMonitorStatus> jobs)//List<CameraModel> cameraModelList
        {
            try
            {
                List<LiveMonitorStatus> LiveMonitorStatusList = new List<LiveMonitorStatus>();
                var reqData = System.Text.Json.JsonSerializer.Serialize(jobs);//cameraModelList
                var requestContent = new StringContent(reqData, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(AIMonitorUrl + "Workplace/StartMonitor", requestContent);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                // pending
                var respData = System.Text.Json.JsonSerializer.Deserialize<List<LiveMonitorStatus>>(content);
                return respData;
            }
            catch
            {

                throw;
            }
        }
        public async Task<LiveCameraUrl> GetLiveVideoURLAsync(Guid cameraId)
        {
            try
            {
                using (var httpClient = _httpClientFactory.CreateClient())
                {
                    var response = await httpClient.GetAsync(AIMonitorUrl + "Workplace/GetLiveVideoURL?CameraId=" + cameraId.ToString());
                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsStringAsync();
                    var respData = System.Text.Json.JsonSerializer.Deserialize<LiveCameraUrl>(content);
                    return respData;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unable to connect live server");
                throw;
            }
        }
        public async Task<DeleteFaceResponse> DeleteAIImage(DeleteAIImageModel deleteAIImage)
        {
            try
            {
                using (var httpClient = _httpClientFactory.CreateClient())
                {
                    var addFace = await httpClient.PostAsync(AIStartAddFace, null);
                    addFace.EnsureSuccessStatusCode();
                    Thread.Sleep(10000);
                }
                //string AIServiceUrl = _configuration.GetValue<string>("AIServiceEmbeddingUrl");
                //AIServiceUrl = AIServiceUrl ?? string.Empty;
                var multipartFormDataContent = new MultipartFormDataContent();
                ////Add Text data
                multipartFormDataContent.Add(new StringContent(deleteAIImage.ImageId), "imageId");
                multipartFormDataContent.Add(new StringContent(deleteAIImage.PersonId), "personId");
                multipartFormDataContent.Add(new StringContent(deleteAIImage.CollectionName), "collectionName");

                // Send the request to the external API
                using (var httpClient = _httpClientFactory.CreateClient())
                {
                    var response = await httpClient.PostAsync(AIPersonDeleteFace, multipartFormDataContent);
                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsStringAsync();
                    var embedding = Newtonsoft.Json.JsonConvert.DeserializeObject<DeleteFaceResponse>(content);
                    return embedding;
                }

            }
            catch
            {
                throw;
            }
        }
        public class DeleteAIImageModel
        {
            public string PersonId { get; set; }
            public string ImageId { get; set; }
            public string CollectionName { get; set; }
        }
        public class EmbeddingModel
        {
            public string Message { get; set; }
            public string Status { get; set; }
            //public List<double> Embedding { get; set; }
            public Data Data { get; set; }
            public DateTime Timestamp { get; set; }
        }
        public class DeleteFaceResponse
        {
            public string Message { get; set; }
            public string Status { get; set; }
            //public List<double> Embedding { get; set; }
            public string Data { get; set; }
            public DateTime Timestamp { get; set; }
        }
        public class Data
        {
            public float[] Embedding { get; set; }
            //public float[] Embedding { get; set; }
        }

        public class LiveCameraUrl
        {
            public string CameraId { get; set; }

            public string LiveURLPred { get; set; }


            public string LiveURLRaw { get; set; }
            public string Message { get; set; }
        }

        public class LiveMonitorStatus
        {
            public string ContainerId { get; set; }
            public string ContainerName { get; set; }
            public string ContainerStatus { get; set; }
            public string LiveCameraURL { get; set; }
            public int Port { get; set; }
            public string[] ProcessIds { get; set; }
            public string Status { get; set; }

        }

        public class FaceEmbedding
        {
            public string Id { get; set; } = string.Empty;
            public string CollectionName { get; set; } = string.Empty;
            public string TenantId { get; set; } = string.Empty;
            public IFormFile ImageData { get; set; }

            public string Metadata { get; set; } = string.Empty;
        }
    }
}


