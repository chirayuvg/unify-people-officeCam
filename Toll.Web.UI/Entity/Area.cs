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
    public class Area
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Coordinates { get; set; }

        public Guid SectorId { get; set; }
        public Sector? Sector { get; set; }

        public Guid BranchId { get; set; }
        public Branch? Branch { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]

        public Guid CustomerId { get; set; }
    }
}
