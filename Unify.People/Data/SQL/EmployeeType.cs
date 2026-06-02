using AppStudio.Framework.Core;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Unify.Data.SQL
{
    [Index(nameof(Name), nameof(TenantId), IsUnique = true)]
    public class EmployeeType
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        //public bool RaiseAlert { get; set; }

       // public Guid? OrganizationId { get; set; }
        //public Organization? Organization { get; set; }


        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid TenantId { get; set; }
    }
}
