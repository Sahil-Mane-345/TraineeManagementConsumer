namespace TraineeAPI.Consumer.Entity;

public class SubmissionFile
{
    public Guid Id { get; set; }

    public required string GeneratedFileName { get; set; }

    public required string OriginalFileName { get; set; }

    public required Guid SubmissionId { get; set; }

    public Submission Submission { get; set; } = null!;

    public required string ContentType { get; set; }

    public required long Size { get; set; }

    public required string CheckSum { get; set; }

    public required Guid UserId { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedDate { get; set; }


}