using AppStudio.Framework.Core;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Unify.Data;
using Unify.Data.SQL;

namespace Unify.People.Services
{
    public class ShiftService
    {
        private readonly PeopleDb _dbContext;
        private readonly ClaimsPrincipal _user;
        private readonly Guid _TenantId;

        public ShiftService(IDbContextFactory<PeopleDb> dbContextFactory, IHttpContextAccessor contextAccessor) 
        {
            _dbContext = dbContextFactory.CreateDbContext();
            _user = contextAccessor.HttpContext.User;
            _TenantId = contextAccessor.HttpContext.User.GetTenantId();
        }
        public async Task<Shift> GeShiftByTimeAsync(Guid TenantId, TimeOnly timeOnly)
        {
            try
            {
                //TimeOnly timeOnly = TimeOnly.FromDateTime(dateTime);

                var shift = await _dbContext.Shifts.Where(m => m.TenantId == TenantId //_user.GetTenantId()
                            && m.StartTime >= timeOnly && m.EndTime <= timeOnly).FirstOrDefaultAsync();
                return shift;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
