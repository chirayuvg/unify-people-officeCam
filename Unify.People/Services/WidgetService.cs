using AppStudio.Model;
using Elastic.Clients.Elasticsearch.Sql;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Unify.People.Models;

namespace Unify.People.Services
{
    public class WidgetService
    {
        private ElasticsearchClient _elasticsearchClient;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public WidgetService()
        {

        }

        //public async Task<List<DashboardWidget>> GetDashboardWidgets(Guid dashboardId)
        //{
        //    var widgetList = await _wdb.DashboardWidgets.Include(m => m.Widget).Where(m => m.DashboardId == dashboardId).ToListAsync();
        //    return widgetList;
        //}

        public async Task<KpiModel> GetKpiDataAsync(Widget widget)
        {
            string query = GetQuery(widget);

            var kpi = new KpiModel();
            if (!string.IsNullOrEmpty(query))
            {
                var lst = GetElasticKpiCount(query);
                //var lst = await _edb.GetAllAsync<KpiModel>(query);
                kpi = lst.FirstOrDefault();
                kpi.Title = widget.Title;
            }
            return kpi;
        }

        //public async Task<GaugeModel> GetGuageDataAsync(Widget widget)
        //{
        //    string query = GetQuery(widget);

        //    var gauge = new GaugeModel();
        //    if (!string.IsNullOrEmpty(query))
        //    {
        //        var lst = await _edb.GetAllAsync<GaugeModel>(query);
        //        gauge = lst.FirstOrDefault();


        //    }
        //    return gauge;
        //}
        public async Task<List<GridModel>> GetGridDataAsync(Widget widget)
        {
            string query = GetQuery(widget);

            var gauge = new List<GridModel>();
            if (!string.IsNullOrEmpty(query))
            {
                var lst = GetElasticGrid(query);
                return lst;
            }
            return gauge;
        }

        public async Task<List<ChartData>> GetChartDataAsync(Widget widget, ChartView chart)
        {
            List<ChartData> lst = new List<ChartData>();

            string query = GetQuery(widget);

            if (!string.IsNullOrEmpty(query))
            {
                lst = GetElasticChartCount(query);
                chart.DataCaption = "Count";
            }
            return lst;
        }

        public string GetQuery(Widget widget)
        {
            if (widget == null) return null;
            string query = widget.Query;
            DateTime fromDate = DateTime.Today;
            DateTime toDate = DateTime.Today;

            switch (widget.Period)
            {
                case Period.ThisWeek:
                    fromDate = fromDate.AddDays(-(int)fromDate.DayOfWeek + (int)DayOfWeek.Monday);
                    toDate = fromDate.AddDays(6);
                    break;
                case Period.ThisMonth:
                    fromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
                case Period.LastMonth:
                    fromDate = new DateTime(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
                case Period.CustomDateRange:
                    fromDate = DateTime.Parse(widget.FromDate.ToString());
                    toDate = DateTime.Parse(widget.ToDate.ToString());
                    break;
                default:
                    break;
            }

            query = string.Format(query, fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd"), widget.TenantId.ToString());
            return query;
        }
        public string GetQueryPrev(Widget widget)
        {
            if (widget == null) return null;
            string query = widget.Query;
            DateTime fromDate = DateTime.Today;
            DateTime toDate = DateTime.Today;

            switch (widget.Period)
            {
                case Period.ThisWeek:
                    fromDate = fromDate.AddDays(-(int)fromDate.DayOfWeek + (int)DayOfWeek.Monday);
                    toDate = fromDate.AddDays(6);
                    break;
                case Period.ThisMonth:
                    fromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
                case Period.LastMonth:
                    fromDate = new DateTime(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
                case Period.CustomDateRange:
                    fromDate = DateTime.Parse(FromDate.ToString());
                    toDate = DateTime.Parse(ToDate.ToString());
                    break;
                default:
                    break;
            }

            query = string.Format(query, fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd"), widget.TenantId.ToString());
            return query;
        }

        public List<KpiModel> GetElasticKpiCount(string sqlQuery)
        {
            decimal count = 0;
            List<KpiModel> charts = new List<KpiModel>();
            try
            {
                ElasticSearch();
                QueryResponse response = _elasticsearchClient.Sql.Query(m => m.Query(sqlQuery));

                if (!response.IsValidResponse)
                {
                    var debugInfo = response.DebugInformation;
                    var error = response.ElasticsearchServerError.Error;
                }
                if (response.IsValidResponse)
                {
                    if (response.Rows != null)
                    {
                        var row = response.Rows.FirstOrDefault();

                        KpiModel kpi = new KpiModel();
                        foreach (var val in row)
                        {
                            kpi.Value = row.FirstOrDefault().As<int>().ToString();
                        }
                        charts.Add(kpi);

                    }
                }
            }
            catch (Exception ex)
            {
                var ex1 = ex.Message;
            }
            return charts;
        }

        public List<ChartData> GetElasticChartCount(string sqlQuery)
        {
            decimal count = 0;
            List<ChartData> charts = new List<ChartData>();
            try
            {
                ElasticSearch();
                QueryResponse response = _elasticsearchClient.Sql.Query(m => m.Query(sqlQuery));
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
                                ChartData chart = new ChartData();
                                chart.Key = row.FirstOrDefault().As<string>();
                                chart.Data = row.Skip(1).Take(1).FirstOrDefault().As<int>();
                                charts.Add(chart);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var ex1 = ex.Message;
            }
            return charts;
        }
        public List<GridModel> GetElasticGrid(string sqlQuery)
        {
            decimal count = 0;
            List<GridModel> charts = new List<GridModel>();
            try
            {
                ElasticSearch();
                QueryResponse response = _elasticsearchClient.Sql.Query(m => m.Query(sqlQuery));
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
                                GridModel chart = new GridModel();
                                chart.Title = row.FirstOrDefault().As<string>();
                                chart.Value = row.Skip(1).Take(1).FirstOrDefault().As<string>();
                                charts.Add(chart);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var ex1 = ex.Message;
            }
            return charts;
        }
        private void ElasticSearch()
        {
            //var clientSettings = new ElasticsearchClientSettings(new Uri("https://5.161.231.88:9200"));
            //clientSettings.DisableDirectStreaming(true);
            //clientSettings.Authentication(new BasicAuthentication("YOUR_ELASTICSEARCH_USER", "YOUR_ELASTICSEARCH_PASSWORD"))
            //      .CertificateFingerprint("YOUR_ELASTICSEARCH_FINGERPRINT");
            //_elasticsearchClient = new ElasticsearchClient(clientSettings);

            var clientSettings = new ElasticsearchClientSettings(new Uri("https://5.161.232.13:9200"));
            clientSettings.DisableDirectStreaming(true);
            clientSettings.Authentication(new BasicAuthentication("YOUR_ELASTICSEARCH_USER", "YOUR_ELASTICSEARCH_PASSWORD"))
                  .CertificateFingerprint("YOUR_ELASTICSEARCH_FINGERPRINT");
            _elasticsearchClient = new ElasticsearchClient(clientSettings);
        }

        public class GridModel
        {
            public string Title { get; set; } = string.Empty;


            public string Value { get; set; } = string.Empty;

        }
    }
}
