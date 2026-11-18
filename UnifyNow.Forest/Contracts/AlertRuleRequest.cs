namespace UnifyNow.Forest.Contracts;

public sealed record CreateAlertRuleRequest(
    Guid TenantId,
    string Name,
    string AreaName,
    string Severity,
    int CooldownMinutes);
