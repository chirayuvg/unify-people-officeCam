using AppStudio.Framework.Core;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Sql;
using Elastic.Transport;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using Unify.Data;
using Unify.Data.SQL;
using Unify.People.Services;
using Unify.People.Models;
using Unify.CCC.Data;
using Employee = Unify.Data.SQL.Employee;
using AppliesTo = Unify.Data.SQL.AppliesTo;
using LatePolicy = Unify.Data.SQL.LatePolicy;
using Shift = Unify.Data.SQL.Shift;
using AlertMessage = Unify.People.Models.AlertMessage;
using Severity = Unify.People.Models.Severity;
using PeopleDb = Unify.Data.PeopleDb;
using DocumentFormat.OpenXml.ExtendedProperties;

namespace Unify.People.RulesEngine
{
    public class AttendanceRulesEngine
    {
        //private readonly ElasticDb _elasticDb;

        private readonly ClaimsPrincipal _user;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly PeopleDb _peopleDb;
        //private readonly CCCDb _cccdb;


        //private readonly EmployeeService _employeeService;
        //private readonly ShiftService _shiftService;
        //private readonly LatePolicyService _latePolicyService;


        private IRepository<AttendanceLog> _AttendanceIndexRep { get; set; }
        private readonly string _cccApiURL;
        private readonly string _peopleApiURL;

