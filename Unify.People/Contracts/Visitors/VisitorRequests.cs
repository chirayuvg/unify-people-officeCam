namespace Unify.People.Contracts.Visitors;

public sealed record ScheduleAppointmentRequest(
    Guid TenantId,
    Guid HostEmployeeId,
    string VisitorName,
    DateOnly AppointmentDate,
    TimeOnly AppointmentTime,
    int ExpectedDurationMinutes,
    string Purpose);

public sealed record CheckInVisitorRequest(
    Guid TenantId,
    Guid AppointmentId,
    Guid VisitorId,
    DateTimeOffset CheckedInAt);

public sealed record CloseMeetingRequest(
    Guid TenantId,
    Guid AppointmentId,
    DateTimeOffset ClosedAt,
    string? Notes);
