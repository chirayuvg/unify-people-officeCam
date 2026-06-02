using AppStudio.Framework.Core;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Unify.People.Data.SQL;

namespace Unify.Data.SQL
{
    [Index(nameof(Name), nameof(TenantId), IsUnique = true)]
    public class Employee
    {
        [Key]
        public Guid Id { get; set; }

        [RequiredValue]
        [MaxLength(100, ErrorMessage = "Length of 'Name' cannot exceed 100 characters.")]
        public string Name { get; set; }

        [RequiredValue]
        [MaxLength(50, ErrorMessage = "Length of 'Code' cannot exceed 50 characters.")]
        public string Code { get; set; }

        //[RequiredValue]
        public Gender? Gender { get; set; }

        [RequiredValue]
        [Display(Name = "Date of Birth")]
        public DateOnly? DoB { get; set; }

        [RequiredValue(ErrorMessage = "Please select Employee Type")]
        public Guid EmployeeTypeId { get; set; }
        public EmployeeType? EmployeeType { get; set; }

        [RequiredValue(ErrorMessage = "Please select Department")]
        public Guid DepartmentId { get; set; }
        public Department? Department { get; set; }

        [RequiredValue(ErrorMessage = "Please select Designation")]
        public Guid DesignationId { get; set; }
        public Designation? Designation { get; set; }

        [RequiredValue(ErrorMessage = "Please select Shift")]
        public Guid ShiftId { get; set; }
        public Shift? Shift { get; set; }

        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Invalid mobile number.")]
        public Int64? MobileNo { get; set; }

        [RequiredValue]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]{2,}\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email address")]
        [MaxLength(50)]
        public string? Email { get; set; }
        
        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid TenantId { get; set; }

        public List<EmployeeImage> EmployeeImages { get; set; } = [];
    }
    public enum Gender
    {
        Male,
        Female,
        TransGender
    }
}
