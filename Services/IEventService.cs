namespace EventHub.Services;

public interface IEventService
{
    Task<EventOperationResult> JoinEventAsync(int eventId, string? userId);
    Task<EventOperationResult> LeaveEventAsync(int eventId, string? userId);
    Task<EventOperationResult> ToggleFavoriteAsync(int eventId, string? userId);
    Task<EventOperationResult> EnsureCapacityCanBeUpdatedAsync(int eventId, int newCapacity);
}
