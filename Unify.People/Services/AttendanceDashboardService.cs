using Elastic.Clients.Elasticsearch.Sql;
using Elastic.Clients.Elasticsearch;
using Unify.Data.SQL;
using Unify.People.Widgets;
using Unify.People.Models;
using Unify.Data;
using Microsoft.EntityFrameworkCore;
using Elastic.Transport;
using Period = Unify.People.Models.Period;
using AppStudio.Model;

namespace Unify.People.Services
{
    public class AttendanceDashboardService
    {

        private readonly WidgetService _widgetService;

        private List<EmployeeData> _firstEntries = new();
        private List<EmployeeData> _lastEntries = new();
        private List<EmployeeData> _lastExits = new();
        private List<LatePolicy> _latePolicies = new();
        private List<EmployeeData> _employeesIndex = new();
        private readonly PeopleDb _peopleDb;
        private ElasticsearchClient _elasticsearchClient;

        public AttendanceDashboardService(IDbContextFactory<PeopleDb> dbContextFactory, WidgetService widgetService)
        {
            _peopleDb = dbContextFactory.CreateDbContext();

            _widgetService = widgetService;
        }

        private async Task liveprocs(string TenantId)
        {
            _lastEntries = await GetAllEmployeesLastEntryTimeAsync(TenantId);
            _lastExits = await GetAllEmployeesLastExitsTimeAsync(TenantId);
            _firstEntries = await GetAllEmployeesFirstEntryTimeAsync(TenantId);
            _latePolicies = await GetLatePolicyOnTenantIdAsync(Guid.Parse(TenantId));

        }

        private void ElasticSearchProduction()
        {

            var clientSettings = new ElasticsearchClientSettings(new Uri("https://5.161.232.13:9200"));
            clientSettings.DisableDirectStreaming(true);
            clientSettings.Authentication(new BasicAuthentication("YOUR_ELASTICSEARCH_USER", "YOUR_ELASTICSEARCH_PASSWORD"))
                  .CertificateFingerprint("YOUR_ELASTICSEARCH_FINGERPRINT");
            _elasticsearchClient = new ElasticsearchClient(clientSettings);

        }

