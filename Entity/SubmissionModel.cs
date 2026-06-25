namespace TraineeAPI.Consumer.Entity;

public class Submission
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required Guid TaskAssignmentId { get; set; }

    public required string SubmissionUrl { get; set; }

    public string Notes { get; set; } = string.Empty;

    public required DateOnly SubmittedDate { get; set; }

    public required string Status { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

}