using System.ComponentModel.DataAnnotations;

namespace EventHub.Models;

public class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kategori adı zorunludur.")]
    [StringLength(80, ErrorMessage = "Kategori adı en fazla 80 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "Açıklama en fazla 250 karakter olabilir.")]
    public string? Description { get; set; }

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
