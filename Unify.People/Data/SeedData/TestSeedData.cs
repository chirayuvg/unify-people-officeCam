using AppStudio.Enterprise.Data;
using AppStudio.Enterprise.Services;
using AppStudio.Framework.Core;
using AppStudio.Framework.EntityFramework.Extensions;
using AppStudio.Model;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using System;
using System.Linq;
using System.Threading.Tasks;
using Unify.CCC.Data;
using Unify.People.Data.SQL;
using Unify.People.Services;

namespace Unify.Data.SQL
{
    public class TestSeedData : SeedData
    {
        public override async Task SeedDataAsync()
        {
            ////Tenant tenant = new Tenant
            ////{
            ////    Id = GuidHelper.EncodeStringToGuid("DTC"),
            ////    Name = "DTC",
            ////    IsActive = true,
            ////    TenantType = TenantType.Company
            ////};
            ////await Entity.UpsertAsync(tenant, m => m.Id == tenant.Id);

            ////TenantUser tenantUser = new()
            ////{
            ////    Id = Guid.NewGuid(),
            ////    Name = "Admin",
            ////    Email = "admin@dtc.com",
            ////    Password = "Dtc@123",
            ////    IsActive = true,
            ////    IsSuperAdmin = false,
            ////    TenantId = tenant.Id,
            ////    Roles = ["Admin"]
            ////};
            ////await Entity.UpsertAsync(tenantUser, m => m.TenantId == tenantUser.TenantId);

            ////Tenant tenant1 = new Tenant
            ////{
            ////    Id = GuidHelper.EncodeStringToGuid("STR"),
            ////    Name = "STR",
            ////    IsActive = true,
            ////    TenantType = TenantType.Company
            ////};
            ////await Entity.UpsertAsync(tenant1, m => m.Id == tenant1.Id);

            ////TenantUser tenantUser1 = new()
            ////{
            ////    Id = Guid.NewGuid(),
            ////    Name = "STR",
            ////    Email = "admin@str.com",
            ////    Password = "Str@123",
            ////    IsActive = true,
            ////    IsSuperAdmin = false,
            ////    TenantId = tenant1.Id,
            ////    Roles = ["Admin"]
            ////};
            ////await Entity.UpsertAsync(tenantUser1, m => m.TenantId == tenantUser1.TenantId);




            //////  Initial dummy records for testing. Comment out during Deployment.
            //////  ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
            ////var context = CoreServices.GetService<PeopleDb>();

            ////// Optional: use a transaction
            ////using var transaction = await context.Database.BeginTransactionAsync();

            ////try
            ////{
            ////    // Disable FK constraints temporarily (PostgreSQL example)
            ////    await context.Database.ExecuteSqlRawAsync("SET session_replication_role = 'replica';");

            ////    // Get all entity types in the DbContext
            ////    var entityTypes = context.Model.GetEntityTypes()
            ////        .Where(t => !t.IsOwned()) // Exclude owned types
            ////        .Select(t => t.GetTableName())
            ////        .Distinct()
            ////        .ToList();

            ////    // Delete from each table (in reverse dependency order if needed)
            ////    foreach (var tableName in entityTypes)
            ////    {
            ////        await context.Database.ExecuteSqlRawAsync($"DELETE FROM \"{tableName}\";");
            ////    }

            ////    // Re-enable FK constraints
            ////    await context.Database.ExecuteSqlRawAsync("SET session_replication_role = 'origin';");

            ////    await transaction.CommitAsync();
            ////}
            ////catch (Exception ex)
            ////{
            ////    await transaction.RollbackAsync();
            ////    Console.WriteLine("Error deleting data: " + ex.Message);
            ////    throw;
            ////}

            ////var context2 = CoreServices.GetService<ApplicationDb>();

            ////// Optional: use a transaction
            ////using var transaction2 = await context2.Database.BeginTransactionAsync();

            ////try
            ////{
            ////    // Disable FK constraints temporarily (PostgreSQL example)
            ////    await context2.Database.ExecuteSqlRawAsync("SET session_replication_role = 'replica';");

            ////    // Get all entity types in the DbContext
            ////    var entityTypes2 = context2.Model.GetEntityTypes()
            ////        .Where(t => !t.IsOwned()) // Exclude owned types
            ////        .Select(t => t.GetTableName())
            ////        .Distinct()
            ////        .ToList();

            ////    // Delete from each table (in reverse dependency order if needed)
            ////    foreach (var tableName in entityTypes2)
            ////    {
            ////        await context2.Database.ExecuteSqlRawAsync($"DELETE FROM \"{tableName}\";");
            ////    }

            ////    // Re-enable FK constraints
            ////    await context2.Database.ExecuteSqlRawAsync("SET session_replication_role = 'origin';");

            ////    await transaction2.CommitAsync();
            ////}
            ////catch (Exception ex)
            ////{
            ////    await transaction2.RollbackAsync();
            ////    Console.WriteLine("Error deleting data: " + ex.Message);
            ////    throw;
            ////}

            //////  Visitor Related Test records below. Comment out during Deployment
            //////  ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
            ////MeetingPlace meetingPlace = new()
            ////{
            ////    Id = Guid.NewGuid(),
            ////    Name = "Meeting Room",
            ////    TenantId = tenant1.Id,
            ////};
            ////await Entity.InsertAsync(meetingPlace);

            ////MeetingPurpose meetingPurpose = new()
            ////{
            ////    Id = Guid.NewGuid(),
            ////    Name = "Official",
            ////    TenantId = tenant1.Id,
            ////};
            ////await Entity.InsertAsync(meetingPurpose);

            ////Visitor visitor = new()
            ////{
            ////    Id = Guid.NewGuid(),
            ////    Name = "Visitor 1",
            ////    MobileNo = 1234567890,
            ////    Company = "Some Company",
            ////    Address = "Kolkata",
            ////    Email = "visitor1@gmail.com",
            ////    TenantId = tenant1.Id,
            ////};
            ////await Entity.InsertAsync(visitor);

            ////Shift shift = new()
            ////{
            ////    Id = Guid.NewGuid(),
            ////    Name = "General",
            ////    StartTime = new TimeOnly(9, 30),
            ////    EndTime = new TimeOnly(18, 30),
            ////    TenantId = tenant1.Id,
            ////};
            ////await Entity.InsertAsync(shift);

            ////EmployeeType employeeType = new()
            ////{
            ////    Id = Guid.NewGuid(),
            ////    Name = "Full-Time",
            ////    TenantId = tenant1.Id,
            ////};
            ////await Entity.InsertAsync(employeeType);

            ////Designation designation = new()
            ////{
            ////    Id = Guid.NewGuid(),
            ////    Name = "Senior Software Engineer",
            ////    TenantId = tenant1.Id,
            ////};
            ////await Entity.InsertAsync(designation);

            ////Department department = new()
            ////{
            ////    Id = Guid.NewGuid(),
            ////    Name = "Software Development",
            ////    TenantId = tenant1.Id,
            ////};
            ////await Entity.InsertAsync(department);

            ////Employee employee = new()
            ////{
            ////    Id = Guid.NewGuid(),
            ////    Name = "Dilip Patra",
            ////    Code = "E001",
            ////    Gender = Gender.Male,
            ////    DoB = new DateOnly(1975, 3, 13),
            ////    EmployeeTypeId = employeeType.Id,
            ////    DepartmentId = department.Id,
            ////    DesignationId = designation.Id,
            ////    ShiftId = shift.Id,
            ////    MobileNo = 8820023025,
            ////    Email = "Dilip.P@dtcinfotech.com",
            ////    TenantId = tenant1.Id,
            ////};
            ////await Entity.InsertAsync(employee);


            ////var optionsBuilder1 = new DbContextOptionsBuilder<Unify.People.Data.PeopleDb.PeopleDb>();
            ////optionsBuilder1.UseSqlServer("Data Source=5.161.232.13,1433;Initial Catalog=PeopleDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True");  // Replace with your actual connection string

            //////optionsBuilder1.UseNpgsql("Host=192.168.0.111;Database=ApplicationDb;Port=5432;username=YOUR_USER;password=YOUR_PASSWORD");  // Replace with your actual connection string

            ////var dbContext1 = new Unify.People.Data.PeopleDb.PeopleDb(optionsBuilder1.Options);

            ////var optionsBuilder2 = new DbContextOptionsBuilder<CCCDb>();
            ////optionsBuilder2.UseSqlServer("Data Source=5.161.232.13,1433;Initial Catalog=CCCDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True");  // Replace with your actual connection string

            ////var dbContext2 = new CCCDb(optionsBuilder2.Options);
            ////var _dbContext1 = dbContext1;
            ////var _dbContext2 = dbContext2;
            ////var random = new Random();

            //////var genders = new List<string> { "Male", "Female", "Other" };
            //////var employeeTypes = new List<string> { "Full-Time", "Intern", "Contract" };
            //////var shifts = new List<string> { "General" };
            ////var actions = new List<string> { "CheckIn", "CheckOut" };
            //////var departments = new List<string> { "Sales", "Admin", "Product", "Service", "Talent Acquisition", "Digital Reality" };
            //////var designations = new List<string> { "HR Manager", "Software Engineer", "Sr. Software Engineer", "Intern", "UI/UX Engineer", "Test Engineer" };
            //////var cameranames = new List<string> { "Entry Camera", "Exit Camera" };
            ////var employee2 = _dbContext1.Employees.ToList();
            ////var employeenames = _dbContext1.Employees.Include(e => e.EmployeeImages).Include(e => e.Department).Include(e => e.Designation).Include(e => e.EmployeeType).ToList();
            ////var employeeimages = _dbContext1.EmployeeImages.ToList();
            //////var namewiseimage = employeenames.Where(e => e.EmployeeImages[0] = employeeimages)
            ////var employeeids = _dbContext1.Employees.Include(e => e.Department).Include(e => e.Designation).Include(e => e.EmployeeType).Include(e => e.EmployeeImages).ToList();
            ////var departments = _dbContext1.Departments.ToList();
            ////var designations = _dbContext1.Designations.ToList();
            ////var genders = _dbContext1.Employees.Include(e => e.Department).Include(e => e.Designation).Include(e => e.EmployeeType).Include(e => e.EmployeeImages).ToList();
            //////var cameras = _dbContext2.Cameras.Select(e => e.Name).ToList();
            ////var cameranames = _dbContext2.Cameras.ToList();
            ////var intelligenceids = _dbContext2.Intelligences.ToList();
            ////var intelligencenames = _dbContext2.Intelligences.ToList();
            ////var shifts = _dbContext1.Shifts.Select(e => e.Name).Distinct().ToList();
            ////var employeetypes = _dbContext1.EmployeeTypes.Select(e => e.Name).Distinct().ToList();
            ////var matchingCameraNames = cameranames
            ////    .Where(e => e.CustomerId == Guid.Parse("b92cad78-e612-4433-a35c-f599bcd011f7"))
            ////    .Select(e => e.Name)
            ////    .ToList();  // Convert to List to get Count and access elements randomly
            ////var matchingCameraIds = cameranames
            ////    .Where(e => e.CustomerId == Guid.Parse("b92cad78-e612-4433-a35c-f599bcd011f7"))
            ////    .Select(e => e.Id)
            ////    .ToList();

            ////int dayOfJanuary = random.Next(1, 32); // Random day between 1 and 31
            ////DateTime randomDate = new DateTime(DateTime.Now.Year, 1, dayOfJanuary);

            ////int randomHour = random.Next(9, 20); // Random hour between 9 and 19 (7 PM is 19:00)
            ////int randomMinute = random.Next(0, 60); // Random minute between 0 and 59
            ////int randomSecond = random.Next(0, 60); // Random second between 0 and 59

            ////TimeSpan randomTime = new TimeSpan(randomHour, randomMinute, randomSecond);

            ////var startDate = new DateTime(2025, 1, 1);
            ////var endDate = DateTime.Now;

            ////int totalDays = (endDate - startDate).Days + 1; // +1 to include both start and end dates

            ////var dummyData = new List<AttendanceLog>();

            ////foreach (var name in employeenames.Select(e => e.Name))
            ////{
            ////    for (int day = 1; day <= totalDays; day++) // 31 days in January
            ////    {
            ////        var date = startDate.AddDays(day); // Generate a date for each day in January

            ////        // Generate random time between 9 AM and 7 PM
            ////        var timestamps = GenerateMultipleTimestamps(date);

            ////        foreach (var timeStamp in timestamps)
            ////        {
            ////            dummyData.Add(new AttendanceLog
            ////            {
            ////                Id = Guid.NewGuid(),
            ////                DateTime = timeStamp,
            ////                TimeStamp = timeStamp,

            ////                TenantId = Guid.Parse("eaf5497e-bf79-0f00-f075-c265a1eade45"),
            ////                IntelligenceId = intelligenceids
            ////                    .Where(e => e.Name == "Attendance")
            ////                    .Select(e => e.Id)
            ////                    .Distinct()
            ////                    .FirstOrDefault(),

            ////                CameraId = matchingCameraIds.Any()
            ////                    ? matchingCameraIds[random.Next(matchingCameraIds.Count)]
            ////                    : null,

            ////                EmployeeId = employeeids
            ////                    .Where(e => e.Name == name)
            ////                    .Select(e => e.Id)
            ////                    .Distinct()
            ////                    .FirstOrDefault(),

            ////                IntelligenceName = "Attendance",
            ////                CameraName = matchingCameraNames.Any()
            ////                    ? matchingCameraNames[random.Next(matchingCameraNames.Count)]
            ////                    : "DefaultCamera",

            ////                Code = $"EMP-{random.Next(1000, 9999)}",
            ////                Name = name,
            ////                EmployeeType = "Full-Time",

            ////                Department = employeenames
            ////                    .Where(e => e.Name == name)
            ////                    .Select(e => e.Department.Name)
            ////                    .Distinct()
            ////                    .FirstOrDefault() ?? string.Empty,

            ////                Designation = employeenames
            ////                    .Where(e => e.Name == name)
            ////                    .Select(e => e.Designation.Name)
            ////                    .Distinct()
            ////                    .FirstOrDefault() ?? string.Empty,

            ////                //EmployeeUnit = "", // Add value if applicable

            ////                Gender = employeenames
            ////                    .Where(e => e.Name == name)
            ////                    .Select(e => e.Gender.ToString())
            ////                    .FirstOrDefault() ?? "Unknown",

            ////                Shift = "General",
            ////                StartTime = TimeOnly.FromDateTime(DateTime.Today.AddHours(9).AddMinutes(30)),
            ////                EndTime = TimeOnly.FromDateTime(DateTime.Today.AddHours(18).AddMinutes(30)),

            ////                BoundingBox = $"[{random.Next(100, 600)},{random.Next(100, 600)},{random.Next(100, 600)},{random.Next(100, 600)}]",
            ////                Action = actions.Any() ? actions[random.Next(actions.Count)] : "DefaultAction",
            ////                Time = (decimal)random.Next(400, 590),

            ////                FirstLogin = random.Next(0, 2) == 1,
            ////                LateLogin = random.Next(0, 2) == 1,
            ////                LateLoginMinutes = Math.Round((decimal)(random.NextDouble() * 30), 2),

            ////                EarlyLogout = random.Next(0, 2) == 1,
            ////                EarlyLogoutMinutes = Math.Round((decimal)(random.NextDouble() * 20), 2),

            ////                ManualEntry = random.Next(0, 2) == 1,
            ////                Reason = "null",

            ////                LastLog = random.Next(0, 2) == 1,
            ////                SimilarityValue = random.Next(0, 2) == 0 ? $"{random.Next(300, 400)}%" : string.Empty,
            ////                FaceCover = "faceImage",

            ////                Image = "https://example.com/image.jpg", // Replace with actual URL or logic
            ////                                                         //Latitude = (float)(random.NextDouble() * 180 - 90),   // -90 to +90
            ////                                                         //Longitude = (float)(random.NextDouble() * 360 - 180)  //
            ////                                                         //-180 to +180

            ////                Embedding = GenerateRandomVector(512), // returns Vector
            ////                Latitude = (float)(random.NextDouble() * 180 - 90),     // -90 to +90
            ////                Longitude = (float)(random.NextDouble() * 360 - 180),   // -180 to +180

            ////            });
            ////        }

            ////    }
            ////}

            ////const int batchSize = 1000;
            ////int totalCount = dummyData.Count;
            ////var context3 = CoreServices.GetService<PeopleDb>();

            ////for (int i = 0; i < totalCount; i += batchSize)
            ////{
            ////    var batch = dummyData.Skip(i).Take(batchSize).ToList();

            ////    //Get primary keys or unique keys used to detect existing records
            ////    var batchIds = batch.Select(x => x.Id).ToList(); // Modify if using composite key

            ////    // Load existing records from the database
            ////    var existingEntries = await context3.AttendanceLogs
            ////        .Where(x => batchIds.Contains(x.Id))
            ////        .ToDictionaryAsync(x => x.Id);

            ////    var newRecords = new List<AttendanceLog>(); // To hold records for AddRangeAsync

            ////    foreach (var item in batch)
            ////    {
            ////        if (existingEntries.TryGetValue(item.Id, out var existing))
            ////        {
            ////            // Update existing entity
            ////            existing.DateTime = item.DateTime;
            ////            existing.TimeStamp = item.TimeStamp;
            ////            existing.CameraId = item.CameraId;
            ////            existing.EmployeeId = item.EmployeeId;
            ////            existing.IntelligenceName = item.IntelligenceName;
            ////            existing.CameraName = item.CameraName;
            ////            existing.Code = item.Code;
            ////            existing.Name = item.Name;
            ////            existing.EmployeeType = item.EmployeeType;
            ////            existing.Department = item.Department;
            ////            existing.Designation = item.Designation;
            ////            existing.Gender = item.Gender;
            ////            existing.Shift = item.Shift;
            ////            existing.StartTime = item.StartTime;
            ////            existing.EndTime = item.EndTime;
            ////            existing.BoundingBox = item.BoundingBox;
            ////            existing.Action = item.Action;
            ////            existing.Time = item.Time;
            ////            existing.FirstLogin = item.FirstLogin;
            ////            existing.LateLogin = item.LateLogin;
            ////            existing.LateLoginMinutes = item.LateLoginMinutes;
            ////            existing.EarlyLogout = item.EarlyLogout;
            ////            existing.EarlyLogoutMinutes = item.EarlyLogoutMinutes;
            ////            existing.ManualEntry = item.ManualEntry;
            ////            existing.Reason = item.Reason;
            ////            existing.LastLog = item.LastLog;
            ////            existing.SimilarityValue = item.SimilarityValue;
            ////            existing.FaceCover = item.FaceCover;
            ////            existing.Image = item.Image;
            ////            existing.Embedding = item.Embedding;
            ////            existing.Latitude = item.Latitude;
            ////            existing.Longitude = item.Longitude;
            ////        }
            ////        else
            ////        {
            ////            // Queue up new record for batch insert
            ////            newRecords.Add(item);
            ////        }
            ////    }

            ////    // Insert new records in bulk
            ////    if (newRecords.Count > 0)
            ////    {
            ////        await context3.AttendanceLogs.AddRangeAsync(newRecords);
            ////    }

            ////    // Save all changes (inserts + updates)
            ////    await context3.SaveChangesAsync();
            ////    context3.ChangeTracker.Clear(); // Clear EF tracking to reduce memory usage
            ////}
        }

