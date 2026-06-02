using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using Unify.CCC.Models;

namespace Unify.CCC.HttpClientService
{
    public class AIService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        public AIService(
            IConfiguration configuration, 
            IHttpClientFactory httpClientFactory) 
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            string AIServiceUrl = _configuration.GetValue<string>("AICamerPostBackUrl");
            AIServiceUrl = AIServiceUrl ?? string.Empty;
        }

        public async Task<bool> CameraConfiguration(List<CameraModel> cameraModels)
        {
            try
            {
                string _aiCameraCallBackUrl = _configuration.GetValue<string>("AICameraCallBackUrl");

                var reqData = JsonSerializer.Serialize(cameraModels);
                var requestContent = new StringContent(reqData, Encoding.UTF8, "application/json");
                using (var httpClient = _httpClientFactory.CreateClient())
                {
                    var response = await httpClient.PostAsync(_aiCameraCallBackUrl + Path.DirectorySeparatorChar + "cameracallback/v2", requestContent);
                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsStringAsync();
                    var respData = JsonSerializer.Deserialize<bool>(content);
                    return respData;
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
