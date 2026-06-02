using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using Unify.STR.Attendance.Data.SQL;

namespace Unify.STR.Attendance.Controllers.Attendance
{
    [AllowAnonymous]
    public class AttendanceDashboardController : Controller
    {
        private readonly PeopleDb _peopleDbContext;

        public AttendanceDashboardController(PeopleDb peopleDbContext)
        {
            _peopleDbContext = peopleDbContext;
        }

        private Employee GetEmployee(string email)
        {
            var employee = _peopleDbContext.Employees
                .Include(e => e.Desigination)
                .Include(e => e.Division)
                .Include(e => e.Range)
                .Include(e => e.Section)
                .Include(e => e.Beat)
                .FirstOrDefault(e => e.Email == email);

            return employee;
        }

        private List<DivisionDashboard> GetDivisionList(string email)
        {
            if (email == null) return new List<DivisionDashboard>();

            var employee = GetEmployee(email);

            var divisions = new List<DivisionDashboard>();
            if (employee.Desigination == null)
            {
                divisions = _peopleDbContext.Divisions
    .Where(d => d.Id == employee.DivisionId)
    .Select(d => new DivisionDashboard { Id = d.Id, Name = d.Name })
    .OrderBy(d => d.Name)
    .ToList();

                var range = _peopleDbContext.Ranges
    .Where(m => m.Id == employee.RangeId)
    .OrderBy(d => d.Name)
    .ToList();
                ViewBag.Ranges = range;

                var section = _peopleDbContext.Sections
    .Where(m => m.Id == employee.SectionId)
    .OrderBy(d => d.Name)
    .ToList();
                ViewBag.Sections = section;

            }
            else if (employee.Desigination.Name == "Chief Conservator of Forests (CCF)")
            {
                divisions = _peopleDbContext.Divisions
                    .Select(d => new DivisionDashboard { Id = d.Id, Name = d.Name })
                    .OrderBy(d => d.Name)
                    .ToList();
            }
            else if (employee.Desigination.Name == "District Forest Officer (DFO)")
            {
                var range = _peopleDbContext.Ranges
                    .Where(m => m.Id == employee.RangeId)
                    .OrderBy(d => d.Name)
                    .ToList();
                ViewBag.Ranges = range;

                divisions = _peopleDbContext.Divisions
                    .Where(d => d.Id == employee.DivisionId)
                    .Select(d => new DivisionDashboard { Id = d.Id, Name = d.Name })
                    .OrderBy(d => d.Name)
                    .ToList();
            }
            else if (employee.Desigination.Name == "Ranger")
            {
                var section = _peopleDbContext.Sections
                    .Where(m => m.Id == employee.SectionId)
                    .OrderBy(d => d.Name)
                    .ToList();
                ViewBag.Sections = section;

                var range = _peopleDbContext.Ranges
                    .Where(m => m.Id == employee.RangeId)
                    .OrderBy(d => d.Name)
                    .ToList();
                ViewBag.Ranges = range;

                divisions = _peopleDbContext.Divisions
                    .Where(d => d.Id == employee.DivisionId)
                    .Select(d => new DivisionDashboard { Id = d.Id, Name = d.Name })
                    .OrderBy(d => d.Name)
                    .ToList();
            }


            return divisions;
        }

        public IActionResult Index()
        {
            FilterSelectionViewModel model = new FilterSelectionViewModel();

            var email = Request.Query["Email"].ToString();
            //email = "superadmin@str.com";
            //email = "admin@str.com";
            //email = "manager@str.com";
            var employee = GetEmployee(email);

            if (employee == null)
            {
                ViewBag.Email = "";
            }
            else if (employee.Desigination == null)
            {
                ViewBag.Email = email;
                ViewBag.UserName = employee.Name;
                ViewBag.Divisions = GetDivisionList(email);

                model.DivisionId = employee.DivisionId;
                model.RangeId = employee.RangeId;
                model.SectionId = employee.SectionId;
                model.BeatId = employee.BeatId;
                model.EmployeeId = employee.Id;
            }
            else
            {
                ViewBag.Email = email;
                ViewBag.UserName = employee.Name;
                ViewBag.Divisions = GetDivisionList(email);

                if (employee.Desigination.Name == "Chief Conservator of Forests (CCF)")
                {
                    ViewBag.Designation = "CCF";
                }
                else if (employee.Desigination.Name == "District Forest Officer (DFO)")
                {
                    ViewBag.Designation = "DFO";
                }
                else if (employee.Desigination.Name == "Ranger")
                {
                    ViewBag.Designation = "Ranger";
                }
            }

            return View(model);
        }

        [HttpGet]
        public JsonResult GetDivisions(string email)
        {
            return Json(GetDivisionList(email));
        }

        [HttpGet]
        public JsonResult GetRanges(Guid divisionId)
        {
            var ranges = _peopleDbContext.Ranges
                .Where(r => r.DivisionId == divisionId)
                .Select(r => new { r.Id, name = r.Name })
                .ToList();

            return Json(ranges);
        }

        [HttpGet]
        public JsonResult GetSections(Guid rangeId)
        {
            var sections = _peopleDbContext.Sections
                .Where(s => s.RangeId == rangeId)
                .Select(s => new { s.Id, name = s.Name })
                .ToList();

            return Json(sections);
        }

