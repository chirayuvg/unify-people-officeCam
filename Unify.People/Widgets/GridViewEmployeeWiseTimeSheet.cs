using AppStudio.Enterprise.Data;
using AppStudio.Framework.Core;
using AppStudio.Model;
using DevExtreme.AspNet.Data;
using Unify.Data;
using Unify.People.Models;
using Unify.People.Services;

namespace Unify.People.Widgets
{
    public class GridViewEmployeeWiseTimeSheet<TModel> : GridView<TModel> where TModel : class, new ()
    {
        public GridViewEmployeeWiseTimeSheet()
        {
            OnLoadData += GridViewEmployeeWiseTimeSheet_OnLoadData;
            Toolbar.Title = "This Week's Late Logins and Early Logouts";
        }

        private async Task<DevExtreme.AspNet.Data.ResponseModel.LoadResult> GridViewEmployeeWiseTimeSheet_OnLoadData(DevExtreme.AspNet.Mvc.DataSourceLoadOptions loadOptions, string? parentId)
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
            var UserId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetUserId();
            TenantUser tenantUser = await Entity.FindByIdAsync<TenantUser>(UserId);
            var _employeeService = AppStudio.Framework.Core.CoreServices.GetService<EmployeeService>();
            var employee = await _employeeService.GetEmployeeByEmailAsync(TenantId, tenantUser.Email);

            var peopleDb = CoreServices.GetService<PeopleDb>();

            List<EmployeeTimeSheet> results = peopleDb.AttendanceLogs
                .Where(a => a.EmployeeId == employee.Id
                         && a.DateTime.Date >= fromDate
                         && a.DateTime.Date <= toDate)
                .GroupBy(a => a.DateTime.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    FirstRecord = g
                        .Where(x => x.Action == "CheckIn")
                        .OrderBy(x => x.DateTime)
                        .FirstOrDefault(),
                    LastRecord = g
                        .Where(x => x.Action == "CheckOut")
                        .OrderByDescending(x => x.DateTime)
                        .FirstOrDefault()
                })
                .AsEnumerable()
                .Select(r => new EmployeeTimeSheet
                {
                    Date = r.Date.ToString("dd-MM-yyyy"),
                    LateLogin = (r.FirstRecord?.LateLogin ?? false) ? "Late Login" : "",
                    FirstLoginTime = r.FirstRecord != null
                        ? TimeOnly.FromDateTime(r.FirstRecord.DateTime).ToString("HH:mm:ss")
                        : "",
                    LastLogoutTime = r.LastRecord != null
                        ? TimeOnly.FromDateTime(r.LastRecord.DateTime).ToString("HH:mm:ss")
                        : "",
                    EarlyLogout = (r.LastRecord?.EarlyLogout ?? false) ? "Early Logout" : "",
                })
                .OrderBy(r => r.Date)
                .ToList();

            var loadResult = DataSourceLoader.Load(results, loadOptions);

            return loadResult;
        }
    }

    public class EmployeeTimeSheet
    {
        public string Date { get; set; }
        public string LateLogin { get; set; }
        public string? FirstLoginTime { get; set; }
        public string? LastLogoutTime { get; set; }
        public string EarlyLogout { get; set; }
    }
}
