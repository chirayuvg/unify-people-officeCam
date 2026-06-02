using AppStudio.Enterprise.Data;
using AppStudio.Framework.Core;
using AppStudio.Model;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Unify.Data;
using Unify.Data.SQL;
using Unify.People.Data.SQL;

namespace Unify.People.Models
{
    [Index(nameof(Code), nameof(TenantId), IsUnique = true)]

    [Index(nameof(Title), nameof(TenantId), IsUnique = true)]
    public class Widget
    {
        public Guid Id { get; set; }

        public string Code { get; set; } = string.Empty;

        [MaxLength(50, ErrorMessage = "Length cannot exceed 50 characters.")]
        public string Title { get; set; } = string.Empty;

        public WidgetType WidgetType { get; set; }

        //public ProcessType ProcessType { get; set; }

        public Period Period { get; set; }

        //public Guid? ProcessId { get; set; }
        //public Process? Process { get; set; }

        //public Guid? AssetId { get; set; }
        //public Asset? Asset { get; set; }

        //public Guid? PropertyId { get; set; }
        //public AssetTypeProperty? Property { get; set; }

        public string Query { get; set; } = string.Empty;
        // FromDate applies for CustomDate Period
        public DateTime? FromDate { get; set; }
        // ToDate applies for CustomDate Period
        public DateTime? ToDate { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid TenantId { get; set; }

    }

    public enum WidgetType
    {
        KPI,
        Guage,
        Chart,
        Grid
    }

    public enum Period
    {
        ThisWeek,
        ThisMonth,
        LastMonth,
        CustomDateRange
    }

    //public enum AppointmentPeriod
    //{
    //    Today,
    //    ThisWeek,
    //    ThisMonth,
    //    LastMonth,
    //    CustomDateRange
    //}

    public class DashboardFilterData
    {
        public Guid DepartmentId { get; set; }
        public Department? Department { get; set; }

        public Period Period { get; set; } = Period.ThisWeek;

        public DateOnly FromDate { get; set; }

        public DateOnly ToDate { get; set; }

        public string? DepartmentName { get; set; } = "";

        public DashboardFilterData()
        {
            DateOnly toDay = DateOnly.FromDateTime(DateTime.Today);

            FromDate = toDay.AddDays(-(int)toDay.DayOfWeek + (int)System.DayOfWeek.Monday);
            ToDate = FromDate.AddDays(6);

            var _peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            var _applicationDb = AppStudio.Framework.Core.CoreServices.GetService<ApplicationDb>();
            var userId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetUserId();
            var tenantUser = _applicationDb.TenantUsers.FirstOrDefault(m => m.Id == userId);

            if (tenantUser.Roles.Contains("User"))
            {
                // Get employee based on tenant user's email
                var employee = _peopleDb.Employees.FirstOrDefault(m => m.Email == tenantUser.Email);

                if (employee != null)
                {
                    // Get department for the employee
                    var department = _peopleDb.Departments.FirstOrDefault(m => m.Id == employee.DepartmentId);

                    if (department != null)
                    {
                        DepartmentId = department.Id;
                        DepartmentName = department.Name;
                    }
                    else
                    {
                        DepartmentId = Guid.Empty;
                        DepartmentName = "";
                    }
                }
                else
                {
                    DepartmentId = Guid.Empty;
                    DepartmentName = "";
                }
            }
            else
            {
                DepartmentId = Guid.Empty;
                DepartmentName = "";
            }
        }

        public void GetDates()
        {
            DateOnly fromDate = DateOnly.FromDateTime(DateTime.Today);
            DateOnly toDate = DateOnly.FromDateTime(DateTime.Today);

            switch (Period)
            {
                case Period.ThisWeek:
                    fromDate = fromDate.AddDays(-(int)fromDate.DayOfWeek + (int)System.DayOfWeek.Monday);
                    toDate = fromDate.AddDays(6);
                    break;
                case Period.ThisMonth:
                    fromDate = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
                case Period.LastMonth:
                    fromDate = new DateOnly(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
                case Period.CustomDateRange:
                    fromDate = DateOnly.Parse(FromDate.ToString());
                    toDate = DateOnly.Parse(ToDate.ToString());

                    if (ToDate < FromDate)
                    {
                        UiAction.AddJsCode("alert('\"To Date\" must be greater than or equals to \"From Date\"');");
                        fromDate = DateOnly.FromDateTime(DateTime.Today);
                        toDate = DateOnly.FromDateTime(DateTime.Today);
                    }

                    break;
                default:
                    break;
            }

            FromDate = DateOnly.Parse(fromDate.ToString("yyyy-MM-dd"));
            ToDate = DateOnly.Parse(toDate.ToString("yyyy-MM-dd"));
        }

        public void GetDepartmentName()
        {
            var _peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            DepartmentName = (DepartmentId == Guid.Empty) ? "" : _peopleDb.Departments.Where(m => m.Id == DepartmentId).FirstOrDefault().Name;

            //var _peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            //var _applicationDb = AppStudio.Framework.Core.CoreServices.GetService<ApplicationDb>();

            //var UserId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetUserId();
            //var tenantUser = _applicationDb.TenantUsers.Where(m => m.Id == UserId);
            //var employee = _peopleDb.Employees.Where(m => m.Email == tenantUser.FirstOrDefault().Email);

            //var departments = _peopleDb.Departments.ToList();
            //if (tenantUser.FirstOrDefault().Name == "Admin")
            //{
            //    departments = departments.ToList();

            //}
            //else
            //{
            //    departments = departments.Where(m => m.Id == employee.FirstOrDefault().DepartmentId).ToList();
            //}

            //DepartmentName = (DepartmentId == Guid.Empty) ? "" : _peopleDb.Departments.Where(m => m.Id == DepartmentId).FirstOrDefault().Name;
        }

    }

