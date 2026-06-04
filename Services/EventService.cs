using EventHub.Data;
using EventHub.Models;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Services;

public class EventService : IEventService
{
    private readonly ApplicationDbContext _context;

    public EventService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EventOperationResult> JoinEventAsync(int eventId, string? userId)
    {
        var eventItem = await _context.Events
            .Include(e => e.Participants)
            .FirstOrDefaultAsync(e => e.Id == eventId && e.IsActive);

        if (eventItem is null || userId is null)
        {
            return EventOperationResult.Missing();
        }

        if (eventItem.Date <= DateTime.Now)
        {
            return EventOperationResult.Failure("Geçmiş tarihli etkinliklere katılamazsınız.");
        }

        if (eventItem.Participants.Any(p => p.UserId == userId))
        {
            return EventOperationResult.Failure("Bu etkinliğe zaten katıldınız.");
        }

        if (eventItem.Participants.Count >= eventItem.Capacity)
        {
            return EventOperationResult.Failure("Etkinlik kontenjanı dolu.");
        }

        _context.EventParticipants.Add(new EventParticipant { EventId = eventId, UserId = userId, JoinDate = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        return EventOperationResult.Success("Etkinliğe katılımınız alındı.");
    }

    public async Task<EventOperationResult> LeaveEventAsync(int eventId, string? userId)
    {
        var participant = await _context.EventParticipants.FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId);

        if (participant is null)
        {
            return EventOperationResult.Failure("Bu etkinlikte kaydınız bulunamadı.");
        }

        _context.EventParticipants.Remove(participant);
        await _context.SaveChangesAsync();

        return EventOperationResult.Success("Etkinlikten ayrıldınız.");
    }

    public async Task<EventOperationResult> ToggleFavoriteAsync(int eventId, string? userId)
    {
        var eventExists = await _context.Events.AnyAsync(e => e.Id == eventId && e.IsActive);
        if (!eventExists || userId is null)
        {
            return EventOperationResult.Missing();
        }

        var favorite = await _context.FavoriteEvents.FirstOrDefaultAsync(f => f.EventId == eventId && f.UserId == userId);
        if (favorite is null)
        {
            _context.FavoriteEvents.Add(new FavoriteEvent { EventId = eventId, UserId = userId, CreatedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();
            return EventOperationResult.Success("Etkinlik favorilere eklendi.");
        }

        _context.FavoriteEvents.Remove(favorite);
        await _context.SaveChangesAsync();
        return EventOperationResult.Success("Etkinlik favorilerden çıkarıldı.");
    }

    public async Task<EventOperationResult> EnsureCapacityCanBeUpdatedAsync(int eventId, int newCapacity)
    {
        var participantCount = await _context.EventParticipants.CountAsync(p => p.EventId == eventId);
        if (newCapacity < participantCount)
        {
            return EventOperationResult.Failure("Kontenjan mevcut katılımcı sayısından düşük olamaz.");
        }

        return EventOperationResult.Success(string.Empty);
    }
}
