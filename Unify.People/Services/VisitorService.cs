using DTC.Unify.HttpClientService;
using Unify.Data.SQL;
using Unify.Data;
using Microsoft.EntityFrameworkCore;

namespace Unify.People.Services
{
    public class VisitorService
    {
        private readonly PeopleDb _peopleDb;
        private readonly AIPersonService _aIPersonService;

        public  VisitorService(IDbContextFactory<PeopleDb> dbContextFactory, AIPersonService aIPersonService)
        {
            _peopleDb = dbContextFactory.CreateDbContext();
            _aIPersonService = aIPersonService;
        }

        public async Task< Visitor> GetVisitorByIdAsync(Guid TenantId,Guid visitorId)
        {
            try
            {
                 Visitor visitor = await _peopleDb.Visitors
                    .Where(m => m.Id == visitorId && m.TenantId == TenantId).SingleOrDefaultAsync();

                return visitor;
            }
            catch
            {
                throw;
            }
        }
    }
}
