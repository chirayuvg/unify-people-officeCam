using AppStudio.Framework.Core;
using Pgvector;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Unify.Data.SQL
{
    public class AttendanceLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [UIControl(ControlType.DateTimePicker)]
        public DateTime DateTime { get; set; }
        public DateTime TimeStamp { get; set; }
        public Guid TenantId { get; set; }
        public Guid IntelligenceId { get; set; }
        public Guid? CameraId { get; set; }
        public Guid EmployeeId { get; set; }

        [MaxLength(50)]
        public string IntelligenceName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? CameraName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string EmployeeType { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Department { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Designation { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Gender { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Shift { get; set; } = string.Empty;
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }

        [MaxLength(50)]
        public string BoundingBox { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Action { get; set; } = string.Empty;
        public decimal Time { get; set; } = decimal.Zero;
        public bool FirstLogin { get; set; } = false;
        public bool LateLogin { get; set; } = false;
        public decimal LateLoginMinutes { get; set; } = decimal.Zero;
        public bool EarlyLogout { get; set; } = false;
        public decimal EarlyLogoutMinutes { get; set; } = decimal.Zero;
        public bool ManualEntry { get; set; } = false;

        [MaxLength(100)]
        public string? Reason { get; set; } = string.Empty;
        public bool LastLog { get; set; } = false;

        [MaxLength(50)]
        public string? SimilarityValue { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? FaceCover { get; set; } = string.Empty;

        [MaxLength(250)]
        [DataType(DataType.ImageUrl)]
        public string Image { get; set; }

        [Column(TypeName = "vector(512)")]
        public Vector? Embedding { get; set; }

        public float? Latitude { get; set; }

        public float? Longitude { get; set; }
    }
}