        [HttpGet]
        public JsonResult GetBeats(Guid sectionId)
        {
            var beats = _peopleDbContext.Beats
                .Where(b => b.SectionId == sectionId)
                .Select(b => new { b.Id, name = b.Name })
                .ToList();

            return Json(beats);
        }

        [HttpGet]
        public JsonResult GetEmployees(Guid beatId)
        {
            var employees = _peopleDbContext.Employees
                .Where(e => e.BeatId == beatId)
                .Select(e => new { e.Id, Name = e.Name })
                .OrderBy(e => e.Name)
                .ToList();

            return Json(employees);
        }

        //[HttpGet]
        //public JsonResult GetAttendancePercentage(string email, Guid? divisionId, Guid? rangeId, Guid? sectionId, Guid? beatId, Guid? employeeId, DateTime fromDate, DateTime toDate)
        //{
        //    if (string.IsNullOrEmpty(email))
        //    {
        //        return Json(new
        //        {
        //            percentage = 0,
        //            fromDate = fromDate.ToString("dd-MM-yyyy"),
        //            toDate = toDate.ToString("dd-MM-yyyy")
        //        });
        //    }

        //    var logs = _peopleDbContext.AttendanceLogs
        //        .Where(x => x.DateTime.Date >= fromDate.Date && x.DateTime.Date <= toDate.Date
        //            && (employeeId == null || x.EmployeeId == employeeId))
        //        .OrderBy(x => x.DateTime)
        //        .ToList();

        //    var employeesQuery = _peopleDbContext.Employees.AsQueryable();

        //    if (divisionId.HasValue && rangeId.HasValue && sectionId.HasValue && beatId.HasValue && employeeId.HasValue)
        //        employeesQuery = employeesQuery.Where(e => e.Beat.Section.Range.DivisionId == divisionId.Value && e.Beat.Section.RangeId == rangeId.Value && e.Beat.SectionId == sectionId.Value && e.BeatId == beatId.Value && e.Id == employeeId.Value);
        //    else if (divisionId.HasValue && rangeId.HasValue && sectionId.HasValue && beatId.HasValue)
        //        employeesQuery = employeesQuery.Where(e => e.Beat.Section.Range.DivisionId == divisionId.Value && e.Beat.Section.RangeId == rangeId.Value && e.Beat.SectionId == sectionId.Value && e.BeatId == beatId.Value);
        //    else if (divisionId.HasValue && rangeId.HasValue && sectionId.HasValue)
        //        employeesQuery = employeesQuery.Where(e => e.Beat.Section.Range.DivisionId == divisionId.Value && e.Beat.Section.RangeId == rangeId.Value && e.Beat.SectionId == sectionId.Value);
        //    else if (divisionId.HasValue && rangeId.HasValue)
        //        employeesQuery = employeesQuery.Where(e => e.Beat.Section.Range.DivisionId == divisionId.Value && e.Beat.Section.RangeId == rangeId.Value);
        //    else if (divisionId.HasValue)
        //        employeesQuery = employeesQuery.Where(e => e.Beat.Section.Range.DivisionId == divisionId.Value);

        //    var employeeIds = employeesQuery.Select(e => e.Id).ToList();

        //    var allDates = Enumerable.Range(0, (toDate.Date - fromDate.Date).Days + 1)
        //        .Select(offset => fromDate.Date.AddDays(offset))
        //        .ToList();

        //    var dailyDataList = new List<DailyAttendanceSummary>();

        //    foreach (var date in allDates)
        //    {
        //        int presentCount = 0;
        //        int lateCount = 0;
        //        int absentCount = 0;
        //        int totalEmployees = employeeIds.Count;

        //        foreach (var empId in employeeIds)
        //        {
        //            var logsForEmployee = logs.Where(x => x.EmployeeId == empId && x.DateTime.Date == date.Date).ToList();

        //            if (logsForEmployee.Any())
        //            {
        //                presentCount++;
        //                var firstCheckIn = logsForEmployee.Where(x => x.Action == "CheckIn").OrderBy(x => x.DateTime).FirstOrDefault();
        //                if (firstCheckIn != null && firstCheckIn.DateTime.TimeOfDay > new TimeSpan(9, 30, 0))
        //                {
        //                    lateCount++;
        //                }
        //            }
        //            else
        //            {
        //                absentCount++;
        //            }


        //        }

        //        absentCount = totalEmployees - presentCount;

        //        dailyDataList.Add(new DailyAttendanceSummary
        //        {
        //            Date = date,
        //            Present = presentCount,
        //            Late = lateCount,
        //            Absent = absentCount
        //        });
        //    }

        //    // Sum totals
        //    var totalPresent = dailyDataList.Sum(x => x.Present);
        //    var totalLate = dailyDataList.Sum(x => x.Late);
        //    var totalAbsent = dailyDataList.Sum(x => x.Absent);
        //    var totalDays = dailyDataList.Count;

        //    // Averages per day
        //    var averagePresent = totalDays > 0 ? totalPresent / (double)totalDays : 0;
        //    var averageLate = totalDays > 0 ? totalLate / (double)totalDays : 0;
        //    var averageAbsent = totalDays > 0 ? totalAbsent / (double)totalDays : 0;

