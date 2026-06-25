using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TraineeAPI.Consumer.Constants;
using TraineeAPI.Consumer.Models;
using TraineeAPI.Consumer.Services;

namespace TraineeAPI.Consumer;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    private readonly ConnectionFactory _connection;

    private readonly IServiceScopeFactory _serviceScopeFactory;

    private readonly int MaxAttempts = 5;


    public Worker(ILogger<Worker> logger, ConnectionFactory connection, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
    {
        _connection = connection;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }


    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
        using var connection = await _connection.CreateConnectionAsync(cancellationToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: RabbitMQConstants.SubmissionProcessingQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>{
                ["x-dead-letter-exchange"] = RabbitMQConstants.DeadLetterExchange,
                ["x-dead-letter-routing-key"] = RabbitMQConstants.DeadLetterRoutingKey
            },
            cancellationToken: cancellationToken
        );

        await channel.ExchangeDeclareAsync(exchange: RabbitMQConstants.DeadLetterExchange, type: ExchangeType.Direct, durable: true, autoDelete: false, cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: RabbitMQConstants.DeadLetterQueue,
            exclusive: false,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken
        );

        await channel.QueueBindAsync(
            queue: RabbitMQConstants.DeadLetterQueue,
            exchange: RabbitMQConstants.DeadLetterExchange,
            routingKey: RabbitMQConstants.DeadLetterRoutingKey,
            cancellationToken: cancellationToken
        );


        var cosnumer = new AsyncEventingBasicConsumer(channel);

        cosnumer.ReceivedAsync += async( _ , args) =>
        {
            try
            {
                var body = args.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                SubmissionProcessingRequestDto file = JsonSerializer.Deserialize<SubmissionProcessingRequestDto>(message)!;

                using var scope = _serviceScopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IFileProcessingService>();

                Console.WriteLine("Received message : " + message);
                await service.FileProcessingAsync(file, args.BasicProperties);


                await channel.BasicAckAsync(deliveryTag: args.DeliveryTag, multiple: false);
            }
            catch (MaxAttemptException ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.Attempts > MaxAttempts)
                {
                    await channel.BasicNackAsync(deliveryTag: args.DeliveryTag, multiple: false, requeue: false);
                }else{
                    await channel.BasicNackAsync(deliveryTag: args.DeliveryTag, multiple: false, requeue: true);
                }
            }catch(Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                Console.WriteLine(ex.InnerException);
                await channel.BasicNackAsync(deliveryTag: args.DeliveryTag, multiple: false, requeue: false);
                throw;
            }
        };

        await channel.BasicConsumeAsync(
            queue: "submission-processing",
            autoAck: false,
            consumer: cosnumer,
            cancellationToken: cancellationToken
        );
        await Task.Delay(Timeout.Infinite, cancellationToken);
            
        }
        catch (System.Exception)
        {
            Console.WriteLine("RabbitMq is not working");
            throw;
        }
    }

}
