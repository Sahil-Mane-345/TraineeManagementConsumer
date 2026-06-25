namespace TraineeAPI.Consumer.Models;

public class SubmissionProcessingRequestDto
{

    public required Guid SubmissionFileId { get; set; }

    public string ContractVersion { get; set; } = "v1";
}