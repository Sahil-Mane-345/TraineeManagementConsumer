using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace TraineeAPI.Consumer;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    private readonly ConnectionFactory _connection;

    public Worker(ILogger<Worker> logger, ConnectionFactory connection)
    {
        _connection = connection;
        _logger = logger;
    }


    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var connection = await _connection.CreateConnectionAsync(cancellationToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: "submission-processing",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken
        );

        var cosnumer = new AsyncEventingBasicConsumer(channel);

        cosnumer.ReceivedAsync += async(saved, args) =>
        {
            var body = args.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            Console.WriteLine("Received message : " + message);
            await channel.BasicAckAsync(deliveryTag: args.DeliveryTag, multiple: false);
        };

        await channel.BasicConsumeAsync(
            queue: "submission-processing",
            autoAck: false,
            consumer: cosnumer,
            cancellationToken: cancellationToken
        );
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

}