    public class DepartmentFilterData
    {
        public Guid DepartmentId { get; set; } = Guid.Empty;
        public Department? Department { get; set; }

        public Period Period { get; set; } = Period.ThisWeek;

        public DateOnly FromDate { get; set; }

        public DateOnly ToDate { get; set; }

        public string? DepartmentName { get; set; } = "";

        public DepartmentFilterData()
        {
            DateOnly toDay = DateOnly.FromDateTime(DateTime.Today);

            FromDate = toDay.AddDays(-(int)toDay.DayOfWeek + (int)System.DayOfWeek.Monday);
            ToDate = FromDate.AddDays(6);

            var _peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            var _applicationDb = AppStudio.Framework.Core.CoreServices.GetService<ApplicationDb>();
            var userId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetUserId();
            var tenantUser = _applicationDb.TenantUsers.FirstOrDefault(m => m.Id == userId);

            if (tenantUser.Roles.Contains("User"))
            {
                // Get employee based on tenant user's email
                var employee = _peopleDb.Employees.FirstOrDefault(m => m.Email == tenantUser.Email);

                if (employee != null)
                {
                    // Get department for the employee
                    var department = _peopleDb.Departments.FirstOrDefault(m => m.Id == employee.DepartmentId);

                    if (department != null)
                    {
                        DepartmentId = department.Id;
                        DepartmentName = department.Name;
                    }
                    else
                    {
                        DepartmentId = Guid.Empty;
                        DepartmentName = "";
                    }
                }
                else
                {
                    DepartmentId = Guid.Empty;
                    DepartmentName = "";
                }
            }
            else
            {
                DepartmentId = Guid.Empty;
                DepartmentName = "";
            }
        }

        public void GetDates()
        {
            DateOnly fromDate = DateOnly.FromDateTime(DateTime.Today);
            DateOnly toDate = DateOnly.FromDateTime(DateTime.Today);

            switch (Period)
            {
                case Period.ThisWeek:
                    fromDate = fromDate.AddDays(-(int)fromDate.DayOfWeek + (int)System.DayOfWeek.Monday);
                    toDate = fromDate.AddDays(6);
                    break;
                case Period.ThisMonth:
                    fromDate = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
                case Period.LastMonth:
                    fromDate = new DateOnly(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
                case Period.CustomDateRange:
                    fromDate = DateOnly.Parse(FromDate.ToString());
                    toDate = DateOnly.Parse(ToDate.ToString());
                    if (ToDate < FromDate)
                    {
                        UiAction.AddJsCode("alert('\"To Date\" must be greater than or equals to \"From Date\"');");
                        fromDate = DateOnly.FromDateTime(DateTime.Today);
                        toDate = DateOnly.FromDateTime(DateTime.Today);


                        break;
                    }
                    break;
                default:
                    break;
            }

            FromDate = DateOnly.Parse(fromDate.ToString("yyyy-MM-dd"));
            ToDate = DateOnly.Parse(toDate.ToString("yyyy-MM-dd"));
        }

        public void GetDepartmentName()
        {
            var _peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            DepartmentName = (DepartmentId == Guid.Empty) ? "" : _peopleDb.Departments.Where(m => m.Id == DepartmentId).FirstOrDefault().Name;
        }
    }

    public class EmployeeFilterData
    {
        public Guid EmployeeId { get; set; } = Guid.Empty;

        public Employee? Employee { get; set; }

        public Period Period { get; set; } = Period.ThisWeek;

        public DateOnly FromDate { get; set; }

