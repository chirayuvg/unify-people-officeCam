using AppStudio.Enterprise.Data;
using AppStudio.Framework.Core;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;
using Microsoft.EntityFrameworkCore;
using Unify.Data;
using Unify.Data.SQL;
using Unify.People.Widgets;

namespace Unify.People.Controllers.Organization
{
    public class ApplyLeaveController : PageController
    {
        private readonly ApplicationDb _applicationDb;
        private readonly PeopleDb _peopleDb;

        private FormView<LeaveLog> formView;
        private GridView<LeaveLog> gridView;

        public ApplyLeaveController(IDbContextFactory<ApplicationDb> applicationDb, IDbContextFactory<PeopleDb> peopleDb)
        {
            _applicationDb = applicationDb.CreateDbContext();
            _peopleDb = peopleDb.CreateDbContext();
        }

        public override void Init()
        {
            this.Page.SetTitle("Leave");

            var kpiLeaveCasual = new KpiLeaveCasual().SetId("kpiLeaveCasual");
            kpiLeaveCasual.Data.Color = KpiColor.SeaGreen;
            kpiLeaveCasual.Data.Icon = "fa-solid fa-person-walking";

            var kpiLeaveCompOff = new KpiLeaveCompOff().SetId("kpiLeaveCompOff");
            kpiLeaveCompOff.Data.Color = KpiColor.SeaGreen;
            kpiLeaveCompOff.Data.Icon = "fa-solid fa-arrows-rotate";

            var kpiLeaveEarned = new KpiLeaveEarned().SetId("kpiLeaveEarned");
            kpiLeaveEarned.Data.Color = KpiColor.SeaGreen;
            kpiLeaveEarned.Data.Icon = "fa-solid fa-briefcase";

            var kpiLeaveFloater = new KpiLeaveFloater().SetId("kpiLeaveFloater");
            kpiLeaveFloater.Data.Color = KpiColor.RoyalBlue;
            kpiLeaveFloater.Data.Icon = "fa-solid fa-gift";

            var kpiLeaveUnpaid = new KpiLeaveUnpaid().SetId("kpiLeaveUnpaid");
            kpiLeaveUnpaid.Data.Color = KpiColor.RoyalBlue;
            kpiLeaveUnpaid.Data.Icon = "fa-solid fa-circle-down";

            this.AddColumns().Controls(c =>
            {
                c.AddControl(kpiLeaveCasual);
                c.AddControl(kpiLeaveCompOff);
                c.AddControl(kpiLeaveEarned);
                c.AddControl(kpiLeaveFloater);
                c.AddControl(kpiLeaveUnpaid);
            });

            formView = this.AddFormView<LeaveLog>();
            formView.GetFieldControl("ApplyDateTime").IsVisible = false;
            formView.GetFieldControl("EmployeeId").IsVisible = false;
            formView.GetFieldControl("Days").IsVisible = false;
            formView.GetFieldControl("ApprovalStatus").IsVisible = false;
            formView.GetFieldControl("ApprovedById").IsVisible = false;
            formView.GetFieldControl("ApprovedByName").IsVisible = false;
            formView.GetFieldControl("ApprovedOn").IsVisible = false;
            formView.GetFieldControl("ApprovalRemarks").IsVisible = false;
            formView.OnBeforeInsert += FormView_OnBeforeInsert;
            formView.OnBeforeUpdate += FormView_OnBeforeUpdate;
            formView.OnBeforeDelete += FormView_OnBeforeDelete;

            //formView.Data.ApplyDateTime = DateTime.Now;
            //formView.Data.ApprovalStatus = "Pending Approval";
            //var applyDateTime = formView.GetFieldControl("ApplyDateTime");
            //applyDateTime.IsReadOnly = true;
            //var approvalStatus = formView.GetFieldControl("ApprovalStatus");
            //approvalStatus.IsReadOnly = true;

            gridView = this.AddGridView<LeaveLog>()
                .SetFormView(formView)
                .ConfigueUI(
                    grid => grid
                        .ColumnAutoWidth(false)
                        .FilterRow(f => f.Visible(false))
                        .HeaderFilter(headerFilter => headerFilter.Visible(false))
                        .RowAlternationEnabled(true)
                        .Scrolling(scrolling => scrolling.ColumnRenderingMode(GridColumnRenderingMode.Virtual))
                        .SearchPanel(s => s.Visible(true).HighlightCaseSensitive(false))
                        .ShowBorders(true)
                        .ShowColumnHeaders(true)
                        .ShowColumnLines(false)
                        .ShowRowLines(false)
                        .WordWrapEnabled(true).Width("100%")
                        .Paging(paging => paging.Enabled(true).PageSize(10))
                        .Pager(pager =>
                        {
                            pager
                                .Visible(true)
                                .DisplayMode(GridPagerDisplayMode.Full)
                                .ShowPageSizeSelector(false)
                                .AllowedPageSizes(new JS("[5]"))
                                .ShowInfo(true)
                                .ShowNavigationButtons(true);
                        })
                );
            gridView.Css.SetHeight("calc(100vh - 200px)");
            gridView.SetDefaultSort("StartDate", true);
            gridView.Fields(field =>
            {
                field.ClearFields();

                field.AddField(f => f.StartDate).ColumnWidth = 200;
                field.AddField(f => f.EndDate).ColumnWidth = 200;
                field.AddField(f => f.LeavePolicyId).ColumnWidth = 200;
                field.AddField(f => f.LeaveRemarks).ColumnWidth = 200;
                field.AddField(f => f.ApplyDateTime).ColumnWidth = 300;
                field.AddField(f => f.ApprovalStatus).ColumnWidth = 200;
            });
            gridView.OnLoadQuery += GridView_OnLoadQuery;
        }

