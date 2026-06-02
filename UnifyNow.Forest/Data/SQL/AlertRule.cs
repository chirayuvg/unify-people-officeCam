using System.ComponentModel.DataAnnotations;

namespace UnifyNow.Forest.Data.SQL
{
    public class AlertRule
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        public Guid? CameraId { get; set; }
        public Camera? Camera { get; set; }

        [Required]
        public Guid? AnimalId { get; set; }
        public Animal? Animal { get; set; }

        [Required]
        public Guid? AreaId { get; set; }
        public Area? Area { get; set; }


        [Required]
        public Guid? TeamId { get; set; }
        public Team? Team { get; set; }
    }
}
