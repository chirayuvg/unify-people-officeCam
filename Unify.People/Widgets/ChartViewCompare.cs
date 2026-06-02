using AppStudio.Framework.Core;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc.Builders;
using DevExtreme.AspNet.Mvc;
using Unify.Data;
using Unify.People.Models;

namespace Unify.People.Widgets
{
    public class ChartViewCompare : ChartView
    {
        public ChartViewCompare()
        {
            ChartType = ChartType.Bar;
            Title = "Gender Wise Present";
            KeyCaption = "Avg Working Hours";
            DataCaption = "Male";
            Data1Caption = "Female";
            OnLoadData += ChartViewCompare_OnLoadData;
            OnBeforeRender += ChartViewCompare_OnBeforeRender;
        }

        private async Task<List<ChartData>> ChartViewCompare_OnLoadData()
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
                .Where(a => a.Action == "CheckIn" && a.DateTime.Date >= fromDate && a.DateTime.Date <= toDate);

            if (!string.IsNullOrEmpty(departmentName)) query = query.Where(m => m.Department == departmentName);
            
            var results = query
                .GroupBy(a => a.DateTime.Date)
                .Select(g => new
                {
                    EntryDate = g.Key,
                    MaleCount = g.Count(x => x.Gender == "Male"),
                    FemaleCount = g.Count(x => x.Gender == "Female")
                    // Add more genders if needed
                })
                .OrderBy(r => r.EntryDate)
                .ToList();


            List<AppStudio.Model.ChartData> chartDatas = new();
            foreach (var value in results)
            {
                AppStudio.Model.ChartData chartData = new();

                chartData.Key = value.EntryDate.ToShortDateString();
                chartData.Data = value.MaleCount;
                chartData.Data1 = value.FemaleCount;
                chartDatas.Add(chartData);
            }

            return chartDatas;
        }

        private async void ChartViewCompare_OnBeforeRender(DevExtreme.AspNet.Mvc.Builders.WidgetBuilder widget)
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