        private static Vector GenerateRandomVector(int size)
        {
            var random = new Random();
            var values = new float[size];

            for (int i = 0; i < size; i++)
            {
                values[i] = (float)(random.NextDouble() * 2 - 1); // random float between -1 and 1
            }

            return new Vector(values);
        }

        public DateTime GenerateRandomTimeStamp(DateTime date)
        {
            var random = new Random();
            var randomHour = random.Next(9, 19); // Random hour between 9 AM and 7 PM
            var randomMinute = random.Next(0, 60); // Random minute
            var randomSecond = random.Next(0, 60); // Random second
            return date.AddHours(randomHour).AddMinutes(randomMinute).AddSeconds(randomSecond);
        }

        public List<DateTime> GenerateMultipleTimestamps(DateTime date)
        {
            var random = new Random();
            var timestamps = new List<DateTime>();

            int numEntries = random.Next(4, 10); // Generate 4 to 10 timestamps per day

            for (int i = 0; i < numEntries; i++)
            {
                var randomHour = random.Next(9, 19); // Between 9 AM and 7 PM
                var randomMinute = random.Next(0, 60);
                var randomSecond = random.Next(0, 60);

                timestamps.Add(date.AddHours(randomHour).AddMinutes(randomMinute).AddSeconds(randomSecond));
            }

            return timestamps.OrderBy(t => t).ToList();
        }
    }
}
