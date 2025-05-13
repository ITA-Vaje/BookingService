using Apache.NMS;
using Apache.NMS.AMQP;
using BookingService.Models;

public class ArtemisPublisher
{
    private readonly string _brokerUri;
    private readonly string _queueName;
    private readonly string _username;
    private readonly string _password;

    public ArtemisPublisher(IConfiguration config)
    {
        _brokerUri = config["Artemis:BrokerUri"];
        _queueName = config["Artemis:QueueName"];
        _username = config["Artemis:Username"];
        _password = config["Artemis:Password"];
    }

    public void SendBookingCreatedMessage(Booking booking)
    {
        SendMessage($"Booking CREATED: ID={booking.Id}");
    }

    public void SendBookingUpdatedMessage(Booking booking)
    {
        SendMessage($"Booking UPDATED: ID={booking.Id}");
    }

    public void SendBookingDeletedMessage(Booking booking)
    {
        SendMessage($"Booking DELETED: ID={booking.Id}");
    }

    private void SendMessage(string message)
    {
        const int maxRetries = 5;
        const int delayMilliseconds = 3000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var factory = new NmsConnectionFactory(_brokerUri);
                using var connection = factory.CreateConnection(_username, _password);
                connection.Start();

                using var session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge);
                var destination = session.GetQueue(_queueName);
                using var producer = session.CreateProducer(destination);
                var textMessage = session.CreateTextMessage(message);
                producer.Send(textMessage);

                Console.WriteLine($"Message sent: {message}");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Attempt {attempt} failed: {ex.Message}");

                if (attempt == maxRetries)
                {
                    Console.WriteLine("Max retry attempts reached. Giving up.");
                    return;
                }

                Thread.Sleep(delayMilliseconds); // Wait before retrying
            }
        }
    }


}
