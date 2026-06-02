using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Sql;
using Elastic.Transport;
using Microsoft.Extensions.Diagnostics.Latency;
using UnifyNow.Forest.Data.Elasticsearch;
using UnifyNow.Forest.Data.SQL;
using static UnifyNow.Forest.Services.GalleryServices;

namespace UnifyNow.Forest.Services
{
    public class LogServices
    {
        private ElasticsearchClient _elasticsearchClient;
        private readonly ElasticDb _elasticDb;
        private Guid _cutomerId = Guid.Parse("25a7ced4-c803-4d99-958a-5e2826882dcc");

        public LogServices(ElasticDb elasticDb)
        {
            _elasticDb = elasticDb;
        }

        public async Task<KPIModel> GetKPIAsync()
        {
            DateTime dt = DateTime.UtcNow.AddMinutes(330);
            string query = string.Empty;

            query = "Select id,fileURL,animalName,cameraName,dateTime,alert,areaName " +
                " from forestindex " +
                " where customerId = '" + _cutomerId + "'" +
                $"AND (YEAR(dateTime) = {dt.Year} AND MONTH(dateTime) = {dt.Month} AND DAY(dateTime) = {dt.Day})" +
                "LIMIT 50000";

            /*
            query = "Select Top 100 fileURL,gender, '' poc ,faceCover " +
                " from personindex " +
                " where timeStamp >= '" + historyRequestModel.FromDate + "' and timeStamp <= '" + historyRequestModel.Todate + "' and cameraId = '" + historyRequestModel.CameraId + "' " +
                " and personType = " + historyRequestModel.Type + "  order By timeStamp desc";
            */
            List<LogModel> objectResponses = new();
            //ElasticSearchProduction();
            ElasticSearchLocal();
            //ElasticSearchStaging();
            QueryResponse response = await _elasticsearchClient.Sql.QueryAsync(m => m.Query(query));
            if (!response.IsValidResponse)
            {
                var debugInfo = response.DebugInformation;
                var error = response.ElasticsearchServerError.Error;
            }
            if (response.IsValidResponse)
            {
                if (response.Rows != null)
                {
                    foreach (var row in response.Rows)
                    {
                        if (row != null)
                        {
                            LogModel objectResponse = new LogModel();
                            objectResponse.id = row.FirstOrDefault().As<string>();
                            objectResponse.folder = row.Skip(1).Take(1).FirstOrDefault().As<string>();
                            objectResponse.animal = row.Skip(2).Take(1).FirstOrDefault().As<string>();
                            objectResponse.camera = row.Skip(3).Take(1).FirstOrDefault().As<string>();
                            objectResponse.date = row.Skip(4).Take(1).FirstOrDefault().As<DateTime>();
                            objectResponse.alert = row.Skip(5).Take(1).FirstOrDefault().As<string>();
                            objectResponse.area = row.Skip(6).Take(1).FirstOrDefault().As<string>();

                            objectResponses.Add(objectResponse);
                        }
                    }

                }

                KPIModel kPIModel = new();
                kPIModel.Total = objectResponses.Count.ToString();
                kPIModel.Detection = objectResponses.Where(m => m.animal == "No Helmet").ToList().Count.ToString();
                kPIModel.Crossing = objectResponses.Where(m => m.animal == "Helmet").ToList().Count.ToString();

                return kPIModel;
            }
            return null;
            }