        private IQueryable<LeaveLog> GridView_OnLoadQuery()
        {
            var userId = CoreServices.CurrentUser.GetUserId();
            var results = _peopleDb.LeaveLogs.Where(m => m.UserId == userId);

            return results;
        }

        public override Task ProcessAsync()
        {
            formView.OnBeforeInsert += FormView_OnBeforeInsert;
            formView.OnBeforeUpdate += FormView_OnBeforeUpdate;

            return base.ProcessAsync();
        }

        private async Task<Result> FormView_OnBeforeInsert(LeaveLog entity, IFormFileCollection files, string parentId)
        {
            var result = new Result();

            var userId = CoreServices.CurrentUser.GetUserId();
            var TenantId = CoreServices.CurrentUser.GetTenantId();
            TenantUser tenantUser = _applicationDb.TenantUsers.Where(m => m.Id == userId).FirstOrDefault();
            var employee = _peopleDb.Employees.Where(m => m.TenantId == TenantId && m.Email == tenantUser.Email).FirstOrDefault();

            entity.UserId = userId;
            entity.EmployeeId = employee.Id;
            entity.ApplyDateTime = DateTime.Now;
            entity.Days = (int)GetDaysDifference(entity.StartDate, entity.EndDate);
            entity.ApprovalStatus = "Pending Approval";

            result.IsSuccess = true;
            return result;
        }

        private async Task<Result> FormView_OnBeforeUpdate(LeaveLog entity, IFormFileCollection files, string parentId)
        {
            var result = new Result();

            if (entity.ApprovalStatus != "Pending Approval")
            {
                UiAction.AddJsCode("alert('This leave application has already been " + entity.ApprovalStatus + ". Can NOT be updated.');");
                result.AddError("LeaveLogId", "Cannot Update: This leave application has already been " + entity.ApprovalStatus + ".");

                result.IsSuccess = false;
                return result;
            }

            entity.Days = (int)GetDaysDifference(entity.StartDate, entity.EndDate);

            result.IsSuccess = true;
            return result;
        }

        private async Task<Result> FormView_OnBeforeDelete(object id)
        {
            var result = new Result();

            var peopleDb = CoreServices.GetService<PeopleDb>();
            var leaveLog = await peopleDb.LeaveLogs
                .Where(m => m.Id == Guid.Parse(id.ToString()))
                .FirstOrDefaultAsync();

            if (leaveLog.ApprovalStatus != "Pending Approval")
            {
                UiAction.AddJsCode("alert('This leave application has already been " + leaveLog.ApprovalStatus + ". Can NOT be updated.');");
                result.AddError("LeaveLogId", "Cannot Update: This leave application has already been " + leaveLog.ApprovalStatus + ".");

                result.IsSuccess = false;
                return result;
            }

            result.IsSuccess = true;
            return result;
        }

        private int? GetDaysDifference(DateOnly? StartDate, DateOnly? EndDate)
        {
            if (StartDate.HasValue && EndDate.HasValue)
            {
                return (EndDate.Value.DayNumber - StartDate.Value.DayNumber) + 1;
            }

            return null;
        }
    }
}
