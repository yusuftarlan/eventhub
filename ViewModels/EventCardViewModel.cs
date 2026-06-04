namespace EventHub.ViewModels;

public class EventCardViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int ParticipantCount { get; set; }
    public string? ImageUrl { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsJoined { get; set; }
    public bool IsFavorite { get; set; }
    public int RemainingCapacity => Math.Max(0, Capacity - ParticipantCount);
}
