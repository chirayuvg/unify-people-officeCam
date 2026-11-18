using Unify.People.Contracts.Attendance;
using Unify.People.Domain.Shared;

namespace Unify.People.Application.Attendance;

/// <summary>
/// Validates attendance, leave and regularisation commands before persistence.
/// Policy and balance lookups will be supplied through repositories later.
/// </summary>
public sealed class AttendanceValidationService
{
    public OperationResult Validate(RecordAttendanceRequest request)
    {
        if (request.TenantId == Guid.Empty)
            return OperationResult.Failure("attendance.tenant.required", "A tenant is required.");
        if (request.EmployeeId == Guid.Empty)
            return OperationResult.Failure("attendance.employee.required", "An employee is required.");
        if (request.OccurredAt == default)
            return OperationResult.Failure("attendance.time.required", "An attendance time is required.");

        return OperationResult.Success();
    }

    public OperationResult Validate(ApplyLeaveRequest request)
    {
        if (request.TenantId == Guid.Empty || request.EmployeeId == Guid.Empty)
            return OperationResult.Failure("leave.subject.required", "A tenant and employee are required.");
        if (request.EndDate < request.StartDate)
            return OperationResult.Failure("leave.date.range", "The leave end date cannot be before the start date.");
        if (string.IsNullOrWhiteSpace(request.Reason))
            return OperationResult.Failure("leave.reason.required", "A leave reason is required.");

        return OperationResult.Success();
    }

    public OperationResult Validate(ApplyRegularizationRequest request)
    {
        if (request.TenantId == Guid.Empty || request.EmployeeId == Guid.Empty)
            return OperationResult.Failure("regularization.subject.required", "A tenant and employee are required.");
        if (request.RequestedCheckOut <= request.RequestedCheckIn)
            return OperationResult.Failure("regularization.time.range", "Check-out must be after check-in.");
        if (string.IsNullOrWhiteSpace(request.Reason))
            return OperationResult.Failure("regularization.reason.required", "A reason is required.");

        return OperationResult.Success();
    }
}
