using AppStudio.Framework.Core;
using Pgvector;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Unify.STR.Attendance.Data.SQL
{
    public class AttendanceLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.CreateVersion7();

        [UIControl(ControlType.DateTimePicker)]
        public DateTime DateTime { get; set; }
        public Guid TenantId { get; set; }
        public Guid UserId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }

        [MaxLength(50)]
        public string? Gender { get; set; }
        public Guid? EmployeeTypeId { get; set; }

        [MaxLength(100)]
        public string? EmployeeType { get; set; }

        public Guid? CircleId { get; set; }

        [MaxLength(100)]
        public string? Circle { get; set; }

        public Guid? DivisionId { get; set; }

        [MaxLength(100)]
        public string? Division { get; set; }

        public Guid? RangeId { get; set; }

        [MaxLength(100)]
        public string? Range { get; set; }

        public Guid? SectionId { get; set; }

        [MaxLength(100)]
        public string? Section { get; set; }
        public Guid? BeatId { get; set; }

        [MaxLength(100)]
        public string? Beat { get; set; }
        public Guid? ShiftId { get; set; }

        [MaxLength(100)]
        public string? Shift { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }

        public bool Latelogin { get; set; } = false;

        [MaxLength(50)]
        public string? Action { get; set; }

        public decimal? Time { get; set; }


        [MaxLength(250)]
        [DataType(DataType.ImageUrl)]
        public string? Image { get; set; }

        [Column(TypeName = "vector(512)")]
        public Vector? Embedding { get; set; }


        //[Column(TypeName = "vector(1024)")]
        //public Vector? Embeddings { get; set; }

        public float? Latitude { get; set; }

        public float? Longitude { get; set; }

        public string? Message { get; set; }
    }
}
