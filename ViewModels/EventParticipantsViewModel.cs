namespace EventHub.ViewModels;

public class EventParticipantsViewModel
{
    public int EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public List<ParticipantRowViewModel> Participants { get; set; } = new();
}

public class ParticipantRowViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime JoinDate { get; set; }
}
