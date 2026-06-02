using AppStudio.Framework.Core;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc.Builders;
using DevExtreme.AspNet.Mvc;
using Unify.Data;
using Unify.People.Models;

namespace Unify.People.Widgets
{
    public class ChartViewEarlyLogouts : ChartView
    {
        public ChartViewEarlyLogouts()
        {
            ChartType = ChartType.Line;
            Title = "This Week's Early Logouts";
            KeyCaption = "Early Logouts";
            DataCaption = "No. of Employees whose Early Logouts were recorded";

            OnLoadData += ChartViewEarlyLogouts_OnLoadData;
            OnBeforeRender += ChartViewEarlyLogouts_OnBeforeRender;
        }

        private async Task<List<ChartData>> ChartViewEarlyLogouts_OnLoadData()
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
            //       $"     , COUNT(*) AS CountEarlyLogout" +
            //       $"  FROM attendanceindex" +
            //       $" WHERE TenantId = '{TenantId}'";
            //if (!string.IsNullOrEmpty(departmentName)) sql += $"   AND department = '{departmentName}'";
            //sql += $"   AND (dateTime >= '{fromDate}' AND dateTime <= '{toDate}')";
            //sql += $"   AND action = 'CheckOut'" +
            //       $"   AND lastLog = true" +
            //       $" AND (HOUR(DATE_ADD('MINUTE', 330, timeStamp)) < 18 OR (HOUR(DATE_ADD('MINUTE', 330, timeStamp)) = 18 AND MINUTE(DATE_ADD('MINUTE', 330, timeStamp)) <= 30))" +
            //       //$"   AND CAST(timeStamp AS Time) < CAST('18:30:00' AS Time)" +
            //       //$"   AND earlyLogout = true" +
            //       $" GROUP BY CAST(dateTime AS Date) " +
            //       $"HAVING COUNT(*) > 0";


            var peopleDb = CoreServices.GetService<PeopleDb>();
            var query = peopleDb.AttendanceLogs
                .Where(m =>  m.DateTime.Date >= fromDate && m.DateTime.Date <= toDate
                    && m.Action == "CheckOut" && m.LastLog == true && m.EarlyLogout == true);
            if (!string.IsNullOrEmpty(departmentName)) query = query.Where(m => m.Department == departmentName);
            var results = query
                .AsEnumerable()
                //.Where(m =>
                //{
                //    var adjustedTime = m.TimeStamp.AddMinutes(330);
                //    return adjustedTime.Hour < 18 || (adjustedTime.Hour == 18 && adjustedTime.Minute <= 30);
                //})
                .GroupBy(m => new { m.DateTime.Date })
                .Select(m => new EarlyLogoutData
                {
                    DateOnly = DateOnly.FromDateTime(m.Key.Date),
                   // Name = m.Name,
                   // Department = m.Key.Department,
                    CountEarlyLogout = m.Count()
                })
                .Where(m => m.CountEarlyLogout > 0)
                .ToList();


            List<AppStudio.Model.ChartData> chartDatas = new();
            foreach (var data in results)
            {
                AppStudio.Model.ChartData chartData = new();

                chartData.Key = data.DateOnly.Day.ToString() + "/" + data.DateOnly.Month.ToString();
                chartData.Data = data.CountEarlyLogout;
                chartDatas.Add(chartData);
            }

            return chartDatas;
        }

        private async void ChartViewEarlyLogouts_OnBeforeRender(WidgetBuilder widget)
        {
            string jsFunction = @"
                    function onChartPointClick(s, e) {
                        e.processOnServer = false;
                    }";

            ChartBuilder chartBuilder = widget as ChartBuilder;
            chartBuilder.Palette(VizPalette.HarmonyLight)
                .AutoHidePointMarkers(true)
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
