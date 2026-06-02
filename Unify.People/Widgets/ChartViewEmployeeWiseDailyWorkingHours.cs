using AppStudio.Enterprise.Data;
using AppStudio.Framework.Core;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using Unify.Data;
using Unify.People.Models;
using Unify.People.Services;

namespace Unify.People.Widgets
{
    public class ChartViewEmployeeWiseDailyWorkingHours: ChartView
    {

        public ChartViewEmployeeWiseDailyWorkingHours()
        {
            ChartType = ChartType.Line;
            Title = "This Week's Daily Working Hours";
            KeyCaption = "Daily Working Hours";
            DataCaption = "Daily Working Hours";
            OnLoadData += ChartViewEmployeeWiseDailyWorkingHours_OnLoadData;
            OnBeforeRender += ChartViewEmployeeWiseDailyWorkingHours_OnBeforeRender;
        }

        private void ChartViewEmployeeWiseDailyWorkingHours_OnBeforeRender(DevExtreme.AspNet.Mvc.Builders.WidgetBuilder widget)
        {
            string jsFunction = @"
                    function onChartPointClick(s, e) {
                        e.processOnServer = false;
                    }";

            ChartBuilder chartBuilder = widget as ChartBuilder;
            chartBuilder
                .Palette(VizPalette.DarkMoon)
                .Margin(m => m.Bottom(20))
                .ArgumentAxis(a => a
                    .ValueMarginsEnabled(true)
                    .DiscreteAxisDivisionMode(DiscreteAxisDivisionMode.BetweenLabels)
                    .Grid(g => g.Visible(false))
                     .Label(c => c.Format(Format.Decimal))       // Set label format to fixed-point with 2 decimal places
                )
                .CommonAxisSettings(c => c.AllowDecimals(true).ValueMarginsEnabled(true))
                //.ValueAxis(c => c.Add().Label(c => c.Format(Format.Decimal)))
                //.CommonSeriesSettings(c => c.Label(c => c.Format(Format.Decimal)))
                .Legend(l => l
                    .VerticalAlignment(VerticalEdge.Bottom)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .ItemTextPosition(DevExtreme.AspNet.Mvc.Position.Bottom)
                )
                .Crosshair(t => t.Enabled(true))
                .Export(e => e.Enabled(true))
                .Tooltip(t => t.Enabled(true))
                .ValueAxis(m => m.Add()
                    .AggregatedPointsPosition(AggregatedPointsPosition.CrossTicks)
                    .AllowDecimals(true)
                    .ConstantLines(c => c.Add()
                        .Color("#fc3535")
                        .DashStyle(DevExtreme.AspNet.Mvc.DashStyle.Dash)
                        .Label(l => l
                            .Visible(true)
                            .HorizontalAlignment(DevExtreme.AspNet.Mvc.HorizontalAlignment.Right)
                            .Font(f => f
                                .Color("#fc3535")
                                .Size(15)
                                .Weight(500)
                            )
                            .Text("9 hours")
                        )
                        .Value(9)
                        .Width(2)
                    )
                )
                .OnPointClick(jsFunction);
        }

        private async Task<List<ChartData>> ChartViewEmployeeWiseDailyWorkingHours_OnLoadData()
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
            var UserId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetUserId();
            TenantUser tenantUser = await Entity.FindByIdAsync<TenantUser>(UserId);
            var _employeeService = AppStudio.Framework.Core.CoreServices.GetService<EmployeeService>();
            var employee = await _employeeService.GetEmployeeByEmailAsync(TenantId, tenantUser.Email);

            var peopleDb = CoreServices.GetService<PeopleDb>();
            var query = peopleDb.AttendanceLogs
                .Where(m => m.DateTime.Date >= fromDate && m.DateTime.Date <= toDate);

            if (!string.IsNullOrEmpty(departmentName)) query = query.Where(m => m.Department == departmentName);
            if (employee != null && tenantUser.Name != "Admin") query = query.Where(m => m.EmployeeId == employee.Id);

            var results = query
                .GroupBy(m => m.DateTime.Date)
                .Select(m => new AverageData
                {
                    DateOnly = DateOnly.FromDateTime(m.Key),
                    Average = Math.Round(
                        (decimal)(m.Max(x => x.TimeStamp) - m.Min(x => x.TimeStamp)).TotalSeconds / 3600m, 2)
                }).OrderByDescending(m=>m.DateOnly)
                .ToList();

            List<AppStudio.Model.ChartData> chartDatas = new();
            foreach (var value in results)
            {
                AppStudio.Model.ChartData chartData = new();

                chartData.Key = value.DateOnly.Day.ToString() + "/" + value.DateOnly.Month.ToString();
                chartData.Data = value.Average;
                chartDatas.Add(chartData);
            }

            return chartDatas;
        }
    }
}
