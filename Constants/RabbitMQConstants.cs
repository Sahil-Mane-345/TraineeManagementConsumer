namespace TraineeAPI.Consumer.Constants;

public static class RabbitMQConstants
{
    public const string SubmissionProcessingQueue = "submission-processing";

    public const string DeadLetterQueue = "submission-processing.dlq";
    public const string DeadLetterExchange = "submission-processing.dlx";

    public const string DeadLetterRoutingKey = "submission-processing.dlr";
}