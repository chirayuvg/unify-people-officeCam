using DTC.Unify.HttpClientService;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Unify.Data;
using Unify.Data.SQL;
using static DTC.Unify.HttpClientService.AIPersonService;

namespace Unify.People.Services
{
    public class EmployeeService
    {
        private readonly PeopleDb _peopleDb;
        private readonly AIPersonService _aIPersonService;
        private readonly FaceService _faceService;

        public EmployeeService(IDbContextFactory<PeopleDb> dbContextFactory, AIPersonService aIPersonService, FaceService faceService)
        {
            _peopleDb = dbContextFactory.CreateDbContext();
            _aIPersonService = aIPersonService;
            _faceService = faceService;
        }

        public async Task<Employee> GetEmployeeByIdAsync(Guid TenantId, Guid employeeId)
        {
            try
            {
                Employee employee = await _peopleDb.Employees
                    .Include(m => m.EmployeeType)
                    .Include(m => m.Department)
                    .Include(m => m.Designation)
                    .Where(m =>
                        m.TenantId == TenantId
                        &&
                        m.Id == employeeId
                    ).SingleOrDefaultAsync();

                return employee;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Employee> GetEmployeeByEmailAsync(Guid TenantId, string Email)
        {
            try
            {
                Employee employee = await _peopleDb.Employees
                    .Include(m => m.EmployeeType)
                    .Include(m => m.Department)
                    .Include(m => m.Designation)
                    .Where(m =>
                        m.TenantId == TenantId
                        &&
                        m.Email.ToLower() == Email.ToLower()
                    ).SingleOrDefaultAsync();

                return employee;
            }
            catch
            {
                throw;
            }
        }

        public async Task AddEmployeeAsync(Employee employee)
        {
            try
            {
                _peopleDb.Employees.AddAsync(employee);
            }
            catch
            {
                throw;
            }
        }

        public async Task DeleteEmployeeImages(Guid id)
        {
            try
            {
                var empImg = await _peopleDb.EmployeeImages.FindAsync(id);
                DeleteAIImageModel deleteAIImage = new();
                deleteAIImage.ImageId = empImg.Id.ToString();
                deleteAIImage.PersonId = empImg.EmployeeId.ToString();
                deleteAIImage.CollectionName = "Attendance";
                var data = await _aIPersonService.DeleteAIImage(deleteAIImage);

            }
            catch
            {

            }
        }
    }
}
