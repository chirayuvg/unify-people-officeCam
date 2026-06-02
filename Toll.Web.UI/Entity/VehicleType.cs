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
    public class VehicleType
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public string? Description { get; set; }

        public bool RaiseAlert { get; set; }
        public TimeOnly? FromTime { get; set; }

        public TimeOnly? ToTime { get; set; }

        public string SpeedOnPlains { get; set; }

        public string SpeedOnGhat { get; set; }

        public decimal? TollAmount { get; set; }
        public decimal? WeightAmount { get; set; }

        public decimal? SpeedAmount { get; set; }

        public decimal? TimeAmount { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid CustomerId { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
