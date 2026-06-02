using AppStudio.Framework.Core;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Unify.People.Services;

namespace Unify.People.Controllers.Regularization
{
    [Authorize("CookiePolicy")]
    public class ApplyRegularizationTimeController : PageController
    {
        private readonly AttendanceServices _attendanceServices;

        public CardView<ApplyRegularizationTimeModel> cardView;

        public ApplyRegularizationTimeController(AttendanceServices attendanceServices)
        {
            _attendanceServices = attendanceServices;
        }

        public override void Init()
        {
            this.Page.SetHeight("200px");
            this.Page.SetWidth("400px");

            var employeeId = this.Page.History.Parameters[0].Value;
            var attendanceId = this.Page.History.Parameters[1].Value;
            var regularizationDate = this.Page.History.Parameters[2].Value;

            cardView = this.AddCardView<ApplyRegularizationTimeModel>();
            cardView.Data.EmployeeId = Guid.Parse(employeeId);
            if (attendanceId != "NULL") cardView.Data.AttendanceId = Guid.Parse(attendanceId);
            cardView.GetFieldControl("RegularizationDate").IsVisible = false;
            cardView.OnSubmit += CardView_OnSubmit;
        }

        private async Task CardView_OnSubmit(ApplyRegularizationTimeModel model, IFormFileCollection files)
        {
            await _attendanceServices.ApplyRegularization(model);
            UiAction.ClosePopup();
        }
    }

    public class ApplyRegularizationTimeModel : IValidatableObject
    {
        public Guid EmployeeId { get; set; }

        public Guid AttendanceId { get; set; }

        public DateOnly RegularizationDate { get; set; }

        [RequiredValue(ErrorMessage = "Start Time is required")]
        public TimeOnly? StartTime { get; set; }

        [RequiredValue(ErrorMessage = "End Time is required")]
        public TimeOnly? EndTime { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartTime.HasValue && EndTime.HasValue)
            {
                if (EndTime <= StartTime)
                {
                    yield return new ValidationResult(
                        "End Time must be greater than Start Time.", new[] { nameof(EndTime) }
                    );
                }
            }
        }
    }
}
