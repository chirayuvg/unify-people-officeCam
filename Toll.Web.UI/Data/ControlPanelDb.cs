using Microsoft.EntityFrameworkCore;
using Toll.Web.UI.Entity;

namespace Toll.Web.UI.Data
{
    public class ControlPanelDb : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerUser> CustomerUsers { get; set; }

        public DbSet<Tenant> Tenants { get; set; }

        public ControlPanelDb(DbContextOptions options) : base(options)
        {

        }


    }
}