using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Unify.CCC.Data;

namespace Unify.CCC.Services
{
    public class LatePolicyService
    {
        private readonly PeopleDb _peopleDb;
        private readonly ClaimsPrincipal _user;
        public LatePolicyService(IDbContextFactory<PeopleDb> dbContextFactory)
        {
            _peopleDb = dbContextFactory.CreateDbContext();
        }

        public async Task<LatePolicy> GetEmployeeLatePolicyAsync(Guid customerId, Guid employeeId)
        {
            try
            {
                LatePolicy latePolicy = new();

                latePolicy = await _peopleDb.LatePolicies
                    .Where(m => m.EmployeeId == employeeId && m.AppliesTo == AppliesTo.Employee && m.CustomerId == customerId).FirstOrDefaultAsync();

                if (latePolicy == null)
                {
                    var employee = await _peopleDb.Employees.Where(m => m.Id == employeeId).FirstOrDefaultAsync();

                    if (employee.DepartmentId != null && employee.DepartmentId != Guid.Empty)
                    {
                        latePolicy = await _peopleDb.LatePolicies
                        .Where(m => m.EmployeeId == employeeId && m.AppliesTo == AppliesTo.Department && m.DepartmentId == employee.DepartmentId && m.CustomerId == customerId).FirstOrDefaultAsync();
                    }
                }

                if (latePolicy == null)
                {
                    latePolicy = await _peopleDb.LatePolicies
                    .Where(m => m.CustomerId == customerId && m.AppliesTo == AppliesTo.All).FirstOrDefaultAsync();
                }
                return latePolicy;
            }
            catch
            {
                throw;
            }
        }

    }
}
