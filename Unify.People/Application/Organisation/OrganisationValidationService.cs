using System.Net.Mail;
using Unify.People.Contracts.Organisation;

namespace Unify.People.Application.Organisation;

/// <summary>
/// Framework-free validation extracted from organisation-management pages.
/// Uniqueness checks belong to a future repository implementation.
/// </summary>
public sealed class OrganisationValidationService
{
    public ValidationResult Validate(CreateDepartmentRequest request)
    {
        var errors = new List<ValidationError>();

        if (request.TenantId == Guid.Empty)
            errors.Add(new("tenantId", "A tenant is required."));

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add(new("name", "A department name is required."));
        else if (request.Name.Trim().Length > 50)
            errors.Add(new("name", "A department name cannot exceed 50 characters."));

        return new(errors);
    }

    public ValidationResult Validate(CreateEmployeeRequest request)
    {
        var errors = new List<ValidationError>();

        if (request.TenantId == Guid.Empty)
            errors.Add(new("tenantId", "A tenant is required."));
        if (string.IsNullOrWhiteSpace(request.EmployeeCode))
            errors.Add(new("employeeCode", "An employee code is required."));
        if (string.IsNullOrWhiteSpace(request.DisplayName))
            errors.Add(new("displayName", "An employee name is required."));
        if (request.DepartmentId == Guid.Empty)
            errors.Add(new("departmentId", "A department is required."));
        if (request.DesignationId == Guid.Empty)
            errors.Add(new("designationId", "A designation is required."));

        if (!string.IsNullOrWhiteSpace(request.EmailAddress) &&
            !MailAddress.TryCreate(request.EmailAddress, out _))
        {
            errors.Add(new("emailAddress", "An email address is invalid."));
        }

        return new(errors);
    }
}
