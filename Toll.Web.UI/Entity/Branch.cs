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
    [Index(nameof(Name), nameof(CustomerId), IsUnique = true)]
    public class Branch
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Country { get; set; } = string.Empty;

        [MaxLength(50)]
        public string State { get; set; } = string.Empty;

        [MaxLength(50)]
        public string City { get; set; } = string.Empty;

        [MaxLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        public decimal TimeZone { get; set; }

        public decimal Longitude { get; set; }

        public decimal Latitude { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]

        public Guid CustomerId { get; set; }
    }
}
