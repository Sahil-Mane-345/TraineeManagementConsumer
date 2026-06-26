
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
        try
        {
            var res = await _httpClient.GetAsync("/trainee");

            if(res.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine("Trainee Not Found");
                return null;
            }
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<TraineeProfileModel>();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine(ex.Message);
            return default;
        }
        catch (System.Exception)
        {
            
            throw;
        }
    }
}