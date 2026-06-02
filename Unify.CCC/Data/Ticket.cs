
using AppStudio.Framework.Core;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Unify.CCC.Data
{
    public class Ticket
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string UserId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        public bool ActionTaken { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid CustomerId { get; set; }
    }
}
