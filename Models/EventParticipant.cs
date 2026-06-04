namespace EventHub.Models;

public class EventParticipant
{
    public int Id { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public DateTime JoinDate { get; set; } = DateTime.UtcNow;
}
