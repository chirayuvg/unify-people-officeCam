using AppStudio.Framework.Core;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc.Builders;
using DevExtreme.AspNet.Mvc;
using Unify.Data;
using Unify.People.Models;

namespace Unify.People.Widgets
{
    public class ChartViewDailyPresnt : ChartView
    {
        public ChartViewDailyPresnt()
        {
            ChartType = ChartType.Line;
            Title = "This Week's Employee Present";
            KeyCaption = "Employee Present";
            DataCaption = "Employee Present";
            OnLoadData += ChartViewAverageWorkingHours_OnLoadData;
            OnBeforeRender += ChartViewDailyPresnt_OnBeforeRender;
        }

        private async Task<List<ChartData>> ChartViewAverageWorkingHours_OnLoadData()
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
            string? departmentName = (cardView.Data.DepartmentName == null) ? null : cardView.Data.DepartmentName;
            var peopleDb = CoreServices.GetService<PeopleDb>();
            var query = peopleDb.AttendanceLogs
                .Where(m => m.DateTime.Date >= fromDate && m.DateTime.Date <= toDate && m.Action=="CheckIn");
            if (!string.IsNullOrEmpty(departmentName)) query = query.Where(m => m.Department == departmentName);
            var results = query
                .GroupBy(a => a.Department)
                .Select(group => new
                {
                    Department = group.Key,  // Grouped department
                    DistinctEntryCount = group.Select(a => a.EmployeeId).Distinct().Count()  // Count distinct EmployeeIds
                })
                .ToList();


            List<AppStudio.Model.ChartData> chartDatas = new();
            foreach (var value in results)
            {
                AppStudio.Model.ChartData chartData = new();

                chartData.Key = value.Department;
                chartData.Data = value.DistinctEntryCount;
                chartDatas.Add(chartData);
            }

            return chartDatas;
        }

        private async void ChartViewDailyPresnt_OnBeforeRender(DevExtreme.AspNet.Mvc.Builders.WidgetBuilder widget)
        {
            string jsFunction = @"
                    function onChartPointClick(s, e) {
                        e.processOnServer = false;
                    }";

            ChartBuilder chartBuilder = widget as ChartBuilder;
            chartBuilder
                .Palette(VizPalette.Carmine)
                .Export(e => e.Enabled(true))
                .AutoHidePointMarkers(true)
                .Tooltip(t => t.Enabled(true))
                .Crosshair(c => c.Enabled(true))
                .Tooltip(t => t.Enabled(true))
                .OnPointClick(jsFunction);
        }
    }
}
  