        public AttendanceRulesEngine(IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor contextAccessor,
            //ElasticDb elasticDb, 
            IDbContextFactory<PeopleDb> dbContextFactory
            //IDbContextFactory<CCCDb> dbContext


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

            //_elasticDb = elasticDb;
            _peopleDb = dbContextFactory.CreateDbContext();
            //_cccdb = dbContext.CreateDbContext();

            //_employeeService = employeeService;
            //_shiftService = shiftService;
            //_latePolicyService = latePolicyService;
        }
        public async Task ProcessAsync(CameraDataModel cameraData)
        {
            var peopleData = await ConvertToAttendanceIndex(cameraData);
            if (peopleData.Count > 0)
            {
                try
                {
                    //await _elasticDb.BulkInsertAsync(peopleData);
                    //SaveElasticAttendanceIndex(peopleData);
                    await _peopleDb.AttendanceLogs.AddRangeAsync(peopleData);
                    await _peopleDb.SaveChangesAsync();
                    //var peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
                    //await peopleDb.AttendanceLogs.AddRangeAsync(peopleData);
                    //await peopleDb.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    var error = ex.Message;
                }
            }
        }
        private async Task<List<AttendanceLog>> ConvertToAttendanceIndex(CameraDataModel cameraDataModel)
        {
            var TenantId = cameraDataModel.TenantId;
            var camera = await GetCameraByIdAsync(cameraDataModel.CameraId);
            List<AttendanceLog> AttendanceLogList = [];
            LatePolicy latePolicy = new LatePolicy();
            string cameraName = cameraDataModel.CameraName;
            // pending camera from cccdb
            Guid sessionId = Guid.NewGuid();
            string? prvClassName = null;
            foreach (var cls in cameraDataModel.Classes.OrderBy(m => m.Name))
            {
                Guid attendanceIndexID = Guid.NewGuid();

                AttendanceLog attendanceLog = new AttendanceLog()
                {
                    Id = attendanceIndexID,
                    DateTime = DateTime.Now,
                    TimeStamp = cameraDataModel.TimeStamp,
                    TenantId = cameraDataModel.TenantId,
                    IntelligenceId = cameraDataModel.IntelligenceId,
                    CameraId = cameraDataModel.CameraId,
                    // EmployeeId  set from properties
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
                    Action = camera.Tag,
                    Time = 0, // set
                    //FirstLogin set
                    //LateLogin set
                    //LateLoginMinutes set
                    //EarlyLogount
                    //EarlyLogoutMinutes
                    ManualEntry = false,
                    Reason = null,
                    //LastLog set
                    //SimilarityValue set
                    //FaceCover set 
                    //FaceImage set
                    Image = attendanceIndexID.ToString()
                };

                if (!string.IsNullOrEmpty(cls.BoundingBox))
                {
                    attendanceLog.BoundingBox = cls.BoundingBox;
                }

                foreach (var prop in cls.Properties)
                {
                    //if (prop.PropertyName.ToLower() == "gender")
                    //{
                    //    //attendanceIndex.Gender = prop.PropertyValue;
                    //}
                    //else if (prop.PropertyName.ToLower() == "age")
                    //{
                    //    //int age;
                    //    //int.TryParse(prop.PropertyValue, out age);
                    //    //attendanceIndex.Age = age;
                    //}
                    if (prop.PropertyName.ToLower() == "personid" && prop.PropertyValue != "")
                    {
                        var employeeId = Guid.Parse(prop.PropertyValue);

                        var employee = await GetEmployeeByIdAsync(TenantId, employeeId);

                        if (employee != null)
                        {
                            //var loggedInCheck = await LoginCheck(employee.TenantId, attendanceIndex.EmployeeId, attendanceIndex.TimeStamp);
                            var loggedInCheck = await LoginCheck(employee.TenantId, employee.Id, attendanceLog.TimeStamp);
                            if (loggedInCheck == true)
                            {
                                // update last log to false 
                                //await UpdateLastLog(attendanceIndex.EmployeeId.ToString(), attendanceIndex.TimeStamp, "false");

                                await UpdateLastLog(employee.Id.ToString(), attendanceLog.TimeStamp, "false");
                            }

                            // set last log to true
                            attendanceLog.LastLog = true;
                            attendanceLog.EmployeeId = employeeId;
                            attendanceLog.Code = employee.Code;
                            attendanceLog.Name = employee.Name;
                            attendanceLog.EmployeeType = employee.EmployeeType.Name;
                            attendanceLog.Department = employee.Department.Name;
                            attendanceLog.Designation = employee.Designation.Name;
                            
                            attendanceLog.Gender = employee.Gender.ToString();
                            attendanceLog.Shift = employee.Shift.Name;

                            // time interval not first entry then in time
                            attendanceLog.Time = 0;

                            // required start 
                            latePolicy = await GetEmployeeLatePolicyAsync(employee.TenantId, employee.Id);
                            // required end

                            if (attendanceLog.Action.ToLower() == "entry")
                            {
                                //var loggedIn = await LoginCheck(employee.TenantId, attendanceLog.EmployeeId, attendanceLog.TimeStamp);
                                var loggedIn = await LoginCheck(employee.TenantId, attendanceLog.EmployeeId, attendanceLog.TimeStamp);


                                if (loggedIn == false)
                                {
                                    // previous day get first log
                                    // previous day early logout
                                    var lastLog = await LastLogAsync(employee.TenantId, attendanceLog.EmployeeId);
                                    var earlyLogoutMinutes = await EarlyLogoutMinutes(employee.TenantId, attendanceLog.EmployeeId,
                                                                lastLog.TimeStamp);
                                    if (earlyLogoutMinutes > 0)
                                    {
                                        await UpdateEarlyLogout(lastLog.Id, lastLog.TimeStamp, "true");
                                    }

                                    attendanceLog.FirstLogin = true;
                                    // late check
                                    TimeOnly timeOnly = TimeOnly.FromDateTime(attendanceLog.TimeStamp);
                                    TimeSpan loginDiff = TimeSpan.Zero;

                                    //var shift = await GetShiftAsync(TenantId, timeOnly);
                                    var shift = await GetShiftAsync(TenantId, employee.ShiftId);

                                    if (shift != null)
                                    {
                                        Console.WriteLine("TimeStamp " + attendanceLog.TimeStamp);
                                        Console.WriteLine("Time " + timeOnly);
                                        //loginDiff = timeOnly.ToTimeSpan() - shift.StartTime.ToTimeSpan();
                                        loginDiff = timeOnly.AddHours(5).AddMinutes(30).ToTimeSpan() - shift.StartTime.ToTimeSpan();
                                        var lateMinutes = (int)loginDiff.TotalMinutes;

                                        attendanceLog.Shift = shift.Name;
                                        attendanceLog.StartTime = shift.StartTime;
                                        attendanceLog.EndTime = shift.EndTime;
                                        Console.WriteLine(lateMinutes);
                                        Console.WriteLine(shift.Name);


                                        if (latePolicy != null)
                                        {
                                            Console.WriteLine("LateLogin" + latePolicy.LateLogin);
                                            Console.WriteLine("ShiftName" + shift.Name);
                                            Console.WriteLine("LateMinutes" + lateMinutes);
                                            //Console.WriteLine(shift.Name);
                                            if (lateMinutes > latePolicy.LateLogin)
                                            {
                                                attendanceLog.LateLogin = true;
                                                attendanceLog.LateLoginMinutes = lateMinutes;
                                            }
                                            else
                                            {
                                                attendanceLog.LateLogin = false;
                                                attendanceLog.LateLoginMinutes = 0;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    attendanceLog.FirstLogin = false;
                                    attendanceLog.LateLogin = false;
                                    attendanceLog.LateLoginMinutes = 0;
                                    attendanceLog.EarlyLogout = false;
                                    attendanceLog.EarlyLogoutMinutes = 0;
                                }

                                //attendanceLog.EarlyLogout = false;
                                //attendanceLog.EarlyLogoutMinutes = 0;

                                if (latePolicy != null && attendanceLog.LateLogin == true)
                                {
                                    AlertMessage alert = new AlertMessage()
                                    {
                                        Id = Guid.NewGuid(),
                                        CreatedTime = DateTime.Now,
                                        Message = "Code: " + employee.Code + " Name: " + employee.Name + " is late by " + attendanceLog.LateLoginMinutes + " minutes",
                                        Severity = Severity.Warning,
                                        Status = "Active",
                                        Action = attendanceLog.Action,
                                        TenantId = attendanceLog.TenantId
                                    };

                                    // post to cccdb pending
                                    //_workplaceClient.AlertAsync(alert);
                                    //Prabhu 280525
                                   // AddAlertMessageAsync(alert);

                                    var lateCount = await LateLoginCount(employee.TenantId, employee.Id) + 1;
                                    if (lateCount > latePolicy.MaxLateLogin)
                                    {
                                        AlertMessage alert1 = new AlertMessage()
                                        {
                                            Id = Guid.NewGuid(),
                                            CreatedTime = DateTime.Now,
                                            Message = "Code: " + employee.Code + " Name: " + employee.Name + " is late by " + attendanceLog.LateLoginMinutes + " minutes and exceeded max late logins ",
                                            Severity = Severity.Warning,
                                            Status = "Active",
                                            Action = attendanceLog.Action,
                                            TenantId = attendanceLog.TenantId
                                        };

                                        // post to cccdb pending
                                        //_workplaceClient.AlertAsync(alert);
                                        //Prabhu 280525
                                        //AddAlertMessageAsync(alert);
                                    }
                                }
                               
                            }
                            else if (attendanceLog.Action.ToLower() == "exit")
                            {
                                // pending early logout
                                // consider DateTime as parameter
                                var employeeInTime = await GetEmployeeInTimeAsync(attendanceLog.EmployeeId.ToString());

                                attendanceLog.Time = employeeInTime;


                                if (latePolicy != null && attendanceLog.EarlyLogout == true)
                                {
                                    AlertMessage alert = new AlertMessage()
                                    {
                                        Id = Guid.NewGuid(),
                                        CreatedTime = DateTime.Now,
                                        // pending
                                        Message = employee.Code + " having name " + employee.Name + " logged out early by " + attendanceLog.EarlyLogoutMinutes + " minutes",
                                        Severity = Severity.Warning,
                                        Status = "Active",
                                        Action = attendanceLog.Action,
                                        TenantId = attendanceLog.TenantId
                                    };

                                    // post to cccdb pending
                                    //_workplaceClient.AlertAsync(alert);
                                    //Prabhu 280525
                                    //AddAlertMessageAsync(alert);

                                    var lateCount = await EarlyLogoutCount(employee.TenantId, employee.Id) + 1;
                                    if (lateCount > latePolicy.MaxLateLogin)
                                    {
                                        AlertMessage alert1 = new AlertMessage()
                                        {
                                            Id = Guid.NewGuid(),
                                            CreatedTime = DateTime.Now,
                                            // pending
                                            Message = employee.Code + " having name " + employee.Name + " has logget out by " + attendanceLog.EarlyLogoutMinutes + " minutes and exceeded max early logouts ",
                                            Severity = Severity.Warning,
                                            Status = "Active",
                                            Action = attendanceLog.Action,
                                            TenantId = attendanceLog.TenantId
                                        };

                                        // post to cccdb pending
                                        //_workplaceClient.AlertAsync(alert);
                                        //Prabhu 280525
                                        //AddAlertMessageAsync(alert);
                                    }
                                }
                            }
                        }
                    }
                    else if (prop.PropertyName.ToLower() == "similarityvalue" && prop.PropertyValue != "")
                    {
                        attendanceLog.SimilarityValue = prop.PropertyValue;
                    }
                    else if (prop.PropertyName.ToLower() == "facecover" && prop.PropertyValue != "")
                    {
                        attendanceLog.FaceCover = prop.PropertyValue;
                    }
                    else if (prop.PropertyName.ToLower() == "faceimage")
                    {
                        byte[] bytes = Convert.FromBase64String(prop.PropertyValue);
                        MemoryStream ms = new MemoryStream(bytes);
                        var fileStorage = CoreServices.FileStorageService;
                        var path = Path.Combine(nameof(attendanceLog), nameof(attendanceLog.Image));
                        await fileStorage.SaveFileAsync(
                            "documents"
                            //, $"{nameof(attendanceLog)}\\{nameof(attendanceLog.Image)}"
                            ,path
                            , attendanceLog.Id.ToString()
                            , MimeHelper.GetMimeType("jpg")
                            , ms
                        );
                    }

                    AttendanceLogList.Add(attendanceLog);
                }
            }
            return AttendanceLogList;
        }

        public async Task<Unify.People.Data.SQL.Camera> GetCameraByIdAsync(Guid id)
        {
            //try
            //{
            //    using (var httpClient = _httpClientFactory.CreateClient())
            //    {
            //        var response = await httpClient.GetAsync(_cccApiURL + "CameraData/GetCameraById?id=" + id);
            //        response.EnsureSuccessStatusCode();
            //        if (response.IsSuccessStatusCode)
            //        {
            //            var result = await response.Content.ReadFromJsonAsync<Camera>();
            //            return result;
            //        }
            //        else
            //        {
            //            return null;
            //        }
            //    }
            //}
            //catch
            //{
            //    throw;
            //}
            try
            {
                Unify.People.Data.SQL.Camera camera = await _peopleDb.Cameras.Where(m => m.Id == id).FirstOrDefaultAsync();

                return camera;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return null;
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

        
        public async Task<Shift> GetShiftAsync(Guid TenantId, Guid? shiftId)
        {
            try
            {
                //Department department = await _peopleDb.Departments.Where(m => m.Id == departmentId).SingleOrDefaultAsync();
                Shift shift = await _peopleDb.Shifts.Where(m => m.Id == shiftId).SingleOrDefaultAsync();
                return shift;
            }
            catch
            {
                throw;
            }
        }

        public async Task<LatePolicy> GetEmployeeLatePolicyAsync(Guid TenantId, Guid employeeId)
        {
            try
            {
                LatePolicy latePolicy = new();

                latePolicy = await _peopleDb.LatePolicies
                    .Where(m => m.EmployeeId == employeeId && m.AppliesTo == AppliesTo.Employee && m.TenantId == TenantId).FirstOrDefaultAsync();

                if (latePolicy == null)
                {
                    var employee = await _peopleDb.Employees.Where(m => m.Id == employeeId).FirstOrDefaultAsync();

                    //if (employee.DepartmentId != null && employee.DepartmentId != Guid.Empty)
                    if (employee != null)
                    {
                        latePolicy = await _peopleDb.LatePolicies
                        .Where(m => m.EmployeeId == employeeId && m.AppliesTo == AppliesTo.Department && m.DepartmentId == employee.DepartmentId && m.TenantId == TenantId).FirstOrDefaultAsync();
                    }
                }

                if (latePolicy == null)
                {
                    latePolicy = await _peopleDb.LatePolicies
                    .Where(m => m.TenantId == TenantId && m.AppliesTo == AppliesTo.All).FirstOrDefaultAsync();
                }
                return latePolicy;
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
                Employee empolyee = await _peopleDb.Employees
                    .Include(m => m.EmployeeType)
                    .Include(m => m.Department)
                    .Include(m => m.Designation)
                    .Include(m => m.Shift)
                    .Where(m => m.Id == employeeId && m.TenantId == TenantId).SingleOrDefaultAsync();

                return empolyee;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> EarlyLogoutMinutes(Guid TenantId, Guid employeeId, DateTime timeStamp)
        {
            // timeStamp is last log timeStamp
            string query = string.Empty;

            int day = timeStamp.Day;
            int month = timeStamp.Month;
            int year = timeStamp.Year;

            DateTime firstLoginTimeStamp = DateTime.Now;
            TimeOnly startTime = new TimeOnly();
            TimeOnly endTime = new TimeOnly();
            // get first log
            var response = await _peopleDb.AttendanceLogs.Where(m => m.EmployeeId == employeeId && m.TenantId == TenantId
            && m.FirstLogin == true && m.TimeStamp.Date == timeStamp.Date).FirstAsync();
            if (response != null)
            {
                firstLoginTimeStamp = response.DateTime;
                startTime = (TimeOnly)response.StartTime;
                endTime = (TimeOnly)response.EndTime;
            }
            // earlylogout
            TimeSpan loginDiff = TimeSpan.Zero;
            TimeSpan logoutDiff = TimeSpan.Zero;
            TimeOnly firstLoginTimeOnly = TimeOnly.FromDateTime(firstLoginTimeStamp);
            TimeOnly lastLogoutTimeOnly = TimeOnly.FromDateTime(timeStamp);
            int earlyLogoutMinutes = 0;
            loginDiff = startTime.ToTimeSpan() - firstLoginTimeOnly.ToTimeSpan(); // 9-7, 9 - 10
            var inMinutes = (int)loginDiff.TotalMinutes;
            // check for negative for lateMinutes
            logoutDiff = endTime.ToTimeSpan() - lastLogoutTimeOnly.ToTimeSpan(); // 6 - 5
            var outMinutes = (int)logoutDiff.TotalMinutes;
            //+  10 - (+10) earlyin earlyout , -10 - (+10) latein earlyout , , -10 - (-10) latein, not earlyout
            earlyLogoutMinutes = inMinutes - outMinutes;

            if (earlyLogoutMinutes < 0)
            {
                earlyLogoutMinutes = -1 * (earlyLogoutMinutes);
            }
            else
            {
                earlyLogoutMinutes = 0;
            }
            return earlyLogoutMinutes;
        }

        /// <summary>
        /// If already logged in returns true
        /// </summary>
        /// <param name="TenantId"></param>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        public async Task<bool> LoginCheck(Guid TenantId, Guid employeeId, DateTime dateTime)
        {
            try
            {
                var response = await _peopleDb.AttendanceLogs.Where(m => m.EmployeeId == employeeId && m.TenantId == TenantId
                && m.TimeStamp.Date == dateTime.Date && m.TimeStamp < dateTime && m.Action == "CheckIn").FirstOrDefaultAsync();

                if (response != null)
                {
                    return true;
                }

                return false;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> LateLoginCount(Guid TenantId, Guid employeeId)
        {
            int count = 0;
            string query = string.Empty;
            DateTime dateTime = DateTime.Parse(DateTime.Today.ToString("yyyy-MM-dd"));
            int year = dateTime.Year;
            int month = dateTime.Month;
            var response = _peopleDb.AttendanceLogs.Where(m => m.EmployeeId == employeeId &&
            m.TenantId == TenantId && m.TimeStamp.Year == year && m.TimeStamp.Month == month &&
            m.LateLogin == true).Count();
            return response;
        }

        public async Task<int> EarlyLogoutCount(Guid TenantId, Guid employeeId)
        {
            int count = 0;
            string query = string.Empty;
            DateTime dateTime = DateTime.Parse(DateTime.Today.ToString("yyyy-MM-dd"));
            int year = dateTime.Year;
            int month = dateTime.Month;
            var response = _peopleDb.AttendanceLogs.Where(m => m.EmployeeId == employeeId &&
            m.TenantId == TenantId && m.TimeStamp.Year == year && m.TimeStamp.Month == month &&
            m.EarlyLogout == true).Count();
            return response;
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
            var respose = await _peopleDb.AttendanceLogs.Where(m => m.EmployeeId == Guid.Parse(employeeId) && m.TimeStamp > date && m.TimeStamp < ldate).ToListAsync();
            foreach (var item in respose)
            {
                AttendanceLog attendanceLog = await _peopleDb.AttendanceLogs.FindAsync(item.Id);
                attendanceLog.LastLog = false;
                _peopleDb.AttendanceLogs.Update(attendanceLog);
                _peopleDb.SaveChanges();
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

            AttendanceLog attendanceLog = await _peopleDb.AttendanceLogs.FindAsync(Guid.Parse(id));
            attendanceLog.LastLog = true;
            _peopleDb.AttendanceLogs.Update(attendanceLog);
            _peopleDb.SaveChanges();


           
        }

        public async Task<LastLog> LastLogAsync(Guid TenantId, Guid employeeId)
        {
            LastLog lastLog = new();
            DateTime timeStamp = DateTime.Now;
            string query = string.Empty;
            AttendanceLog attendanceLog = await _peopleDb.AttendanceLogs.Where(m => m.EmployeeId == employeeId && m.TenantId == TenantId).OrderByDescending(m => m.TimeStamp).FirstOrDefaultAsync();
            lastLog.Id = attendanceLog.Id.ToString();
            lastLog.TimeStamp  = attendanceLog.DateTime;
            return lastLog;
        }

        public async Task<int> GetEmployeeInTimeAsync(string employeeId)
        {
            DateTime dtExit = DateTime.UtcNow.AddHours(5).AddMinutes(30);
            int day = DateTime.Now.Day;
            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;
            var lastEntry  = await _peopleDb.AttendanceLogs
                .Where(x => x.EmployeeId == Guid.Parse(employeeId)
                            && x.Action == "CheckIn"
                            && x.TimeStamp <= dtExit
                            && x.TimeStamp.Day == day
                            && x.TimeStamp.Month == month
                            && x.TimeStamp.Year == year)
                .OrderByDescending(x => x.TimeStamp)
                .FirstOrDefaultAsync();
            int? minutesWorked = null;

            if (lastEntry != null)
            {
                minutesWorked = (int)(dtExit - lastEntry.TimeStamp).TotalMinutes;
            }
            return 0;
        }
    }
    public class EmployeeTime
    {
        public int EmployeeMinutes { get; set; }
    }
    public class Camera
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public CameraProtocol CameraProtocol { get; set; }

        public string ConnectionString { get; set; } = string.Empty;

        public string? AOI { get; set; }
        public string Tag { get; set; }
        public List<CameraMetaData>? CameraMetaData { get; set; } = new();
        public Guid TenantId { get; set; }
    }

    public class CameraMetaData
    {
        public Guid Id { get; set; }
        [MaxLength(50)]
        public string Key { get; set; }
        [MaxLength(50)]
        public string Value { get; set; }
        public Guid CameraId { get; set; }
        public Camera? Camera { get; set; }
    }
    public enum CameraProtocol
    {
        RTSP,
        USB
    }

    public class LastLog
    {
        public string Id { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    //public class CameraDataModel
    //{
    //    public string? Id { get; set; } = Guid.NewGuid().ToString();
    //    public DateTime DateTime { get; set; } = DateTime.Now;
    //    public Guid CameraId { get; set; }
    //    public Guid TenantId { get; set; }
    //    public Guid TenantId { get; set; }
    //    public Guid ProcessId { get; set; }
    //    public Guid? BranchId { get; set; }
    //    public Guid? SectorId { get; set; }
    //    public Guid? AreaId { get; set; }
    //    public string? Usecases { get; set; }
    //    public List<ClassData> Classes { get; set; } = new List<ClassData>();
    //    public string? FileURL { get; set; }
    //}

    //public class ClassData
    //{
    //    public string Name { get; set; }

    //    //public int TrackingId { get; set; }
    //    public string TrackingId { get; set; }
    //    public string BoundingBox { get; set; } = string.Empty;

    //    public string Polygon { get; set; } = string.Empty;

    //    public List<ClassProperty> Properties { get; set; } = new List<ClassProperty>();
    //}

    //public class ClassProperty
    //{
    //    public string PropertyName { get; set; } = string.Empty;

    //    public string PropertyValue { get; set; }
    //}
}
