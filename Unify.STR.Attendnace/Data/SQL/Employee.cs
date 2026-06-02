using AppStudio.Framework.Core;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Unify.STR.Attendance.Data.SQL;

namespace Unify.STR.Attendance.Data.SQL
{
    [Index(nameof(Code), nameof(TenantId), IsUnique = true)]
    public class Employee 
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } 

        [MaxLength(50)]
        public string Code { get; set; }
        public Gender Gender { get; set; }
        public DateOnly DoB { get; set; }

        public DateOnly DoJ { get; set; }

        public Guid? DesiginationId { get; set; }
        public Desigination? Desigination { get; set; }
        public Guid EmployeeTypeId { get; set; }
        public EmployeeType? EmployeeType { get; set; }

        public Guid? CircleId { get; set; }
        public Circle? Circle { get; set; }

        public Guid? DivisionId { get; set; }
        public Division? Division { get; set; }

        public Guid? RangeId { get; set; }
        public Range? Range { get; set; }

        public Guid? SectionId { get; set; }
        public Section? Section { get; set; }
        public Guid? BeatId { get; set; }
        public Beat? Beat { get; set; }
        public Guid? ShiftId { get; set; }
        public Shift? Shift { get; set; }

        public string? AgencyName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Invalid mobile number")]
        public string MobileNo { get; set; }

        [Required]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email address")]
        [MaxLength(50)]
        public string Email { get; set; }

        public List<EmployeeImage> EmployeeImages { get; set; } = [];

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid TenantId { get; set; }
    }
    public enum Gender
    {
        Male,
        Female,
        TransGender
    }
}
