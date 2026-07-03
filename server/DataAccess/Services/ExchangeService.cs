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

    public async Task<IEnumerable<ExchangeOffer>> GetActiveOffersAsync()
    {
        var q = _db.ExchangeOffers
            .Include(o => o.UserPlant)
            .ThenInclude(up => up!.Plant)
            .Where(o => o.IsActive);

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

    public async Task<bool> ConfirmExchangeAsync(Guid exchangeOfferId, string currentUserId, string otherUserId)
    {
        // 1. Находим активное предложение обмена по ID
        var offer = await _db.ExchangeOffers
            .Include(o => o.UserPlant)
            .ThenInclude(up => up!.Plant)
            .FirstOrDefaultAsync(o => o.Id == exchangeOfferId && o.IsActive);

        if (offer == null) return false;

        // 2. Убеждаемся, что текущий пользователь — владелец объявления
        if (offer.OwnerId != currentUserId) return false;

        // 3. Убеждаемся, что к объявлению привязано растение
        if (offer.UserPlantId == null || offer.UserPlant == null) return false;

        var userPlant = offer.UserPlant;

        // 4. Убеждаемся, что растение принадлежит текущему пользователю
        if (userPlant.OwnerId != currentUserId) return false;

        // 5. Меняем владельца растения на собеседника (нового хозяина)
        userPlant.OwnerId = otherUserId;

        // 6. Очищаем все напоминания для этого растения
        var oldReminders = await _db.Reminders
            .Where(r => r.UserPlantId == userPlant.Id)
            .ToListAsync();
        _db.Reminders.RemoveRange(oldReminders);

        // 7. Создаем новые напоминания для нового хозяина на основе каталога растения
        if (userPlant.Plant != null)
        {
            var catalogPlant = userPlant.Plant;

            int wateringInterval = catalogPlant.WateringFrequencyDays > 0 ? catalogPlant.WateringFrequencyDays : 7;
            var wateringReminder = new Reminder
            {
                Id = Guid.NewGuid(),
                UserPlantId = userPlant.Id,
                Type = ReminderType.Watering,
                IntervalDays = wateringInterval,
                NextDueAt = DateTime.UtcNow.AddDays(wateringInterval),
                Enabled = true
            };
            _db.Reminders.Add(wateringReminder);

            if (catalogPlant.RepottingFrequencyMonths.HasValue && catalogPlant.RepottingFrequencyMonths.Value > 0)
            {
                int repottingIntervalDays = catalogPlant.RepottingFrequencyMonths.Value * 30;
                var repottingReminder = new Reminder
                {
                    Id = Guid.NewGuid(),
                    UserPlantId = userPlant.Id,
                    Type = ReminderType.Repotting,
                    IntervalDays = repottingIntervalDays,
                    NextDueAt = DateTime.UtcNow.AddMonths(catalogPlant.RepottingFrequencyMonths.Value),
                    Enabled = true
                };
                _db.Reminders.Add(repottingReminder);
            }
        }
        else
        {
            // Если растение «своё» (не из каталога), создаем базовое напоминание о поливе по умолчанию
            var wateringReminder = new Reminder
            {
                Id = Guid.NewGuid(),
                UserPlantId = userPlant.Id,
                Type = ReminderType.Watering,
                IntervalDays = 7,
                NextDueAt = DateTime.UtcNow.AddDays(7),
                Enabled = true
            };
            _db.Reminders.Add(wateringReminder);
        }

        // 8. Переводим объявление в неактивный статус (обмен завершен)
        offer.IsActive = false;

        await _db.SaveChangesAsync();
        return true;
    }
}
