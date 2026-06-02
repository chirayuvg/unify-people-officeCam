using AppStudio.Framework.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Unify.Data.SQL;
using Unify.People.Data.SQL;

namespace Unify.Data
{
    public class PeopleDb : DbContext
    {
        public DbSet<Camera> Cameras { get; set; }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeType> EmployeeTypes { get; set; }
        public DbSet<EmployeeImage> EmployeeImages { get; set; }

        public DbSet<Department> Departments { get; set; }
        public DbSet<Designation> Designations { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<LatePolicy> LatePolicies { get; set; }
        public DbSet<LeavePolicy> LeavePolicies { get; set; }
        public DbSet<RegularizationPolicy> RegularizationPolicies { get; set; }
        public DbSet<Visitor> Visitors { get; set; }
        public DbSet<VisitorImage> VisitorImages { get; set; }
        public DbSet<DoorStatus> DoorStatuses { get; set; }

        public DbSet<MeetingPurpose> MeetingPurposes { get; set; } 
        public DbSet<MeetingPlace> MeetingPlaces { get; set; }

        public DbSet<AttendanceLog> AttendanceLogs { get; set; }
        public DbSet<AttendanceLogTest> AttendanceLogTests { get; set; }
        public DbSet<Regularization> Regularizations { get; set; }
        public DbSet<LeaveLog> LeaveLogs { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<VisitorLog> VisitorLogs { get; set; }

        public PeopleDb(DbContextOptions<PeopleDb> options) : base(options)
        {

        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("vector");
            modelBuilder.ConvertAllEnumsToStrings();
        }
    }
}
