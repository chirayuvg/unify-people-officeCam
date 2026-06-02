using AppBuilder.Framework.Core;
using AppBuilder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toll.Web.UI.Entity
{
    public class ValidateVehicle
    {
        public Guid Id { get; set; }
        public Int64 TicketNumber { get; set; }
        public string VehicleNumber { get; set; }
        public int TimeTaken { get; set; }
        public string? Remarks { get; set; }
        public string? FileURL { get; set; }

        public Guid BoothId { get; set; }
        public Booth? Booth { get; set; }
        public Guid FromBoothId { get; set; }
        public Booth? FromBooth { get; set; }
        public Guid? VehicleBillingId { get; set; }
        public VehicleBilling? VehicleBilling { get; set; }
        public DateTime DateTime { get; set; }
        public Guid UserId { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid CustomerId { get; set; }


    }
}
