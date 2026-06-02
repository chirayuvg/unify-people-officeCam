using AppStudio.Framework.Core;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using Unify.Data;
using Unify.Data.SQL;
using Unify.People.Models;

namespace Unify.People.Controllers.Visitors
{
    public class VisitorDashboardController : PageController
    {
        public override void Init()
        {
            this.Page.Title = "Live Analytics";
            this.Page.ShowScrollBar = true;

            var kpiTodaysVisitors = new KpiView().SetId("kpiTodaysVisitors");
            kpiTodaysVisitors.Data.Value = "-";
            kpiTodaysVisitors.Data.Color = KpiColor.SeaGreen;
            kpiTodaysVisitors.Data.Icon = "fa-solid fa-right-to-bracket";
            kpiTodaysVisitors.OnLoadData += KpiTodaysVisitors_OnLoadData;
        }

        private async Task<KpiModel> KpiTodaysVisitors_OnLoadData()
        {
            var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();
            DateTime toDay = DateTime.Now;

            var peopleDb = CoreServices.GetService<PeopleDb>();
            var count = peopleDb.VisitorLogs
                .Where(m => m.DateTime.Date == toDay.Date)
                .Distinct()
                .Count();
            var result = new AggregateIntegerData { Value = count };

            KpiModel kpiModel = new();
            kpiModel.Title = "Logged In (Today)";
            kpiModel.Value = (result == null) ? "0" : result.Value.ToString();
            kpiModel.Color = KpiColor.SeaGreen;
            kpiModel.Icon = "fa-solid fa-right-to-bracket";

            return kpiModel;
        }
    }

    public class VisitorFilterData
    {
        public Guid? TenantId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public Period Period { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public VisitorFilterData()
        {
            FromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            ToDate = DateTime.Today.AddDays(1);
        }
    }

    //public class VisitorDashboardPage : PageModel
    //{
    //    public KpiView TodaysVisitors { get; set; } = new();
    //    public ChartView WeeklyVisitors { get; set; } = new();
    //    public GridView<VisitorsFrequency> VisitorsFrequencyGrid { get; set; } = new();
    //    public ChartView VisitorsFrequencyChart { get; set; } = new();

    //    public CardView<VisitorFilterData> CardView { get; set; } = new();

    //    public VisitorDashboardPage()
    //    {
    //        this.Layout = GroupLayout.Grid;
    //        this.OnPageLoad += Page_OnPageLoad;

    //        this.CardView.ControlStyle = CardViewStyle.Filter;
    //        var department = this.CardView.GetFieldControl(m => m.Department);
    //        List<string> departments = new();
    //        departments = null; // _attendanceDb.Departments.Select(m => m.Name).ToList();
    //        if (departments != null)
    //        {
    //            department.SetDataSource(departments);
    //            //department.SetLookupDataSource(departments);
    //        }
    //        var dayType = this.CardView.GetFieldControl(m => m.Period);

    //        #region KPIs
    //        this.TodaysVisitors.IsRefresh = false;
    //        this.TodaysVisitors.Style.Row = 0;
    //        this.TodaysVisitors.Style.Column = 0;
    //        this.TodaysVisitors.Style.ColumnSpan = 2;
    //        this.TodaysVisitors.OnLoadData += TodaysVisitors_OnLoadData;
    //        #endregion KPIs

    //        #region CardView
    //        this.CardView.Style.Row = 1;
    //        this.CardView.Style.Column = 0;
    //        this.CardView.Style.ColumnSpan = 4;
    //        #endregion CardView

    //        #region WeeklyVisitors
    //        this.WeeklyVisitors.Style.Row = 2;
    //        this.WeeklyVisitors.Style.Column = 0;
    //        this.WeeklyVisitors.Style.ColumnSpan = 4;
    //        this.WeeklyVisitors.ChartType = ChartType.Bar;
    //        this.WeeklyVisitors.OnLoadData += WeeklyVisitors_OnLoadData;
    //        #endregion WeeklyVisitors

    //        #region Visitors Frequency
    //        this.VisitorsFrequencyGrid.Style.Row = 3;
    //        this.VisitorsFrequencyGrid.Style.Column = 0;
    //        this.VisitorsFrequencyGrid.Style.ColumnSpan = 2;
    //        this.VisitorsFrequencyGrid.OnLoadData += VisitorsFrequencyGrid_OnLoadData;

