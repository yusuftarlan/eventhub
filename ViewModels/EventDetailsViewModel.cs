namespace EventHub.ViewModels;

public class EventDetailsViewModel
{
    public EventCardViewModel Event { get; set; } = new();
    public string OrganizerName { get; set; } = string.Empty;
    public string FullDescription { get; set; } = string.Empty;
}
