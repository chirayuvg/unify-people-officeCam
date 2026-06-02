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
    public class VehiclePricing
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? VehicleTypeId { get; set; }

        public VehicleType? VehicleType { get; set; }

        public decimal? VehiclePrice { get; set; }

         [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid CustomerId { get; set; }
    }
}
