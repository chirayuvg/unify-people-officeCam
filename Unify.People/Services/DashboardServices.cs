using AppStudio.Framework.ElasticSearch;
using AppStudio.Model;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Sql;
using Elastic.Transport;
using Unify.People.Models;

namespace Unify.People.Services
{
    public class DashboardServices
    {

        private readonly ESDbContext _esDb;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string AIServiceUrl;
        private readonly IConfiguration _configuration;
        private ElasticsearchClient _elasticsearchClient;

        private readonly WidgetService _widgetService;
        private List<EmployeeData> _firstEntries = new();
        private List<EmployeeData> _lastEntries = new();
        private List<EmployeeData> _lastExits = new();
        private List<EmployeeData> _employeesIndex = new();

        public DashboardServices(
             IConfiguration configuration, IHttpClientFactory httpClientFactory
           , WidgetService widgetService)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            AIServiceUrl = _configuration.GetValue<string>("AIServiceLiveURL") ?? string.Empty;
            _widgetService = widgetService;
        }

        public async Task<List<EmployeeData>> DaywiseSummaryAsync(AttendanceFilterData attendanceFilterData)
        {
            // pending holidays
            string query = string.Empty;
            if (attendanceFilterData.Department != "" && attendanceFilterData.Department != null)
            {
                query = "SELECT employeeId, sum(time) time, CAST(timeStamp AS Date) Date FROM attendanceindex" +
                            " where action = 'CheckOut' and timeStamp >= '{0}' and timeStamp <= '{1}' and TenantId = '{2}' " +
                            "and department = '" + attendanceFilterData.Department + "' " +
                            "Group By employeeId, CAST(timeStamp AS Date)";
            }
            else
            {
                query = "SELECT employeeId, sum(time) time, CAST(timeStamp AS Date) Date FROM attendanceindex" +
                            " where action = 'CheckOut' and timeStamp >= '{0}' and timeStamp <= '{1}' and TenantId = '{2}' " +
                            "Group By employeeId, CAST(timeStamp AS Date)";
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
                            // employeeId, name, department, designation,
                            EmployeeData employeeIndex = new EmployeeData();
                            employeeIndex.EmployeeId = row.FirstOrDefault().As<Guid>();
                            employeeIndex.Time = row.Skip(1).Take(1).FirstOrDefault().As<decimal>();
                            employeeIndex.DateTime = row.Skip(2).Take(1).FirstOrDefault().As<DateTime>();

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
        public async Task<List<EmployeeData>> EmployeeWiseAverageHoursAsync(AttendanceFilterData attendanceFilterData)
        {
            // pending holidays
            string query = string.Empty;
            if (attendanceFilterData.Department != null)
            {
                query = "SELECT employeeId, name, department, designation, (Sum(time)/DATEDIFF('day', '{0}'::datetime, '{1}'::datetime))/60 time FROM attendanceindex" +
                            " where action = 'CheckOut' and timeStamp >= '{0}' and timeStamp <= '{1}' and TenantId = '{2}' " +
                            "and department = '" + attendanceFilterData.Department + "' " +
                            "Group By employeeId,action, name, department, designation";
            }
            else
            {
                query = "SELECT employeeId, name, department, designation, (Sum(time)/DATEDIFF('day', '{0}'::datetime, '{1}'::datetime))/60 time FROM attendanceindex" +
                            " where action = 'CheckOut' and timeStamp >= '{0}' and timeStamp <= '{1}' and TenantId = '{2}' " +
                            "Group By employeeId,action, name, department, designation";
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
                            // employeeId, name, department, designation,
                            EmployeeData employeeIndex = new EmployeeData();
                            employeeIndex.EmployeeId = row.FirstOrDefault().As<Guid>();
                            employeeIndex.Name = row.Skip(1).Take(1).FirstOrDefault().As<string>();

                            employeeIndex.Department = row.Skip(2).Take(1).FirstOrDefault().As<string>();
                            employeeIndex.Designation = row.Skip(3).Take(1).FirstOrDefault().As<string>();
                            employeeIndex.Time = row.Skip(4).Take(1).FirstOrDefault().As<decimal>();

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

        public async Task<List<EmployeeData>> DepartmentWiseAverageHoursAsync(AttendanceFilterData attendanceFilterData)
        {
            // pending holidays
            string query = string.Empty;
            if (attendanceFilterData.Department != null)
            {
                query = "SELECT department, (Sum(time)/DATEDIFF('day', '{0}'::datetime, '{1}'::datetime))/60 time FROM attendanceindex" +
                            " where action = 'CheckOut' and timeStamp >= '{0}' and timeStamp <= '{1}' and TenantId = '{2}' " +
                            "and department = '" + attendanceFilterData.Department + "' " +
                            "Group By department,action";
            }
            else
            {
                query = "SELECT department, (Sum(time)/DATEDIFF('day', '{0}'::datetime, '{1}'::datetime))/60 time FROM attendanceindex" +
                            " where action = 'CheckOut' and timeStamp >= '{0}' and timeStamp <= '{1}' and TenantId = '{2}' " +
                            "Group By department,action";
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
                            employeeIndex.Department = row.FirstOrDefault().As<string>();
                            employeeIndex.Time = row.Skip(1).Take(1).FirstOrDefault().As<decimal>();

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

        public async Task<List<ChartDatas>> GetEmployeeLateLoginCount(AttendanceFilterData attendanceFilterData)
        {
            List<ChartDatas> lstChartData = new();


            string query = "SELECT name,count(action) count FROM attendanceindex " +
              " where lateLogin = true and action = 'CheckIn' and timeStamp >= '{0}' and timeStamp <= '{1}' and TenantId = '{2}' group by name";

            Widget widget = new Widget();
            widget.Query = query;
            widget.Period = attendanceFilterData.Period;
            widget.FromDate = attendanceFilterData.FromDate;
            widget.ToDate = attendanceFilterData.ToDate;
            widget.TenantId = Guid.Parse(attendanceFilterData.TenantId.ToString());
            query = _widgetService.GetQuery(widget);

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
                            ChartDatas chartData = new ChartDatas();
                            chartData.Name = row.FirstOrDefault().As<string>();
                            chartData.Value = row.Skip(1).Take(1).FirstOrDefault().As<int>().ToString();
                            lstChartData.Add(chartData);
                        }
                    }

                }
            }
            return lstChartData;
        }

        public async Task<List<ChartDatas>> GetEmployeeEarlyLogoutCount(AttendanceFilterData attendanceFilterData)
        {
            List<ChartDatas> lstChartData = new();

            string query = "SELECT name,count(action) count FROM attendanceindex " +
              " where earlyLogout = true and action = 'CheckOut' and timeStamp >= '{0}' and timeStamp <= '{1}' and TenantId = '{2}' group by name";

            Widget widget = new Widget();
            widget.Query = query;
            widget.Period = attendanceFilterData.Period;
            widget.FromDate = attendanceFilterData.FromDate;
            widget.ToDate = attendanceFilterData.ToDate;
            widget.TenantId = Guid.Parse(attendanceFilterData.TenantId.ToString());
            query = _widgetService.GetQuery(widget);

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
                            ChartDatas chartData = new ChartDatas();
                            chartData.Name = row.FirstOrDefault().As<string>();
                            chartData.Value = row.Skip(1).Take(1).FirstOrDefault().As<int>().ToString();
                            lstChartData.Add(chartData);
                        }
                    }
                }
            }
            return lstChartData;
        }
        public async Task<List<ChartDatas>> DaywiseLateLoginChartAsync(AttendanceFilterData attendanceFilterData)
        {
            // pending holidays
            string query = string.Empty;
            if (attendanceFilterData.Department != null)
            {
                query = "SELECT CAST(timeStamp AS Date) Date, count(action) count FROM attendanceindex" +
                            " where lateLogin = true and action = 'CheckIn' and timeStamp >= '{0}' and timeStamp <= '{1}' and TenantId = '{2}' " +
                            "and department = '" + attendanceFilterData.Department + "' " +
                            "Group By CAST(timeStamp AS Date)";
            }
            else
            {
                query = "SELECT CAST(timeStamp AS Date) Date, count(action) count FROM attendanceindex" +
                            " where lateLogin = true and action = 'CheckIn' and timeStamp >= '{0}' and timeStamp <= '{1}' and TenantId = '{2}' " +
                            "Group By CAST(timeStamp AS Date)";
            }

            Widget widget = new Widget();
            widget.Query = query;
            widget.Period = attendanceFilterData.Period;
            widget.FromDate = attendanceFilterData.FromDate;
            widget.ToDate = attendanceFilterData.ToDate;
            widget.TenantId = Guid.Parse(attendanceFilterData.TenantId.ToString());
            query = _widgetService.GetQuery(widget);

            List<ChartDatas> lstChartData = new();

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
                            ChartDatas chartData = new ChartDatas();
                            chartData.Name = row.FirstOrDefault().As<DateTime>().ToString();
                            chartData.Value = row.Skip(1).Take(1).FirstOrDefault().As<int>().ToString();

                            lstChartData.Add(chartData);
                        }
                    }

                }
                return lstChartData;
            }
   ;
            return lstChartData;
        }

        public async Task<List<ChartData>> DaywiseEarlyLogoutChartAsync(AttendanceFilterData attendanceFilterData)
        {
            // pending holidays
            string query = string.Empty;
            if (attendanceFilterData.Department != null)
            {
                query = "SELECT CAST(timeStamp AS Date) Date, count(action) count FROM attendanceindex" +
                            " where earlyLogout = true and action = 'CheckOut' and timeStamp >= '{0}' and timeStamp <= '{1}' and TenantId = '{2}' " +
                            "and department = '" + attendanceFilterData.Department + "' " +
                            "Group By CAST(timeStamp AS Date)";
            }
            else
            {
                query = "SELECT CAST(timeStamp AS Date) Date, count(action) count FROM attendanceindex" +
                            " where earlyLogout = true and action = 'CheckOut' and timeStamp >= '{0}' and timeStamp <= '{1}' and TenantId = '{2}' " +
                            "Group By CAST(timeStamp AS Date)";
            }

            Widget widget = new Widget();
            widget.Query = query;
            widget.Period = attendanceFilterData.Period;
            widget.FromDate = attendanceFilterData.FromDate;
            widget.ToDate = attendanceFilterData.ToDate;
            widget.TenantId = Guid.Parse(attendanceFilterData.TenantId.ToString());
            query = _widgetService.GetQuery(widget);

            List<ChartData> lstChartData = new();

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
                            ChartData chartData = new ChartData();
                            chartData.Key = row.FirstOrDefault().As<DateTime>().ToString();
                            chartData.Data = row.Skip(1).Take(1).FirstOrDefault().As<int>();

                            lstChartData.Add(chartData);
                        }
                    }

                }
                return lstChartData;
            }
          ;
            return lstChartData;
        }


        public async Task<List<ChartData>> DayNameLateLoginChartAsync(AttendanceFilterData attendanceFilterData)
        {
            // pending holidays
            string query = string.Empty;
            if (attendanceFilterData.Department != null)
            {
                query = "SELECT DAY_NAME(timeStamp) DayName, count(action) count FROM attendanceindex" +
                            " where lateLogin = true and action = 'CheckIn' and timeStamp >= '{0}' and timeStamp <= '{1}' and TenantId = '{2}' " +
                            "and department = '" + attendanceFilterData.Department + "' " +
                            "Group By DAY_NAME(timeStamp) ";
            }
            else
            {
                query = "SELECT DAY_NAME(timeStamp) DayName, count(action) count FROM attendanceindex" +
                            " where lateLogin = true and action = 'CheckIn' and timeStamp >= '{0}' and timeStamp <= '{1}' and TenantId = '{2}' " +
                            "Group By DAY_NAME(timeStamp) ";
            }

            Widget widget = new Widget();
            widget.Query = query;
            widget.Period = attendanceFilterData.Period;
            widget.FromDate = attendanceFilterData.FromDate;
            widget.ToDate = attendanceFilterData.ToDate;
            widget.TenantId = Guid.Parse(attendanceFilterData.TenantId.ToString());
            query = _widgetService.GetQuery(widget);

            List<ChartData> lstChartData = new();

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
                            ChartData chartData = new ChartData();
                            chartData.Key = row.FirstOrDefault().As<string>();
                            chartData.Data = row.Skip(1).Take(1).FirstOrDefault().As<int>();

                            lstChartData.Add(chartData);
                        }
                    }

                }
                return lstChartData;
            }
          ;
            return lstChartData;
        }

        public async Task<List<ChartData>> DayNameEarlyLogoutChartAsync(AttendanceFilterData attendanceFilterData)
        {
            // pending holidays
            string query = string.Empty;
            if (attendanceFilterData.Department != null)
            {
                query = "SELECT DAY_NAME(timeStamp) DayName, count(action) count FROM attendanceindex" +
                            " where earlyLogout = true and action = 'CheckOut' and timeStamp >= '{0}' and timeStamp <= '{1}' and TenantId = '{2}' " +
                            "and department = '" + attendanceFilterData.Department + "' " +
                            "Group By  DAY_NAME(timeStamp)";
            }
            else
            {
                query = "SELECT DAY_NAME(timeStamp) DayName, count(action) count  FROM attendanceindex" +
                            " where earlyLogout = true and action = 'CheckOut' and timeStamp >= '{0}' and timeStamp <= '{1}' and TenantId = '{2}' " +
                            "Group By  DAY_NAME(timeStamp)";
            }

            Widget widget = new Widget();
            widget.Query = query;
            widget.Period = attendanceFilterData.Period;
            widget.FromDate = attendanceFilterData.FromDate;
            widget.ToDate = attendanceFilterData.ToDate;
            widget.TenantId = Guid.Parse(attendanceFilterData.TenantId.ToString());
            query = _widgetService.GetQuery(widget);

            List<ChartData> lstChartData = new();

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
                            ChartData chartData = new ChartData();
                            chartData.Key = row.FirstOrDefault().As<string>();
                            chartData.Data = row.Skip(1).Take(1).FirstOrDefault().As<int>();

                            lstChartData.Add(chartData);
                        }
                    }

                }
                return lstChartData;
            }
          ;
            return lstChartData;
        }
        private void ElasticSearchProduction()
        {

            var clientSettings = new ElasticsearchClientSettings(new Uri("https://5.161.232.13:9200"));
            clientSettings.DisableDirectStreaming(true);
            clientSettings.Authentication(new BasicAuthentication("YOUR_ELASTICSEARCH_USER", "YOUR_ELASTICSEARCH_PASSWORD"))
                  .CertificateFingerprint("YOUR_ELASTICSEARCH_FINGERPRINT");
            _elasticsearchClient = new ElasticsearchClient(clientSettings);

        }
    }
    public class ChartDatas
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
