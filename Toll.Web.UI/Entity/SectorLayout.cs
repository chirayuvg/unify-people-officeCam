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
    public class SectorLayout
    {
        [Key]
        [Required]
        public Guid Id { get; set; } = Guid.NewGuid();
        public int X { get; set; }
        public int Y { get; set; }
        public Guid AssetId { get; set; }
        //public Asset? Asset { get; set; }
        public string PartName { get; set; }
        public string PropertyName { get; set; }
        public Guid SectorId { get; set; }
        public Sector Sector { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid CustomerId { get; set; }

    }


}
