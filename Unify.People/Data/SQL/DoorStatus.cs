using AppStudio.Framework.Core;
using System.ComponentModel.DataAnnotations;

namespace Unify.Data.SQL
{
    public class DoorStatus
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public int Status { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid TenantId { get; set; }
    }
}
