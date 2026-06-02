using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Toll.Web.UI.Data
{
    public class IdentityDb : IdentityDbContext<IdentityUser>, IDataProtectionKeyContext
    {
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

        public IdentityDb(DbContextOptions<IdentityDb> options)
            : base(options)
        {
        }
    }
}
