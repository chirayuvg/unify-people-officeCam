using AppStudio.Framework.Core;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

using Unify.STR.Attendance.Data.SQL;
using Unify.STR.Attendance.Data.SQL;

namespace Unify.STR.Attendnace.Models
{
    [Index(nameof(Code), nameof(TenantId), IsUnique = true)]

    [Index(nameof(Title), nameof(TenantId), IsUnique = true)]
    public class Widget
    {
        public Guid Id { get; set; }

        public string Code { get; set; } = string.Empty;

        [MaxLength(50)]
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
       
        public Guid DivisionId { get; set; } = Guid.Empty;
        public Division? Division { get; set; }

        public string? UnitName { get; set; } = "";
        public Period Period { get; set; } = Period.ThisWeek;

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }
        public string? DivisionName { get; set; } = "";

        public DashboardFilterData()
        {
            DateTime toDay = DateTime.Today;

            FromDate = toDay.AddDays(-(int)toDay.DayOfWeek + (int)System.DayOfWeek.Monday);
            ToDate = FromDate.AddDays(6);
        }

        public void GetDates()
        {
            DateTime fromDate = DateTime.Today;
            DateTime toDate = DateTime.Today;

            switch (Period)
            {
                case Period.ThisWeek:
                    fromDate = fromDate.AddDays(-(int)fromDate.DayOfWeek + (int)System.DayOfWeek.Monday);
                    toDate = fromDate.AddDays(6);
                    break;
                case Period.ThisMonth:
                    fromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
                case Period.LastMonth:
                    fromDate = new DateTime(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
                case Period.CustomDateRange:
                    fromDate = DateTime.Parse(FromDate.ToString());
                    toDate = DateTime.Parse(ToDate.ToString());
                    break;
                default:
                    break;
            }

            FromDate = DateTime.Parse(fromDate.ToString("yyyy-MM-dd"));
            ToDate = DateTime.Parse(toDate.ToString("yyyy-MM-dd"));
        }

        public void GetDepartmentName()
        {
            var _peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            DivisionName = (DivisionId == Guid.Empty) ? "" : _peopleDb.Divisions.Where(m => m.Id == DivisionId).FirstOrDefault().Name;
        }

        public void GetDivisionName()
        {
            var _peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            DivisionName = (DivisionId == Guid.Empty) ? "" : _peopleDb.Divisions.Where(m => m.Id == DivisionId).FirstOrDefault().Name;
        }


    }

    public class STRFilterData
    {
       
        public Guid DivisionId { get; set; } = Guid.Empty;
        public Division? Division { get; set; }

        public Period Period { get; set; } = Period.ThisWeek;

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public string? DivisionName { get; set; } = "";

        public STRFilterData()
        {
            DateTime toDay = DateTime.Today;

            FromDate = toDay.AddDays(-(int)toDay.DayOfWeek + (int)System.DayOfWeek.Monday);
            ToDate = FromDate.AddDays(6);
        }

        public void GetDates()
        {
            DateTime fromDate = DateTime.Today;
            DateTime toDate = DateTime.Today;

            switch (Period)
            {
                case Period.ThisWeek:
                    fromDate = fromDate.AddDays(-(int)fromDate.DayOfWeek + (int)System.DayOfWeek.Monday);
                    toDate = fromDate.AddDays(6);
                    break;
                case Period.ThisMonth:
                    fromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
                case Period.LastMonth:
                    fromDate = new DateTime(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
                case Period.CustomDateRange:
                    fromDate = DateTime.Parse(FromDate.ToString());
                    toDate = DateTime.Parse(ToDate.ToString());
                    break;
                default:
                    break;
            }

            FromDate = DateTime.Parse(fromDate.ToString("yyyy-MM-dd"));
            ToDate = DateTime.Parse(toDate.ToString("yyyy-MM-dd"));
        }

        public void GetDepartmentName()
        {
            var _peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            DivisionName = (DivisionId == Guid.Empty) ? "" : _peopleDb.Divisions.Where(m => m.Id == DivisionId).FirstOrDefault().Name;
        }
    }
    public class LeaderboardEmployeeData
    {
        public string EmployeeName { get; set; }
        public string? Department { get; set; }
        public decimal AvgDailyWorkingHours { get; set; }
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

    public class EarlyLogoutData
    {
        public DateOnly DateOnly { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }
        public decimal CountEarlyLogout { get; set; }
    }

    public class AggregateData
    {
        public int CountRecord { get; set; }
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



}
