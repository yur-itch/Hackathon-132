using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Data;
using PlantCare.Api.Models;
using PlantCare.Api.Services.Interfaces;

namespace PlantCare.Api.Services.Background;

public class ReminderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ReminderBackgroundService> _logger;

    public ReminderBackgroundService(IServiceProvider services, ILogger<ReminderBackgroundService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Reminder Background Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckDueRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking due reminders.");
            }

            // Проверка каждые 12 часов
            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
        }

        _logger.LogInformation("Reminder Background Service is stopping.");
    }

    private async Task CheckDueRemindersAsync(CancellationToken stoppingToken)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var push = scope.ServiceProvider.GetRequiredService<IPushService>();

        var now = DateTime.UtcNow;
        var dueReminders = await db.Reminders
            .Include(r => r.UserPlant)
            .Where(r => r.Enabled && r.NextDueAt <= now)
            .ToListAsync(stoppingToken);

        if (dueReminders.Count == 0) return;

        // Уже уведомляли именно про этот NextDueAt — не шлём повторно. Как только
        // NextDueAt сдвинется вперёд (после /done или ручного обновления даты),
        // NotifiedAt < NextDueAt снова станет true, и напоминание сможет уведомить заново.
        var toNotify = dueReminders.Where(r => r.NotifiedAt is null || r.NotifiedAt < r.NextDueAt).ToList();

        _logger.LogInformation(
            "Found {Count} due reminders for care operations ({NewCount} not yet notified).",
            dueReminders.Count, toNotify.Count);

        foreach (var r in toNotify)
        {
            var plantName = r.UserPlant?.Nickname ?? "растение";
            var (title, body) = r.Type switch
            {
                ReminderType.Watering => ("Пора полить", $"«{plantName}» пора полить."),
                ReminderType.Repotting => ("Пора пересадить", $"«{plantName}» пора пересадить."),
                ReminderType.Fertilizing => ("Пора подкормить", $"«{plantName}» пора подкормить."),
                _ => ("Напоминание PlantCare", $"Требуется уход за «{plantName}».")
            };

            _logger.LogInformation("Reminder: Plant ID {PlantId} ('{Nickname}') requires {Type} (due: {DueAt})",
                r.UserPlantId, plantName, r.Type, r.NextDueAt);

            if (r.UserPlant is not null)
            {
                await push.NotifyOwnerAsync(r.UserPlant.OwnerId, title, body, stoppingToken);
            }

            r.NotifiedAt = now;
        }

        await db.SaveChangesAsync(stoppingToken);
    }
}
