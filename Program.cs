using RabbitMQ.Client;
using TraineeAPI.Consumer;

var builder = Host.CreateApplicationBuilder(args);

var rabbitMQSection = builder.Configuration.GetSection("RabbitMQ");

builder.Services.AddSingleton( sp => new ConnectionFactory()
{
    HostName = rabbitMQSection["HostName"]!,
    Port = Convert.ToInt32(rabbitMQSection["Port"]),
    UserName = rabbitMQSection["UserName"]!,
    Password = rabbitMQSection["Password"]!,
    VirtualHost = rabbitMQSection["VirtualHost"]!,
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