        public DateOnly ToDate { get; set; }

        public string? EmployeeName { get; set; } = "";

        public EmployeeFilterData()
        {
            DateOnly toDay = DateOnly.FromDateTime(DateTime.Today);

            FromDate = toDay.AddDays(-(int)toDay.DayOfWeek + (int)System.DayOfWeek.Monday);
            ToDate = FromDate.AddDays(6);

            var _peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            var _applicationDb = AppStudio.Framework.Core.CoreServices.GetService<ApplicationDb>();
            var userId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetUserId();
            var tenantUser = _applicationDb.TenantUsers.FirstOrDefault(m => m.Id == userId);

            if (tenantUser.Roles.Contains("User"))
            {
                // Get employee based on tenant user's email
                var employee = _peopleDb.Employees.FirstOrDefault(m => m.Email == tenantUser.Email);

                if (employee != null)
                {
                    if (employee != null)
                    {
                        EmployeeId = employee.Id;
                        EmployeeName = employee.Name;
                    }
                    else
                    {
                        EmployeeId = Guid.Empty;
                        EmployeeName = "";
                    }
                }
                else
                {
                    EmployeeId = Guid.Empty;
                    EmployeeName = "";
                }
            }
            else
            {
                EmployeeId = Guid.Empty;
                EmployeeName = "";
            }
        }

        public void GetDates()
        {
            DateOnly fromDate = DateOnly.FromDateTime(DateTime.Today);
            DateOnly toDate = DateOnly.FromDateTime(DateTime.Today);

            switch (Period)
            {
                case Period.ThisWeek:
                    fromDate = fromDate.AddDays(-(int)fromDate.DayOfWeek + (int)System.DayOfWeek.Monday);
                    toDate = fromDate.AddDays(6);
                    break;
                case Period.ThisMonth:
                    fromDate = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
                case Period.LastMonth:
                    fromDate = new DateOnly(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
                case Period.CustomDateRange:
                    fromDate = DateOnly.Parse(FromDate.ToString());
                    toDate = DateOnly.Parse(ToDate.ToString());
                    if (ToDate < FromDate)
                    {
                        UiAction.AddJsCode("alert('\"To Date\" must be greater than or equals to \"From Date\"');");
                        fromDate = DateOnly.FromDateTime(DateTime.Today);
                        toDate = DateOnly.FromDateTime(DateTime.Today);


                        break;
                    }
                    break;
                default:
                    break;
            }

            FromDate = DateOnly.Parse(fromDate.ToString("yyyy-MM-dd"));
            ToDate = DateOnly.Parse(toDate.ToString("yyyy-MM-dd"));
        }

        public void GetEmployeeName()
        {
            var _peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            EmployeeName = (EmployeeId == Guid.Empty) ? "" : _peopleDb.Employees.Where(m => m.Id == EmployeeId).FirstOrDefault().Name;
        }
    }

    public class STRFilterData
    {
        public Guid DepartmentId { get; set; } = Guid.Empty;
        public Department? Department { get; set; }

        public Period Period { get; set; } = Period.ThisWeek;

        public DateOnly FromDate { get; set; }

        public DateOnly ToDate { get; set; }

        public string? DepartmentName { get; set; } = "";

        public STRFilterData()
        {
            DateOnly toDay = DateOnly.FromDateTime(DateTime.Today);

            FromDate = toDay.AddDays(-(int)toDay.DayOfWeek + (int)System.DayOfWeek.Monday);
            ToDate = FromDate.AddDays(6);
        }

        public void GetDates()
        {
            DateOnly fromDate = DateOnly.FromDateTime(DateTime.Today);
            DateOnly toDate = DateOnly.FromDateTime(DateTime.Today);

            switch (Period)
            {
                case Period.ThisWeek:
                    fromDate = fromDate.AddDays(-(int)fromDate.DayOfWeek + (int)System.DayOfWeek.Monday);
                    toDate = fromDate.AddDays(6);
                    break;
                case Period.ThisMonth:
                    fromDate = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
                case Period.LastMonth:
                    fromDate = new DateOnly(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
                case Period.CustomDateRange:
                    fromDate = DateOnly.Parse(FromDate.ToString());
                    toDate = DateOnly.Parse(ToDate.ToString());
                    break;
                default:
                    break;
            }

            FromDate = DateOnly.Parse(fromDate.ToString("yyyy-MM-dd"));
            ToDate = DateOnly.Parse(toDate.ToString("yyyy-MM-dd"));
        }

        public void GetDepartmentName()
        {
            var _peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            DepartmentName = (DepartmentId == Guid.Empty) ? "" : _peopleDb.Departments.Where(m => m.Id == DepartmentId).FirstOrDefault().Name;
        }
    }

