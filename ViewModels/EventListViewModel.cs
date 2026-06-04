using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventHub.ViewModels;

public class EventListViewModel
{
    public List<EventCardViewModel> Events { get; set; } = new();
    public int? CategoryId { get; set; }
    public string? SearchTerm { get; set; }
    public SelectList? Categories { get; set; }
}
