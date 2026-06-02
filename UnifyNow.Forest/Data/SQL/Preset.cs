using Nest;
using System.ComponentModel.DataAnnotations;

namespace UnifyNow.Forest.Data.SQL
{
    public class Preset
    {
        [Key]
        [Required]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; }

        public Guid? CameraId { get; set; }
        public Camera? Camera { get; set; }

        public int Duration {  get; set; }
        public string? Pan {  get; set; }

        public string? Tilt { get; set; }

        public string? Zoom { get; set; }

        public Guid? AreaId { get; set; }
        public Area? Area { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
