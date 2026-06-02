using AppStudio.Enterprise.Data;
using AppStudio.Framework.Core;
using AppStudio.Model;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using Unify.Data;
using Unify.People.Models;
using Unify.People.Services;

namespace Unify.People.Widgets
{
    public class GridViewEmployeeWiseMyMeetings<TModel> : GridView<TModel> where TModel : class, new()
    {
        private EmployeeService _employeeService;

        public GridViewEmployeeWiseMyMeetings()
        {
            OnLoadData += GridViewEmployeeWiseMyMeetings_OnLoadData;
            Toolbar.Title = "This Week's My Meetings";
        }

        public void SetEmployeeService(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        private async Task<LoadResult> GridViewEmployeeWiseMyMeetings_OnLoadData(DataSourceLoadOptions loadOptions, string? parentId)
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

            var UserId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetUserId();
            TenantUser tenantUser = await Entity.FindByIdAsync<TenantUser>(UserId);
            var employee = await _employeeService.GetEmployeeByEmailAsync(TenantId, tenantUser.Email);

            var peopleDb = CoreServices.GetService<PeopleDb>();
            var results = peopleDb.Appointments
                .Where(m =>
                    //a.TenantId == TenantId
                    m.AppointmentDate >= DateOnly.FromDateTime(fromDate)
                    &&
                    m.AppointmentDate <= DateOnly.FromDateTime(toDate)
                    &&
                    m.EmployeeId == employee.Id
                )
                .OrderBy(m => m.AppointmentDate)
                .ToList();

            var loadResult = DataSourceLoader.Load(results, loadOptions);

            return loadResult;
        }
    }
}
