using RabbitMQ.Client;
using TraineeAPI.Consumer.Models;

namespace TraineeAPI.Consumer.Services;

public interface IFileProcessingService
{
    Task FileProcessingAsync(SubmissionProcessingRequestDto file, IReadOnlyBasicProperties basicProperties);
}