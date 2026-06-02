using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Data;
using Unify.People.Models;
using Unify.People.Widgets;
using Unify.Data;
using Unify.Data.SQL;

namespace Unify.People.Controllers.Attendance
{
    public class STRDashboardController : PageController
    {
        private CardView<DashboardFilterData> cardView;

        private ChartViewDailyPresnt chartViewDailyPresnt;

        private ChartViewCompare chartViewCompare;

        private GridViewLeaderboardWorkingDays<LeaderboardAverageWorking> GridViewLeaderboardWorkingDays;

       
        public override void Init()
        {
            this.Page.Title = "Live Analytics";
            this.Page.ShowScrollBar = true;

            var kpiLoggedIn = new KPIStrLoggedIn().SetId("kpiLoggedIn");
            kpiLoggedIn.Data.Value = "-";
            kpiLoggedIn.Data.Color = KpiColor.SeaGreen;
            kpiLoggedIn.Data.Icon = "fa-solid fa-right-to-bracket";

            var kpiEarlyLogOuts = new KPIStrLoggedOut().SetId("kpiLogOuts");
            kpiEarlyLogOuts.Data.Value = "-";
            kpiEarlyLogOuts.Data.Color = KpiColor.Orange;
            kpiEarlyLogOuts.Data.Icon = "fa-solid fa-door-open";

            var kpiStrWeekly = new KPIStrWeekly().SetId("kpiStrWeekly");
            kpiStrWeekly.Data.Value = "-";
            kpiStrWeekly.Data.Color = KpiColor.Red;
            kpiStrWeekly.Data.Icon = "fa-solid fa-clock";

            var kpiStrMonthly = new KPIStrMonthly().SetId("kpiStrMonthly");
            kpiStrMonthly.Data.Value = "-";
            kpiStrMonthly.Data.Color = KpiColor.RoyalBlue;
            kpiStrMonthly.Data.Icon = "fa-solid fa-people-group";

            this.AddColumns().Controls(c =>
            {
                c.AddControl(kpiLoggedIn);
                c.AddControl(kpiEarlyLogOuts);
                c.AddControl(kpiStrWeekly);
                c.AddControl(kpiStrMonthly);
            });

            cardView = this.AddCardView<DashboardFilterData>().SetControlStyle(CardViewStyle.Filter);
            cardView.Title = "Trends";
            cardView.ControlStyle = CardViewStyle.Filter;
            cardView.SubmitOnDataChange = true;
            cardView.LabelPosition = LabelPosition.Left;
            cardView.GetFieldControl(m => m.DepartmentId).OnLoadLookupData += Department_OnLoadLookupData;
            cardView.GetFieldControl(m => m.DepartmentName).IsVisible = false;
            cardView.OnSubmit += CardView_OnSubmit;
            chartViewDailyPresnt = this.AddControl<ChartViewDailyPresnt>();
            chartViewCompare = this.AddControl<ChartViewCompare>();
            GridViewLeaderboardWorkingDays = this.AddControl<GridViewLeaderboardWorkingDays<LeaderboardAverageWorking>>();
        }
       
        private async Task<DevExtreme.AspNet.Data.ResponseModel.LoadResult> Department_OnLoadLookupData(DevExtreme.AspNet.Mvc.DataSourceLoadOptions loadOptions)
        {
            var _peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            var departments = _peopleDb.Departments.ToList();
            departments.Add(new Department() { Id = Guid.Empty, Name = "--All--" });
            LoadResult loadResult = DataSourceLoader.Load(departments, loadOptions);

            return loadResult;
        }
        private async Task CardView_OnSubmit(DashboardFilterData model, IFormFileCollection files)
        {
            model.GetDates();
            model.GetDepartmentName();
        }
    }
}
