
using System.Net.Http.Json;
using TraineeAPI.Consumer.Models;

namespace TraineeAPI.Consumer.Services.TraineeProfile;

public class TraineeProfileService : ITraineeProfileService
{
    private readonly HttpClient _httpClient;
    public TraineeProfileService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<TraineeProfileModel?> GetTrainee()
    {
        var trainees = await _httpClient.GetAsync("/trainee");
        return await trainees.Content.ReadFromJsonAsync<TraineeProfileModel>();
    }
}