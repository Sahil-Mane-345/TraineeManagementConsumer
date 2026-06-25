using Microsoft.EntityFrameworkCore;
using TraineeAPI.Consumer.Context;
using TraineeAPI.Consumer.Services;

namespace TraineeAPI.Consumer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServiceExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionStringMySQL = configuration.GetConnectionString("DefaultConnection")!;
        ServerVersion serverVersion = ServerVersion.AutoDetect(connectionStringMySQL);

        services.AddDbContext<AppDbContext>( opt =>
        {
            opt.UseMySql(connectionStringMySQL, serverVersion);
        });

        services.AddScoped<IFileProcessingService, FileProcessingService>();

        return services;
    }
}