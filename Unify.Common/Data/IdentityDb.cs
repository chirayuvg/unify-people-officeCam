using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Unify.Common.Identity.Data
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




