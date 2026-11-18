namespace UnifyNow.Forest.Domain;

public sealed record AlertRule(
    Guid Id,
    Guid TenantId,
    string Name,
    string AreaName,
    string Severity,
    TimeSpan Cooldown);
