using RabbitMQ.Client;
using TraineeAPI.Consumer;
using TraineeAPI.Consumer.Extensions;
using TraineeAPI.Consumer.Services;
using TraineeAPI.Consumer.Services.TraineeProfile;

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

// builder.Services.ConfigureHttpClientDefaults(builder => builder.AddStandardResilienceHandler());

builder.Services.AddHttpClient<ITraineeProfileService, TraineeProfileService>(sp =>
{
    sp.BaseAddress = new Uri(builder.Configuration["TraineeDirectoryApi:BaseUrl"]!);
    sp.Timeout = TimeSpan.FromSeconds(Convert.ToInt16(builder.Configuration["TraineeDirectoryApi:TimeOutSeconds"]!));

}).AddStandardResilienceHandler( opt =>
{
    opt.Retry.MaxRetryAttempts = 3;

    opt.CircuitBreaker.FailureRatio = 0.5;
    opt.CircuitBreaker.MinimumThroughput = 2;
    opt.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
    opt.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);

    opt.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10);
    opt.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);
});

// builder.Services.AddHostedService<TraineeDirectoryBackgroundService>();

builder.Services.AddServiceExtensions(builder.Configuration);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