    //        this.VisitorsFrequencyChart.Style.Row = 3;
    //        this.VisitorsFrequencyChart.Style.Column = 1;
    //        this.VisitorsFrequencyChart.Style.ColumnSpan = 2;
    //        this.VisitorsFrequencyChart.ChartType = ChartType.Bar;
    //        this.VisitorsFrequencyChart.OnLoadData += VisitorsFrequencyChart_OnLoadData;
    //        #endregion Visitors Frequency
    //    }

    //    private async Task Page_OnPageLoad()
    //    {
    //        var fromDate = this.CardView.GetFieldControl(m => m.FromDate);
    //        var toDate = this.CardView.GetFieldControl(m => m.ToDate);
    //        var period = CardView.Data.Period.ToString();

    //        if (period != "CustomDateRange")
    //        {
    //            fromDate.IsVisible = false;
    //            toDate.IsVisible = false;
    //        }
    //    }

    //    private async Task<KpiModel> TodaysVisitors_OnLoadData()
    //    {
    //        KpiModel kpiModel = new();
    //        kpiModel.Title = "Today's Visitors";
    //        kpiModel.Value = "50";

    //        return kpiModel;
    //    }

    //    private async Task<List<ChartData>> WeeklyVisitors_OnLoadData()
    //    {
    //        List<AppStudio.Model.ChartData> chartDatas = new();

    //        AppStudio.Model.ChartData chartData = new();
    //        chartData.Key = $"Week 1";
    //        chartData.Data = 548;
    //        chartDatas.Add(chartData);

    //        chartData = new();
    //        chartData.Key = $"Week 2";
    //        chartData.Data = 386;
    //        chartDatas.Add(chartData);

    //        chartData = new();
    //        chartData.Key = $"Week 3";
    //        chartData.Data = 673;
    //        chartDatas.Add(chartData);

    //        chartData = new();
    //        chartData.Key = $"Week 4";
    //        chartData.Data = 463;
    //        chartDatas.Add(chartData);

    //        return chartDatas;
    //    }

    //    private async Task<DevExtreme.AspNet.Data.ResponseModel.LoadResult> VisitorsFrequencyGrid_OnLoadData(DevExtreme.AspNet.Mvc.DataSourceLoadOptions loadOptions, string? parentId)
    //    {
    //        List<VisitorsFrequency> gridDatas = new();

    //        VisitorsFrequency gridData = new();
    //        gridData.VisitorName = $"Dilip Patra";
    //        gridData.Frequency = 50;
    //        gridDatas.Add(gridData);

    //        gridData = new();
    //        gridData.VisitorName = $"Abhishek Singh";
    //        gridData.Frequency = 30;
    //        gridDatas.Add(gridData);

    //        gridData = new();
    //        gridData.VisitorName = $"Vaibhav Bhardwaj";
    //        gridData.Frequency = 60;
    //        gridDatas.Add(gridData);

    //        gridData = new();
    //        gridData.VisitorName = $"Prabhu TRS";
    //        gridData.Frequency = 40;
    //        gridDatas.Add(gridData);

    //        LoadResult loadResult = new LoadResult();
    //        loadResult.data = gridDatas;

    //        return loadResult;
    //    }

    //    private async Task<List<ChartData>> VisitorsFrequencyChart_OnLoadData()
    //    {
    //        List<AppStudio.Model.ChartData> chartDatas = new();

    //        AppStudio.Model.ChartData chartData = new();
    //        chartData.Key = $"Dilip Patra";
    //        chartData.Data = 50;
    //        chartDatas.Add(chartData);

    //        chartData = new();
    //        chartData.Key = $"Abhishek Singh";
    //        chartData.Data = 30;
    //        chartDatas.Add(chartData);

    //        chartData = new();
    //        chartData.Key = $"Vaibhav Bhardwaj";
    //        chartData.Data = 60;
    //        chartDatas.Add(chartData);

    //        chartData = new();
    //        chartData.Key = $"Prabhu TRS";
    //        chartData.Data = 40;
    //        chartDatas.Add(chartData);

    //        return chartDatas;
    //    }
    //}

    //public class VisitorsFrequency
    //{
    //    public string VisitorName { get; set; }
    //    public int Frequency { get; set; }
    //}
}
