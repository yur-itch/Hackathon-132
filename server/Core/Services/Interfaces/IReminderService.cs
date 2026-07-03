using PlantCare.Api.Dtos;
using PlantCare.Api.Models;

namespace PlantCare.Api.Services.Interfaces;

public interface IReminderService
{
    /// <summary>
    /// Получить все напоминания пользователя.
    /// </summary>
    /// <param name="ownerId">ID владельца</param>
    /// <param name="dueOnly">Фильтр: только просроченные или на сегодня</param>
    Task<IEnumerable<Reminder>> GetMineAsync(string ownerId, bool dueOnly);

    /// <summary>
    /// Создать напоминание для конкретного растения.
    /// </summary>
    Task<Reminder?> CreateAsync(CreateReminderDto dto, string ownerId);

    /// <summary>
    /// Обновить параметры напоминания.
    /// </summary>
    Task<bool> UpdateAsync(int id, UpdateReminderDto dto, string ownerId);

    /// <summary>
    /// Отметить напоминание выполненным (сдвигает NextDueAt на IntervalDays вперед).
    /// </summary>
    Task<bool> MarkDoneAsync(int id, string ownerId);

    /// <summary>
    /// Удалить напоминание.
    /// </summary>
    Task<bool> DeleteAsync(int id, string ownerId);
}
