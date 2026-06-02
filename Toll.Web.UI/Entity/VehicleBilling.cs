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
    public class VehicleBilling
    {
        [Key]
        public Guid Id { get; set; }
        public Int64 TicketNumber { get; set; }
        public string VehicleNumber { get; set; }
        public decimal TollAmount { get; set; }
        public decimal? WeightVoilationAmount { get; set; }
        public decimal? SpeedVoilationAmount { get; set; }
        public decimal? TimeVoilationAmount { get; set; }
        public decimal? TotalAmount { get; set; }
        public bool? PaymentStatus { get; set; }
        public string? Remarks { get; set; }
        public string? FileURL { get; set; }

        public bool Exempted { get; set; }
        public Guid? AreaId { get; set; }
        public Area? Area { get; set; }
        public Guid? BoothId { get; set; }
        public Booth? Booth { get; set; }
        public Guid ShiftId { get; set; }
        public Shift? Shift { get; set; }
        public Guid? CameraId { get; set; }
        public Camera? Camera { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid CustomerId { get; set; }
        public Guid? VehicleTypeId { get; set; }

        public VehicleType? VehicleType { get; set; }

        public Guid? VehicleCategoryId { get; set; }

        public VehicleCategory? VehicleCategory { get; set; }


        public Guid UserId { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
