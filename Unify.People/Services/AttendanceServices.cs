using AppStudio.Enterprise.Data;
using AppStudio.Framework.Core;
using DTC.Unify.HttpClientService;
using JasperFx.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using SixLabors.ImageSharp;
using Unify.Data;
using Unify.Data.SQL;
using Unify.People.Controllers.Attendance;
using Unify.People.Controllers.Regularization;
using Unify.People.RulesEngine;
using AttendanceLog = Unify.Data.SQL.AttendanceLog;

namespace Unify.People.Services
{
    public class AttendanceServices
    {
        private readonly PeopleDb _dbContext;
        private readonly IWebHostEnvironment _env;
        private readonly FaceService _faceService;

        public AttendanceServices(IDbContextFactory<PeopleDb> dbContextFactory, FaceService faceService, IWebHostEnvironment env)
        {
            _dbContext = dbContextFactory.CreateDbContext();
            _env = env;
            _faceService = faceService;
        }

        public async Task LoginAsync(AttModel model)
        {
            try
            {
                var tenentId = CoreServices.CurrentUser.GetTenantId();
                Guid? empId = null;
                float[] embedding;

                if (tenentId == Guid.Empty)
                {
                    tenentId = model.TenantId ?? Guid.Empty;
                    //embedding = model.Embedding;
                    embedding = await _faceService.GetEmbeddings(model.Image);

                    if (embedding != null && embedding.Length != 0)
                    {
                        empId = await GetEmployeeId(embedding);
                    }
                }
                else
                {
                    embedding = model.Embedding;
                    empId = await GetEmployeeId(embedding);
                }

                Vector vector = new Vector(embedding);

                AttendanceLog attendanceLog = new()
                {
                    Id = Guid.NewGuid(),
                    DateTime = DateTime.UtcNow.AddHours(5).AddMinutes(30),
                    TimeStamp = DateTime.UtcNow.AddHours(5).AddMinutes(30),
                    TenantId = tenentId,
                    IntelligenceId = Guid.Parse("AE2EE380-56D6-4C9F-8DDB-08DD23E0CD5A"),
                    IntelligenceName = "Attendance",
                    CameraId = Guid.Parse("3CD15053-3022-4C11-969A-4ED98D253B55"),
                    CameraName = "Emp",
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                    Embedding = vector,
                    Image = "Image.png",
                    Code = string.Empty,
                    Name = string.Empty,
                    Designation = string.Empty,
                    Department = string.Empty,
                    EmployeeType = string.Empty,
                    Gender = string.Empty,
                    Action = model.Action,
                };

                if (empId != null)
                {
                    var emp = await Entity.Query<Employee>()
                        .Include(m => m.Department)
                        .Include(m => m.Designation)
                        .Include(m => m.EmployeeType)
                        .FirstOrDefaultAsync(m => m.Id == empId);

                    attendanceLog.EmployeeId = emp.Id;
                    attendanceLog.Name = emp.Name;
                    attendanceLog.Code = emp.Code;
                    attendanceLog.Designation = emp.Designation?.Name;
                    attendanceLog.Department = emp.Department?.Name;
                    attendanceLog.EmployeeType = emp.EmployeeType?.Name;
                    attendanceLog.Gender = emp.Gender.ToString();

                    LatePolicy latePolicy = await GetEmployeeLatePolicyAsync(tenentId, emp.Id);

                    if (attendanceLog.Action.ToLower() == "checkin")
                    {
                        var loggedIn = await LoginCheckAsync(attendanceLog.TenantId, attendanceLog.EmployeeId, attendanceLog.TimeStamp);

                        if (loggedIn == false)
                        {
                            // previous day get first log
                            // previous day early logout
                            var lastLog = await LastLogAsync(attendanceLog.TenantId, attendanceLog.EmployeeId);

                            if (lastLog != null)
                            {
                                var earlyLogoutMinutes =
                                    await EarlyLogoutMinutesAsync(attendanceLog.TenantId, attendanceLog.EmployeeId, lastLog.TimeStamp);

                                if (earlyLogoutMinutes > 0)
                                {
                                    await UpdateEarlyLogoutAsync(lastLog.Id, lastLog.TimeStamp, "true");
                                }
                            }

                            attendanceLog.FirstLogin = true;
                            // late check
                            TimeOnly timeOnly = TimeOnly.FromDateTime(attendanceLog.TimeStamp);
                            TimeSpan loginDiff = TimeSpan.Zero;

                            //var shift = await GetShiftAsync(TenantId, timeOnly);
                            var shift = await GetShiftAsync(emp.TenantId, emp.ShiftId);

                            if (shift != null)
                            {
                                //Console.WriteLine("TimeStamp " + attendanceLog.TimeStamp);
                                //Console.WriteLine("Time " + timeOnly);
                                //loginDiff = timeOnly.ToTimeSpan() - shift.StartTime.ToTimeSpan();
                                loginDiff = timeOnly.ToTimeSpan() - shift.StartTime.ToTimeSpan();
                                var lateMinutes = (decimal)loginDiff.TotalMinutes;

                                attendanceLog.Shift = shift.Name;
                                attendanceLog.StartTime = shift.StartTime;
                                attendanceLog.EndTime = shift.EndTime;
                                //Console.WriteLine(lateMinutes);
                                //Console.WriteLine(shift.Name);

                                if (latePolicy != null)
                                {
                                    if (lateMinutes > latePolicy.LateLogin)
                                    {
                                        attendanceLog.LateLogin = true;
                                        attendanceLog.LateLoginMinutes = lateMinutes;
                                    }
                                    else
                                    {
                                        attendanceLog.LateLogin = false;
                                        attendanceLog.LateLoginMinutes = 0;
                                    }
                                }
                            }
                        }
                        else
                        {
                            attendanceLog.FirstLogin = false;
                            attendanceLog.LateLogin = false;
                            attendanceLog.LateLoginMinutes = 0;
                            attendanceLog.EarlyLogout = false;
                            attendanceLog.EarlyLogoutMinutes = 0;
                        }
                    }
                    else if (attendanceLog.Action.ToLower() == "checkout")
                    {
                        // pending early logout
                        // consider DateTime as parameter
                        var employeeInTime = await GetEmployeeInTimeAsync(attendanceLog.EmployeeId, attendanceLog.DateTime);
                        attendanceLog.Time = employeeInTime;
                    }

                    model.EmployeeName = emp.Name;
                    model.EmployeeCode = emp.Code;
                    model.Designation = emp.Designation?.Name;
                    model.Department = emp.Department?.Name;
                    model.EmployeeType = emp.EmployeeType?.Name;
                    model.Gender = emp.Gender.ToString();

                    //  Update DoorStatus to open the door
                    var doorStatus = await _dbContext.DoorStatuses.FirstOrDefaultAsync(m => m.TenantId == tenentId);
                    doorStatus.Status = 1;
                    await Entity.UpdateAsync(doorStatus.Id, doorStatus);
                }

                var files = new FormFileCollection
                {
                    model.Image
                };

                var res = await Entity.InsertAsync(attendanceLog, files);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<Guid?> GetEmployeeId(float[] embeddings)
        {
            try
            {
                Vector capturedVector = new Vector(embeddings);

                var items = await _dbContext.EmployeeImages
                    .Select(m => new { Entity = m, Distance = m.Embedding!.CosineDistance(capturedVector) })
                    .Where(m => m.Distance < 0.50)    // 75%
                    //.Where(m => m.Distance < 0.25)    // >87%
                    //.Where(m => m.Distance < 0.10)    // 95%
                    .OrderBy(m => m.Distance)
                    .Select(m => new { m.Entity.EmployeeId, m.Distance, m.Entity.Embedding })
                    .Take(3)
                    .ToListAsync();

                bool allIdsSame = items.Select(m => m.EmployeeId).Distinct().Count() == 1;

                if (allIdsSame)
                {
                    var firstItem = items.FirstOrDefault();
                    var cosineSimilarityResult = EmbeddingComparer.CosineSimilarity(
                        embeddings.Select(f => (double)f).ToArray()
                        , firstItem.Embedding.ToArray().Select(f => (double)f).ToArray()
                    );

                    if (cosineSimilarityResult >= 0.5)
                        return items.FirstOrDefault()?.EmployeeId;
                    else
                        return null;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<decimal> GetEmployeeInTimeAsync(Guid employeeId, DateTime checkOutTime)
        {
            var toDay = checkOutTime.Date;
            var logs = await _dbContext.AttendanceLogs
                .Where(m => m.EmployeeId == employeeId && m.DateTime.Date == toDay && m.DateTime <= checkOutTime)
                .OrderBy(m => m.DateTime)
                .ToListAsync();
            DateTime? validCheckIn = null;
            bool isCheckInMatched = false;
            foreach (var log in logs)
            {
                if (log.Action == "CheckIn" && validCheckIn == null)
                {
                    validCheckIn = log.DateTime;
                }
                else if (log.Action == "CheckOut" && validCheckIn != null)
                {
                    //  CheckIn already matched with a CheckOut — reset
                    validCheckIn = null;
                }
            }
            //  Use the earliest unmatched CheckIn (before this CheckOut)
            if (validCheckIn != null)
            {
                decimal workingMinutes = (decimal)Math.Round((checkOutTime - validCheckIn.Value).TotalMinutes, 0);
                return workingMinutes;
            }
            return 0;
        }

        private async Task<Shift> GetShiftAsync(Guid TenantId, Guid? shiftId)
        {
            try
            {
                //Department department = await _peopleDb.Departments.Where(m => m.Id == departmentId).SingleOrDefaultAsync();
                Shift shift = await _dbContext.Shifts.Where(m => m.Id == shiftId).SingleOrDefaultAsync();
                return shift;
            }
            catch
            {
                throw;
            }
        }

        private async Task UpdateLastLogAsync(LastLog lastLog)
        {
            Guid Id = Guid.Parse(lastLog.Id);
            AttendanceLog attendanceLog = await _dbContext.AttendanceLogs.FindAsync(Id);
            attendanceLog.LastLog = true;
            _dbContext.AttendanceLogs.Update(attendanceLog);
            _dbContext.SaveChanges();
        }

        private async Task UpdateEarlyLogoutAsync(string id, DateTime timeStamp, string lastLog)
        {
            int day = timeStamp.Day;
            int month = timeStamp.Month;
            int year = timeStamp.Year;

            var date = new DateTime(year, month, day, 0, 0, 0);
            var ldate = new DateTime(year, month, day, 23, 59, 59);

            string strDate = date.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string strlDate = ldate.ToString("yyyy-MM-ddTHH:mm:ssZ");

            AttendanceLog attendanceLog = await _dbContext.AttendanceLogs.FindAsync(Guid.Parse(id));
            attendanceLog.EarlyLogout = true;
            _dbContext.AttendanceLogs.Update(attendanceLog);
            _dbContext.SaveChanges();
        }

        private async Task<LatePolicy> GetEmployeeLatePolicyAsync(Guid TenantId, Guid employeeId)
        {
            try
            {
                LatePolicy latePolicy = new();

                latePolicy = await _dbContext.LatePolicies
                    .Where(m => m.EmployeeId == employeeId && m.AppliesTo == AppliesTo.Employee && m.TenantId == TenantId).FirstOrDefaultAsync();

                if (latePolicy == null)
                {
                    var employee = await _dbContext.Employees.Where(m => m.Id == employeeId).FirstOrDefaultAsync();

                    //if (employee.DepartmentId != null && employee.DepartmentId != Guid.Empty)
                    if (employee != null)
                    {
                        latePolicy = await _dbContext.LatePolicies
                        .Where(m => m.EmployeeId == employeeId && m.AppliesTo == AppliesTo.Department && m.DepartmentId == employee.DepartmentId && m.TenantId == TenantId).FirstOrDefaultAsync();
                    }
                }

                if (latePolicy == null)
                {
                    latePolicy = await _dbContext.LatePolicies
                    .Where(m => m.TenantId == TenantId && m.AppliesTo == AppliesTo.All).FirstOrDefaultAsync();
                }
                return latePolicy;
            }
            catch
            {
                throw;
            }
        }

        private async Task<bool> LoginCheckAsync(Guid TenantId, Guid employeeId, DateTime dateTime)
        {
            try
            {
                var response = await _dbContext.AttendanceLogs.Where(m => m.EmployeeId == employeeId && m.TenantId == TenantId
                && m.TimeStamp.Date == dateTime.Date && m.TimeStamp < dateTime && m.Action == "CheckIn").FirstOrDefaultAsync();

                if (response != null)
                {
                    return true;
                }

                return false;
            }
            catch
            {
                throw;
            }
        }

        private async Task<int> EarlyLogoutMinutesAsync(Guid TenantId, Guid employeeId, DateTime timeStamp)
        {
            // timeStamp is last log timeStamp
            string query = string.Empty;

            int day = timeStamp.Day;
            int month = timeStamp.Month;
            int year = timeStamp.Year;

            DateTime firstLoginTimeStamp = DateTime.Now;
            TimeOnly startTime = new TimeOnly();
            TimeOnly endTime = new TimeOnly();
            // get first log
            var response1 = await _dbContext.AttendanceLogs.Where(m => m.EmployeeId == employeeId && m.TenantId == TenantId
            && m.FirstLogin == true && m.TimeStamp.Date == timeStamp.Date).Take(1).ToListAsync();
            var response2 = await _dbContext.AttendanceLogs.Where(m => m.EmployeeId == employeeId && m.TenantId == TenantId
&& m.FirstLogin == false && m.TimeStamp.Date == timeStamp.Date).OrderByDescending(m => m.TimeStamp).Take(1).ToListAsync();

            if (response2.Count == 0)
            {
                return 0;
            }
            if (response1.Count > 0)
            {
                firstLoginTimeStamp = response1[0].DateTime;
                startTime = TimeOnly.FromDateTime(response1[0].TimeStamp);
                endTime = TimeOnly.FromDateTime(response2[0].TimeStamp);
            }
            // earlylogout
            TimeSpan loginDiff = TimeSpan.Zero;
            TimeSpan logoutDiff = TimeSpan.Zero;
            startTime = new TimeOnly(startTime.Hour, startTime.Minute);
            endTime = new TimeOnly(endTime.Hour, endTime.Minute);

            TimeOnly firstLoginTimeOnly = startTime;
            TimeOnly lastLogoutTimeOnly = endTime;
            TimeOnly start = new TimeOnly(9, 30);    // 09:30 AM
            TimeOnly end = new TimeOnly(18, 30);     // 06:30 PM
            start = new TimeOnly(start.Hour, start.Minute);
            end = new TimeOnly(end.Hour, end.Minute);

            int earlyLogoutMinutes = 0;
            loginDiff = startTime - start; // 9-7, 9 - 10


            var inMinutes = (int)loginDiff.TotalMinutes;
            // check for negative for lateMinutes

            if (end > endTime)
            {
                logoutDiff = end - endTime; // 6 - 5
            }
            else if (end < endTime)
            {
                logoutDiff = endTime - end; // 6 - 5
            }

            //logoutDiff = endTime - end; // 6 - 5
            var outMinutes = (int)logoutDiff.TotalMinutes;
            //+  10 - (+10) earlyin earlyout , -10 - (+10) latein earlyout , , -10 - (-10) latein, not earlyout
            //earlyLogoutMinutes = inMinutes - outMinutes;

            if (outMinutes < 0)
            {
                outMinutes = -1 * (outMinutes);
            }
            else
            {
                outMinutes = 1 * (outMinutes);
            }
            return outMinutes;
        }

        private async Task<LastLog?> LastLogAsync(Guid TenantId, Guid employeeId)
        {
            LastLog lastLog = new();
            DateTime timeStamp = DateTime.Now;
            string query = string.Empty;
            AttendanceLog attendanceLog = await _dbContext.AttendanceLogs.Where(m => m.EmployeeId == employeeId && m.TenantId == TenantId).OrderByDescending(m => m.TimeStamp).FirstOrDefaultAsync();

            if (attendanceLog != null)
            {
                lastLog.Id = attendanceLog.Id.ToString();
                lastLog.TimeStamp = attendanceLog.DateTime;

                return lastLog;
            }
            else
            {
                return null;
            }
        }

        public async Task<Vector> AssignEmployeeToAttendanceLog(string attendanceLogId, Employee employee)
        {
            var attendanceLogRep = CoreServices.GetRepository<Unify.Data.SQL.AttendanceLog>();
            Guid id = Guid.Parse(attendanceLogId);
            var attendanceLog = await attendanceLogRep.FindByIdAsync(id);

            var empTypeRep = CoreServices.GetRepository<EmployeeType>();
            var empType = await empTypeRep.FindByIdAsync(employee.EmployeeTypeId);
            var departmentRep = CoreServices.GetRepository<Department>();
            var department = await departmentRep.FindByIdAsync(employee.DepartmentId);
            var designationRep = CoreServices.GetRepository<Designation>();
            var designation = await designationRep.FindByIdAsync(employee.DesignationId);

            var shiftRep = CoreServices.GetRepository<Shift>();
            var shift = await shiftRep.FindByIdAsync(employee.ShiftId);

            attendanceLog.EmployeeId = employee.Id;
            attendanceLog.Code = employee.Code;
            attendanceLog.Name = employee.Name;
            attendanceLog.EmployeeType = empType.Name;
            attendanceLog.Department = department.Name;
            attendanceLog.Designation = designation.Name;
            attendanceLog.Gender = employee.Gender.ToString();
            attendanceLog.Shift = shift.Name;

            LatePolicy latePolicy = await GetEmployeeLatePolicyAsync(attendanceLog.TenantId, employee.Id);

            if (attendanceLog.Action.ToLower() == "checkin")
            {
                var loggedIn = await LoginCheckAsync(attendanceLog.TenantId, attendanceLog.EmployeeId, attendanceLog.TimeStamp);

                if (loggedIn == false)
                {
                    // previous day get first log
                    // previous day early logout
                    var lastLog = await LastLogAsync(attendanceLog.TenantId, attendanceLog.EmployeeId);

                    if (lastLog != null)
                    {
                        var earlyLogoutMinutes =
                            await EarlyLogoutMinutesAsync(attendanceLog.TenantId, attendanceLog.EmployeeId, lastLog.TimeStamp);

                        if (earlyLogoutMinutes > 0)
                        {
                            await UpdateEarlyLogoutAsync(lastLog.Id, lastLog.TimeStamp, "true");
                        }
                        await UpdateLastLogAsync(lastLog);

                    }

                    attendanceLog.FirstLogin = true;
                    // late check
                    TimeOnly timeOnly = TimeOnly.FromDateTime(attendanceLog.TimeStamp);
                    TimeSpan loginDiff = TimeSpan.Zero;

                    if (shift != null)
                    {
                        //Console.WriteLine("TimeStamp " + attendanceLog.TimeStamp);
                        //Console.WriteLine("Time " + timeOnly);
                        //loginDiff = timeOnly.ToTimeSpan() - shift.StartTime.ToTimeSpan();
                        loginDiff = timeOnly.AddHours(5).AddMinutes(30).ToTimeSpan() - shift.StartTime.ToTimeSpan();
                        var lateMinutes = (decimal)loginDiff.TotalMinutes;

                        attendanceLog.Shift = shift.Name;
                        attendanceLog.StartTime = shift.StartTime;
                        attendanceLog.EndTime = shift.EndTime;
                        //Console.WriteLine(lateMinutes);
                        //Console.WriteLine(shift.Name);

                        if (latePolicy != null)
                        {
                            if (lateMinutes > latePolicy.LateLogin)
                            {
                                attendanceLog.LateLogin = true;
                                attendanceLog.LateLoginMinutes = lateMinutes;
                            }
                            else
                            {
                                attendanceLog.LateLogin = false;
                                attendanceLog.LateLoginMinutes = 0;
                            }
                        }
                    }
                }
                else
                {
                    attendanceLog.FirstLogin = false;
                    attendanceLog.LateLogin = false;
                    attendanceLog.LateLoginMinutes = 0;
                    attendanceLog.EarlyLogout = false;
                    attendanceLog.EarlyLogoutMinutes = 0;
                }
            }
            else if (attendanceLog.Action.ToLower() == "checkout")
            {
                // pending early logout
                // consider DateTime as parameter
                var employeeInTime = await GetEmployeeInTimeAsync(attendanceLog.EmployeeId, attendanceLog.DateTime);
                attendanceLog.Time = employeeInTime;
            }

            await attendanceLogRep.UpdateAsync(attendanceLog);

            return attendanceLog.Embedding;
        }

        public async Task ApplyRegularization(ApplyRegularizationTimeModel applyRegularizationTimeModel)
        {
            try
            {
                Regularization regularization = new Regularization();

                regularization.Id = Guid.NewGuid();
                regularization.ApplyDateTime = DateTime.Now;
                regularization.EmployeeId = applyRegularizationTimeModel.EmployeeId;
                regularization.AttendanceId = applyRegularizationTimeModel.AttendanceId;
                regularization.RegularizationDate = DateOnly.FromDateTime(DateTime.Now);   //applyRegularizationModel.RegularizationDate;
                regularization.StartTime = applyRegularizationTimeModel.StartTime;
                regularization.EndTime = applyRegularizationTimeModel.EndTime;

                await Entity.InsertAsync(regularization);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task LeaveApproval(LeaveApprovalModel leaveApprovalModel)
        {
            try
            {
                var TenantId = CoreServices.CurrentUser.GetTenantId();
                var userId = CoreServices.CurrentUser.GetUserId();
                TenantUser tenantUser = await Entity.FindByIdAsync<TenantUser>(userId);
                var _employeeService = CoreServices.GetService<EmployeeService>();
                var employee = await _employeeService.GetEmployeeByEmailAsync(TenantId, tenantUser.Email);

                LeaveLog leaveLog = await _dbContext.LeaveLogs.FirstOrDefaultAsync(m => m.Id == leaveApprovalModel.LeaveId);

                leaveLog.ApprovalStatus = leaveApprovalModel.ApprovalStatus;
                leaveLog.ApprovedById = (employee == null) ? userId : employee.Id;
                leaveLog.ApprovedByName = (employee == null) ? tenantUser.Name : employee.Name;
                leaveLog.ApprovedOn = DateTime.Now;
                leaveLog.ApprovalRemarks = leaveApprovalModel.ApprovalRemarks;

                var res = await Entity.UpdateAsync(leaveLog.Id, leaveLog);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
