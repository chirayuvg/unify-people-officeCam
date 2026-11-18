using UnifyNow.Forest.Contracts;

namespace UnifyNow.Forest.Application;

/// <summary>Validation for Forest monitoring rules; message delivery is deferred.</summary>
public sealed class AlertRuleValidationService
{
    private static readonly HashSet<string> AllowedSeverities =
        new(StringComparer.OrdinalIgnoreCase) { "Low", "Medium", "High", "Critical" };

    public IReadOnlyCollection<string> Validate(CreateAlertRuleRequest request)
    {
        var errors = new List<string>();
        if (request.TenantId == Guid.Empty) errors.Add("A tenant is required.");
        if (string.IsNullOrWhiteSpace(request.Name)) errors.Add("A rule name is required.");
        if (string.IsNullOrWhiteSpace(request.AreaName)) errors.Add("An area is required.");
        if (!AllowedSeverities.Contains(request.Severity)) errors.Add("Severity must be Low, Medium, High or Critical.");
        if (request.CooldownMinutes < 0) errors.Add("Cooldown cannot be negative.");
        return errors;
    }
}
