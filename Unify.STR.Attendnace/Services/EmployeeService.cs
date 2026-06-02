using AppStudio.Framework.ElasticSearch;
using DTC.Unify.HttpClientService;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Pgvector;
using Unify.STR.Attendance.Data.SQL;
using Unify.STR.Attendance.Data.SQL;
//using Unify.Attendance.Data.Elasticsearch;
//using Unify.People.ElasticSearch;


namespace Unify.STR.Attendance.Services
{
    //public class EmployeeService
    //{
    //    private readonly PeopleDb _peopleDb;
    //    private readonly AIPersonService _aIPersonService;
    //    private readonly FaceService _faceService;

    //    public EmployeeService(IDbContextFactory<PeopleDb> dbContextFactory, AIPersonService aIPersonService, FaceService faceService)
    //    {
    //        _peopleDb = dbContextFactory.CreateDbContext();
    //        _aIPersonService = aIPersonService;
    //        _faceService = faceService;
    //    }

    //    public async Task<Employee> GetEmployeeByIdAsync(Guid TenantId, Guid employeeId)
    //    {
    //        try
    //        {
    //            Employee empolyee = await _peopleDb.Employees.Include(m => m.EmployeeType)
    //                .Include(m => m.Department).Include(m => m.Designation)
    //                .Where(m => m.Id == employeeId && m.TenantId == TenantId).SingleOrDefaultAsync();

    //            return empolyee;
    //        }
    //        catch
    //        {
    //            throw;
    //        }
    //    }

    //    public async Task AddEmployeeAsync(Employee employee)
    //    {
    //        try
    //        {
    //            _peopleDb.Employees.AddAsync(employee);
    //        }
    //        catch
    //        {
    //            throw;
    //        }
    //    }

    //    public async Task DeleteEmployeeImages(Guid id)
    //    {
    //        try
    //        {
    //            var empImg = await _peopleDb.EmployeeImages.FindAsync(id);
    //            DeleteAIImageModel deleteAIImage = new();
    //            deleteAIImage.ImageId = empImg.Id.ToString();
    //            deleteAIImage.PersonId = empImg.EmployeeId.ToString();
    //            deleteAIImage.CollectionName = "Attendance";
    //            var data = await _aIPersonService.DeleteAIImage(deleteAIImage);

    //        }
    //        catch
    //        {

    //        }
    //    }
    //    public async Task SaveEmployeeEmbeddings(PersonImageRequest request)
    //    {
    //        try
    //        {
    //            var employee = await GetEmployeeByIdAsync(Guid.Parse(request.TenantId), Guid.Parse(request.PersonId));
    //            request.Name = employee.Name;
    //            request.PersonType = employee.EmployeeType.Name;
    //            request.Gender = employee.Gender.ToString();
    //            request.CollectionName = "Attendance";
    //            // get embedding from ai service
    //            var data = await _aIPersonService.SendImageAI(request);              
    //            Vector vector = new Vector(data);
    //            var empImg = await _peopleDb.EmployeeImages.FindAsync(Guid.Parse(request.ImageId));
    //            empImg.Embedding = vector;
    //            if (vector != null)
    //            {
    //                _peopleDb.EmployeeImages.Update(empImg);
    //                await _peopleDb.SaveChangesAsync();

    //            }
    //            else
    //            {
    //                _peopleDb.EmployeeImages.Remove(empImg);
    //                await _peopleDb.SaveChangesAsync();
    //                //return "Please upload valid Images";
    //            }
    //        }
    //        catch
    //        {
    //            throw;
    //        }
    //    }
    //}

    //public class EmployeeService
    //{
    //    private readonly PeopleDb _peopleDb;
    //    private readonly AIPersonService _aIPersonService;
    //    private readonly FaceService _faceService;

    //    public EmployeeService(IDbContextFactory<PeopleDb> dbContextFactory, AIPersonService aIPersonService, FaceService faceService)
    //    {
    //        _peopleDb = dbContextFactory.CreateDbContext();
    //        _aIPersonService = aIPersonService;
    //        _faceService = faceService;
    //    }

    //    public async Task<Employee> GetEmployeeByIdAsync(Guid TenantId, Guid employeeId)
    //    {
    //        try
    //        {
    //            Employee empolyee = await _peopleDb.Employees.Include(m => m.EmployeeType)
    //                .Include(m => m.Department).Include(m => m.Designation)
    //                .Where(m => m.Id == employeeId && m.TenantId == TenantId).SingleOrDefaultAsync();

    //            return empolyee;
    //        }
    //        catch
    //        {
    //            throw;
    //        }
    //    }

    //    public async Task AddEmployeeAsync(Employee employee)
    //    {
    //        try
    //        {
    //            _peopleDb.Employees.AddAsync(employee);
    //        }
    //        catch
    //        {
    //            throw;
    //        }
    //    }

