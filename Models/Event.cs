using EventHub.Validation;
using System.ComponentModel.DataAnnotations;

namespace EventHub.Models;

public class Event
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Başlık zorunludur.")]
    [StringLength(100, ErrorMessage = "Başlık en fazla 100 karakter olabilir.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Açıklama zorunludur.")]
    [MinLength(20, ErrorMessage = "Açıklama en az 20 karakter olmalıdır.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Etkinlik tarihi zorunludur.")]
    [FutureDate(ErrorMessage = "Etkinlik tarihi gelecekte olmalıdır.")]
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "Konum zorunludur.")]
    [StringLength(150, ErrorMessage = "Konum en fazla 150 karakter olabilir.")]
    public string Location { get; set; } = string.Empty;

    [Range(2, int.MaxValue, ErrorMessage = "Kontenjan 1'den büyük olmalıdır.")]
    public int Capacity { get; set; }

    [Display(Name = "Görsel URL")]
    [Url(ErrorMessage = "Geçerli bir görsel URL giriniz.")]
    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "Kategori seçimi zorunludur.")]
    [Range(1, int.MaxValue, ErrorMessage = "Kategori seçimi zorunludur.")]
    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    [Required]
    public string OrganizerId { get; set; } = string.Empty;

    public ApplicationUser? Organizer { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public ICollection<EventParticipant> Participants { get; set; } = new List<EventParticipant>();
    public ICollection<FavoriteEvent> FavoriteEvents { get; set; } = new List<FavoriteEvent>();
}
