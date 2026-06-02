using AppStudio.Framework.Core;
using System.ComponentModel.DataAnnotations;

namespace Unify.Data.SQL
{
    public class LeaveLog : IValidatableObject
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }

        public Guid EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public DateTime? ApplyDateTime { get; set; }

        [RequiredValue(ErrorMessage = "Leave Policy is required")]
        public Guid LeavePolicyId { get; set; }
        public LeavePolicy? LeavePolicy { get; set; }

        [RequiredValue(ErrorMessage = "Start Date is required")]
        public DateOnly? StartDate { get; set; }

        [RequiredValue(ErrorMessage = "End Date is required")]
        public DateOnly? EndDate { get; set; }

        public int Days { get; set; }

        [RequiredValue(ErrorMessage = "Leave Remarks is required")]
        public string LeaveRemarks { get; set; }

        [MaxLength(20)]
        public string ApprovalStatus { get; set; }

        public Guid? ApprovedById { get; set; }

        public string? ApprovedByName { get; set; }

        public DateTime? ApprovedOn { get; set; }

        public string? ApprovalRemarks { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid TenantId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartDate.HasValue && EndDate.HasValue)
            {
                if (EndDate < StartDate)
                {
                    yield return new ValidationResult(
                        "End Date must be greater than or equal to Start Date.", new[] { nameof(EndDate) }
                    );
                }
            }
        }
    }
}
