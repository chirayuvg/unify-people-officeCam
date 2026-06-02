using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Web;
using System.Text;

namespace UnifyNow.Forest.RulesEngine
{
    public class SaveDataRabbitMQBackgroundService : BackgroundService
    {
        private readonly ILogger _logger;
        private IConnection _connection;
        private RabbitMQ.Client.IModel _channel;
        IConfiguration _configuration;

        ForestRulesEngine _forestRulesEngine;


        public SaveDataRabbitMQBackgroundService(ILoggerFactory loggerFactory,
            IConfiguration configuration
            //, WorkplaceRulesEngine rulesEngine
            , ForestRulesEngine forestRulesEngine
            )
        {
            this._logger = loggerFactory.CreateLogger<SaveDataRabbitMQBackgroundService>();
            _configuration = configuration;
            //_rulesEngine = rulesEngine;
           _forestRulesEngine = forestRulesEngine;  
            InitRabbitMQ();
        }

        private void InitRabbitMQ()
        {
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
            // new code
            _channel.QueueDeclare(queue: "task_queue", durable: true, exclusive: false, autoDelete: false,
                                arguments: null);

            // new code

            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (ch, ea) =>
            {

                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // handle the received message
                // commented
                await HandleMessage(message);
                // commented

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume("task_queue", false, consumer);

            // new code
        }

        private async Task HandleMessage(string message)
        {
            _logger.LogDebug($"consumer received {message}");

            var cameraData = System.Text.Json.JsonSerializer.Deserialize<CameraDataModel>(message);
            string processName = cameraData.IntelligenceName.ToLower();
            await _forestRulesEngine.ProcessAsync(cameraData);
        }
        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e) { }
        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerRegistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e) { }
        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) { }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
    public class CameraDataModel
    {
        public Guid CameraId { get; set; }
        public string CameraName { get; set; }
        public Guid IntelligenceId { get; set; }
        public string IntelligenceName { get; set; }
        public Guid CustomerId { get; set; }
        public List<SegmentsModel> SegmentsModels { get; set; }
        public List<CameraMetaDataModel> CameraMetaDataModels { get; set; }
        public DateTime DateTime { get; set; } = DateTime.UtcNow.AddMinutes(330);
        public DateTime TimeStamp { get; set; }
        public List<ClassData> Classes { get; set; } = new List<ClassData>();
        public string? FileURL { get; set; }

        public string? PresetId { get; set; }
    }

    public class ClassData
    {
        public string Name { get; set; }
        public string BoundingBox { get; set; } = string.Empty;
        public string Polygon { get; set; } = string.Empty;
        //public int TrackingId { get; set; }
        public string TrackingId { get; set; }


        public List<ClassProperty> Properties { get; set; } = new List<ClassProperty>();
    }

    public class ClassProperty
    {
        public string PropertyName { get; set; } = string.Empty;
        public string PropertyValue { get; set; }
    }

    public class SegmentsModel
    {
        public string Polygon { get; set; }
        public List<SegmentMetaDataModel> SegmentMetaDataModels { get; set; }
    }

    public class SegmentMetaDataModel
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
    public class CameraMetaDataModel
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