        public async Task<KPIModel> GetKPILogsAsync()
        {
            KPIModel kPIModel = new();
            int det = 0;
            int cross = 0;
            DateTime dt = DateTime.UtcNow.AddMinutes(330);
            string query = string.Empty;
            string query1 = string.Empty;

            query = "Select Count(*) as Count " +
                " from forestindex " +
                " where customerId = '" + _cutomerId + "' AND alert = 'Elephant Detection' " +
                $"AND (YEAR(dateTime) = {dt.Year} AND MONTH(dateTime) = {dt.Month} AND DAY(dateTime) = {dt.Day})";

            /*
            query = "Select Top 100 fileURL,gender, '' poc ,faceCover " +
                " from personindex " +
                " where timeStamp >= '" + historyRequestModel.FromDate + "' and timeStamp <= '" + historyRequestModel.Todate + "' and cameraId = '" + historyRequestModel.CameraId + "' " +
                " and personType = " + historyRequestModel.Type + "  order By timeStamp desc";
            */
            List<LogModel> objectResponses = new();
            //ElasticSearchProduction();
            ElasticSearchLocal();
            //ElasticSearchStaging();
            QueryResponse response = await _elasticsearchClient.Sql.QueryAsync(m => m.Query(query));
            if (!response.IsValidResponse)
            {
                var debugInfo = response.DebugInformation;
                var error = response.ElasticsearchServerError.Error;
            }
            if (response.IsValidResponse)
            {
                if (response.Rows != null)
                {
                    foreach (var row in response.Rows)
                    {
                        if (row != null)
                        {
                            cross = row.FirstOrDefault().As<int>();
                        }
                    }

                }
            }
            query1 = "Select Count(*) as Count " +
               " from forestindex " +
               " where customerId = '" + _cutomerId + "' AND alert = 'Village Spotting' " +
               $"AND (YEAR(dateTime) = {dt.Year} AND MONTH(dateTime) = {dt.Month} AND DAY(dateTime) = {dt.Day})";
               
            QueryResponse response1 = await _elasticsearchClient.Sql.QueryAsync(m => m.Query(query1));
            if (!response1.IsValidResponse)
            {
                var debugInfo = response.DebugInformation;
                var error = response.ElasticsearchServerError.Error;
            }
            if (response1.IsValidResponse)
            {
                if (response1.Rows != null)
                {
                    foreach (var row in response1.Rows)
                    {
                        if (row != null)
                        {
                            det = row.FirstOrDefault().As<int>();
                        }
                    }

                }
            }
            int total = cross + det;
            kPIModel.Total = total.ToString();
            kPIModel.Detection = cross.ToString();
            kPIModel.Crossing = det.ToString();

            return kPIModel;
        }



        public async Task<List<LogModel>> GetLiveDetectionAsync()
        {
            DateTime dt = DateTime.UtcNow.AddMinutes(330);
            string query = string.Empty;

            query = "Select Top 100 id,fileURL, animalName, cameraId, cameraName, timeStamp,alert " +
                " from forestindex " +
                " where customerId = '" + _cutomerId + "' order By timeStamp desc";

            /*
            query = "Select Top 100 fileURL,gender, '' poc ,faceCover " +
                " from personindex " +
                " where timeStamp >= '" + historyRequestModel.FromDate + "' and timeStamp <= '" + historyRequestModel.Todate + "' and cameraId = '" + historyRequestModel.CameraId + "' " +
                " and personType = " + historyRequestModel.Type + "  order By timeStamp desc";
            */
            List<LogModel> objectResponses = new();
            //ElasticSearchProduction();
            ElasticSearchLocal();
            //ElasticSearchStaging();
            QueryResponse response = await _elasticsearchClient.Sql.QueryAsync(m => m.Query(query));
            if (!response.IsValidResponse)
            {
                var debugInfo = response.DebugInformation;
                var error = response.ElasticsearchServerError.Error;
            }
            if (response.IsValidResponse)
            {
                if (response.Rows != null)
                {
                    foreach (var row in response.Rows)
                    {
                        if (row != null)
                        {
                            LogModel objectResponse = new LogModel();
                            var forest = AppStudio.Framework.Core.CoreServices.GetService<ForestDb>();
                            objectResponse.id = row.FirstOrDefault().As<string>();
                            objectResponse.folder = row.Skip(1).Take(1).FirstOrDefault().As<string>();
                            objectResponse.animal = row.Skip(2).Take(1).FirstOrDefault().As<string>();
                            objectResponse.animalId = forest.Animals.Select(m => m.Id).FirstOrDefault();
                            objectResponse.cameraId = row.Skip(3).Take(1).FirstOrDefault().As<string>();
                            objectResponse.camera = row.Skip(4).Take(1).FirstOrDefault().As<string>();
                            objectResponse.area = forest.Areas.Where(m => m.CameraId == Guid.Parse(objectResponse.cameraId)).Select(m => m.Name).FirstOrDefault();//"Area1";
                            objectResponse.areaId = forest.Areas.Where(m => m.CameraId == Guid.Parse(objectResponse.cameraId)).Select(m => m.Id).FirstOrDefault();
                            objectResponse.date = row.Skip(5).Take(1).FirstOrDefault().As<DateTime>();
                            objectResponse.alert = row.Skip(6).Take(1).FirstOrDefault().As<string>();

                            objectResponses.Add(objectResponse);
                        }
                    }

                }

            }
            return objectResponses;

        }

