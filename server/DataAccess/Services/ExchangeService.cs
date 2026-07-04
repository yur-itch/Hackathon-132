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

    public async Task<(CreateOfferResult Result, ExchangeOffer? Offer)> CreateOfferAsync(
        string ownerId,
        string title,
        string? description,
        int wantedPlantId,
        Guid userPlantId)
    {
        // Отдаваемое растение должно существовать и принадлежать создателю объявления
        var offeredPlant = await _db.UserPlants
            .Include(up => up.Plant)
            .FirstOrDefaultAsync(up => up.Id == userPlantId && up.OwnerId == ownerId);
        if (offeredPlant is null)
        {
            return (CreateOfferResult.OfferedPlantNotFound, null);
        }

        // Желаемое растение должно быть из каталога
        var wantedPlant = await _db.Plants.FindAsync(wantedPlantId);
        if (wantedPlant is null)
        {
            return (CreateOfferResult.WantedPlantNotFound, null);
        }

        var offer = new ExchangeOffer
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Title = title,
            Description = description ?? "",
            WantedPlantId = wantedPlantId,
            UserPlantId = userPlantId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _db.ExchangeOffers.Add(offer);
        await _db.SaveChangesAsync();

        offer.UserPlant = offeredPlant;
        offer.WantedPlant = wantedPlant;
        return (CreateOfferResult.Created, offer);
    }

    public async Task<bool> CanUserAccessChatAsync(Guid exchangeOfferId, string userId)
    {
        var offer = await _db.ExchangeOffers
            .FirstOrDefaultAsync(o => o.Id == exchangeOfferId);
        if (offer is null) return false;

        // Владелец объявления всегда имеет доступ к своим чатам
        if (offer.OwnerId == userId) return true;

        // Остальные — только если у них есть желаемое растение в коллекции
        return await _db.UserPlants
            .AnyAsync(up => up.OwnerId == userId && up.PlantId == offer.WantedPlantId);
    }

    public async Task<IEnumerable<ExchangeOffer>> GetActiveOffersAsync()
    {
        var q = _db.ExchangeOffers
            .Include(o => o.UserPlant)
            .ThenInclude(up => up!.Plant)
            .Include(o => o.WantedPlant)
            .Where(o => o.IsActive);

        return await q.OrderByDescending(o => o.CreatedAt).ToListAsync();
    }

    public async Task<ExchangeOffer?> GetOfferByIdAsync(Guid id)
    {
        return await _db.ExchangeOffers
            .Include(o => o.UserPlant)
            .ThenInclude(up => up!.Plant)
            .Include(o => o.WantedPlant)
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

    public async Task<ConfirmExchangeResult> ConfirmExchangeAsync(Guid exchangeOfferId, string currentUserId, string otherUserId)
    {
        // 1. Активное объявление, владелец = текущий пользователь
        var offer = await _db.ExchangeOffers
            .Include(o => o.UserPlant)
            .ThenInclude(up => up!.Plant)
            .FirstOrDefaultAsync(o => o.Id == exchangeOfferId && o.IsActive);

        if (offer is null || offer.OwnerId != currentUserId)
        {
            return ConfirmExchangeResult.OfferNotFound;
        }

        // 2. Отдаваемое растение всё ещё существует и принадлежит владельцу
        //    (мог отдать его в другом обмене или удалить из коллекции)
        var offeredPlant = offer.UserPlant;
        if (offer.UserPlantId is null || offeredPlant is null || offeredPlant.OwnerId != currentUserId)
        {
            return ConfirmExchangeResult.OfferedPlantMissing;
        }

        // 3. У собеседника всё ещё есть подходящее растение (желаемого вида).
        //    Если несколько — берём добавленное раньше всех.
        var responderPlant = await _db.UserPlants
            .Include(up => up.Plant)
            .Where(up => up.OwnerId == otherUserId && up.PlantId == offer.WantedPlantId)
            .OrderBy(up => up.AddedAt)
            .FirstOrDefaultAsync();

        if (responderPlant is null)
        {
            return ConfirmExchangeResult.WantedPlantMissing;
        }

        // 4. Двусторонняя передача: растения меняются хозяевами
        offeredPlant.OwnerId = otherUserId;
        responderPlant.OwnerId = currentUserId;

        // 5. Напоминания пересоздаём для обоих новых хозяев
        await ResetRemindersForNewOwnerAsync(offeredPlant);
        await ResetRemindersForNewOwnerAsync(responderPlant);

        // 6. Обмен завершён — объявление закрывается
        offer.IsActive = false;

        await _db.SaveChangesAsync();
        return ConfirmExchangeResult.Confirmed;
    }

    // Сбрасывает и пересоздаёт напоминания для растения после смены владельца:
    // интервалы берутся из каталога, а для «своего» растения — дефолт (полив раз в 7 дней).
    private async Task ResetRemindersForNewOwnerAsync(UserPlant plant)
    {
        var oldReminders = await _db.Reminders
            .Where(r => r.UserPlantId == plant.Id)
            .ToListAsync();
        _db.Reminders.RemoveRange(oldReminders);

        var catalogPlant = plant.Plant;

        int wateringInterval = catalogPlant is { WateringFrequencyDays: > 0 }
            ? catalogPlant.WateringFrequencyDays
            : 7;

        _db.Reminders.Add(new Reminder
        {
            Id = Guid.NewGuid(),
            UserPlantId = plant.Id,
            Type = ReminderType.Watering,
            IntervalDays = wateringInterval,
            NextDueAt = DateTime.UtcNow.AddDays(wateringInterval),
            Enabled = true
        });

        if (catalogPlant?.RepottingFrequencyMonths is > 0)
        {
            int months = catalogPlant.RepottingFrequencyMonths.Value;
            _db.Reminders.Add(new Reminder
            {
                Id = Guid.NewGuid(),
                UserPlantId = plant.Id,
                Type = ReminderType.Repotting,
                IntervalDays = months * 30,
                NextDueAt = DateTime.UtcNow.AddMonths(months),
                Enabled = true
            });
        }
    }
}
