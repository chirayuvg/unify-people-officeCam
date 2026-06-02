using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toll.Web.UI.Entity
{
    public class Balance
    {
        public Guid Id { get; set; }

        public decimal OpeningBalance { get; set; }

        public decimal? ClosingBalance { get; set; }

        public Guid UserId { get; set; }

        public Guid CustomerId { get; set; }
        public Guid BoothId { get; set; }
        public Guid ShiftId { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
