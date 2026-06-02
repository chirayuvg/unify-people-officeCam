using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Reflection.Emit;
using System.Security.Claims;
using Unify.CCC.Data;

namespace Unify.CCC.Data
{
    public class CCCDb : DbContext
    {
        public DbSet<Camera> Cameras { get; set; }
        public DbSet<CameraMetaData> CameraMetaData { get; set; }
        public DbSet<Segment> Segments { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<Intelligence> Intelligences { get; set; }
        public DbSet<Monitor> Monitors { get; set; }
        public DbSet<SegmentMetaData> SegmentMetaData { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<AlertMessage> AlertMessages { get; set; }

        public CCCDb(DbContextOptions<CCCDb> options) : base(options)
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
