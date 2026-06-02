using AppStudio.Framework.Core;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace UnifyNow.Forest.Data.SQL
{
    [Index(nameof(Name), nameof(CustomerId), IsUnique = true)]
    public class Member
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [Required]
        public Guid? DesignationId { get; set; }
        public Designation? Designation { get; set; }

        [Required]
        public Guid? DepartmentId { get; set; }
        public Department? Department { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        public string? AlternativePhoneNumber { get; set; } 


        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid CustomerId { get; set; }

    }
}
