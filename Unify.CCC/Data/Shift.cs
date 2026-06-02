
using AppStudio.Framework.Core;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Unify.CCC.Data
{
    [Index(nameof(Name), nameof(CustomerId), IsUnique = true)]
    public class Shift
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid CustomerId { get; set; }
    }
}
