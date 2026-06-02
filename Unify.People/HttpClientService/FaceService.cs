using AppStudio.Framework.Core;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System.Text;
using Unify.People.Services;
using static DTC.Unify.HttpClientService.AIPersonService;

namespace DTC.Unify.HttpClientService
{
    public class FaceService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public FaceService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _httpClient = _httpClientFactory.CreateClient();
            var baseUrl = _configuration.GetValue<string>("FaceServiceUrl") ?? string.Empty;
            _httpClient.BaseAddress = new Uri(baseUrl);
        }
        public async Task<float[]> GetEmbeddings(IFormFile file)
        {
            try
            {
                Result result = new();
                var multipartFormDataContent = new MultipartFormDataContent();
                ////Add Text data

                // Add file data
                if (file != null)
                {
                    var fileContent = new StreamContent(file.OpenReadStream());
                    fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                    {
                        Name = "image",
                        FileName = file.FileName
                    };
                    multipartFormDataContent.Add(fileContent);
                }

                using (var httpClient = _httpClientFactory.CreateClient())
                {
                    var baseUrl = _configuration.GetValue<string>("FaceServiceUrl") ?? string.Empty;
                    httpClient.BaseAddress = new Uri(baseUrl);
                    var response = await httpClient.PostAsync("GenerateEmbedding", multipartFormDataContent);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var embedding = Newtonsoft.Json.JsonConvert.DeserializeObject<EmbeddingModel>(content);
                        return embedding.Data.Embedding;
                    }
                    else
                    {
                        Data data = new();
                        return data.Embedding;
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Error(ex, "error while extracting the embeddings");
            }
            return null;
        }
        public async Task<Result> InsertAsync(FaceEmbedding entity)
        {
            Result result = new();

            var multipartFormDataContent = new MultipartFormDataContent();
            ////Add Text data
            multipartFormDataContent.Add(new StringContent(entity.Id), "id");
            multipartFormDataContent.Add(new StringContent(entity.TenantId), "tenantId");
            multipartFormDataContent.Add(new StringContent(entity.Metadata), "metaData");
            multipartFormDataContent.Add(new StringContent(entity.CollectionName), "collectionName");

            // Add file data
            if (entity.ImageData != null && entity.ImageData.Length > 0)
            {
                var fileContent = new StreamContent(entity.ImageData.OpenReadStream());
                fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                {
                    Name = "image",
                    FileName = entity.ImageData.FileName
                };
                multipartFormDataContent.Add(fileContent);
            }

            using (var httpClient = _httpClientFactory.CreateClient())
            {
                var response = await httpClient.PostAsync("Insert", multipartFormDataContent);
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var embedding = Newtonsoft.Json.JsonConvert.DeserializeObject<EmbeddingModel>(content);
                }
                else
                {
                    result.IsSuccess = false;
                    result.AddError(response.ReasonPhrase);
                }
            }
            return result;
        }

        public async Task<Result> UpdateAsync(FaceEmbedding entity)
        {
            Result result = new();

            var multipartFormDataContent = new MultipartFormDataContent();
            ////Add Text data
            multipartFormDataContent.Add(new StringContent(entity.Id), "id");
            multipartFormDataContent.Add(new StringContent(entity.TenantId), "tenantId");
            multipartFormDataContent.Add(new StringContent(entity.Metadata), "metaData");
            multipartFormDataContent.Add(new StringContent(entity.CollectionName), "collectionName");

            // Add file data
            if (entity.ImageData != null && entity.ImageData.Length > 0)
            {
                var fileContent = new StreamContent(entity.ImageData.OpenReadStream());
                fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                {
                    Name = "image",
                    FileName = entity.ImageData.FileName
                };
                multipartFormDataContent.Add(fileContent);
            }

            using (var httpClient = _httpClientFactory.CreateClient())
            {
                var response = await httpClient.PostAsync("Update", multipartFormDataContent);
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var embedding = Newtonsoft.Json.JsonConvert.DeserializeObject<EmbeddingModel>(content);
                    result.IsSuccess = true;
                }
                else
                {
                    result.IsSuccess = false;
                    result.AddError(response.ReasonPhrase);
                }
            }
            return result;
        }
        public async Task<string> FindAsync(FindRequest entity)
        {
            Result result = new();

            var multipartFormDataContent = new MultipartFormDataContent();
            ////Add Text data
            multipartFormDataContent.Add(new StringContent(entity.TenantId), "tenantId");
            multipartFormDataContent.Add(new StringContent(entity.CollectionName), "collectionName");

            // Add file data
            if (entity.ImageData != null && entity.ImageData.Length > 0)
            {
                var fileContent = new StreamContent(entity.ImageData.OpenReadStream());
                fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                {
                    Name = "image",
                    FileName = entity.ImageData.FileName
                };
                multipartFormDataContent.Add(fileContent);
            }

            using (var httpClient = _httpClientFactory.CreateClient())
            {
                var response = await httpClient.PostAsync("Find", multipartFormDataContent);
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var respose = Newtonsoft.Json.JsonConvert.DeserializeObject<FindRespose>(content);
                    return response.ToString();
                }
                else
                {
                    result.IsSuccess = false;
                    result.AddError(response.ReasonPhrase);
                    return string.Empty;
                }
            }
        }


        public async Task<Result> DeleteAsync(DeleteEmbedding deleteEmbedding)
        {
            try
            {

                var multipartFormDataContent = new MultipartFormDataContent();
                ////Add Text data


                // Send the request to the external API
                using (var httpClient = _httpClientFactory.CreateClient())
                {
                    var response = await httpClient.PostAsJsonAsync("Delete", deleteEmbedding);
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        return new Result();
                    }

                }
            }
            catch
            {

            }
            return new Result() { IsSuccess = false };
        }



        public class FaceEmbedding
        {
            public string Id { get; set; } = string.Empty;
            public string CollectionName { get; set; } = string.Empty;
            public string TenantId { get; set; } = string.Empty;
            public IFormFile ImageData { get; set; }

            public string Metadata { get; set; } = string.Empty;
        }

        public class DeleteEmbedding
        {
            public string Id { get; set; }
            public string CollectionName { get; set; }
            public string TenantId { get; set; }
        }

        public class FindRequest
        {
            public string CollectionName { get; set; }
            public string TenantId { get; set; }
            public IFormFile ImageData { get; set; }
        }

        public class FindRespose
        {
            public string Id { get; set; }
        }
        public class EmbeddingRequest
        {
            public IFormFile File { get; set; }
        }

        //public class EmbeddingRespose
        //{
        //    public byte[] Embedding { get; set; }
        //}

        public class EmbeddingModel
        {
            public string Message { get; set; }
            public string Status { get; set; }
            //public List<double> Embedding { get; set; }
            public Data Data { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public class Data
        {
            public float[] Embedding { get; set; }
            //public float[] Embedding { get; set; }
        }

    }
}


