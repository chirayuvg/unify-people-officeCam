
using AppStudio.Framework.Core;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Unify.CCC.Data
{
    [Index(nameof(Name), nameof(CustomerId), IsUnique = true)]
    public class Employee
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(50)]
        public string Code { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public DateOnly DoB { get; set; }
        public Guid? EmployeeTypeId { get; set; }
        public EmployeeType? EmployeeType { get; set; }
        public Guid? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public Guid? DesignationId { get; set; }
        public Designation? Designation { get; set; }
        public Int64? MobileNo { get; set; }
        [MaxLength(50)]
        public string? Email { get; set; }
        public List<EmployeeImage> EmployeeImages { get; set; } = [];

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid CustomerId { get; set; }
    }
    public enum Gender
    {
        Male,
        Female,
        TransGender
    }
}