        public async Task<List<AnimalLogModel>> GetLiveGallaryAsync()
        {
            string baseImagePath = "/GalleryAPI/Download?path=/documents/inference_images";
            DateTime dt = DateTime.UtcNow.AddMinutes(330);
            string query = string.Empty;

            query = "Select Top 100 id,fileURL,alert,cameraName,animalName,dateTime " +
                " from forestindex " +
                " where customerId = '" + _cutomerId + "' order By timeStamp desc";

            /*
            query = "Select Top 100 fileURL,gender, '' poc ,faceCover " +
                " from personindex " +
                " where timeStamp >= '" + historyRequestModel.FromDate + "' and timeStamp <= '" + historyRequestModel.Todate + "' and cameraId = '" + historyRequestModel.CameraId + "' " +
                " and personType = " + historyRequestModel.Type + "  order By timeStamp desc";
            */
            List<AnimalLogModel> objectResponses = new();
            //ElasticSearchProduction();
            ElasticSearchLocal();
            //ElasticSearchStaging();
            QueryResponse response = await _elasticsearchClient.Sql.QueryAsync(m => m.Query(query));
            if (!response.IsValidResponse)
            {
                var debugInfo = response.DebugInformation;
                var error = response.ElasticsearchServerError.Error;
            }
            if (response.IsValidResponse)
            {
                if (response.Rows != null)
                {
                    foreach (var row in response.Rows)
                    {
                        if (row != null)
                        {
                            AnimalLogModel objectResponse = new();
                            objectResponse.Id = row.FirstOrDefault().As<Guid>();
                            objectResponse.Src = baseImagePath + "/" + row.Skip(1).Take(1).FirstOrDefault().As<string>();
                            objectResponse.Title = objectResponse.Id.ToString();
                            objectResponse.Category = row.Skip(2).Take(1).FirstOrDefault().As<string>();
                            objectResponse.Camera = row.Skip(3).Take(1).FirstOrDefault().As<string>();
                            objectResponse.Location = "Area1";
                            objectResponse.DateTime = row.Skip(5).Take(1).FirstOrDefault().As<DateTime>();
                            objectResponses.Add(objectResponse);
                        }
                    }

                }

            }
            return objectResponses;

        }
        private void ElasticSearchLocal()
        {

            var clientSettings = new ElasticsearchClientSettings(new Uri("https://192.168.0.125:9200"));
            clientSettings.DisableDirectStreaming(true);
            clientSettings.Authentication(new BasicAuthentication("YOUR_ELASTICSEARCH_USER", "YOUR_ELASTICSEARCH_PASSWORD"))
                  .CertificateFingerprint("YOUR_ELASTICSEARCH_FINGERPRINT");
            _elasticsearchClient = new ElasticsearchClient(clientSettings);

        }
    }
    public class KPIModel
    {
        public string Total { get; set; }
       public string Detection { get; set; }
       public string Crossing { get; set; }
    }


    public class LogModel
    {
        public string id { get; set; }
        public string folder { get; set; }

        public string animal { get; set; }

        public Guid animalId { get; set; }

        public string cameraId { get; set; }

        public Guid areaId { get; set; }

        public string area { get; set; }
        public string camera { get; set; }
        public DateTime date { get; set; }

        public string alert { get; set; }


    }


    public class Rootobject
    {
        public string cameraId { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string cameraProtocol { get; set; }
        public string connectionString { get; set; }
        public string aoi { get; set; }
        public Monitormodel[] monitorModels { get; set; }
        public string customerId { get; set; }
    }

    public class Monitormodel
    {
        public Intelligencemodel intelligenceModel { get; set; }
        public int monitorFrequency { get; set; }
    }

    public class Intelligencemodel
    {
        public string intelligenceId { get; set; }
        public string name { get; set; }
        public string monitorType { get; set; }
    }

}
