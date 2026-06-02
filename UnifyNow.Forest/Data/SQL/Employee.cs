
using AppStudio.Framework.Core;
using System.ComponentModel.DataAnnotations;

namespace UnifyNow.Forest.Data.SQL
{
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
        public Guid? EmployeeTypeId { get; set; }
        public Guid? DepartmentId { get; set; }

        public Guid? DesignationId { get; set; }
        public Int64? MobileNo { get; set; }

        [MaxLength(50)]
        public string? Email { get; set; }
       

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
