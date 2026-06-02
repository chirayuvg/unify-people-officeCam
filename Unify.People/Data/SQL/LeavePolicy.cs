using AppStudio.Framework.Core;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Unify.Data.SQL
{
    [Index(nameof(Name), nameof(TenantId), IsUnique = true)]
    public class LeavePolicy
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int QuotaPerYear { get; set; }

        [Required]
        public bool AllowCarryForward { get; set; }

        [Required]
        public int CarryForwardLimit { get; set; } = 0;

        [Required]
        public bool SystemGenerated { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid TenantId { get; set; }
    }
}