    public class DateOnlyFilterData
    {
        public Period Period { get; set; } = Period.ThisWeek;

        public DateOnly FromDate { get; set; }

        public DateOnly ToDate { get; set; }

        public DateOnlyFilterData()
        {
            DateOnly toDay = DateOnly.FromDateTime(DateTime.Today);

            FromDate = toDay.AddDays(-(int)toDay.DayOfWeek + (int)System.DayOfWeek.Monday);
            ToDate = FromDate.AddDays(6);
        }

        public void GetDates()
        {
            DateOnly fromDate = DateOnly.FromDateTime(DateTime.Today);
            DateOnly toDate = DateOnly.FromDateTime(DateTime.Today);

            switch (Period)
            {
                case Period.ThisWeek:
                    fromDate = fromDate.AddDays(-(int)fromDate.DayOfWeek + (int)System.DayOfWeek.Monday);
                    toDate = fromDate.AddDays(6);
                    break;
                case Period.ThisMonth:
                    fromDate = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
                case Period.LastMonth:
                    fromDate = new DateOnly(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
                case Period.CustomDateRange:
                    fromDate = DateOnly.Parse(FromDate.ToString());
                    toDate = DateOnly.Parse(ToDate.ToString());
                    if (ToDate < FromDate)
                    {
                        UiAction.AddJsCode("alert('\"To Date\" must be greater than or equals to \"From Date\"');");
                        fromDate = DateOnly.FromDateTime(DateTime.Today);
                        toDate = DateOnly.FromDateTime(DateTime.Today);


                        break;
                    }
                    break;
                default:
                    break;
            }

            FromDate = DateOnly.Parse(fromDate.ToString("yyyy-MM-dd"));
            ToDate = DateOnly.Parse(toDate.ToString("yyyy-MM-dd"));
        }
    }

    public class LeaderboardEmployeeData
    {
        public string EmployeeName { get; set; }
        public string? Department { get; set; }
        public decimal AvgDailyWorkingHours { get; set; }
    }

    public class LeaderboardEmployeeData2
    {
        public string EmployeeName { get; set; }
        public string? Department { get; set; }
        public string AvgDailyWorkingHours { get; set; }
    }

    public class LeaderboardAverageWorking
    {
        public string Name { get; set; }
        public decimal WorkingDays { get; set; }
    }

    public class LeaderboardDepartmentData
    {
        public string Department { get; set; }
        public decimal AvgDailyWorkingHours { get; set; }
    }

    public class LeaderboardDepartmentData2
    {
        public string Department { get; set; }
        public string AvgDailyWorkingHours { get; set; }
    }

    public class LeaderboardLateLoginData
    {
        public string EmployeeName { get; set; }
        public string? Department { get; set; }
        public decimal CountLateLogin { get; set; }
    }

    public class LeaderboardEarlyLogoutData
    {
        public string EmployeeName { get; set; }
        public string? Department { get; set; }
        public decimal CountEarlyLogout { get; set; }
    }

    public class EmployeeWiseTimeSheet
    {
        public DateOnly Date { get; set; }
        public string? LateLogin { get; set; }
        public string? EarlyLogout { get; set; }
    }

    public class EarlyLogoutData
    {
        public DateOnly DateOnly { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }
        public decimal CountEarlyLogout { get; set; }
    }

    public class AggregateIntegerData
    {
        public int Value { get; set; }
    }

    public class AggregateDecimalData
    {
        public Decimal Value { get; set; }
    }

    public class AggregateStringData
    {
        public string Value { get; set; }
    }

    public class AverageData
    {
        public DateOnly DateOnly { get; set; }
        public decimal Average { get; set; }

        public List<AppStudio.Model.ChartData> chartDatas { get; set; } = [];
    }

    public class LateLoginData
    {
        public DateOnly DateOnly { get; set; }
        public decimal CountLateLogin { get; set; }
    }

    public class TimeSheetData
    {
        public string Employee { get; set; }
        public string Department { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }

    public class MyMeeting
    {
        public DateOnly DateOnly { get; set; }
        public string Visitor { get; set; }
        public string Status { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }

    public class MyLeave
    {
        public Guid Id { get; set; }
        public string AppliedOn { get; set; }
        public string LeavePolicy { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string LeaveRemarks { get; set; }
        public string ApprovalStatus { get; set; }
    }

    public class LeaveApproval
    {
        public Guid Id { get; set; }
        public string Employee { get; set; }
        public string AppliedOn { get; set; }
        public string LeavePolicy { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string LeaveRemarks { get; set; }
        public string ApprovalStatus { get; set; }
        public string ApprovalRemarks { get; set; }
    }
}
