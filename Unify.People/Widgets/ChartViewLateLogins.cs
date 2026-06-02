using AppStudio.Framework.Core;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using Unify.Data;
using Unify.People.Models;

namespace Unify.People.Widgets
{
    public class ChartViewLateLogins : ChartView
    {
        public string? Department { get; set; } = "";
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public Period Period { get; set; }

        public ChartViewLateLogins()
        {
            ChartType = ChartType.Line;
            Title = "This Week's Late Logins";
            KeyCaption = "Late Logins";
            DataCaption = "No. of Employees whose Late Logins were recorded";

            OnLoadData += ChartViewLateLogins_OnLoadData;
            OnBeforeRender += ChartViewLateLogins_OnBeforeRender;
        }

        private async Task<List<ChartData>> ChartViewLateLogins_OnLoadData()
        {
            var cardView = this.Page.FindControl<CardView<DashboardFilterData>>();
            var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();
            DateTime fromDate =
                (cardView.Data.FromDate == DateOnly.MinValue)
                ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
                : DateTime.Parse(cardView.Data.FromDate.ToString("yyyy-MM-dd"));
            DateTime toDate =
                (cardView.Data.ToDate == DateOnly.MinValue)
                ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
                : DateTime.Parse(cardView.Data.ToDate.ToString("yyyy-MM-dd"));
            string departmentName = (cardView.Data.DepartmentName == null || cardView.Data.DepartmentName == "") ? "" : cardView.Data.DepartmentName;


            //string sql =
            //       $"SELECT CAST(dateTime AS Date) AS DateOnly" +
            //       $"     , COUNT(*) AS CountLateLogin" +
            //       $"  FROM attendanceindex" +
            //       $" WHERE TenantId = '{TenantId}'";
            //if (!string.IsNullOrEmpty(departmentName)) sql += $"   AND department = '{departmentName}'";
            //sql += $"   AND (dateTime >= '{fromDate}' AND dateTime <= '{toDate}')";
            //sql += $"   AND action = 'CheckIn'" +
            //       $"   AND firstLogin = true" +
            //       $"   AND (HOUR(DATE_ADD('HOUR', 5, DATE_ADD('MINUTE', 30, timeStamp))) > 9 OR (HOUR(DATE_ADD('HOUR', 5, DATE_ADD('MINUTE', 30, timeStamp))) = 9 AND MINUTE(DATE_ADD('HOUR', 5, DATE_ADD('MINUTE', 30, timeStamp))) >= 30))" +
            //       //$"   AND startTime > '09:30:00'" +
            //       $" GROUP BY CAST(dateTime AS Date) " +
            //       $"HAVING COUNT(*) > 0";


            var peopleDb = CoreServices.GetService<PeopleDb>();
            var query = peopleDb.AttendanceLogs
                .Where(m => //m.TenantId == TenantId
                     m.DateTime.Date >= fromDate.Date
                    && m.DateTime.Date <= toDate.Date
                    && m.Action == "CheckIn"
                    && m.FirstLogin == true);

            if (!string.IsNullOrEmpty(departmentName)) query = query.Where(m => m.Department == departmentName);

            var results = query
                .AsEnumerable()
                .Where(m =>
                {
                    var adjustedTime = m.TimeStamp.AddHours(5).AddMinutes(30);
                    return adjustedTime.Hour > 9 || (adjustedTime.Hour == 9 && adjustedTime.Minute >= 30);
                })
                .GroupBy(m => m.DateTime.Date)
                .Select(m => new LateLoginData
                {
                    DateOnly = DateOnly.FromDateTime(m.Key),
                    CountLateLogin = m.Count()
                })
                .Where(m => m.CountLateLogin > 0)
                .ToList();


            List<AppStudio.Model.ChartData> chartDatas = new();
            foreach (var data in results)
            {
                AppStudio.Model.ChartData chartData = new();

                chartData.Key = data.DateOnly.Day.ToString() + "/" + data.DateOnly.Month.ToString();
                chartData.Data = data.CountLateLogin;
                chartDatas.Add(chartData);
            }

            return chartDatas;
        }

        private async void ChartViewLateLogins_OnBeforeRender(WidgetBuilder widget)
        {
            string jsFunction = @"
                    function onChartPointClick(s, e) {
                        e.processOnServer = false;
                    }";

            ChartBuilder chartBuilder = widget as ChartBuilder;
            chartBuilder
                .Palette(VizPalette.Carmine)
                .Export(e => e.Enabled(true))
                //.Margin(m => m.Bottom(20))
                .AutoHidePointMarkers(true)
                //.CommonAxisSettings(c => c.AllowDecimals(true).EndOnTick(true))
                .Tooltip(t => t.Enabled(true))
                .Crosshair(c => c.Enabled(true))
                .Tooltip(t => t.Enabled(true))
                .OnPointClick(jsFunction);

            //chartBuilder.Margin(m => m.Bottom(20));
            //chartBuilder.AutoHidePointMarkers(new JS("10"));
            //chartBuilder.CommonAxisSettings(c => c.DiscreteAxisDivisionMode(DiscreteAxisDivisionMode.BetweenLabels).Color("#FF00FF"));
            // chartBuilder.ContainerBackgroundColor(new JS(@"#FF0000"));
            //chartBuilder.CommonSeriesSettings(c => c.ValueField(Name).Color("blue"));
            //    chartBuilder.ArgumentAxis(a => a
            //    .ValueMarginsEnabled(false)
            //    //.DiscreteAxisDivisionMode(DiscreteAxisDivisionMode.CrossLabels)
            //    .Grid(g => g.Visible(false))
            //);
            //chartBuilder.Margin(m => m.Bottom(20));
            //chartBuilder.AutoHidePointMarkers(new JS("10"));
            //chartBuilder.CommonAxisSettings(c => c.DiscreteAxisDivisionMode(DiscreteAxisDivisionMode.BetweenLabels).Color("#FF00FF"));
            // chartBuilder.ContainerBackgroundColor(new JS(@"#FF0000"));
            //chartBuilder.CommonSeriesSettings(c => c.ValueField(Name).Color("blue"));
            //    chartBuilder.ArgumentAxis(a => a
            //    .ValueMarginsEnabled(false)
            //    //.DiscreteAxisDivisionMode(DiscreteAxisDivisionMode.CrossLabels)
            //    .Grid(g => g.Visible(false))
            //);
        }
    }
}
