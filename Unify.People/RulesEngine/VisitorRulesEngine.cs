using AppStudio.Framework.Core;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Sql;
using Elastic.Transport;
using Newtonsoft.Json;
using System.Text;
using System.Security.Claims;
using Unify.People.Services;
using Unify.Data.SQL;
using Unify.People.Models;

namespace Unify.People.RulesEngine
{
    public class VisitorRulesEngine
    {
        //private readonly ElasticDb _elasticDb;

        private readonly ClaimsPrincipal _user;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        //private readonly VisitorService _visitorService;
        //private readonly ShiftService _shiftService;
        //private readonly LatePolicyService _latePolicyService;

        private ElasticsearchClient _elasticsearchClient;
        private IRepository<VisitorLog> _VisitorIndexRep { get; set; }
        private readonly string _cccApiURL;
        private readonly string _peopleApiURL;

        public VisitorRulesEngine(IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor contextAccessor
            //ElasticDb elasticDb

            //VisitorService visitorService,
            //ShiftService shiftService,
            //LatePolicyService latePolicyService,
        )
        {
            //_user = contextAccessor.HttpContext.User;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _cccApiURL = _configuration.GetValue<string>("CCCApiUrl");
            _peopleApiURL = _configuration.GetValue<string>("PeopleApiUrl");

            //_elasticDb = elasticDb;
            //_visitorService = visitorService;
            //_shiftService = shiftService;
            //_latePolicyService = latePolicyService;
        }
        public async Task ProcessAsync(CameraDataModel cameraData)
        {
            //var peopleData = await ConvertToVisitorLog(cameraData);
            //if (peopleData.Count > 0)
            //{
            //    await _elasticDb.BulkInsertAsync(peopleData);
            //}
        }

        private async Task SaveElasticVisitorLog(List<VisitorLog> processData)
        {
            ElasticSearchConnectionSettings();
            var res = _elasticsearchClient.IndexManyAsync(processData, "visitorlog").Result;
            if (!res.IsValidResponse)
            {
                var debugInfo = res.DebugInformation;
                var error = res.ElasticsearchServerError.Error;
            }
            if (res.IsValidResponse)
            {

            }
        }

