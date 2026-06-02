using AppStudio.Framework.Core;
using System.ComponentModel.DataAnnotations;

namespace UnifyNow.Forest.Data.SQL
{
    public class Intelligence
    {
        public Guid Id { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
        public MonitorType MonitorType { get; set; }
      
        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid CustomerId { get; set; }
    }

    public enum MonitorType
    {
        Manual,
        RealTime,
        Schedule
    }
}
