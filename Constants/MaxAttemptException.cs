namespace TraineeAPI.Consumer.Constants;

public class MaxAttemptException : Exception
{
    public int Attempts { get; set; }

    public MaxAttemptException(int attempts, string message ): base(message)
    {
        Attempts = attempts;
    }
}