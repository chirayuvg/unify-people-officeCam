using AppBuilder.Framework.Core;
using AppBuilder.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toll.Web.UI.Entity
{
    public class VehicleCategory
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        public bool RaiseAlert { get; set; }
        public bool Exempted { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid CustomerId { get; set; }
    }
}
