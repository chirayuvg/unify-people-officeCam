using AppStudio.Framework.Core;
using AppStudio.Model;
using DevExtreme.AspNet.Data.ResponseModel;
using Unify.Data;
using Unify.People.Models;

namespace Unify.People.Widgets
{
    public class GridViewLeaveApproval<TModel> : GridView<TModel> where TModel : class, new()
    {
        public GridViewLeaveApproval()
        {
            OnLoadData += GridViewMyLeaves_OnLoadData;
            Toolbar.Title += "Applied Leaves";
        }

        private async Task<DevExtreme.AspNet.Data.ResponseModel.LoadResult> GridViewMyLeaves_OnLoadData(DevExtreme.AspNet.Mvc.DataSourceLoadOptions loadOptions, string? parentId)
        {
            var cardView = this.Page.FindControl<CardView<EmployeeFilterData>>();
            var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();
            var employeeId = cardView.Data.EmployeeId;
            DateTime fromDate =
                (cardView.Data.FromDate == DateOnly.MinValue)
                ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
                : DateTime.Parse(cardView.Data.FromDate.ToString("yyyy-MM-dd"));
            DateTime toDate =
                (cardView.Data.ToDate == DateOnly.MinValue)
                ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
                : DateTime.Parse(cardView.Data.ToDate.ToString("yyyy-MM-dd"));

            var peopleDb = CoreServices.GetService<PeopleDb>();
            var results =
                (
                    from leaveLog in peopleDb.LeaveLogs
                    join employee in peopleDb.Employees
                        on leaveLog.EmployeeId equals employee.Id
                    join policy in peopleDb.LeavePolicies
                        on leaveLog.LeavePolicyId equals policy.Id
                    where leaveLog.EmployeeId == employeeId
                        && (leaveLog.ApplyDateTime.Value.Date >= fromDate.Date && leaveLog.ApplyDateTime.Value.Date <= toDate.Date)
                    select new LeaveApproval
                    {
                        Id = leaveLog.Id,
                        Employee = employee.Name,
                        AppliedOn = leaveLog.ApplyDateTime.Value.ToString("dd-MM-yyyy HH:mm:ss"),
                        LeavePolicy = policy.Name,
                        StartDate = leaveLog.StartDate ?? default,
                        EndDate = leaveLog.EndDate ?? default,
                        LeaveRemarks = leaveLog.LeaveRemarks,
                        ApprovalStatus = leaveLog.ApprovalStatus,
                        ApprovalRemarks = leaveLog.ApprovalRemarks
                    }
                )
                .OrderByDescending(m => m.StartDate).ThenByDescending(m => m.EndDate)
                .ToList();

            int totalCount = results.Count;
            int pageSize = 5;
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            int currentPage = (loadOptions.Skip / pageSize) + 1;

            results = results.Skip(loadOptions.Skip).Take(loadOptions.Take).ToList();

            return new LoadResult
            {
                data = results,
                totalCount = totalCount
            };
        }
    }
}
