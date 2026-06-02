namespace Unify.CCC.Models
{
    public class IntelligenceModel
    {
        public Guid IntelligenceId { get; set; }
        public string Name { get; set; }
        public string MonitorType { get; set; }
        public List<AlertModel>? Alerts { get; set; } = new();
    }

    public enum MonitorType
    {
        Manual,
        RealTime,
        Schedule
    }
}
