namespace EventHub.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalEvents { get; set; }
    public int TotalUsers { get; set; }
    public int TotalCategories { get; set; }
    public int TotalParticipations { get; set; }
    public List<EventCardViewModel> TopEvents { get; set; } = new();
}
