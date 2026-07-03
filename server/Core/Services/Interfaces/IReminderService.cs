using PlantCare.Api.Models;

namespace PlantCare.Api.Services.Interfaces;

public interface IReminderService
{
    /// <summary>
    /// Получить все напоминания пользователя.
    /// </summary>
    Task<IEnumerable<Reminder>> GetMineAsync(string ownerId, bool dueOnly);

    /// <summary>
    /// Создать напоминание для конкретного растения.
    /// </summary>
    Task<Reminder?> CreateAsync(
        string ownerId, 
        Guid userPlantId, 
        ReminderType type, 
        int intervalDays, 
        DateTime? nextDueAt);

    /// <summary>
    /// Обновить параметры напоминания.
    /// </summary>
    Task<bool> UpdateAsync(
        string ownerId, 
        Guid id, 
        int intervalDays, 
        bool enabled);

    /// <summary>
    /// Отметить напоминание выполненным (сдвигает NextDueAt на IntervalDays вперед).
    /// </summary>
    Task<bool> MarkDoneAsync(Guid id, string ownerId);

    /// <summary>
    /// Удалить напоминание.
    /// </summary>
    Task<bool> DeleteAsync(Guid id, string ownerId);
}
