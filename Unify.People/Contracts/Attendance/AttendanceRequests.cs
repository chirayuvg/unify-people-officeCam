namespace Unify.People.Contracts.Attendance;

public sealed record RecordAttendanceRequest(
    Guid TenantId,
    Guid EmployeeId,
    DateTimeOffset OccurredAt,
    AttendanceEventType EventType,
    Guid? CameraId = null);

public enum AttendanceEventType
{
    CheckIn,
    CheckOut
}

public sealed record ApplyLeaveRequest(
    Guid TenantId,
    Guid EmployeeId,
    DateOnly StartDate,
    DateOnly EndDate,
    string Reason);

public sealed record ApplyRegularizationRequest(
    Guid TenantId,
    Guid EmployeeId,
    DateOnly AttendanceDate,
    TimeOnly RequestedCheckIn,
    TimeOnly RequestedCheckOut,
    string Reason);
