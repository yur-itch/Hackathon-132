using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Data;
using PlantCare.Api.Dtos;
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

    public async Task<ExchangeOffer> CreateOfferAsync(CreateExchangeOfferDto dto, string ownerId)
    {
        var offer = new ExchangeOffer
        {
            OwnerId = ownerId,
            Title = dto.Title,
            Description = dto.Description,
            WantedPlantDescription = dto.WantedPlantDescription,
            UserPlantId = dto.UserPlantId,
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

    public async Task<ExchangeOffer?> GetOfferByIdAsync(int id)
    {
        return await _db.ExchangeOffers
            .Include(o => o.UserPlant)
            .ThenInclude(up => up!.Plant)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<bool> CloseOfferAsync(int id, string ownerId)
    {
        var offer = await _db.ExchangeOffers.FirstOrDefaultAsync(o => o.Id == id && o.OwnerId == ownerId);
        if (offer == null) return false;

        offer.IsActive = false;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<ChatMessage?> SendMessageAsync(SendChatMessageDto dto, string senderId)
    {
        // Проверяем, что объявление существует
        var offer = await _db.ExchangeOffers.AnyAsync(o => o.Id == dto.ExchangeOfferId);
        if (!offer) return null;

        var message = new ChatMessage
        {
            ExchangeOfferId = dto.ExchangeOfferId,
            SenderId = senderId,
            ReceiverId = dto.ReceiverId,
            Text = dto.Text,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        _db.ChatMessages.Add(message);
        await _db.SaveChangesAsync();
        return message;
    }

    public async Task<IEnumerable<ChatMessage>> GetChatMessagesAsync(int exchangeOfferId, string otherUserId, string currentUserId)
    {
        return await _db.ChatMessages
            .Where(m => m.ExchangeOfferId == exchangeOfferId &&
                        ((m.SenderId == currentUserId && m.ReceiverId == otherUserId) ||
                         (m.SenderId == otherUserId && m.ReceiverId == currentUserId)))
            .OrderBy(m => m.SentAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChatDto>> GetMyChatsAsync(string currentUserId)
    {
        // 1. Получаем все сообщения пользователя
        var messages = await _db.ChatMessages
            .Include(m => m.ExchangeOffer)
            .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
            .ToListAsync();

        // 2. Группируем сообщения по (ExchangeOfferId, Собеседник)
        var chatGroups = messages
            .GroupBy(m => new
            {
                m.ExchangeOfferId,
                OtherUserId = m.SenderId == currentUserId ? m.ReceiverId : m.SenderId
            });

        var chats = new List<ChatDto>();

        foreach (var group in chatGroups)
        {
            var lastMessage = group.OrderByDescending(m => m.SentAt).First();
            var offerTitle = lastMessage.ExchangeOffer?.Title ?? "Объявление удалено";

            // Ищем отображаемое имя собеседника в БД
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

            chats.Add(new ChatDto(
                ExchangeOfferId: group.Key.ExchangeOfferId,
                ExchangeOfferTitle: offerTitle,
                OtherUserId: group.Key.OtherUserId,
                OtherUserDisplayName: otherUserDisplayName,
                LastMessageAt: lastMessage.SentAt,
                LastMessageText: lastMessage.Text
            ));
        }

        return chats.OrderByDescending(c => c.LastMessageAt).ToList();
    }
}
