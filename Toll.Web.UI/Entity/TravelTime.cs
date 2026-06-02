using AppBuilder.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toll.Web.UI.Entity
{
    public class TravelTime
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        public Guid? FromAreaId { get; set; }

        public Area FromArea { get; set; }
        public Guid? ToAreaId { get; set; }
        public Area ToArea { get; set; }

        public decimal Distance { get; set; }

        public decimal MinTime { get; set; }
        public decimal MaxTime { get; set; }

        public Guid CustomerId { get; set; }

        public bool ViceVersa { get; set; }



    }
}
