using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toll.Web.UI.Entity;


namespace Toll.Web.UI
{
    public class TollDbUIContext : DbContext
    {
        //public DbSet<UserData> UserData { get; set; }
        private readonly IConfiguration configuration;

        public DbSet<AppSetting> AppSettings { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleType> VehicleTypes { get; set; }
        public DbSet<VehicleNature> VehicleNatures { get; set; }
        public DbSet<VehicleCategory> VehicleCategories { get; set; }
        public DbSet<TravelTime> TravelTimes { get; set; }
        public DbSet<VehiclePricing> VehiclePricings { get; set; }
        public DbSet<Camera> Cameras { get; set; }
        public DbSet<CameraType> CameraTypes { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Booth> Booths { get; set; }
        public DbSet<Area> Areas { get; set; }

        public DbSet<VehicleBilling> VehicleBillings { get; set; }

        public DbSet<ValidateVehicle> ValidateVehicles { get; set; }
        public TollDbUIContext(DbContextOptions options) : base(options)
        {

        }
    }
}
