using Microsoft.EntityFrameworkCore;
using Unify.CCC.Data;

namespace Unify.CCC.Services
{
    public class EmployeeService
    {
        private readonly PeopleDb _peopleDb;

        public EmployeeService(IDbContextFactory<PeopleDb> dbContextFactory)
        {
            _peopleDb = dbContextFactory.CreateDbContext();
        }

        public async Task<Employee> GetEmployeeByIdAsync(Guid customerId, Guid employeeId)
        {
            try
            {
                Employee empolyee = await _peopleDb.Employees.Include(m => m.EmployeeType)
                    .Include(m => m.Department).Include(m => m.Designation)
                    .Where(m => m.Id == employeeId && m.CustomerId == customerId).SingleOrDefaultAsync();

                return empolyee;
            }
            catch
            {
                throw;
            }
        }
    }
}
