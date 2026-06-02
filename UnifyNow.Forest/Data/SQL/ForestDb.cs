
using Elastic.Clients.Elasticsearch;
using Microsoft.EntityFrameworkCore;

namespace UnifyNow.Forest.Data.SQL
{
    public class ForestDb : DbContext
    {
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<AlertLog> AlertLogs { get; set; }
        public DbSet<Animal> Animals { get; set; }
        public DbSet<Area> Areas { get; set; }
        //public DbSet<AreaofInterest> AreaofInterests { get; set; }
        public DbSet<Camera> Cameras { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Designation> Designations { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Team> Teams { get; set; }

        public DbSet<Preset> Presets { get; set; }

        public DbSet<AlertRule> AlertRules { get; set; }

        public DbSet<WhatsAppMessage> WhatsAppMessages { get; set; }

        public DbSet<TeamMember> TeamMembers { get; set; }

        public DbSet<Watch> Watches { get; set; }

        public DbSet<WatchDetail> WatchDetails { get; set; }

        public DbSet<Intelligence> Intelligences { get; set; }

        public DbSet<Monitor> Monitors { get; set; }

        public DbSet<ForestImages> ForestImages { get; set; }

        public DbSet<Ticket> Tickets { get; set; }

        public DbSet<Message> Messages { get; set; }


        public ForestDb(DbContextOptions<ForestDb> options) : base(options)
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