        //    // Average total employees per day (Present + Absent)
        //    var averageTotalEmployees = totalDays > 0 ? (totalPresent + totalAbsent) / (double)totalDays : 0;

        //    // Percentages
        //    var presentPercentage = averageTotalEmployees > 0 ? (averagePresent / averageTotalEmployees) * 100 : 0;
        //    var latePercentage = averageTotalEmployees > 0 ? (averageLate / averageTotalEmployees) * 100 : 0;
        //    var absentPercentage = averageTotalEmployees > 0 ? (averageAbsent / averageTotalEmployees) * 100 : 0;

        //    return Json(new
        //    {
        //        percentage =  Math.Round(presentPercentage,0),
        //        fromDate = fromDate.ToString("dd-MM-yyyy"),
        //        toDate = toDate.ToString("dd-MM-yyyy")
        //    });


        //}

        //public JsonResult GetAttendancePercentage(string email, Guid? divisionId, Guid? rangeId, Guid? sectionId, Guid? beatId, Guid? employeeId, DateTime fromDate, DateTime toDate)
        //{
        //    if (string.IsNullOrEmpty(email))
        //    {
        //        return Json(new
        //        {
        //            Percentage = 0,
        //            FromDate = fromDate.ToString("dd-MM-yyyy"),
        //            ToDate = toDate.ToString("dd-MM-yyyy")
        //        });
        //    }

        //    // Step 1: Get total working days
        //    int totalDays = (toDate.Date - fromDate.Date).Days + 1;
        //    var employeesQuery = _peopleDbContext.Employees.AsQueryable();

        //    List<DivisionDashboard> divisions = GetDivisionList(email);

        //    var divisionIds = divisions.Select(d => d.Id.ToString()).ToList();

        //    var logs = _peopleDbContext.AttendanceLogs
        //        .Where(x =>
        //            x.DateTime.Date >= fromDate.Date
        //            && x.DateTime.Date <= toDate.Date
        //            && (employeeId == null || x.EmployeeId == employeeId)
        //        );
        //    if (divisionIds.Count > 0)
        //    {
        //        logs = logs.Where(x => x.DivisionId.HasValue && divisionIds.Contains(x.DivisionId.ToString()));

        //    };

        //     var logs2=logs  .OrderBy(x => x.DateTime)
        //        .ToList();
        //    if (divisionId.HasValue && rangeId.HasValue && sectionId.HasValue && beatId.HasValue && employeeId.HasValue)
        //        employeesQuery = employeesQuery.Where(e => e.Beat.Section.Range.DivisionId == divisionId.Value && e.Beat.Section.RangeId == rangeId.Value && e.Beat.SectionId == sectionId.Value && e.BeatId == beatId.Value && e.Id == employeeId.Value);
        //    else if (divisionId.HasValue && rangeId.HasValue && sectionId.HasValue && beatId.HasValue)
        //        employeesQuery = employeesQuery.Where(e => e.Beat.Section.Range.DivisionId == divisionId.Value && e.Beat.Section.RangeId == rangeId.Value && e.Beat.SectionId == sectionId.Value && e.BeatId == beatId.Value);
        //    else if (divisionId.HasValue && rangeId.HasValue && sectionId.HasValue)
        //        employeesQuery = employeesQuery.Where(e => e.Beat.Section.Range.DivisionId == divisionId.Value && e.Beat.Section.RangeId == rangeId.Value && e.Beat.SectionId == sectionId.Value);
        //    else if (divisionId.HasValue && rangeId.HasValue)
        //        employeesQuery = employeesQuery.Where(e => e.Beat.Section.Range.DivisionId == divisionId.Value && e.Beat.Section.RangeId == rangeId.Value);
        //    else if (divisionId.HasValue)
        //        employeesQuery = employeesQuery.Where(e => e.Beat.Section.Range.DivisionId == divisionId.Value);

        //    var employeeIds = employeesQuery.Select(e => e.Id).ToList();

        //    var allDates = Enumerable.Range(0, (toDate.Date - fromDate.Date).Days + 1)
        //        .Select(offset => fromDate.Date.AddDays(offset))
        //        .ToList();

        //    // Generate attendance map for each employee-date
        //    var attendance = (from empId in employeeIds
        //                      from date in allDates
        //                      let logsForDay = logs2
        //                          .Where(x => x.EmployeeId == empId && x.DateTime.Date == date)
        //                          .ToList()
        //                      let hasLog = logsForDay.Any()
        //                      let firstCheckIn = logsForDay
        //                          .Where(x => x.Action == "CheckIn")
        //                          .OrderBy(x => x.DateTime)
        //                          .FirstOrDefault()
        //                      let isLate = firstCheckIn != null && firstCheckIn.DateTime.TimeOfDay > new TimeSpan(9, 30, 0)
        //                      select new
        //                      {
        //                          EmployeeId = empId,
        //                          Date = date,
        //                          Present = hasLog ? 1 : 0,
        //                          Late = isLate ? 1 : 0,
        //                          Absent = hasLog ? 0 : 1
        //                      }).ToList();

