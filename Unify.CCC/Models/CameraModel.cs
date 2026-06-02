namespace Unify.CCC.Models
{
    public class CameraModel
    {
        public Guid CameraId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CameraProtocol { get; set; }
        public string ConnectionString { get; set; } = string.Empty;
        public string? AOI { get; set; }
        public List<CameraMetaDataModel>? CameraMetaDataModels { get; set; } = new();
        public List<MonitorModel>? MonitorModels { get; set; } = new();
        public List<SegmentModel>? SegmentsModels { get; set; } = new();
        public Guid CustomerId { get; set; }
    }

    public enum CameraProtocol
    {
        RTSP,
        USB
    }
}
