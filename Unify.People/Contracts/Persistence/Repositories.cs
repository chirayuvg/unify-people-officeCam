using Unify.People.Domain.Attendance;
using Unify.People.Domain.Organisation;
using Unify.People.Domain.Visitors;

namespace Unify.People.Contracts.Persistence;

/// <summary>
/// Persistence boundaries for the future SQL Server adapter. Application code
/// depends on these contracts rather than EF Core or a database provider.
/// </summary>
public interface IOrganisationRepository
{
    Task<bool> DepartmentNameExistsAsync(Guid tenantId, string name, CancellationToken cancellationToken = default);
    Task AddDepartmentAsync(Department department, CancellationToken cancellationToken = default);
    Task AddEmployeeAsync(Employee employee, CancellationToken cancellationToken = default);
}

public interface IAttendanceRepository
{
    Task AddAsync(AttendanceEntry attendanceEntry, CancellationToken cancellationToken = default);
}

public interface IVisitorAppointmentRepository
{
    Task AddAsync(VisitorAppointment appointment, CancellationToken cancellationToken = default);
    Task<VisitorAppointment?> FindAsync(Guid tenantId, Guid appointmentId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