        private async Task<List<VisitorLog>> ConvertToVisitorLog(CameraDataModel cameraDataModel)
        {
            var TenantId = cameraDataModel.TenantId;
            var camera = await GetCameraByIdAsync(cameraDataModel.CameraId);
            List<VisitorLog> VisitorLogList = [];
            LatePolicy latePolicy = new LatePolicy();
            string cameraName = cameraDataModel.CameraName;
            // pending camera from cccdb
            Guid sessionId = Guid.NewGuid();
            string? prvClassName = null;
            foreach (var cls in cameraDataModel.Classes.OrderBy(m => m.Name))
            {
                VisitorLog visitorLog = new VisitorLog()
                {
                    Id = Guid.NewGuid(),
                    DateTime = DateTime.Now,
                    TimeStamp = cameraDataModel.TimeStamp,
                    TenantId = cameraDataModel.TenantId,
                    IntelligenceId = cameraDataModel.IntelligenceId,
                    CameraId = cameraDataModel.CameraId,
                    // VisitorId  set from properties
                    IntelligenceName = cameraDataModel.IntelligenceName,
                    CameraName = cameraDataModel.CameraName,

                    //Code set
                    //Name set
                    //EmployeeType set
                    //Department set
                    //Designation set
                    //Gender set
                    //Shift set
                    //StartTime set
                    //EndTime set
                    //BoundingBox set
                    //Action = camera.Tag,
                    //Time = 0, // set
                              //FirstLogin set
                              //LateLogin set
                              //LateLoginMinutes set
                              //EarlyLogount
                              //EarlyLogoutMinutes
                    //ManualEntry = false,
                    //Reason = null,
                    //LastLog set
                    //SimilarityValue set
                    //FaceCover set 
                    //FaceImage set
                    //FileURL = cameraDataModel.FileURL,
                };


                if (!string.IsNullOrEmpty(cls.BoundingBox))
                {
                    //visitorLog.BoundingBox = cls.BoundingBox;
                }

                foreach (var prop in cls.Properties)
                {
                    if (prop.PropertyName.ToLower() == "personid")
                    {
                        var visitorId = Guid.Parse(prop.PropertyValue);

                        var visitor = await GetVisitorByIdAsync(TenantId, visitorId);

                        if (visitor != null)
                        {
                            visitorLog.VisitorId = visitor.Id;
                            visitorLog.VisitorName = visitor.Name;
                            var appointment = await GetAppointmentByVisitorDateAsync(TenantId, DateOnly.FromDateTime(cameraDataModel.TimeStamp));
                            if (appointment != null)
                            {
                                var employee = await GetEmployeeByIdAsync(TenantId, appointment.EmployeeId);
                                //visitorLog.Appointement = true;
                                //visitorIndex.StartTime = appointment.StartTime.Value;
                                //visitorIndex.EndTime = appointment.EndTime.Value;

                                //visitorLog.LastLog = true;
                                visitorLog.EmployeeId = employee.Id;
                                visitorLog.EmployeeCode = employee.Code;
                                visitorLog.EmployeeName = employee.Name;
                                //visitorIndex.EmployeeType = employee.EmployeeType.Name;
                                //visitorLog.Department = employee.Department.Name;
                                //visitorLog.Designation = employee.Designation.Name;
                                //visitorLog.Gender = employee.Gender.ToString();
                                // time interval not first entry then in time
                                //visitorLog.Time = 0;

                            }
                            // pending check
                            var loggedInCheck = await LoginCheck(visitor.TenantId, visitorLog.EmployeeId, visitorLog.TimeStamp);
                            if (loggedInCheck == true)
                            {
                                // update last log to false 
                                await UpdateLastLog(visitorLog.EmployeeId.ToString(), visitorLog.TimeStamp, "false");
                            }
                            // set last log to true


                            //if (visitorLog.Action.ToLower() == "entry")
                            //{
                            //    // pending
                            //    var loggedIn = await LoginCheck(visitor.TenantId, visitorLog.VisitorId, visitorLog.TimeStamp);

                            //    if (loggedIn == false)
                            //    {
                            //        // previous day get first log
                            //        // previous day early logout
                            //        //var lastLog = await LastLogAsync(visitor.TenantId, visitorIndex.VisitorId);
                            //        //var earlyLogoutMinutes = await EarlyLogoutMinutes(visitor.TenantId, visitorIndex.VisitorId,
                            //        //                           lastLog.TimeStamp);
                            //        //if (earlyLogoutMinutes > 0)
                            //        //{
                            //        //    await UpdateEarlyLogout(lastLog.Id, lastLog.TimeStamp, "true");
                            //        //}

                            //        //visitorLog.FirstLogin = true;
                            //        // late check
                            //        //TimeOnly timeOnly = TimeOnly.FromDateTime(visitorIndex.TimeStamp);
                            //        //TimeSpan loginDiff = TimeSpan.Zero;
                            //        //var shift = await GetShiftAsync(TenantId, timeOnly);
                            //        //if (shift != null)
                            //        //{
                            //        //    loginDiff = timeOnly.ToTimeSpan() - shift.StartTime.ToTimeSpan();
                            //        //    var lateMinutes = (int)loginDiff.TotalMinutes;

                            //        //    visitorIndex.Shift = shift.Name;
                            //        //    visitorIndex.StartTime = shift.StartTime;
                            //        //    visitorIndex.EndTime = shift.EndTime;


                            //        //    if (latePolicy != null)
                            //        //    {
                            //        //        if (lateMinutes > latePolicy.LateLogin)
                            //        //        {
                            //        //            visitorIndex.LateLogin = true;
                            //        //            visitorIndex.LateLoginMinutes = lateMinutes;
                            //        //        }
                            //        //        else
                            //        //        {
                            //        //            visitorIndex.LateLogin = false;
                            //        //            visitorIndex.LateLoginMinutes = 0;
                            //        //        }
                            //        //    }
                            //        //}
                            //    }
                            //    else
                            //    {
                            //        //visitorLog.FirstLogin = false;
                            //        //visitorLog.LateLogin = false;
                            //        //visitorLog.LateLoginMinutes = 0;
                            //        //visitorLog.EarlyLogout = false;
                            //        //visitorLog.EarlyLogoutMinutes = 0;
                            //    }

                            //    //visitorIndex.EarlyLogout = false;
                            //    //visitorIndex.EarlyLogoutMinutes = 0;

                            //    if (latePolicy != null && visitorLog.LateLogin == true)
                            //    {
                            //        AlertMessage alert = new AlertMessage()
                            //        {
                            //            Id = Guid.NewGuid(),
                            //            CreatedTime = DateTime.Now,
                            //            Message = " Name: " + visitor.Name + " is late by " + visitorIndex.LateLoginMinutes + " minutes",
                            //            Severity = Severity.Warning,
                            //            Status = "Active",
                            //            Action = visitorIndex.Action,
                            //            TenantId = visitorIndex.TenantId
                            //        };

                            //        // post to cccdb pending
                            //        //_workplaceClient.AlertAsync(alert);
                            //        AddAlertMessageAsync(alert);

                            //        var lateCount = await LateLoginCount(visitor.TenantId, visitor.Id) + 1;
                            //        if (lateCount > latePolicy.MaxLateLogin)
                            //        {
                            //            AlertMessage alert1 = new AlertMessage()
                            //            {
                            //                Id = Guid.NewGuid(),
                            //                CreatedTime = DateTime.Now,
                            //                Message = " Name: " + visitor.Name + " is late by " + visitorIndex.LateLoginMinutes + " minutes and exceeded max late logins ",
                            //                Severity = Severity.Warning,
                            //                Status = "Active",
                            //                Action = visitorIndex.Action,
                            //                TenantId = visitorIndex.TenantId
                            //            };

                            //            // post to cccdb pending
                            //            //_workplaceClient.AlertAsync(alert);
                            //            AddAlertMessageAsync(alert);
                            //        }
                            //    }
                            //    //}
                            //    // alert data required
                            //    // alert based on EmployeeType Raise Alert black list
                            //    //if (employee.EmployeeType.RaiseAlert == true)
                            //    //{
                            //    //    AlertMessage alert = new AlertMessage()
                            //    //    {
                            //    //        Id = Guid.NewGuid(),
                            //    //        CreatedTime = DateTime.Now,
                            //    //        Message = "Code: " + employee.Code + " Name: " + employee.Name + " is " + employee.EmployeeType.Name,
                            //    //        Severity = Severity.Warning,
                            //    //        Status = "Active",
                            //    //        Action = visitorIndex.Action,
                            //    //        TenantId = visitorIndex.TenantId
                            //    //    };

                            //    //    //_workplaceClient.AlertAsync(alert);
                            //    //    // post to cccdb pending
                            //    //    //_workplaceClient.AlertAsync(alert);
                            //    //    AddAlertMessageAsync(alert);
                            //    //}
                            //}

                            //else if (visitorLog.Action.ToLower() == "exit")
                            //{
                            //    // pending early logout
                            //    // consider DateTime as parameter
                            //    var visitorInTime = await GetVisitorInTimeAsync(visitorIndex.VisitorId.ToString());

                            //    visitorIndex.Time = visitorInTime;


                            //    if (latePolicy != null && visitorIndex.EarlyLogout == true)
                            //    {
                            //        AlertMessage alert = new AlertMessage()
                            //        {
                            //            Id = Guid.NewGuid(),
                            //            CreatedTime = DateTime.Now,
                            //            // pending
                            //            Message = " Name " + visitor.Name + " logged out early by " + visitorIndex.EarlyLogoutMinutes + " minutes",
                            //            Severity = Severity.Warning,
                            //            Status = "Active",
                            //            Action = visitorIndex.Action,
                            //            TenantId = visitorIndex.TenantId
                            //        };

                            //        // post to cccdb pending
                            //        //_workplaceClient.AlertAsync(alert);
                            //        AddAlertMessageAsync(alert);

                            //        var lateCount = await EarlyLogoutCount(visitor.TenantId, visitor.Id) + 1;
                            //        if (lateCount > latePolicy.MaxLateLogin)
                            //        {
                            //            AlertMessage alert1 = new AlertMessage()
                            //            {
                            //                Id = Guid.NewGuid(),
                            //                CreatedTime = DateTime.Now,
                            //                // pending
                            //                Message = " Name " + visitor.Name + " has logget out by " + visitorIndex.EarlyLogoutMinutes + " minutes and exceeded max early logouts ",
                            //                Severity = Severity.Warning,
                            //                Status = "Active",
                            //                Action = visitorIndex.Action,
                            //                TenantId = visitorIndex.TenantId
                            //            };

                            //            // post to cccdb pending
                            //            //_workplaceClient.AlertAsync(alert);
                            //            AddAlertMessageAsync(alert);
                            //        }
                            //    }
                            //}
                        }
                    }
                    else if (prop.PropertyName.ToLower() == "similarityvalue")
                    {
                        //visitorLog.SimilarityValue = prop.PropertyValue;
                    }
                    else if (prop.PropertyName.ToLower() == "facecover")
                    {
                        //visitorLog.FaceCover = prop.PropertyValue;
                    }
                    else if (prop.PropertyName.ToLower() == "faceimage")
                    {
                        //visitorLog.FaceImage = prop.PropertyValue;
                    }

                    if (visitorLog.EmployeeId != Guid.Empty)
                    {
                        VisitorLogList.Add(visitorLog);
                    }
                }
            }

            return VisitorLogList;
        }

