namespace Unify.People.Domain.Organisation;

public sealed class Department
{
    public Department(Guid id, Guid tenantId, string name)
    {
        Id = id;
        TenantId = tenantId;
        Name = name;
    }

    public Guid Id { get; }
    public Guid TenantId { get; }
    public string Name { get; private set; }

    public void Rename(string name) => Name = name;
}

public sealed class Employee
{
    public Employee(Guid id, Guid tenantId, string employeeCode, string displayName)
    {
        Id = id;
        TenantId = tenantId;
        EmployeeCode = employeeCode;
        DisplayName = displayName;
    }

    public Guid Id { get; }
    public Guid TenantId { get; }
    public string EmployeeCode { get; }
    public string DisplayName { get; private set; }
    public Guid? DepartmentId { get; private set; }
    public Guid? DesignationId { get; private set; }

    public void AssignOrganisation(Guid departmentId, Guid designationId)
    {
        DepartmentId = departmentId;
        DesignationId = designationId;
    }
}