        public async Task<List<Shift>> GetShiftsAsync(Guid TenantId)
        {
            var shifts = await _peopleDb.Shifts.Where(m => m.TenantId == TenantId).ToListAsync();
            return shifts;
        }
        public async Task<List<EmployeeData>> EmployeeWiseFirstEntryAsync(AttendanceFilterData attendanceFilterData)
        {
            // pending late login graph
            string query = string.Empty;
            //
            var shifts = await GetShiftsAsync(Guid.Parse(attendanceFilterData.TenantId.ToString()));
            if (attendanceFilterData.Department != null)
            {
                //employeeId,action,min(timeStamp) timeStamp,name,department,designation,shift
                /*
               
                 
                 
                query = "SELECT employeeId, action, timeStamp, name, department, designation, shift, time FROM attendanceindex" +
                            " where timeStamp >= '{0}' and timeStamp <= '{1}' and TenantId = '{2}' " +
                            "and departmentId = '" + attendanceFilterData.DepartmentId + "' order by timeStamp";
                */
                query = "SELECT employeeId,action,min(timeStamp) timeStamp,name,department,designation,shift FROM attendanceindex " +
              " where  timeStamp >= '{0}' and timeStamp <= '{1}' and action = 'CheckIn' and TenantId = '{2}' " +
              "and department = '" + attendanceFilterData.Department + "' " +
              " Group By employeeId,action,name,department,designation,shift,Cast(timeStamp as Date) order by timeStamp desc";

            }
            else
            {
                query = "SELECT employeeId,action,min(timeStamp) timeStamp,name,department,designation,shift FROM attendanceindex " +
              " where  timeStamp >= '{0}' and timeStamp <= '{1}' and action = 'CheckIn' and TenantId = '{2}' " +
              " Group By employeeId,action,name,department,designation,shift,Cast(timeStamp as Date) order by timeStamp desc";

            }

            Widget widget = new Widget();
            widget.Query = query;
            widget.Period = attendanceFilterData.Period;
            widget.FromDate = attendanceFilterData.FromDate;
            widget.ToDate = attendanceFilterData.ToDate;
            widget.TenantId = Guid.Parse(attendanceFilterData.TenantId.ToString());
            query = _widgetService.GetQuery(widget);
            List<EmployeeData> employeesIndexData = new();

            ElasticSearchProduction();
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
                            EmployeeData employeeIndex = new EmployeeData();
                            employeeIndex.EmployeeId = row.FirstOrDefault().As<Guid>();
                            employeeIndex.Action = row.Skip(1).Take(1).FirstOrDefault().As<string>();
                            employeeIndex.DateTime = row.Skip(2).Take(1).FirstOrDefault().As<DateTime>();
                            employeeIndex.Name = row.Skip(3).Take(1).FirstOrDefault().As<string>();

                            employeeIndex.Department = row.Skip(4).Take(1).FirstOrDefault().As<string>();
                            employeeIndex.Designation = row.Skip(5).Take(1).FirstOrDefault().As<string>();
                            employeeIndex.Shift = row.Skip(6).Take(1).FirstOrDefault().As<string>();
                            employeeIndex.Time = row.Skip(7).Take(1).FirstOrDefault().As<decimal>();

                            var shift = shifts.Where(m => m.Name.ToLower() == employeeIndex.Shift.ToLower()).SingleOrDefault();
                            if (TimeOnly.FromDateTime(employeeIndex.DateTime) > shift.StartTime)
                            {
                                employeeIndex.Late = true;
                                employeeIndex.Count = 1;
                            }

                            employeesIndexData.Add(employeeIndex);
                        }
                    }

                }
                _employeesIndex = employeesIndexData;
                return employeesIndexData;
            }
            _employeesIndex = employeesIndexData;
            return employeesIndexData;

        }

        public async Task<List<EmployeeData>> EmployeeWiseLastExitAsync(AttendanceFilterData attendanceFilterData)
        {
            // pending early logout graph
            string query = string.Empty;
            //
            var shifts = await GetShiftsAsync(Guid.Parse(attendanceFilterData.TenantId.ToString()));
            if (attendanceFilterData.Department != null)
            {

                query = "SELECT employeeId,action,max(timeStamp) timeStamp,name,department,designation,shift FROM attendanceindex " +
              " where  timeStamp >= '{0}' and timeStamp <= '{1}' and action = 'CheckOut' and TenantId = '{2}' " +
              "and department = '" + attendanceFilterData.Department + "' " +
              " Group By employeeId,action,name,department,designation,shift,Cast(timeStamp as Date) order by timeStamp desc";

            }
            else
            {
                query = "SELECT employeeId,action,max(timeStamp) timeStamp,name,department,designation,shift FROM attendanceindex " +
              " where  timeStamp >= '{0}' and timeStamp <= '{1}' and action = 'CheckOut' and TenantId = '{2}' " +
              " Group By employeeId,action,name,department,designation,shift,Cast(timeStamp as Date) order by timeStamp desc";

            }

            Widget widget = new Widget();
            widget.Query = query;
            widget.Period = attendanceFilterData.Period;
            widget.FromDate = attendanceFilterData.FromDate;
            widget.ToDate = attendanceFilterData.ToDate;
            widget.TenantId = Guid.Parse(attendanceFilterData.TenantId.ToString());
            query = _widgetService.GetQuery(widget);
            List<EmployeeData> employeesIndexData = new();

            ElasticSearchProduction();
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
                            EmployeeData employeeIndex = new EmployeeData();
                            employeeIndex.EmployeeId = row.FirstOrDefault().As<Guid>();
                            employeeIndex.Action = row.Skip(1).Take(1).FirstOrDefault().As<string>();
                            employeeIndex.DateTime = row.Skip(2).Take(1).FirstOrDefault().As<DateTime>();
                            employeeIndex.Name = row.Skip(3).Take(1).FirstOrDefault().As<string>();

                            employeeIndex.Department = row.Skip(4).Take(1).FirstOrDefault().As<string>();
                            employeeIndex.Designation = row.Skip(5).Take(1).FirstOrDefault().As<string>();
                            employeeIndex.Shift = row.Skip(6).Take(1).FirstOrDefault().As<string>();
                            employeeIndex.Time = row.Skip(7).Take(1).FirstOrDefault().As<decimal>();

                            var shift = shifts.Where(m => m.Name.ToLower() == employeeIndex.Shift.ToLower()).SingleOrDefault();
                            if (TimeOnly.FromDateTime(employeeIndex.DateTime) > shift.EndTime)
                            {
                                employeeIndex.Early = true;
                                employeeIndex.Count = 1;

                            }

                            employeesIndexData.Add(employeeIndex);
                        }
                    }

                }
                _employeesIndex = employeesIndexData;
                return employeesIndexData;
            }
            _employeesIndex = employeesIndexData;
            return employeesIndexData;

        }

        /// <summary>
        /// First login employee wise.
        /// </summary>
        /// <param name="TenantId"></param>
        /// <returns></returns>
        public async Task<List<EmployeeData>> GetAllEmployeesFirstEntryTimeAsync(string TenantId)
        {

            DateTime dtExit = DateTime.UtcNow;
            int day = DateTime.Now.Day;
            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;
            List<EmployeeData> peopleIndexs = new();
            //string query = "SELECT employeeId,action,min(timeStamp) timeStamp,name,department,designation,shift " +
            //    "FROM attendanceindex " +
            //    " where  timeStamp <= '" + dtExit.ToString("yyyy-MM-ddTHH:mm:ssZ") + "' and day(timeStamp) = " + day + "  and month(timeStamp) = " + month +
            //    " and year(timeStamp) = " + year + " and action = 'CheckIn' and TenantId = '" + TenantId +
            //    "' Group By employeeId,action,name,department,designation,shift order by timeStamp desc";
            string query = "SELECT employeeId,action,min(timeStamp) timeStamp,name,department,designation,shift FROM attendanceindex " +
                " where  timeStamp >= '{0}' and timeStamp <= '{1}' and action = 'CheckIn' and TenantId = '{2}' " +
                " Group By employeeId,action,name,department,designation,shift order by timeStamp desc";

            Widget widget = new Widget();
            widget.Query = query;
            widget.Period = Period.ThisWeek;
            widget.TenantId = Guid.Parse(TenantId);
            query = _widgetService.GetQuery(widget);

            //var data = await _esDb.GetAllAsync<EmployeeData>(query);
            ElasticSearchProduction();
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
                            EmployeeData peopleIndex = new EmployeeData();
                            peopleIndex.EmployeeId = row.FirstOrDefault().As<Guid>();
                            peopleIndex.Action = row.Skip(1).Take(1).FirstOrDefault().As<string>();
                            peopleIndex.DateTime = row.Skip(2).Take(1).FirstOrDefault().As<DateTime>();
                            peopleIndex.Name = row.Skip(3).Take(1).FirstOrDefault().As<string>();

                            peopleIndex.Department = row.Skip(4).Take(1).FirstOrDefault().As<string>();
                            peopleIndex.Designation = row.Skip(5).Take(1).FirstOrDefault().As<string>();
                            peopleIndex.Shift = row.Skip(6).Take(1).FirstOrDefault().As<string>();

                            peopleIndexs.Add(peopleIndex);
                        }
                    }

                }
            }
            return peopleIndexs;
        }

        /// <summary>
        /// Last logged in employee wise
        /// </summary>
        /// <param name="TenantId"></param>
        /// <returns></returns>
        public async Task<List<EmployeeData>> GetAllEmployeesLastEntryTimeAsync(string TenantId)
        {
            DateTime dtExit = DateTime.UtcNow;
            int day = DateTime.Now.Day;
            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;
            List<EmployeeData> peopleIndexs = new();
            //string query = "SELECT employeeId,action,max(timeStamp) timeStamp,name,department,designation,shift " +
            //    "FROM attendanceindex " +
            //    " where  timeStamp <= '" + dtExit.ToString("yyyy-MM-ddTHH:mm:ssZ") + "' and day(timeStamp) = " + day + "  and month(timeStamp) = " + month +
            //    " and year(timeStamp) = " + year + " and action = 'CheckIn' and TenantId = '" + TenantId +
            //    "' Group By employeeId,action,name,department,designation,shift order by timeStamp desc";

            string query = "SELECT employeeId,action,max(timeStamp) timeStamp,name,department,designation,shift FROM attendanceindex " +
              " where   timeStamp >= '{0}' and timeStamp <= '{1}' and action = 'CheckIn' and TenantId = '{2}' " +
              " Group By employeeId,action,name,department,designation,shift order by timeStamp desc";


            Widget widget = new Widget();
            widget.Query = query;
            widget.Period = Period.ThisWeek;
            widget.TenantId = Guid.Parse(TenantId);
            query = _widgetService.GetQuery(widget);

            //var data = await _esDb.GetAllAsync<EmployeeData>(query);
            ElasticSearchProduction();
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
                            EmployeeData peopleIndex = new EmployeeData();
                            peopleIndex.EmployeeId = row.FirstOrDefault().As<Guid>();
                            peopleIndex.Action = row.Skip(1).Take(1).FirstOrDefault().As<string>();
                            peopleIndex.DateTime = row.Skip(2).Take(1).FirstOrDefault().As<DateTime>();
                            peopleIndex.Name = row.Skip(3).Take(1).FirstOrDefault().As<string>();

                            peopleIndex.Department = row.Skip(4).Take(1).FirstOrDefault().As<string>();
                            peopleIndex.Designation = row.Skip(5).Take(1).FirstOrDefault().As<string>();
                            peopleIndex.Shift = row.Skip(6).Take(1).FirstOrDefault().As<string>();

                            peopleIndexs.Add(peopleIndex);
                        }
                    }

                }
            }
            return peopleIndexs;
        }


        public async Task<KpiModel> LiveLogInData(string TenantId)
        {
            //_lastEntries = await GetAllEmployeesLastEntryTimeAsync(TenantId);
            //_lastExits = await GetAllEmployeesLastExitsTimeAsync(TenantId);
            //_firstEntries = await GetAllEmployeesFirstEntryTimeAsync(TenantId);
            //_latePolicies = await GetLatePolicyOnTenantIdAsync(Guid.Parse(TenantId));
            //_allEmployeeShifts = await GetAllEmployeesShiftAsync(Guid.Parse(TenantId));
            await liveprocs(TenantId);
            int count = 0;
            foreach (var data in _lastEntries)
            {
                var entry = _lastExits.Any(m => m.EmployeeId == data.EmployeeId && m.DateTime < data.DateTime);
                if (entry)
                {
                    count++;
                }
            }

            KpiModel model = new KpiModel();
            model.Title = "LoggedIn";
            model.Value = count.ToString();
            return model;
        }

        /// <summary>
        /// Last logout timeStamp employee wise
        /// </summary>
        /// <param name="TenantId"></param>
        /// <returns></returns>
        public async Task<List<EmployeeData>> GetAllEmployeesLastExitsTimeAsync(string TenantId)
        {
            DateTime dtExit = DateTime.UtcNow;
            int day = DateTime.Now.Day;
            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;
            List<EmployeeData> peopleIndexs = new();
            //string query = "SELECT employeeId,action,max(timeStamp) timeStamp,name,department,designation,shift " +
            //    "FROM attendanceindex " +
            //    " where  timeStamp <= '" + dtExit.ToString("yyyy-MM-ddTHH:mm:ssZ") + "' and day(timeStamp) = " + day + "  and month(timeStamp) = " + month +
            //    " and year(timeStamp) = " + year + " and action = 'CheckOut' and TenantId = '" + TenantId +
            //    "' Group By employeeId,action,name,department,designation,shift order by timeStamp desc"

            string query = "SELECT employeeId,action,max(timeStamp) timeStamp,name,department,designation,shift FROM attendanceindex " +
           "  where  timeStamp >= '{0}' and timeStamp <= '{1}' and action = 'CheckOut' and TenantId = '{2}'" +
           " Group By employeeId,action,name,department,designation,shift order by timeStamp desc";

            Widget widget = new Widget();
            widget.Query = query;
            widget.Period = Period.ThisWeek;
            widget.TenantId = Guid.Parse(TenantId);
            query = _widgetService.GetQuery(widget);

            //var data = await _esDb.GetAllAsync<EmployeeData>(query);
            ElasticSearchProduction();
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
                            EmployeeData peopleIndex = new EmployeeData();
                            peopleIndex.EmployeeId = row.FirstOrDefault().As<Guid>();
                            peopleIndex.Action = row.Skip(1).Take(1).FirstOrDefault().As<string>();
                            peopleIndex.DateTime = row.Skip(2).Take(1).FirstOrDefault().As<DateTime>();
                            peopleIndex.Name = row.Skip(3).Take(1).FirstOrDefault().As<string>();

                            peopleIndex.Department = row.Skip(4).Take(1).FirstOrDefault().As<string>();
                            peopleIndex.Designation = row.Skip(5).Take(1).FirstOrDefault().As<string>();
                            peopleIndex.Shift = row.Skip(6).Take(1).FirstOrDefault().As<string>();

                            peopleIndexs.Add(peopleIndex);
                        }
                    }

                }
            }
            return peopleIndexs;
        }

        public async Task<List<LatePolicy>> GetLatePolicyOnTenantIdAsync(Guid TenantId)
        {
            try
            {
                var latePolicies = await _peopleDb.LatePolicies
                    .Where(m => m.TenantId == TenantId).ToListAsync();
                return latePolicies;
            }
            catch
            {
                throw;
            }
        }


    }

    public class EmployeeData
    {
        public Guid EmployeeId { get; set; }
        public DateTime DateTime { get; set; }
        public string Action { get; set; }
        public string Name { get; set; }
        public string? Department { get; set; }
        public string? Designation { get; set; }
        public string? Shift { get; set; }
        public decimal Time { get; set; }

        public bool Late { get; set; }
        public bool Early { get; set; }

        public int Count { get; set; }
        public int Sum { get; set; }
        public int AvgDailyWorkingHours { get; set; }

        public int CountLateLogins { get; set; }
        public int CountEarlyLogouts { get; set; }
    }

    public class DepartmentData
    {
        public Guid DepartmentId { get; set; }
        public string Department { get; set; }
        public decimal AvgDailyWorkingHours { get; set; }
    }

    public class AttendanceFilterData
    {
        public Guid? TenantId { get; set; }

        //public Guid? DepartmentId { get; set; }

        //public Department? Department { get; set; }
        public string Department { get; set; }
        public Period Period { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public AttendanceFilterData()
        {
            FromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            ToDate = DateTime.Today.AddDays(1);
        }
    }
}
