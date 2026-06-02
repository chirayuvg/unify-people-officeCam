using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Unify.Data;
using Unify.Data.SQL;

namespace Unify.People.Services
{
    public class LatePolicyService
    {
        private readonly PeopleDb _peopleDb;
        private readonly ClaimsPrincipal _user;
        public LatePolicyService(IDbContextFactory<PeopleDb> dbContextFactory)
        {
            _peopleDb = dbContextFactory.CreateDbContext();
        }

        public async Task<LatePolicy> GetEmployeeLatePolicyAsync(Guid TenantId, Guid employeeId)
        {
            try
            {
                LatePolicy latePolicy = new();

                latePolicy = await _peopleDb.LatePolicies
                    .Where(m => m.EmployeeId == employeeId && m.AppliesTo == AppliesTo.Employee && m.TenantId == TenantId).FirstOrDefaultAsync();

                if (latePolicy == null)
                {
                    var employee = await _peopleDb.Employees.Where(m => m.Id == employeeId).FirstOrDefaultAsync();

                    if (employee.DepartmentId != null && employee.DepartmentId != Guid.Empty)
                    //if (employee != null)
                    {
                        latePolicy = await _peopleDb.LatePolicies
                        .Where(m => m.EmployeeId == employeeId && m.AppliesTo == AppliesTo.Department && m.DepartmentId == employee.DepartmentId && m.TenantId == TenantId).FirstOrDefaultAsync();
                    }
                }

                if (latePolicy == null)
                {
                    latePolicy = await _peopleDb.LatePolicies
                    .Where(m => m.TenantId == TenantId && m.AppliesTo == AppliesTo.All).FirstOrDefaultAsync();
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

//public class Rootobject
//{
    
//    public Segmentsmodel[] segmentsModels { get; set; }
//    public Camerametadatamodel[] cameraMetaDataModels { get; set; }
//    public DateTime timeStamp { get; set; }
//    public string fileURL { get; set; }
//    public Class1[] classes { get; set; }
//}

//public class Segmentsmodel
//{
//    public string polygon { get; set; }
//    public Segmentmetadatamodel[] segmentMetaDataModels { get; set; }
//}

//public class Segmentmetadatamodel
//{
//    public string key { get; set; }
//    public string value { get; set; }
//}

//public class Camerametadatamodel
//{
//    public string key { get; set; }
//    public string value { get; set; }
//}

//public class Class1
//{
//    public string name { get; set; }
//    public string boundingBox { get; set; }
//    public string polygon { get; set; }
//    public string trackingId { get; set; }
//    public Property1[] properties { get; set; }
//}

//public class Property1
//{
//    public string propertyName { get; set; }
//    public string propertyValue { get; set; }
//}
