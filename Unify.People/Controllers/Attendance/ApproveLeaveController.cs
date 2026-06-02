using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using Microsoft.AspNetCore.Authorization;
using Unify.People.Services;

namespace Unify.People.Controllers.Attendance
{
    [Authorize("CookiePolicy")]
    public class ApproveLeaveController : PageController
    {
        private readonly AttendanceServices _attendanceServices;

        public CardView<LeaveApprovalModel> cardView;

        public ApproveLeaveController(AttendanceServices attendanceServices)
        {
            _attendanceServices = attendanceServices;
        }

        public override void Init()
        {
            this.Page.Title = "Leave Approval";
            this.Page.SetHeight("200px");
            this.Page.SetWidth("500px");

            var leaveId = this.Page.History.Parameters[0].Value;
            var approvalStatus = this.Page.History.Parameters[1].Value;
            this.Page.Title = approvalStatus;

            cardView = this.AddCardView<LeaveApprovalModel>();
            cardView.Data.LeaveId = Guid.Parse(leaveId);
            cardView.Data.ApprovalStatus = approvalStatus;
            cardView.Data.ApprovalRemarks = "";
            cardView.OnSubmit += CardView_OnSubmit;
        }

        private async Task CardView_OnSubmit(LeaveApprovalModel model, IFormFileCollection files)
        {
            await _attendanceServices.LeaveApproval(model);
            UiAction.AddJsCode("alert('Leave Approval has been updated.');");
            UiAction.ClosePopup();
            UiAction.ShowPage("/ApproveLeave/Index");
        }
    }

    public class LeaveApprovalModel
    {
        public Guid LeaveId { get; set; }

        public string ApprovalStatus { get; set; }

        public string ApprovalRemarks { get; set; }
    }
}
