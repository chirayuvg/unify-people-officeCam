using AppStudio.Framework.Core;
using Unify.CCC.Data;

namespace Unify.CCC.Services
{
    public class AlertMessageService
    {
        private readonly CCCDb _cccdb;
        private readonly Guid _customerId;
        public AlertMessageService(CCCDb cccdb)
        {
            _cccdb = cccdb;
            _customerId = CoreServices.CurrentUser.GetTenantId();
        }

        public async Task<AlertMessage> AddAlertMessageAsync(AlertMessage alertMessage)
        {
            try
            {
                await _cccdb.AlertMessages.AddAsync(alertMessage);
                await _cccdb.SaveChangesAsync();

                return alertMessage;
            }
            catch
            {
                throw;
            }
        }
    }
}
