using System.Text;

namespace UnifyNow.Forest.HttpClientService
{
    public class AIForestService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string PresetURL;
        private readonly HttpClient _httpClient;
        public AIForestService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _httpClient = _httpClientFactory.CreateClient();
            PresetURL = _configuration.GetValue<string>("PresetURL") ?? string.Empty;
        }
        public virtual async Task<PresetResposeModel> AddPresetAsync(PresetAIModel presetModel)//List<CameraModel> cameraModelList
        {
            try
            {
                var reqData = System.Text.Json.JsonSerializer.Serialize(presetModel);//cameraModelList
                var requestContent = new StringContent(reqData, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("http://192.168.0.125:8015/AddPreset", requestContent);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                // pending
                var respData = System.Text.Json.JsonSerializer.Deserialize<PresetResposeModel>(content);
                return respData;
            }
            catch
            {

                throw;
            }
        }
    }

    public class PresetAIModel
    {
        public string presetName { get; set; }
        public string presetId { get; set; }
        public string cameraName { get; set; }
        public string cameraId { get; set; }
        public string areaName { get; set; }
        public string pan { get; set; } = string.Empty;
        public string tilt { get; set; } = string.Empty;
        public string zoom { get; set; } = string.Empty;
        public int duration { get; set; }
    }

    //public class PresetResposeModel
    //{
    //    public string presetId { get; set; }
    //    public float pan { get; set; }
    //    public float tilt { get; set; }
    //    public float zoom { get; set; }
    //    public int speed { get; set; }
    //}

    public class PresetResposeModel
    {
        public string status { get; set; }
        public string message { get; set; }
        public Data data { get; set; }
        public DateTime timestamp { get; set; }
    }

    public class Data
    {
        public string presetId { get; set; }
        public float pan { get; set; }
        public float tilt { get; set; }
        public float zoom { get; set; }
        public float speed { get; set; }
    }







}
