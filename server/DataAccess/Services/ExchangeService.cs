using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Data;
using PlantCare.Api.Models;
using PlantCare.Api.Services.Interfaces;

namespace PlantCare.Api.Services.Implementations;

public class ExchangeService : IExchangeService
{
    private readonly AppDbContext _db;

    public ExchangeService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ExchangeOffer> CreateOfferAsync(
        string ownerId, 
        string title, 
        string description, 
        string wantedPlantDescription, 
        Guid? userPlantId)
    {
        var offer = new ExchangeOffer
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Title = title,
            Description = description,
            WantedPlantDescription = wantedPlantDescription,
            UserPlantId = userPlantId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _db.ExchangeOffers.Add(offer);
        await _db.SaveChangesAsync();

        if (offer.UserPlantId.HasValue)
        {
            offer.UserPlant = await _db.UserPlants
                .Include(up => up.Plant)
                .FirstOrDefaultAsync(up => up.Id == offer.UserPlantId.Value);
        }

        return offer;
    }

    public async Task<IEnumerable<ExchangeOffer>> GetActiveOffersAsync(string? search)
    {
        var q = _db.ExchangeOffers
            .Include(o => o.UserPlant)
            .ThenInclude(up => up!.Plant)
            .Where(o => o.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLowerInvariant();
            q = q.Where(o => o.Title.ToLower().Contains(searchLower) ||
                             o.Description.ToLower().Contains(searchLower) ||
                             o.WantedPlantDescription.ToLower().Contains(searchLower));
        }

        return await q.OrderByDescending(o => o.CreatedAt).ToListAsync();
    }

    public async Task<ExchangeOffer?> GetOfferByIdAsync(Guid id)
    {
        return await _db.ExchangeOffers
            .Include(o => o.UserPlant)
            .ThenInclude(up => up!.Plant)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<bool> CloseOfferAsync(Guid id, string ownerId)
    {
        var offer = await _db.ExchangeOffers.FirstOrDefaultAsync(o => o.Id == id && o.OwnerId == ownerId);
        if (offer == null) return false;

        offer.IsActive = false;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<ChatMessage?> SendMessageAsync(
        string senderId, 
        Guid exchangeOfferId, 
        string receiverId, 
        string text)
    {
        var offerExists = await _db.ExchangeOffers.AnyAsync(o => o.Id == exchangeOfferId);
        if (!offerExists) return null;

        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            ExchangeOfferId = exchangeOfferId,
            SenderId = senderId,
            ReceiverId = receiverId,
            Text = text,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        _db.ChatMessages.Add(message);
        await _db.SaveChangesAsync();
        return message;
    }

    public async Task<IEnumerable<ChatMessage>> GetChatMessagesAsync(
        Guid exchangeOfferId, 
        string otherUserId, 
        string currentUserId)
    {
        return await _db.ChatMessages
            .Where(m => m.ExchangeOfferId == exchangeOfferId &&
                        ((m.SenderId == currentUserId && m.ReceiverId == otherUserId) ||
                         (m.SenderId == otherUserId && m.ReceiverId == currentUserId)))
            .OrderBy(m => m.SentAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<(ExchangeOffer Offer, string OtherUserId, string OtherUserDisplayName, ChatMessage LastMessage)>> GetMyChatsAsync(string currentUserId)
    {
        var messages = await _db.ChatMessages
            .Include(m => m.ExchangeOffer)
            .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
            .ToListAsync();

        var chatGroups = messages
            .GroupBy(m => new
            {
                m.ExchangeOfferId,
                OtherUserId = m.SenderId == currentUserId ? m.ReceiverId : m.SenderId
            });

        var chats = new List<(ExchangeOffer Offer, string OtherUserId, string OtherUserDisplayName, ChatMessage LastMessage)>();

        foreach (var group in chatGroups)
        {
            var lastMessage = group.OrderByDescending(m => m.SentAt).First();
            var offer = lastMessage.ExchangeOffer ?? new ExchangeOffer { Id = group.Key.ExchangeOfferId, Title = "Удаленное объявление" };

            string otherUserDisplayName = "Пользователь";
            if (int.TryParse(group.Key.OtherUserId, out int userId))
            {
                var user = await _db.Users.FindAsync(userId);
                if (user != null)
                {
                    otherUserDisplayName = user.DisplayName;
                }
            }
            else if (group.Key.OtherUserId == "local")
            {
                otherUserDisplayName = "Анонимный цветовод";
            }

            chats.Add((offer, group.Key.OtherUserId, otherUserDisplayName, lastMessage));
        }

        return chats.OrderByDescending(c => c.LastMessage.SentAt).ToList();
    }
}
