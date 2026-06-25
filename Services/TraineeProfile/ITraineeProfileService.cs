using TraineeAPI.Consumer.Models;

namespace TraineeAPI.Consumer.Services.TraineeProfile;

public interface ITraineeProfileService
{
    Task<TraineeProfileModel?> GetTrainee();
}