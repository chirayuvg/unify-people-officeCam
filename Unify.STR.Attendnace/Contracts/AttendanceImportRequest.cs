namespace Unify.STR.Attendance.Contracts;

public sealed record AttendanceImportRequest(
    Guid TenantId,
    string EmployeeCode,
    DateOnly AttendanceDate,
    TimeOnly CheckInTime,
    TimeOnly? CheckOutTime);
