
using AppStudio.Framework.Core;
using System.ComponentModel.DataAnnotations;

namespace Unify.CCC.Data
{
    public class LatePolicy
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        public AppliesTo AppliesTo { get; set; }
        public int LateLogin { get; set; }
        public int MaxLateLogin { get; set; }
        public int EarlyLogout { get; set; }
        public int MaxEarlyLogout { get; set; }
        public Guid? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public Guid? EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid CustomerId { get; set; }
    }
    public enum AppliesTo
    {
        All,
        Department,
        Employee
    }
}
