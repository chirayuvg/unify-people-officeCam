using AppBuilder.Framework.Core;
using AppBuilder.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toll.Web.UI.Entity
{
    [Index(nameof(Name), nameof(CustomerId), IsUnique = true)]
    public class Sector
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        //Dimension in meters
        public decimal Width { get; set; }

        public decimal Length { get; set; }

        public decimal Height { get; set; }

        [MaxLength(100)]
        [DataType(DataType.ImageUrl)]
        public string FloorPlan { get; set; }

        public Guid BranchId { get; set; }
        public Branch Branch { get; set; }

        public List<SectorLayout> layouts { get; set; } = [];

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid CustomerId { get; set; }
    }
}
