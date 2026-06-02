using AppStudio.Framework.Core;
using System.ComponentModel.DataAnnotations;

namespace Unify.Data.SQL
{
    public class LatePolicy
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public AppliesTo AppliesTo { get; set; }

        public Guid? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public Guid? EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        [Display(Name = "Grace Period for Late Login (Minutes)")]
        [Range (0, 60)]
        public int LateLogin { get; set; }      //  15 Minutes, eg from 9:30 to 9:45

        [Display(Name = "Max. allowed Late Logins (Count)")]
        [Range(0, 10)]
        public int MaxLateLogin { get; set; }   //  5 Times a month

        [Display(Name = "Grace Period for Early Logout (Minutes)")]
        [Range(0, 60)]
        public int EarlyLogout { get; set; }    //  15 Minutes, eg before 6:30 to 6:15

        [Display(Name = "Max. allowed Early Logouts (Count)")]
        [Range(0, 10)]
        public int MaxEarlyLogout { get; set; } //  5 Times a month

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
