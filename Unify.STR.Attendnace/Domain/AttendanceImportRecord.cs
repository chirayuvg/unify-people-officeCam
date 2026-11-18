namespace Unify.STR.Attendance.Domain;

public sealed record AttendanceImportRecord(
    Guid Id,
    Guid TenantId,
    string EmployeeCode,
    DateOnly AttendanceDate,
    TimeOnly CheckInTime,
    TimeOnly? CheckOutTime);
