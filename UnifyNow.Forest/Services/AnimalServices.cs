using Microsoft.EntityFrameworkCore;
using UnifyNow.Forest.Data.SQL;

namespace UnifyNow.Forest.Services
{
    public class AnimalServices
    {
        private readonly ForestDb _forestDb;

        public AnimalServices(IDbContextFactory<ForestDb> dbContextFactory)
        {
            _forestDb = dbContextFactory.CreateDbContext();
        }

        public async Task<List<Animal>> GetAnimalssAsync()
        {
            try
            {
                var animals = await _forestDb.Animals.ToListAsync();

                return animals;
            }
            catch
            {
                throw;
            }
        }
    }
}