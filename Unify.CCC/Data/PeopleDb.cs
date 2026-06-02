using Microsoft.EntityFrameworkCore;

namespace Unify.CCC.Data
{
    public class PeopleDb : DbContext
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeType> EmployeeTypes { get; set; }

        public DbSet<Department> Departments { get; set; }
        public DbSet<Designation> Designations { get; set; }
        public DbSet<EmployeeImage> EmployeeImages { get; set; }

        public DbSet<Shift> Shifts { get; set; }
        public DbSet<LatePolicy> LatePolicies { get; set; }
        public PeopleDb(DbContextOptions<PeopleDb> options) : base(options)
        {

        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {

        }


        protected override void OnModelCreating(ModelBuilder builder)
        {

        }
    }
}