    //    public async Task DeleteEmployeeImages(Guid id)
    //    {
    //        try
    //        {
    //            var empImg = await _peopleDb.EmployeeImages.FindAsync(id);
    //            DeleteAIImageModel deleteAIImage = new();
    //            deleteAIImage.ImageId = empImg.Id.ToString();
    //            deleteAIImage.PersonId = empImg.EmployeeId.ToString();
    //            deleteAIImage.CollectionName = "Attendance";
    //            var data = await _aIPersonService.DeleteAIImage(deleteAIImage);

    //        }
    //        catch
    //        {

    //        }
    //    }
    //    public async Task SaveEmployeeEmbeddings(PersonImageRequest request)
    //    {
    //        try
    //        {
    //            var employee = await GetEmployeeByIdAsync(Guid.Parse(request.TenantId), Guid.Parse(request.PersonId));
    //            request.Name = employee.Name;
    //            request.PersonType = employee.EmployeeType.Name;
    //            request.Gender = employee.Gender.ToString();
    //            request.CollectionName = "Attendance";
    //            // get embedding from ai service
    //            var data = await _aIPersonService.SendImageAI(request);
    //            Vector vector = new Vector(data);
    //            var empImg = await _peopleDb.EmployeeImages.FindAsync(Guid.Parse(request.ImageId));
    //            empImg.Embedding = vector;
    //            if (vector != null)
    //            {
    //                _peopleDb.EmployeeImages.Update(empImg);
    //                await _peopleDb.SaveChangesAsync();

    //            }
    //            else
    //            {
    //                _peopleDb.EmployeeImages.Remove(empImg);
    //                await _peopleDb.SaveChangesAsync();
    //                //return "Please upload valid Images";
    //            }
    //        }
    //        catch
    //        {
    //            throw;
    //        }
    //    }
    //}



    public class EmployeeService
    {
        private readonly PeopleDb _peopleDb;
        private readonly FaceService _faceService;

        public EmployeeService(IDbContextFactory<PeopleDb> dbContextFactory, FaceService faceService)
        {
            _peopleDb = dbContextFactory.CreateDbContext();
            
            _faceService = faceService;
        }

        public async Task<Employee> GetEmployeeByIdAsync(Guid TenantId, Guid employeeId)
        {
            try
            {
                Employee empolyee = await _peopleDb.Employees.Include(m => m.EmployeeType)
                    .Where(m => m.Id == employeeId && m.TenantId == TenantId).SingleOrDefaultAsync();

                return empolyee;
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
        public async Task<string> MapArea(float Latitude, float Longitude, string userId)
        {
            try
            {
                var emp = await _peopleDb.Employees
                       .Include(m => m.Beat).Where(m => m.Id == Guid.Parse(userId)).FirstOrDefaultAsync();
                var reader = new WKTReader();
                Geometry geometry = (Geometry)reader.Read(emp.Beat.Polygon);
                Polygon poly = geometry as Polygon;
                var geometryFactory = new GeometryFactory();


                var polygon = geometryFactory.CreatePolygon(poly.Coordinates);
                var point = geometryFactory.CreatePoint(new Coordinate(Longitude,Latitude));


                double distanceDegrees = point.Distance(polygon);

                const double earthRadiusMeters = 6371000;
                double meters = distanceDegrees * (Math.PI / 180) * earthRadiusMeters;
                double roundedMeters = Math.Round(meters);

                string message = string.Empty;
                if (roundedMeters > 0)
                {

                    message = "you are " + roundedMeters + " meters away from the " + emp.Beat.Name + " area";
                }
                else
                {
                    message = "you are in the " + emp.Beat.Name + " area";
                }
                return message;
            }
            catch
            {
                throw;
            }
        }

        //public async Task SaveEmployeeEmbeddings(PersonImageRequest request)
        //{
        //    try
        //    {
        //        var employee = await GetEmployeeByIdAsync(Guid.Parse(request.TenantId), Guid.Parse(request.PersonId));
        //        request.Name = employee.Name;
        //        request.PersonType = employee.EmployeeType.Name;
        //        request.Gender = employee.Gender.ToString();
        //        request.CollectionName = "Attendance";
        //        // get embedding from ai service
        //        var data = await _aIPersonService.SendImageAI(request);
        //        Vector vector = new Vector(data);
        //        var empImg = await _peopleDb.EmployeeImages.FindAsync(Guid.Parse(request.ImageId));
        //        empImg.Embedding = vector;
        //        if (vector != null)
        //        {
        //            _peopleDb.EmployeeImages.Update(empImg);
        //            await _peopleDb.SaveChangesAsync();

        //        }
        //        else
        //        {
        //            _peopleDb.EmployeeImages.Remove(empImg);
        //            await _peopleDb.SaveChangesAsync();
        //            //return "Please upload valid Images";
        //        }
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}
    }

}
