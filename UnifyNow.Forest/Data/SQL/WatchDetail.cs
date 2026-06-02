using System.ComponentModel.DataAnnotations;

namespace UnifyNow.Forest.Data.SQL
{
    public class WatchDetail
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        public Guid? WatchId { get; set; }
        public Watch? Watch { get; set; }

        public Guid? AreaofInterestId { get; set; }
        public AreaofInterest? AreaofInterest { get; set; }
    }
}
