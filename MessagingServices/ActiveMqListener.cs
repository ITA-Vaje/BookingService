using Apache.NMS;
using Apache.NMS.AMQP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookingService.MessagingServices
{
    public class ArtemisListener : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<ArtemisListener> _logger;

        public ArtemisListener(IConfiguration config, ILogger<ArtemisListener> logger)
        {
            _config = config;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                var brokerUri = _config["Artemis:BrokerUri"];
                var queueName = _config["Artemis:QueueName"];
                var username = _config["Artemis:Username"];
                var password = _config["Artemis:Password"];

                const int maxRetries = 5;
                const int delayMilliseconds = 3000;

                //for (int attempt = 1; attempt <= maxRetries; attempt++)
                //{
                //    try
                //    {
                //        var factory = new NmsConnectionFactory(brokerUri);
                //        _logger.LogInformation("Attempting to connect to Artemis broker at {brokerUri} (attempt {attempt})", brokerUri, attempt);

                //        using var connection = factory.CreateConnection(username, password);
                //        connection.Start();

                //        using var session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge);
                //        IDestination destination = session.GetQueue(queueName);
                //        using var consumer = session.CreateConsumer(destination);

                //        _logger.LogInformation("Artemis Listener started on queue: {Queue}", queueName);

                //        while (!stoppingToken.IsCancellationRequested)
                //        {
                //            IMessage message = consumer.Receive();
                //            if (message is ITextMessage textMessage)
                //            {
                //                _logger.LogInformation("Received message: {Message}", textMessage.Text);
                //                // Add your message processing logic here
                //            }
                //        }

                //        break; // Exit retry loop if connected and running
                //    }
                //    catch (Exception ex)
                //    {
                //        _logger.LogError(ex, "Failed to connect to Artemis on attempt {attempt}.", attempt);
                //        if (attempt == maxRetries)
                //        {
                //            _logger.LogError("Max retry attempts reached. Artemis listener will not start.");
                //            return;
                //        }

                //        Thread.Sleep(delayMilliseconds);
                //    }
                //}
            }, stoppingToken);
        }
    }
}
