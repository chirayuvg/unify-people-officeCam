using Unify.People.Contracts.Attendance;

namespace Unify.People.Domain.Attendance;

public sealed record AttendanceEntry(
    Guid Id,
    Guid TenantId,
    Guid EmployeeId,
    DateTimeOffset OccurredAt,
    AttendanceEventType EventType,
    Guid? CameraId);
