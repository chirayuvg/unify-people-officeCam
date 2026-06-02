using AppStudio.Framework.Core;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UnifyNow.Forest.Data.Elasticsearch;
using UnifyNow.Forest.Data.SQL;
using UnifyNow.Forest.Services;

namespace UnifyNow.Forest.RulesEngine
{
    public class ForestRulesEngine
    {
        private readonly ElasticDb _elasticDb;

        private readonly ClaimsPrincipal _user;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ForestDb _forestDb;


        //private readonly EmployeeService _employeeService;
        //private readonly ShiftService _shiftService;
        //private readonly LatePolicyService _latePolicyService;


        private ElasticsearchClient _elasticsearchClient;
        private IRepository<ForestIndex> _ForestIndex { get; set; }
        private readonly string _cccApiURL;
        private readonly string _peopleApiURL;
        private readonly IServiceProvider _serviceProvider;

        public ForestRulesEngine(IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor contextAccessor,
            ElasticDb elasticDb, IDbContextFactory<ForestDb> dbContextFactory,
            IServiceProvider serviceProvider

            //EmployeeService employeeService,
            //ShiftService shiftService,
            //LatePolicyService latePolicyService,
            )
        {
            //_user = contextAccessor.HttpContext.User;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _cccApiURL = _configuration.GetValue<string>("CCCApiUrl");
            _peopleApiURL = _configuration.GetValue<string>("PeopleApiUrl");

            _elasticDb = elasticDb;
            _forestDb = dbContextFactory.CreateDbContext();
            _serviceProvider = serviceProvider;

            //_employeeService = employeeService;
            //_shiftService = shiftService;
            //_latePolicyService = latePolicyService;
        }
        public async Task ProcessAsync(CameraDataModel cameraData)
        {
            var forestData = await ConvertToForestIndex(cameraData);
            if (forestData.Count > 0)
            {
                try
                {
                    //await _elasticDb.BulkInsertAsync(peopleData);
                    SaveElasticAttendanceIndex(forestData);
                }
                catch (Exception ex)
                {
                    var error = ex.Message;
                }
            }
        }

        private async Task SaveElasticAttendanceIndex(List<ForestIndex> processData)
        {
            ElasticSearchConnectionSettings();
            var res = _elasticsearchClient.IndexManyAsync(processData, "forestindex").Result;
            if (!res.IsValidResponse)
            {
                var debugInfo = res.DebugInformation;
                var error = res.ElasticsearchServerError.Error;
            }
            if (res.IsValidResponse)
            {

            }
        }
        //private async Task<List<AttendanceIndex>> ConvertToAttendanceIndex(CameraDataModel cameraDataModel)
        //{

        //    return null;
        //}
        public async Task ExecuteRulesAsync(string phone)
        {
            if (phone != null)
            {
                SmsRequestModel smsRequest = new();
                smsRequest.Phone = phone;
                using var scope = _serviceProvider.CreateScope();
                var smsService = scope.ServiceProvider.GetRequiredService<SMSServices>();
               await smsService.SendMessage(smsRequest);
            }
        }
        //public async Task CreateTicket(TicketModel ticketModel)
        //{
        //        using var scope = _serviceProvider.CreateScope();
        //        var smsService = scope.ServiceProvider.GetRequiredService<TicketServices>();
        //        await smsService.AddTicket(ticketModel);
            
        //}

