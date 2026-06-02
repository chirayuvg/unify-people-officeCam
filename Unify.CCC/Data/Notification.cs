using AppStudio.Framework.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unify.CCC.Data
{
    [Index(nameof(Name), IsUnique = true)]
    public class Notification
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        public Guid AlertId { get; set; }
        public Alert? Alert { get; set; }

        public DateTime? DateTime { get; set; }

        public Guid UserId { get; set; }

        public Guid ChannelId { get; set; }
        public Channel? Channel { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid CustomerId { get; set; }
    }
}
