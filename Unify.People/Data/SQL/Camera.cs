using AppStudio.Framework.Core;
using System.ComponentModel.DataAnnotations;

namespace Unify.People.Data.SQL
{
    public class Camera
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Description { get; set; } = string.Empty;

        public CameraProtocol CameraProtocol { get; set; }

        [Required]
        [MaxLength(100)]
        public string ConnectionString { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string? AOI { get; set; }
        public string Tag { get; set; }
 

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid CustomerId { get; set; }
    }

    public enum CameraProtocol
    {
        RTSP,
        USB
    }
}
