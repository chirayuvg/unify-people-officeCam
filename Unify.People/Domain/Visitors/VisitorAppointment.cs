namespace Unify.People.Domain.Visitors;

public enum AppointmentStatus
{
    Scheduled,
    InProgress,
    Closed,
    Cancelled
}

public sealed class VisitorAppointment
{
    public VisitorAppointment(Guid id, Guid tenantId, Guid hostEmployeeId, DateTimeOffset scheduledFor)
    {
        Id = id;
        TenantId = tenantId;
        HostEmployeeId = hostEmployeeId;
        ScheduledFor = scheduledFor;
    }

    public Guid Id { get; }
    public Guid TenantId { get; }
    public Guid HostEmployeeId { get; }
    public DateTimeOffset ScheduledFor { get; }
    public AppointmentStatus Status { get; private set; } = AppointmentStatus.Scheduled;

    public void Start() => Status = AppointmentStatus.InProgress;
    public void Close() => Status = AppointmentStatus.Closed;
}