        public async Task<Camera> GetCameraByIdAsync(Guid id)
        {
            try
            {
                using (var httpClient = _httpClientFactory.CreateClient())
                {
                    var response = await httpClient.GetAsync(_cccApiURL + "CameraData/GetCameraById?id=" + id);
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<Camera>();
                        return result;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<AlertMessage> AddAlertMessageAsync(AlertMessage alertMessage)
        {
            try
            {
                var reqData = System.Text.Json.JsonSerializer.Serialize(alertMessage);
                var requestContent = new StringContent(reqData, Encoding.UTF8, "application/json");
                using (var httpClient = _httpClientFactory.CreateClient())
                {
                    var response = await httpClient.PostAsync(_cccApiURL + "AlertMessageData/AddAlertMessage", requestContent);
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<AlertMessage>();
                        return result;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<Unify.Data.SQL.Shift> GetShiftAsync(Guid TenantId, TimeOnly time)
        {
            try
            {
                using (var httpClient = _httpClientFactory.CreateClient())
                {
                    var response = await httpClient.GetAsync(_peopleApiURL + $"ShiftData/GetShiftByTime?TenantId={TenantId}&time={time}");

                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<Unify.Data.SQL.Shift>();
                        return result;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<Visitor> GetVisitorByIdAsync(Guid TenantId, Guid visitorId)
        {
            try
            {
                using (var httpClient = _httpClientFactory.CreateClient())
                {
                    var response = await httpClient.
                        GetAsync(_peopleApiURL + $"VisitorData/GetVisigtorById?TenantId={TenantId}&visitorId={visitorId}");

                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<Visitor>();
                        return result;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<Appointment> GetAppointmentByVisitorDateAsync(Guid TenantId, DateOnly date)
        {
            try
            {
                using (var httpClient = _httpClientFactory.CreateClient())
                {
                    var response = await httpClient.GetAsync(_peopleApiURL + $"AppointmentData/GetAppointmentByVisitorDate?TenantId={TenantId}&date={date}");

                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<Appointment>();
                        return result;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<Employee> GetEmployeeByIdAsync(Guid TenantId, Guid employeeId)
        {
            try
            {
                using (var httpClient = _httpClientFactory.CreateClient())
                {
                    var response = await httpClient.
                        GetAsync(_peopleApiURL + $"EmployeeData/GetEmployeeById?TenantId={TenantId}&employeeId={employeeId}");

                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<Employee>();
                        return result;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }
        private void ElasticSearchConnectionSettings()
        {
            var clientSettings = new ElasticsearchClientSettings(new Uri("https://5.161.232.13:9200"));
            clientSettings.DisableDirectStreaming(true);
            clientSettings.Authentication(new BasicAuthentication("YOUR_ELASTICSEARCH_USER", "YOUR_ELASTICSEARCH_PASSWORD"))
                  .CertificateFingerprint("YOUR_ELASTICSEARCH_FINGERPRINT");
            _elasticsearchClient = new ElasticsearchClient(clientSettings);
        }

        /// <summary>
        /// If already logged in returns true
        /// </summary>
        /// <param name="TenantId"></param>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        public async Task<bool> LoginCheck(Guid TenantId, Guid employeeId, DateTime dateTime)
        {
            string query = string.Empty;
            int year = dateTime.Year;
            int month = dateTime.Month;
            int day = dateTime.Day;
            query = "Select employeeId from visitorindex " +
                " where employeeId ='" + employeeId + "' and TenantId = '" + TenantId + "' " +
                " and Year(dateTime) = " + year + " and Month(dateTime) = " + month + " and Day(dateTime) = " + day +
                " and dateTime < '" + dateTime + "' and action = 'CheckIn'";

            ElasticSearchConnectionSettings();
            QueryResponse response = await _elasticsearchClient.Sql.QueryAsync(m => m.Query(query));
            if (response.Rows != null && response.Rows.Count > 0)
            {
                return true;
            }

            return false;
        }

        public async Task<int> LateLoginCount(Guid TenantId, Guid employeeId)
        {
            int count = 0;
            string query = string.Empty;
            DateTime dateTime = DateTime.Parse(DateTime.Today.ToString("yyyy-MM-dd"));
            int year = dateTime.Year;
            int month = dateTime.Month;
            query = "Select Count(*) Count from visitorindex " +
                " where employeeId = '" + employeeId + "' and TenantId = '" + TenantId + "' and Year(dateTime) = " + year + " " +
                " and Month(dateTime) = " + month + " and LateLogin = true";

            ElasticSearchConnectionSettings();
            QueryResponse response = await _elasticsearchClient.Sql.QueryAsync(m => m.Query(query));
            if (response.Rows != null && response.Rows.Count > 0)
            {
                foreach (var row in response.Rows)
                {
                    if (row != null)
                    {
                        count = row.FirstOrDefault().As<int>();
                        return count;
                    }
                }

            }

            return count;
        }

        public async Task<int> EarlyLogoutCount(Guid TenantId, Guid employeeId)
        {
            int count = 0;
            string query = string.Empty;
            DateTime dateTime = DateTime.Parse(DateTime.Today.ToString("yyyy-MM-dd"));
            int year = dateTime.Year;
            int month = dateTime.Month;
            query = "Select Count(*) Count from visitorindex " +
                " where employeeId = '" + employeeId + "' and TenantId = '" + TenantId + "' and Year(dateTime) = " + year + " " +
                " and Month(dateTime) = " + month + " and EarlyLogout = true";

            ElasticSearchConnectionSettings();
            QueryResponse response = await _elasticsearchClient.Sql.QueryAsync(m => m.Query(query));
            if (response.Rows != null && response.Rows.Count > 0)
            {
                foreach (var row in response.Rows)
                {
                    if (row != null)
                    {
                        count = row.FirstOrDefault().As<int>();
                        return count;
                    }
                }

            }

            return count;
        }

        private int Age(DateTime startDateTime, DateTime endDateTime)
        {
            int yearsDifference = endDateTime.Year - startDateTime.Year;

            // Adjust if the end date is before the start date's anniversary
            if (endDateTime < startDateTime.AddYears(yearsDifference))
            {
                yearsDifference--;
            }

            return yearsDifference;
        }
        private async Task UpdateLastLog(string employeeId, DateTime timeStamp, string lastLog)
        {
            int day = timeStamp.Day;
            int month = timeStamp.Month;
            int year = timeStamp.Year;

            var date = new DateTime(year, month, day, 0, 0, 0);
            var ldate = new DateTime(year, month, day, 23, 59, 59);

            string strDate = date.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string strlDate = ldate.ToString("yyyy-MM-ddTHH:mm:ssZ");

            string scriptText = $@"
            {{
              ""script"": {{
                ""source"": ""if ( ctx._source.employeeId == params.employeeId) {{ ctx._source.lastLog = params.newlastLog }}"",
                ""lang"": ""painless"",
                ""params"": {{
                  ""employeeId"": ""{employeeId}"",
                  ""newlastLog"": {lastLog}
                }}
              }},
              ""query"": {{
                ""range"": {{
                  ""timeStamp"": {{
                    ""gte"":""{strDate}"",
                    ""lte"": ""{strlDate}""
                  }}
                }}
                
              }}
            }}";

            var esresponse = await _elasticsearchClient.Transport.RequestAsync<Elastic.Transport.StringResponse>(Elastic.Transport.HttpMethod.POST, "VisitorIndex/_update_by_query", Elastic.Transport.PostData.String(scriptText));


            if (esresponse.ApiCallDetails.HasSuccessfulStatusCode)
            {
                dynamic data = JsonConvert.DeserializeObject(esresponse.Body);

                //Console.WriteLine("Update successful");
            }
            else
            {
                //Console.WriteLine("Update failed: " + esresponse.Body);
            }
        }

        private async Task UpdateEarlyLogout(string id, DateTime timeStamp, string lastLog)
        {
            int day = timeStamp.Day;
            int month = timeStamp.Month;
            int year = timeStamp.Year;

            var date = new DateTime(year, month, day, 0, 0, 0);
            var ldate = new DateTime(year, month, day, 23, 59, 59);

            string strDate = date.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string strlDate = ldate.ToString("yyyy-MM-ddTHH:mm:ssZ");

            string scriptText = $@"
            {{
              ""script"": {{
                ""source"": ""if ( ctx._source.id == params.id) {{ ctx._source.earlylogout = params.newearlylogout }}"",
                ""lang"": ""painless"",
                ""params"": {{
                  ""id"": ""{id}"",
                  ""newearlylogout"": {lastLog}
                }}
              }},
              ""query"": {{
                ""range"": {{
                  ""timeStamp"": {{
                    ""gte"":""{strDate}"",
                    ""lte"": ""{strlDate}""
                  }}
                }}
                
              }}
            }}";

            var esresponse = await _elasticsearchClient.Transport.RequestAsync<Elastic.Transport.StringResponse>(Elastic.Transport.HttpMethod.POST, "VisitorIndex/_update_by_query", Elastic.Transport.PostData.String(scriptText));


            if (esresponse.ApiCallDetails.HasSuccessfulStatusCode)
            {
                dynamic data = JsonConvert.DeserializeObject(esresponse.Body);

                //Console.WriteLine("Update successful");
            }
            else
            {
                //Console.WriteLine("Update failed: " + esresponse.Body);
            }
        }

        public async Task<DateTime> LastLog(Guid TenantId, Guid employeeId)
        {
            DateTime timeStamp = DateTime.Now;
            string query = string.Empty;

            query = "Select Top 1 timeStamp, startTime, endTime from visitorindex " +
                " where employeeId = '" + employeeId + "' and TenantId = '" + TenantId + "' order by timeStamp desc";

            ElasticSearchConnectionSettings();
            QueryResponse response = await _elasticsearchClient.Sql.QueryAsync(m => m.Query(query));
            if (response.Rows != null && response.Rows.Count > 0)
            {
                foreach (var row in response.Rows)
                {
                    if (row != null)
                    {
                        timeStamp = row.FirstOrDefault().As<DateTime>();
                        return timeStamp;
                    }
                }
            }

            return timeStamp;
        }

        public async Task<int> GetVisitorInTimeAsync(string visitorId)
        {
            DateTime dtExit = DateTime.UtcNow;
            int day = DateTime.Now.Day;
            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;

            string query = "Select DATEDIFF('minute', dateTime::datetime, '" + dtExit.ToString("yyyy-MM-ddThh:mm:ss") + "'::datetime) AS EmployeeMinutes " +
                " from  (Select Top 1 visitorId, dateTime, action  from visitorindex " +
                "where visitorId = '" + visitorId + "' and dateTime <= '" + dtExit.ToString("yyyy-MM-ddThh:mm:ss") + "' and " +
                "action = 'CheckIn' and day(dateTime) = " + day + " and  " +
                "month(dateTime) = " + month + " and " +
                "year(dateTime) = " + year + " order by dateTime desc)";

            ElasticSearchConnectionSettings();
            QueryResponse response = await _elasticsearchClient.Sql.QueryAsync(m => m.Query(query));
            if (response.Rows != null && response.Rows.Count > 0)
            {
                foreach (var row in response.Rows)
                {
                    if (row != null)
                    {
                        return row.FirstOrDefault().As<int>();
                    }
                }
            }

            return 0;
        }
    }
}
