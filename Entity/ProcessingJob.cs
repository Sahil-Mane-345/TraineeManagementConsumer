
namespace TraineeAPI.Consumer.Entity;


public enum ProcessingJobEnumValues { Queued, Processing, Completed, Failed }

public class ProcessingJob
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int Attempts { get; set; } = 0;

    public required string CorrelationId { get; set; }

    public required Guid SubmissionFileId { get; set; }

    public required string MessageId { get; set; }

    public string? ErrorSummary { get; set; }

    public required string Status { get; set; }

    public DateTime? StartedTime { get; set; }

    public DateTime? CompletedTime { get; set; }
}