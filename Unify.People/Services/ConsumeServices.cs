using AppStudio.Framework.Core;
using AppStudio.Framework.Messaging;
using Unify.People.RulesEngine;

namespace Unify.People.Services
{
    public class ConsumeServices : IMessageHandler<CameraDataModel>
    {
        public async Task Handle(CameraDataModel messages)
        {
            switch (messages.IntelligenceName.ToLower())
            {
                case "attendance":
                    AttendanceRulesEngine attendance = CoreServices.GetService<AttendanceRulesEngine>();
                    await attendance.ProcessAsync(messages);
                    break;
                case "visitor":
                    VisitorRulesEngine visitor = CoreServices.GetService<VisitorRulesEngine>();
                    await visitor.ProcessAsync(messages);
                    break;
                case "security":
                    break;
                default:
                    break;
            }
        }
    }
    public class CameraDataModel : IRemoteMessage
    {
        public Guid CameraId { get; set; }
        public string CameraName { get; set; }
        public Guid IntelligenceId { get; set; }
        public string IntelligenceName { get; set; }
        public Guid TenantId { get; set; }
        public List<SegmentsModel> SegmentsModels { get; set; }
        public List<CameraMetaDataModel> CameraMetaDataModels { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;
        public DateTime TimeStamp { get; set; }
        public List<ClassData> Classes { get; set; } = new List<ClassData>();
        public string? FileURL { get; set; }
    }

    public class ClassData
    {
        public string Name { get; set; }
        public string BoundingBox { get; set; } = string.Empty;
        public string Polygon { get; set; } = string.Empty;
        //public int TrackingId { get; set; }
        public string TrackingId { get; set; }


        public List<ClassProperty> Properties { get; set; } = new List<ClassProperty>();
    }

    public class ClassProperty
    {
        public string PropertyName { get; set; } = string.Empty;
        public string PropertyValue { get; set; }
    }

    public class SegmentsModel
    {
        public string Polygon { get; set; }
        public List<SegmentMetaDataModel> SegmentMetaDataModels { get; set; }
    }

    public class SegmentMetaDataModel
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
    public class CameraMetaDataModel
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
