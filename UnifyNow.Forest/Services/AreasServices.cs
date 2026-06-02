using Microsoft.EntityFrameworkCore;
using UnifyNow.Forest.Data.SQL;

namespace UnifyNow.Forest.Services
{
    public class AreasServices
    {
        private readonly ForestDb _forestDb;

        public AreasServices(IDbContextFactory<ForestDb> dbContextFactory)
        {
            _forestDb = dbContextFactory.CreateDbContext();
        }

        public async Task<List<Area>> GetAreasAsync()
        {
            try
            {
                var areas = await _forestDb.Areas.ToListAsync();

                return areas;
            }
            catch
            {
                throw;
            }
        }
    }
}

