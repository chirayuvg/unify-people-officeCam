using Unify.CCC.Contracts;

namespace Unify.CCC.Application;

/// <summary>Validation extracted from camera and monitor configuration pages.</summary>
public sealed class CameraValidationService
{
    public IReadOnlyCollection<string> Validate(RegisterCameraRequest request)
    {
        var errors = new List<string>();
        if (request.TenantId == Guid.Empty) errors.Add("A tenant is required.");
        if (string.IsNullOrWhiteSpace(request.Name)) errors.Add("A camera name is required.");
        if (!Uri.TryCreate(request.ConnectionAddress, UriKind.Absolute, out _)) errors.Add("A valid camera connection address is required.");
        if (string.IsNullOrWhiteSpace(request.Protocol)) errors.Add("A camera protocol is required.");
        return errors;
    }

    public IReadOnlyCollection<string> Validate(ConfigureMonitorRequest request)
    {
        var errors = new List<string>();
        if (request.TenantId == Guid.Empty || request.CameraId == Guid.Empty || request.IntelligenceId == Guid.Empty)
            errors.Add("Tenant, camera and intelligence are required.");
        if (request.FrequencySeconds < 1) errors.Add("Monitor frequency must be at least one second.");
        return errors;
    }
}
