using AppStudio.Framework.Core;
using System.ComponentModel.DataAnnotations;

namespace Unify.STR.Attendance.Data.SQL
{
    public class LatePolicy 
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public AppliesTo AppliesTo { get; set; }

        public Guid? DivisionId { get; set; }
        public Division? Division { get; set; }

        public Guid? EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        [Display(Name = "Grace Period for Late Login (Minutes)")]
        public int LateLogin { get; set; }

        public int MaxLateLogin { get; set; }

        public int EarlyLogout { get; set; }

        public int MaxEarlyLogout { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid TenantId { get; set; }
    }

    public enum AppliesTo
    {
        All,
        Department,
        Employee
    }
}
