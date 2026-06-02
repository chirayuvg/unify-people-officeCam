using AppStudio.Framework.Core;

namespace UnifyNow.Forest.Data.SQL
{
   

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
}
