namespace Unify.People.Contracts.Organisation;

public sealed record CreateDepartmentRequest(
    Guid TenantId,
    string Name);

public sealed record CreateEmployeeRequest(
    Guid TenantId,
    string EmployeeCode,
    string DisplayName,
    string EmailAddress,
    Guid DepartmentId,
    Guid DesignationId);

public sealed record ValidationError(string Field, string Message);

public sealed record ValidationResult(IReadOnlyCollection<ValidationError> Errors)
{
    public bool IsValid => Errors.Count == 0;

    public static ValidationResult Success() => new([]);
}
