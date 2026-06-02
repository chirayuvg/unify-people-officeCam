
using AppStudio.Framework.Core;
using System.ComponentModel.DataAnnotations;
using Unify.CCC.Data;

namespace Unify.CCC.Data
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
        public List<CameraMetaData>? CameraMetaData { get; set; } = new();
        //internal List<Segment>? Segments { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid CustomerId { get; set; }
    }

    public enum CameraProtocol
    {
        RTSP,
        USB
    }

    public class CameraMetaData
    {
        public Guid Id { get; set; }
        [MaxLength(50)]
        public string Key { get; set; }
        [MaxLength(50)]
        public string Value { get; set; }
        public Guid CameraId {  get; set; }
        public Camera? Camera { get; set; }
    }

    public class Intelligence
    {
        public Guid Id { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
        public MonitorType MonitorType { get; set; }
        public List<Alert>? Alerts { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid CustomerId { get; set; }
    }

    public class Alert
    {
        public Guid Id { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
        public Guid IntelligenceId { get; set; }
        public Intelligence? Intelligence { get; set; }
    }

    public enum MonitorType
    {
        Manual,
        RealTime,
        Schedule
    }

    public class Monitor
    {

        public Guid Id { get; set; }
        public Guid IntelligenceId { get; set; }
        public Intelligence? Intelligence { get; set; }
        public int MonitorFrequency { get; set; }
        public Guid CameraId { get; set; }
        public Camera? Camera { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid CustomerId { get; set; }

    }

    public class Segment
    {

        public Guid Id { get; set; }
        public string Polygon { get; set; }
        public List<SegmentMetaData>? SegmentMetaData { get; set; } = new();
        public Guid CameraId { get; set; }
        public Camera? Camera { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid CustomerId { get; set; }
    }

    public class SegmentMetaData
    {
        public Guid Id { get; set; }
        [MaxLength(50)]
        public string Key { get; set; }
        [MaxLength(50)]
        public string Value { get; set; }
        public Guid SegmentId { get; set; }
        public Segment? Segment { get; set; }
    }
}