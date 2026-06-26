

using TraineeAPI.Consumer.Services.TraineeProfile;
using TraineeAPI.Consumer.Models;
using System.Text.Json;
using Polly.Timeout;
namespace TraineeAPI.Consumer.Services;

public class TraineeDirectoryBackgroundService : BackgroundService
{
    private readonly ITraineeProfileService _traineeProfile;
    public TraineeDirectoryBackgroundService(ITraineeProfileService traineeProfile)
    {
        _traineeProfile = traineeProfile;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
            try
            {
                
                TraineeProfileModel trainee = await _traineeProfile.GetTrainee() ?? throw new Exception("Trainee Not Found");
                Console.WriteLine(JsonSerializer.Serialize(trainee));
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"Task Cancellation error : {ex.Message}");
            }
            catch (TimeoutRejectedException ex)
            {
                Console.WriteLine($"Time out rejected exception error : {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception : {ex.Message}");
            }
        
    }
}