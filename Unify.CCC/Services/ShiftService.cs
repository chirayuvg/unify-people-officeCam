
using AppStudio.Framework.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Unify.CCC.Data;

namespace Unify.CCC.Services
{
    public class ShiftService
    {
        private readonly PeopleDb _dbContext;
        private readonly ClaimsPrincipal _user;
        private readonly Guid _customerId;

        public ShiftService(IDbContextFactory<PeopleDb> dbContextFactory, IHttpContextAccessor contextAccessor)
        {
            _dbContext = dbContextFactory.CreateDbContext();
            _user = contextAccessor.HttpContext.User;
            _customerId = contextAccessor.HttpContext.User.GetTenantId();
        }
        public async Task<Shift> GeShiftByTimeAsync(Guid customerId, TimeOnly timeOnly)
        {
            try
            {
                Shift shift = new();
                //TimeOnly timeOnly = TimeOnly.FromDateTime(dateTime);

                //var shift = await _dbContext.Shifts.Where(m => m.CustomerId == customerId //_user.GetCustomerId()
                //            && m.StartTime >= timeOnly && m.EndTime <= timeOnly).FirstOrDefaultAsync();
                var shiftLate = await _dbContext.Shifts.Where(m => m.CustomerId == customerId //_user.GetCustomerId()
                           && m.StartTime <= timeOnly && m.EndTime >= timeOnly).FirstOrDefaultAsync();
                if (shiftLate != null)
                {
                    shift = shiftLate;
                }
                else
                {
                    var shiftEarly = await _dbContext.Shifts.Where(m => m.CustomerId == customerId //_user.GetCustomerId()
                           && m.StartTime >= timeOnly && m.EndTime >= timeOnly).FirstOrDefaultAsync();
                    if (shiftEarly != null)
                    {
                        shift = shiftEarly;

                    }
                    else
                    {
                        shift = await _dbContext.Shifts.Where(m => m.CustomerId == customerId //_user.GetCustomerId()
                          ).OrderByDescending(m => m.EndTime).FirstOrDefaultAsync();

                    }
                }
                return shift;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
