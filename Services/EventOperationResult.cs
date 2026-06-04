namespace EventHub.Services;

public class EventOperationResult
{
    public bool Succeeded { get; private set; }
    public bool NotFound { get; private set; }
    public string Message { get; private set; } = string.Empty;

    public static EventOperationResult Success(string message)
    {
        return new EventOperationResult { Succeeded = true, Message = message };
    }

    public static EventOperationResult Failure(string message)
    {
        return new EventOperationResult { Message = message };
    }

    public static EventOperationResult Missing()
    {
        return new EventOperationResult { NotFound = true };
    }
}
