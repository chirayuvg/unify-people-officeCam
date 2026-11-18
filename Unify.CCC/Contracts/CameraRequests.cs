namespace Unify.CCC.Contracts;

public sealed record RegisterCameraRequest(
    Guid TenantId,
    string Name,
    string ConnectionAddress,
    string Protocol);

public sealed record ConfigureMonitorRequest(
    Guid TenantId,
    Guid CameraId,
    Guid IntelligenceId,
    int FrequencySeconds);
