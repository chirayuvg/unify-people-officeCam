using Pgvector;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Unify.Data.SQL
{
    public class VisitorLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime DateTime { get; set; }

        public DateTime TimeStamp { get; set; }

        public Guid TenantId { get; set; }

        public Guid IntelligenceId { get; set; }

        public Guid? CameraId { get; set; }

        public Guid AppointmentId { get; set; }

        public Guid VisitorId { get; set; }

        public Guid EmployeeId { get; set; }

        public Guid? MeetingPurposeId { get; set; }

        public Guid? MeetingPlaceId { get; set; }


        public string IntelligenceName { get; set; } = string.Empty;

        public string? CameraName { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public string VisitorName { get; set; } = string.Empty;

        public Int64? MobileNo { get; set; }

        public string Company { get; set; }

        public string? Address { get; set; }

        public string? Email { get; set; }

        public string EmployeeCode { get; set; } = string.Empty;

        public string EmployeeName { get; set; } = string.Empty;

        public string MeetingPurpose { get; set; } = string.Empty;

        public string MeetingPlace { get; set; } = string.Empty;

        public DateOnly AppointmentDate { get; set; }

        public TimeOnly AppointmentTime { get; set; }

        public DateOnly? MeetingDate { get; set; }

        public TimeOnly? StartTime { get; set; }

        public TimeOnly? EndTime { get; set; }

        public string? BadgeNo { get; set; }

        [MaxLength(250)]
        [DataType(DataType.ImageUrl)]
        public string Image { get; set; }

        [Column(TypeName = "vector(512)")]
        public Vector Embedding { get; set; }

        public float Latitude { get; set; }

        public float Longitude { get; set; }
    }
}
