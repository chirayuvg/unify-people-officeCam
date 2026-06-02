using System.ComponentModel.DataAnnotations;

namespace UnifyNow.Forest.Data.SQL
{
    public class Alert
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public Severity Severity { get; set; }

        public string Animal { get; set; }

        public string AOI { get; set; }

        public string? FileURL { get; set; }

        public Guid? TeamId { get; set; }
        public Team? Team { get; set; }


    }

    public enum Severity
    {
        Critical,
        High,
        Medium,
        Low
    }
}