        //    // Aggregate per employee
        //    var perEmployee = attendance
        //        .GroupBy(x => x.EmployeeId)
        //        .Select(g => new
        //        {
        //            EmployeeId = g.Key,
        //            WasPresent = g.Any(x => x.Present == 1),
        //            WasLate = g.Any(x => x.Late == 1),
        //            WasAbsentAllDays = g.All(x => x.Absent == 1)
        //        }).ToList();

        //    // Final counts
        //    var summary = new
        //    {
        //        Present = perEmployee.Count(x => x.WasPresent),
        //        Late = perEmployee.Count(x => x.WasLate),
        //        Absent = perEmployee.Count(x => x.WasAbsentAllDays)
        //    };

        //    // Step 6: Calculate percentage
        //    double percentage = (employeeIds.Count() > 0) ? ((summary.Present * 100.0) / employeeIds.Count()) : 0;

        //    // Step 7: Return result
        //    return Json(new
        //    {
        //        Percentage = Math.Round(percentage),
        //        FromDate = fromDate.ToString("dd-MM-yyyy"),
        //        ToDate = toDate.ToString("dd-MM-yyyy")
        //    });
        //}

        [HttpGet]

        public JsonResult GetAttendanceChartData(string email, Guid? divisionId, Guid? rangeId, Guid? sectionId, Guid? beatId, Guid? employeeId, DateTime fromDate, DateTime toDate)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new
                {
                    chartData = new[]
        {
        new { status = "Present", count = 0 },
        new { status = "Late", count = 0 },
        new { status = "Absent", count = 0 }
    },
                    percentageSummary = new
                    {
                        percentage = 0,
                        fromDate = fromDate.ToString("dd-MM-yyyy"),
                        toDate = toDate.ToString("dd-MM-yyyy")
                    }
                });
            }

            var logs = _peopleDbContext.AttendanceLogs
                .Where(x => x.DateTime.Date >= fromDate.Date && x.DateTime.Date <= toDate.Date
                    && (employeeId == null || x.EmployeeId == employeeId))
                .OrderBy(x => x.DateTime)
                .ToList();

            var employeesQuery = _peopleDbContext.Employees.AsQueryable();

            if (divisionId.HasValue && rangeId.HasValue && sectionId.HasValue && beatId.HasValue && employeeId.HasValue)
                employeesQuery = employeesQuery.Where(e => e.Beat.Section.Range.DivisionId == divisionId.Value && e.Beat.Section.RangeId == rangeId.Value && e.Beat.SectionId == sectionId.Value && e.BeatId == beatId.Value && e.Id == employeeId.Value);
            else if (divisionId.HasValue && rangeId.HasValue && sectionId.HasValue && beatId.HasValue)
                employeesQuery = employeesQuery.Where(e => e.Beat.Section.Range.DivisionId == divisionId.Value && e.Beat.Section.RangeId == rangeId.Value && e.Beat.SectionId == sectionId.Value && e.BeatId == beatId.Value);
            else if (divisionId.HasValue && rangeId.HasValue && sectionId.HasValue)
                employeesQuery = employeesQuery.Where(e => e.Beat.Section.Range.DivisionId == divisionId.Value && e.Beat.Section.RangeId == rangeId.Value && e.Beat.SectionId == sectionId.Value);
            else if (divisionId.HasValue && rangeId.HasValue)
                employeesQuery = employeesQuery.Where(e => e.Beat.Section.Range.DivisionId == divisionId.Value && e.Beat.Section.RangeId == rangeId.Value);
            else if (divisionId.HasValue)
                employeesQuery = employeesQuery.Where(e => e.Beat.Section.Range.DivisionId == divisionId.Value);

            var employeeIds = employeesQuery.Select(e => e.Id).ToList();

            var allDates = Enumerable.Range(0, (toDate.Date - fromDate.Date).Days + 1)
                .Select(offset => fromDate.Date.AddDays(offset))
                .ToList();

            var dailyDataList = new List<DailyAttendanceSummary>();

            foreach (var date in allDates)
            {
                int presentCount = 0;
                int lateCount = 0;
                int absentCount = 0;
                int totalEmployees = employeeIds.Count;

                foreach (var empId in employeeIds)
                {
                    var logsForEmployee = logs.Where(x => x.EmployeeId == empId && x.DateTime.Date == date.Date).ToList();

                    if (logsForEmployee.Any())
                    {
                        presentCount++;
                        var firstCheckIn = logsForEmployee.Where(x => x.Action == "CheckIn").OrderBy(x => x.DateTime).FirstOrDefault();
                        if (firstCheckIn != null && firstCheckIn.DateTime.TimeOfDay > new TimeSpan(9, 30, 0))
                        {
                            lateCount++;
                        }
                    }
                    else
                    {
                        absentCount++;
                    }


                }

                absentCount = totalEmployees - presentCount;

                dailyDataList.Add(new DailyAttendanceSummary
                {
                    Date = date,
                    Present = presentCount,
                    Late = lateCount,
                    Absent = absentCount,

                });
            }

            // Sum totals
            var totalPresent = dailyDataList.Sum(x => x.Present);
            var totalLate = dailyDataList.Sum(x => x.Late);
            var totalAbsent = dailyDataList.Sum(x => x.Absent);
            var totalDays = dailyDataList.Count;

            // Averages per day
            var averagePresent = totalDays > 0 ? totalPresent / (double)totalDays : 0;
            var averageLate = totalDays > 0 ? totalLate / (double)totalDays : 0;
            var averageAbsent = totalDays > 0 ? totalAbsent / (double)totalDays : 0;

            // Average total employees per day (Present + Absent)
            var averageTotalEmployees = totalDays > 0 ? (totalPresent + totalAbsent) / (double)totalDays : 0;

            // Percentages
            var presentPercentage = averageTotalEmployees > 0 ? (averagePresent / averageTotalEmployees) * 100 : 0;
            var latePercentage = averageTotalEmployees > 0 ? (averageLate / averageTotalEmployees) * 100 : 0;
            var absentPercentage = averageTotalEmployees > 0 ? (averageAbsent / averageTotalEmployees) * 100 : 0;
            return Json(new
            {
                chartData = new[]
    {
        new { status = "Present", count = (int)averagePresent },
        new { status = "Late", count = (int)averageLate },
        new { status = "Absent", count = (int)averageAbsent }
    },
                percentageSummary = new
                {
                    percentage = Math.Round(presentPercentage, 0),
                    fromDate = fromDate.ToString("dd-MM-yyyy"),
                    toDate = toDate.ToString("dd-MM-yyyy")
                }
            });


            //            return Json(new[]
            //{
            //            new { status = "Present", count = (int)averagePresent },
            //            new { status = "Late", count = (int)averageLate },
            //            new { status = "Absent", count = (int)averageAbsent },

            //        });

            //return Json(new
            //{
            //    dailyData = dailyDataList,
            //    summary = new
            //    {
            //        totalPresent,
            //        totalLate,
            //        totalAbsent,
            //        averagePresent,
            //        averageLate,
            //        averageAbsent,
            //        averageTotalEmployees,
            //        presentPercentage,
            //        latePercentage,
            //        absentPercentage
            //    }
            //});
        }

        public class DailyAttendanceSummary
        {
            public DateTime Date { get; set; }
            public int Present { get; set; }
            public int Late { get; set; }
            public int Absent { get; set; }
        }

        //public JsonResult GetAttendanceChartData(string email, Guid? divisionId, Guid? rangeId, Guid? sectionId, Guid? beatId, Guid? employeeId, DateTime fromDate, DateTime toDate)
        //{
        //    if (string.IsNullOrEmpty(email))
        //    {
        //        return Json(new[]
        //        {
        //            new { status = "Present", count = 0 },
        //            new { status = "Late", count = 0 },
        //            new { status = "Absent", count = 0 }
        //        });
        //    }

        //    var logs = _peopleDbContext.AttendanceLogs
        //        .Where(x => x.DateTime.Date >= fromDate.Date && x.DateTime.Date <= toDate.Date
        //            && (employeeId == null || x.EmployeeId == employeeId))
        //        .OrderBy(x => x.DateTime)
        //        .ToList();

        //    var employeesQuery = _peopleDbContext.Employees.AsQueryable();

        //    if (divisionId.HasValue && rangeId.HasValue && sectionId.HasValue && beatId.HasValue && employeeId.HasValue)
        //        employeesQuery = employeesQuery.Where(e => e.Beat.Section.Range.DivisionId == divisionId.Value && e.Beat.Section.RangeId == rangeId.Value && e.Beat.SectionId == sectionId.Value && e.BeatId == beatId.Value && e.Id == employeeId.Value);
        //    else if (divisionId.HasValue && rangeId.HasValue && sectionId.HasValue && beatId.HasValue)
        //        employeesQuery = employeesQuery.Where(e => e.Beat.Section.Range.DivisionId == divisionId.Value && e.Beat.Section.RangeId == rangeId.Value && e.Beat.SectionId == sectionId.Value && e.BeatId == beatId.Value);
        //    else if (divisionId.HasValue && rangeId.HasValue && sectionId.HasValue)
        //        employeesQuery = employeesQuery.Where(e => e.Beat.Section.Range.DivisionId == divisionId.Value && e.Beat.Section.RangeId == rangeId.Value && e.Beat.SectionId == sectionId.Value);
        //    else if (divisionId.HasValue && rangeId.HasValue)
        //        employeesQuery = employeesQuery.Where(e => e.Beat.Section.Range.DivisionId == divisionId.Value && e.Beat.Section.RangeId == rangeId.Value);
        //    else if (divisionId.HasValue)
        //        employeesQuery = employeesQuery.Where(e => e.Beat.Section.Range.DivisionId == divisionId.Value);

        //    var employeeIds = employeesQuery.Select(e => e.Id).ToList();
        //    var allDates = Enumerable.Range(0, (toDate.Date - fromDate.Date).Days + 1)
        //        .Select(offset => fromDate.Date.AddDays(offset))
        //        .ToList();

        //    // Generate attendance map for each employee-date
        //    var attendance = (from empId in employeeIds
        //                      from date in allDates
        //                      let logsForDay = logs
        //                          .Where(x => x.EmployeeId == empId && x.DateTime.Date == date)
        //                          .ToList()
        //                      let hasLog = logsForDay.Any()
        //                      let firstCheckIn = logsForDay
        //                          .Where(x => x.Action == "CheckIn")
        //                          .OrderBy(x => x.DateTime)
        //                          .FirstOrDefault()
        //                      let isLate = firstCheckIn != null && firstCheckIn.DateTime.TimeOfDay > new TimeSpan(9, 30, 0)
        //                      select new
        //                      {
        //                          EmployeeId = empId,
        //                          Date = date,
        //                          Present = hasLog ? 1 : 0,
        //                          Late = isLate ? 1 : 0,
        //                          Absent = hasLog ? 0 : 1
        //                      }).ToList();

        //    // Aggregate per employee
        //    var perEmployee = attendance
        //        .GroupBy(x => x.EmployeeId)
        //        .Select(g => new
        //        {
        //            EmployeeId = g.Key,
        //            WasPresent = g.Any(x => x.Present == 1),
        //            WasLate = g.Any(x => x.Late == 1),
        //            WasAbsentAllDays = g.All(x => x.Absent == 1)
        //        }).ToList();

        //    // Final counts
        //    var summary = new
        //    {
        //        Present = perEmployee.Count(x => x.WasPresent),
        //        Late = perEmployee.Count(x => x.WasLate),
        //        Absent = perEmployee.Count(x => x.WasAbsentAllDays)
        //    };

        //    return Json(new[]
        //    {
        //        new { status = "Present", count = summary.Present },
        //        new { status = "Late", count = summary.Late },
        //        new { status = "Absent", count = summary.Absent }
        //    });
        //}

        [HttpGet]
        public JsonResult GetTodayMissedLogins(string email, Guid? divisionId, Guid? rangeId, Guid? sectionId, Guid? beatId, Guid? employeeId, DateTime fromDate, DateTime toDate)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new Employee());
            }

            var today = DateTime.Today;
            var employeesQuery = _peopleDbContext.Employees.AsQueryable();

            if (divisionId.HasValue && rangeId.HasValue && sectionId.HasValue && beatId.HasValue && employeeId.HasValue)
                employeesQuery = employeesQuery.Where(e => e.Beat.Section.Range.DivisionId == divisionId.Value && e.Beat.Section.RangeId == rangeId.Value && e.Beat.SectionId == sectionId.Value && e.BeatId == beatId.Value && e.Id == employeeId.Value);
            else if (divisionId.HasValue && rangeId.HasValue && sectionId.HasValue && beatId.HasValue)
                employeesQuery = employeesQuery.Where(e => e.Beat.Section.Range.DivisionId == divisionId.Value && e.Beat.Section.RangeId == rangeId.Value && e.Beat.SectionId == sectionId.Value && e.BeatId == beatId.Value);
            else if (divisionId.HasValue && rangeId.HasValue && sectionId.HasValue)
                employeesQuery = employeesQuery.Where(e => e.Beat.Section.Range.DivisionId == divisionId.Value && e.Beat.Section.RangeId == rangeId.Value && e.Beat.SectionId == sectionId.Value);
            else if (divisionId.HasValue && rangeId.HasValue)
                employeesQuery = employeesQuery.Where(e => e.Beat.Section.Range.DivisionId == divisionId.Value && e.Beat.Section.RangeId == rangeId.Value);
            else if (divisionId.HasValue)
                employeesQuery = employeesQuery.Where(e => e.Beat.Section.Range.DivisionId == divisionId.Value);

            var employeeIds = employeesQuery.Select(e => e.Id).ToList();

            // Step 1: Get all employees
            var allEmployees = _peopleDbContext.Employees
                 .Select(e => new
                 {
                     e.Id,
                     e.Name,
                     Division = e.Division.Name,
                     Beat = e.Beat.Name
                 }).Where(e => e.Id != null && employeeIds.Contains(e.Id))

                .ToList();

            // Step 2: Get today's attendance logs
            var todaysLogs = _peopleDbContext.AttendanceLogs
                .Where(x => x.DateTime.Date == today)
                .Select(x => new
                {
                    x.EmployeeId,
                    x.DateTime,
                    x.Action,
                })
                .ToList(); // in-memory processing

            // Step 3: Determine which employees are present today
            var presentEmployeeIds = todaysLogs
                .GroupBy(x => x.EmployeeId)
                .Where(g => g.Any(x => x.Action == "CheckIn"))
                .Select(g => g.Key)
                .ToHashSet();

            // Step 4: Filter employees who are **not in the present list**
            var absentEmployees = allEmployees
                .Where(emp => !presentEmployeeIds.Contains(emp.Id))
                .Select(emp => new
                {
                    emp.Name,
                    emp.Division,
                    emp.Beat
                })
                .ToList();

            return Json(absentEmployees);
        }

        [HttpGet]
        public JsonResult GetAttendanceByDivision(string email, Guid? divisionId, Guid? rangeId, Guid? sectionId, DateTime fromDate, DateTime toDate)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new
                {
                    Division = "",
                    Present = 0,
                    Late = 0,
                    Absent = 0
                });
            }

            var employeesQuery = _peopleDbContext.Divisions.AsQueryable();

            if (divisionId.HasValue)
                employeesQuery = employeesQuery.Where(e => e.Id == divisionId.Value);

            var employeeIds = employeesQuery.Select(e => e.Id).ToList();
            var logs = _peopleDbContext.AttendanceLogs.AsQueryable();
            logs = logs.Where(x => x.DateTime.Date >= fromDate.Date && x.DateTime.Date <= toDate.Date);

            if (divisionId.HasValue)
            {
                logs = logs.Where(x => x.DateTime.Date >= fromDate.Date && x.DateTime.Date <= toDate.Date && x.DivisionId.HasValue && employeeIds.Contains(x.DivisionId.Value));
            }
            else
            {
                logs = logs.Where(x => x.DateTime.Date >= fromDate.Date && x.DateTime.Date <= toDate.Date);
                //.OrderBy(x => x.DateTime)
                //.Select(x => new
                //{
                //    x.EmployeeId,
                //    x.DateTime,
                //    x.Action,
                //    x.Division
                //})
                //.ToList(); // In-memory for DateTime.Date support
            }

            var logs2 = logs
            .OrderBy(x => x.DateTime)
            .Select(x => new
            {
                x.EmployeeId,
                x.DateTime,
                x.Action,
                x.Division
            })
            .ToList(); // In-memory for DateTime.Date support
            // Step 1: get distinct employees with their divisions
            var employees = logs2
                .GroupBy(x => x.EmployeeId)
                .Select(g =>
                {
                    var empLogs = g.ToList();
                    var hasAnyLog = empLogs.Any();
                    var hadLateCheckIn = empLogs
                        .Where(x => x.Action == "CheckIn")
                        .Any(x => x.DateTime.TimeOfDay > new TimeSpan(9, 30, 0));
                    var division = empLogs.FirstOrDefault()?.Division ?? "Unknown";

                    return new
                    {
                        EmployeeId = g.Key,
                        Division = division,
                        Present = hasAnyLog ? 1 : 0,
                        Late = hadLateCheckIn ? 1 : 0,
                        Absent = hasAnyLog ? 0 : 1
                    };
                })
                .ToList();

            // Step 2: group by division
            var result = employees
                .GroupBy(x => x.Division)
                .Select(g => new
                {
                    Division = g.Key,
                    Present = g.Sum(x => x.Present),
                    Late = g.Sum(x => x.Late),
                    Absent = g.Sum(x => x.Absent)
                })
                .OrderBy(x => x.Division)
                .ToList();

            // Return final JSON
            return Json(result.Select(x => new
            {
                Division = x.Division,
                Present = x.Present,
                Late = x.Late,
                Absent = x.Absent
            }));
        }

        [HttpGet]
        public JsonResult GetAttendanceBySection(string email, Guid? divisionId, Guid? rangeId, Guid? sectionId, DateTime fromDate, DateTime toDate)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new
                {
                    Section = "",
                    Present = 0,
                    Late = 0,
                    Absent = 0
                });
            }

            var employeesQuery = _peopleDbContext.Employees.AsQueryable();

            // employeesQuery = employeesQuery.Where(e => e.Beat.Section.Range.DivisionId == divisionId.Value);
            //employeesQuery = employeesQuery.Where(e => e.Beat.Section.RangeId == rangeId.Value);

            if (divisionId.HasValue && rangeId.HasValue && sectionId.HasValue)
                employeesQuery = employeesQuery.Where(e => e.DivisionId == divisionId.Value && e.RangeId == rangeId.Value && e.SectionId == sectionId.Value);
            else if (divisionId.HasValue && rangeId.HasValue)
            {
                employeesQuery = employeesQuery.Where(e => e.DivisionId == divisionId.Value && e.RangeId == rangeId.Value);
            }
            else if (divisionId.HasValue)
            {
                employeesQuery = employeesQuery.Where(e => e.DivisionId == divisionId.Value);
            }
            var employeeIds = employeesQuery.Select(e => e.Id).ToList();


            var logs = _peopleDbContext.AttendanceLogs.AsQueryable();

            logs = logs.Where(x => x.DateTime.Date >= fromDate.Date && x.DateTime.Date <= toDate.Date);


            if (divisionId.HasValue && rangeId.HasValue && sectionId.HasValue)
            {
                logs = logs.Where(x => x.DateTime.Date >= fromDate.Date && x.DateTime.Date <= toDate.Date && x.EmployeeId.HasValue && employeeIds.Contains(x.EmployeeId.Value));
            }
            else if (divisionId.HasValue && rangeId.HasValue)
            {
                logs = logs.Where(x => x.DateTime.Date >= fromDate.Date && x.DateTime.Date <= toDate.Date && x.EmployeeId.HasValue && employeeIds.Contains(x.EmployeeId.Value));
            }
            else if (divisionId.HasValue)
            {
                logs = logs.Where(x => x.DateTime.Date >= fromDate.Date && x.DateTime.Date <= toDate.Date && x.EmployeeId.HasValue && employeeIds.Contains(x.EmployeeId.Value));
            }

            else
            {
                logs = logs.Where(x => x.DateTime.Date >= fromDate.Date && x.DateTime.Date <= toDate.Date);
            }

            var logs2 = logs
                .OrderBy(x => x.DateTime)
                .Select(x => new
                {
                    x.EmployeeId,
                    x.DateTime,
                    x.Action,
                    Section = x.Section
                })
                .ToList(); // In-memory for DateTime.Date logic

            // Step 1: Group by EmployeeId to determine present/late/absent once per employee
            var perEmployeeStatus = logs2
                .GroupBy(x => x.EmployeeId)
                .Select(g =>
                {
                    var empLogs = g.ToList();
                    var section = empLogs.FirstOrDefault()?.Section ?? "Unknown";

                    var hasAnyLog = empLogs.Any();
                    var hadLateCheckIn = empLogs
                        .Where(x => x.Action == "CheckIn")
                        .Any(x => x.DateTime.TimeOfDay > new TimeSpan(9, 30, 0));

                    return new
                    {
                        EmployeeId = g.Key,
                        Section = section,
                        Present = hasAnyLog ? 1 : 0,
                        Late = hadLateCheckIn ? 1 : 0,
                        Absent = hasAnyLog ? 0 : 1
                    };
                })
                .ToList();

            // Step 2: Group by Section and sum totals
            var result = perEmployeeStatus
                .Where(x => !string.IsNullOrWhiteSpace(x.Section))
                .GroupBy(x => x.Section)
                .Select(g => new
                {
                    Section = g.Key,
                    Present = g.Sum(x => x.Present),
                    Late = g.Sum(x => x.Late),
                    Absent = g.Sum(x => x.Absent)
                })
                .OrderBy(x => x.Section)
                .ToList();

            // Step 3: Return final output
            return Json(result.Select(x => new
            {
                Section = x.Section,
                Present = x.Present,
                Late = x.Late,
                Absent = x.Absent
            }));
        }

        [HttpGet]
        public IActionResult DownloadCSV(string email, Guid? divisionId, Guid? rangeId, Guid? sectionId, Guid? beatId, Guid? employeeId, DateTime fromDate, DateTime toDate)
        {
            var csv = "Date,Name,Code,Division,Range,Section,Beat,Login Time,Late Login";

            if (!string.IsNullOrEmpty(email))
            {
                //// Step 1: Get first check-in per employee per day
                //var firstLogs = _peopleDbContext.AttendanceLogs
                //    .Where(log => log.Action == "CheckIn"
                //               && log.DateTime.Date >= fromDate
                //               && log.DateTime.Date <= toDate)
                //    .GroupBy(log => new { log.EmployeeId, LogDate = log.DateTime.Date })
                //    .Select(g => new
                //    {
                //        g.Key.EmployeeId,
                //        LogDate = g.Key.LogDate,
                //        FirstCheckInTime = g.Min(x => x.DateTime)
                //    })
                //    .ToList(); // executes in memory
                var firstLogs = _peopleDbContext.AttendanceLogs
                    .Where(log => log.Action == "CheckIn"
                               && log.DateTime.Date >= fromDate
                               && log.DateTime.Date <= toDate)
                    .AsEnumerable() // <-- This makes the rest run in-memory
                    .GroupBy(log => new { log.EmployeeId, Date = log.DateTime.Date })
                    .Select(g => g.OrderBy(x => x.DateTime).First())
                    .OrderByDescending(x => x.DateTime)
                    .ThenBy(x => x.Name)
                    .ToList();

                // Step 2: Get all logs in range
                var logsInRange = _peopleDbContext.AttendanceLogs
                    .Where(al => al.DateTime.Date >= fromDate && al.DateTime.Date <= toDate)
                    .ToList(); // executes in memory

                // Step 3: Join and apply ordering
                //var result =
                //    (from al in logsInRange
                //     join fl in firstLogs
                //         on new { al.EmployeeId, LogDate = al.DateTime.Date, al.DateTime }
                //         equals new { fl.EmployeeId, LogDate = fl.LogDate, DateTime = fl.FirstCheckInTime }
                //     orderby al.DateTime, al.Name ascending
                //     select al
                //    ).ToList();
                var result = _peopleDbContext.AttendanceLogs
                    .Where(log => log.Action == "CheckIn"
                               && log.DateTime.Date >= fromDate
                               && log.DateTime.Date <= toDate)
                    .AsEnumerable() // ← Crucial: switch to in-memory
                    .GroupBy(log => new { log.EmployeeId, Date = log.DateTime.Date })
                    .Select(g => g.OrderBy(x => x.DateTime).First()) // First CheckIn of the day
                    .OrderBy(x => x.DateTime)
                    .ThenBy(x => x.Name)
                    .ToList();

                foreach (AttendanceLog attendanceLog in result)
                {
                    string lateLogin = (attendanceLog.Latelogin) ? "Yes" : "";
                    csv += @$"{Environment.NewLine}{attendanceLog.DateTime:dd-MM-yyyy},{attendanceLog.Name},{attendanceLog.Code},{attendanceLog.Division},{attendanceLog.Range},{attendanceLog.Section},{attendanceLog.Beat},{attendanceLog.DateTime:HH:mm:ss},{lateLogin}";
                }
            }

            var bytes = Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", "Attendance_Report.csv");
        }
    }

    public class DivisionDashboard
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
    }
}
