using AppStudio.Framework.Core;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Unify.Data.SQL
{
    [Index(nameof(PrimaryVisitor), nameof(TenantId), IsUnique = false)]
    public class Appointment
    {
        public Guid Id { get; set; }

        [RequiredValue]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Invalid mobile number.")]

        public Int64? MobileNo { get; set; }

        [RequiredValue]
        [MaxLength(100, ErrorMessage = "Length cannot exceed 100 characters.")]
        public string PrimaryVisitor { get; set; } = string.Empty;

        [RequiredValue]
        [MaxLength(50, ErrorMessage = "Length cannot exceed 50 characters.")]
        public string Company { get; set; }

        [MaxLength(250, ErrorMessage = "Length cannot exceed 250 characters.")]
        public string? Address { get; set; }

        [MaxLength(50, ErrorMessage = "Length cannot exceed 50 characters.")]
        [EmailAddress]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]{2,}\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }

        [RequiredValue(ErrorMessage ="Employee is required")]
        public Guid EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        [RequiredValue(ErrorMessage = "Meeting Purpose is required")]
        public Guid MeetingPurposeId { get; set; }
        public MeetingPurpose? MeetingPurpose { get; set; }

        [RequiredValue(ErrorMessage = "Meeting Place is required")]
        public Guid MeetingPlaceId { get; set; }
        public MeetingPlace? MeetingPlace { get; set; }

        [RequiredValue(ErrorMessage = "Appointment date is required")]
        public DateOnly AppointmentDate { get; set; } = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

        [RequiredValue(ErrorMessage = "Appointment Time is required")]
        public TimeOnly AppointmentTime { get; set; } = new TimeOnly((DateTime.Now.Hour + 1), 0);

        [RequiredValue(ErrorMessage = "No.of Visitors is required")]
        [Range(1, 20, ErrorMessage = "Value must be between 1 and 20.")]
        public int NoOfVisitors { get; set; } = 1;

        [MaxLength(50, ErrorMessage = "Length cannot exceed 50 characters.")]
        public string? Notes { get; set; }

        [RequiredValue]
        public string Status { get; set; } = "Pending";

        public DateOnly? MeetingDate { get; set; }

        public TimeOnly? StartTime { get; set; }

        public TimeOnly? EndTime { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid TenantId { get; set; }
    }
}
