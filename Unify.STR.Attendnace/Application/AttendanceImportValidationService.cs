using Unify.STR.Attendance.Contracts;

namespace Unify.STR.Attendance.Application;

/// <summary>Provider-free validation for STR attendance upload records.</summary>
public sealed class AttendanceImportValidationService
{
    public IReadOnlyCollection<string> Validate(AttendanceImportRequest request)
    {
        var errors = new List<string>();
        if (request.TenantId == Guid.Empty) errors.Add("A tenant is required.");
        if (string.IsNullOrWhiteSpace(request.EmployeeCode)) errors.Add("An employee code is required.");
        if (request.CheckOutTime is not null && request.CheckOutTime <= request.CheckInTime)
            errors.Add("Check-out must be after check-in.");
        return errors;
    }
}
