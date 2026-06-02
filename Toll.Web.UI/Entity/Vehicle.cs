using AppBuilder.Framework.Core;
using AppBuilder.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toll.Web.UI.Entity
{
    [Index(nameof(Number), nameof(CustomerId), IsUnique = true)]
    public class Vehicle
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Number { get; set; } = string.Empty;

        public Guid? VehicleTypeId { get; set; }

        public VehicleType VehicleType { get; set; }

        public Guid? VehicleCategoryId { get; set; }

        public VehicleCategory VehicleCategory { get; set; }

        public Guid? VehicleNatureId { get; set; }

        public VehicleNature VehicleNature { get; set; }

        public Exemption Exemption { get; set; }


        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid CustomerId { get; set; }

    }
    public enum Exemption
    {
        Exempted, NotExempted, NotAllowed
    }
}