        private async Task<List<ForestIndex>> ConvertToForestIndex(CameraDataModel cameraDataModel)
        {

            //var customerId = cameraDataModel.CustomerId;
            //var camera = await GetCameraByIdAsync(cameraDataModel.CameraId);
            //var employee = 
            List<ForestIndex> ForestIndexList = [];
            string cameraName = cameraDataModel.CameraName;
            // pending camera from cccdb
            Guid sessionId = Guid.NewGuid();
            string? prvClassName = null;
            foreach (var cls in cameraDataModel.Classes.OrderBy(m => m.Name))
            {
                var members = await _forestDb.Members.OrderBy(r => Guid.NewGuid()).FirstOrDefaultAsync();
                ForestIndex forestIndex = new();


                    forestIndex.Id = Guid.NewGuid().ToString();
                    forestIndex.CameraId = cameraDataModel.CameraId;
                    forestIndex.CameraName = cameraDataModel.CameraName;
                    forestIndex.IntelligenceId = cameraDataModel.IntelligenceId;
                    forestIndex.IntelligenceName = cameraDataModel.IntelligenceName;
                    forestIndex.DateTime = DateTime.UtcNow.AddMinutes(330);
                    forestIndex.TimeStamp = cameraDataModel.TimeStamp;
                    forestIndex.FileURL = "/documents/inference_images/" + cameraDataModel.FileURL;
                    forestIndex.CustomerId = cameraDataModel.CustomerId;
                    forestIndex.AnimalName = cls.Name;
                if (cameraDataModel.PresetId != null)
                {
                    var area = _forestDb.Presets.Include(x => x.Area).Where(x => x.Id == Guid.Parse(cameraDataModel.PresetId)).FirstOrDefault();
                    forestIndex.AreaId = area.AreaId.ToString();
                    forestIndex.AreaName = area.Area.Name;
                    forestIndex.PresetId = area.Id.ToString();
                    forestIndex.PresetName = area.Name;
                    //TicketModel ticketModel = new();
                    //ticketModel.Title = "Checkpost Crossing";
                    //ticketModel.CameraId = cameraDataModel.CameraId.ToString();
                    //ticketModel.AnimalId = await _forestDb.Animals.Where(m => m.Name == cls.Name).Select(m => m.Id.ToString()).FirstOrDefaultAsync();
                    //ticketModel.AreaId = forestIndex.AreaId;
                    //ticketModel.MemberId = members.Id.ToString();
                    //ticketModel.Type = "Alert";
                    //ticketModel.Remark = "Checkpost Crossing Detected for " + cls.Name + " at " + cameraName + " on " + forestIndex.DateTime.ToString("dd/MM/yyyy HH:mm:ss");
                    //var stream = new FileStream(forestIndex.FileURL, FileMode.Open, FileAccess.Read);
                    //var fileName = Path.GetFileName(forestIndex.FileURL);

                    //var formFile = new FormFile(stream, 0, stream.Length, "image", fileName)
                    //{
                    //    Headers = new HeaderDictionary(),
                    //    ContentType = "image/jpeg"
                    //};
                    //ticketModel.File = formFile;
                    //await CreateTicket(ticketModel);

                }
                    forestIndex.Alert = cls.Name;
                forestIndex.TrackingId = cls.TrackingId;
                forestIndex.BoundingBox = cls.BoundingBox;
               

                if (cls.Name == "Elephant")
                {
                    forestIndex.Alert = "Elephant Detection";
                    await ExecuteRulesAsync(members.PhoneNumber);
                }
                else if (cls.Name == "Deer" || cls.Name == "Gaur" || cls.Name == "Sambar" || cls.Name == "Other")
                {
                    forestIndex.Alert = "Village Spotting";
                    //await ExecuteRulesAsync();
                }   
                else
                {
                    forestIndex.Alert = "Village Spotting";
                    //await ExecuteRulesAsync();
                }

                    //foreach(var prop in cls.Properties)
                    //{
                    //    if(prop.PropertyName == "No Helmet")
                    //    {
                    //       forestIndex.PersonName = prop.PropertyValue;
                    //        if(prop.PropertyValue == "Ankit Arunkumar Tiwari" || prop.PropertyValue == "Prabhu TRS")
                    //        {
                    //            forestIndex.Alert = "Checkpost Crossing";
                    //        }
                    //        else
                    //        {
                    //            forestIndex.Alert = "Animal Spotted";
                    //        }
                    //    }
                    //}

                    ForestIndexList.Add(forestIndex);
            }
                return ForestIndexList;
        }
        private void ElasticSearchConnectionSettings()
        {
            var clientSettings = new ElasticsearchClientSettings(new Uri("https://192.168.0.125:9200"));
            clientSettings.DisableDirectStreaming(true);
            clientSettings.Authentication(new BasicAuthentication("YOUR_ELASTICSEARCH_USER", "YOUR_ELASTICSEARCH_PASSWORD"))
                  .CertificateFingerprint("YOUR_ELASTICSEARCH_FINGERPRINT");
            _elasticsearchClient = new ElasticsearchClient(clientSettings);
        }
    }
}
