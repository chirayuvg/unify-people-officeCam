using AppStudio.Framework.Core;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Web;

namespace Unify.People.Services
{
    public class InferenceServices
    {
        private readonly ILogger _logger;
        private IConnection _connection;
        private IModel _channel;
        IConfiguration _configuration;
        public InferenceServices(ILoggerFactory loggerFactory,
            IConfiguration configuration)
        {
            this._logger = loggerFactory.CreateLogger<InferenceServices>();
            _configuration = configuration;
        }
        public async Task<bool> InferenceDataAsync(CameraDataModel cameraDataModel)
        {
            try
            {
                await CoreServices.Messaging.SendLocalAsync(cameraDataModel);
               // PublishRabbitMQ(cameraDataModel);
                return true;
            }
            catch
            {
                throw;
            }
        }
    }
    public class HubDataModel
    {
        public string Message { get; set; }

        public string UseCase { get; set; }


    }
}
