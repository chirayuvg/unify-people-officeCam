using Unify.STR.Attendance.Data.SQL;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using Unify.STR.Attendance.Data.SQL;

namespace Unify.STR.Attendance.Data.SQL
{
    public class PeopleDb : DbContext
    {

        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeType> EmployeeTypes { get; set; }
        public DbSet<EmployeeImage> EmployeeImages { get; set; }
        public DbSet<LatePolicy> LatePolicies { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<AttendanceLog> AttendanceLogs { get; set; }
        public DbSet<Beat> Beats { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Circle> Circles { get; set; }
        public DbSet<Division> Divisions { get; set; }
        public DbSet<Range> Ranges { get; set; }

        public DbSet<Desigination> Designations { get; set; }

        public DbSet<Tasks> Tasks { get; set; }

        public DbSet<TasksHistory> TasksHistorys { get; set; }


        public PeopleDb(DbContextOptions<PeopleDb> options) : base(options)
        {

        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasPostgresExtension("vector");
            builder.Entity<EmployeeImage>()
                .HasIndex(i => i.Embedding)
                .HasMethod("hnsw")
                .HasOperators("vector_cosine_ops")
                .HasStorageParameter("m", 16)
                .HasStorageParameter("ef_construction", 64);
        }
    }
}
