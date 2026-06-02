using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Web;
using UnifyNow.Forest.RulesEngine;

namespace UnifyNow.Forest.Services
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
        private void PublishRabbitMQ(CameraDataModel cameraDataModel)
        {
            //string queueName = hubDataModel.UseCase.ToLower() + "_queue";
            string queueName = "task_queue";
            //string connectionString = _configuration.GetConnectionString("RabbitMQ");
            string username = _configuration.GetValue<string>("RabbitMQSettings:UserName");// _configuration.GetConnectionString("UserName");
            string password = _configuration.GetValue<string>("RabbitMQSettings:Password");//_configuration.GetConnectionString("Password");
            string url = _configuration.GetValue<string>("RabbitMQSettings:Url");//_configuration.GetConnectionString("Url");


            //string connectionString = $"amqp://YOUR_RABBITMQ_USER:YOUR_RABBITMQ_PASSWORD@{url}/{HttpUtility.UrlEncode("/")}";

            var factory = new ConnectionFactory
            {
                HostName = "192.168.0.125",
                UserName = "unifynow",
                Password = "unifynow",
                Port = 5672
            };

            _connection = factory.CreateConnection();

            // create channel
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false,
                                arguments: null);

            var bodyToPublish = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(cameraDataModel));
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(exchange: string.Empty,
                                 routingKey: queueName,
                                 basicProperties: null,
                                 body: bodyToPublish);


        }
        public async Task<bool> InferenceDataAsync(CameraDataModel cameraDataModel)
        {
            try
            {
                PublishRabbitMQ(cameraDataModel);
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