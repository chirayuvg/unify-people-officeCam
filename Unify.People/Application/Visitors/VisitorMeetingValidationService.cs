using Unify.People.Contracts.Visitors;
using Unify.People.Domain.Shared;

namespace Unify.People.Application.Visitors;

/// <summary>
/// Contains the validation that was previously spread across appointment and
/// meeting pages. State persistence remains a future infrastructure concern.
/// </summary>
public sealed class VisitorMeetingValidationService
{
    public OperationResult Validate(ScheduleAppointmentRequest request)
    {
        if (request.TenantId == Guid.Empty || request.HostEmployeeId == Guid.Empty)
            return OperationResult.Failure("appointment.host.required", "A tenant and host employee are required.");
        if (string.IsNullOrWhiteSpace(request.VisitorName))
            return OperationResult.Failure("appointment.visitor.required", "A visitor name is required.");
        if (request.ExpectedDurationMinutes is < 1 or > 1_440)
            return OperationResult.Failure("appointment.duration.invalid", "Expected duration must be between 1 and 1,440 minutes.");
        if (string.IsNullOrWhiteSpace(request.Purpose))
            return OperationResult.Failure("appointment.purpose.required", "A meeting purpose is required.");

        return OperationResult.Success();
    }

    public OperationResult Validate(CheckInVisitorRequest request)
    {
        if (request.TenantId == Guid.Empty || request.AppointmentId == Guid.Empty || request.VisitorId == Guid.Empty)
            return OperationResult.Failure("visitor.checkin.subject.required", "A tenant, appointment and visitor are required.");
        if (request.CheckedInAt == default)
            return OperationResult.Failure("visitor.checkin.time.required", "A check-in time is required.");

        return OperationResult.Success();
    }

    public OperationResult Validate(CloseMeetingRequest request)
    {
        if (request.TenantId == Guid.Empty || request.AppointmentId == Guid.Empty)
            return OperationResult.Failure("meeting.close.subject.required", "A tenant and appointment are required.");
        if (request.ClosedAt == default)
            return OperationResult.Failure("meeting.close.time.required", "A closing time is required.");

        return OperationResult.Success();
    }
}
