namespace EventHub.Services;

public class DatabaseStatusService
{
    private readonly object _lock = new();

    public bool IsAvailable { get; private set; }
    public string? LastError { get; private set; }

    public void MarkAvailable()
    {
        lock (_lock)
        {
            IsAvailable = true;
            LastError = null;
        }
    }

    public void MarkUnavailable(string? error)
    {
        lock (_lock)
        {
            IsAvailable = false;
            LastError = error;
        }
    }
}
