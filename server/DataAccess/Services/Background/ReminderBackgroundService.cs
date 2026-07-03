using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Data;

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
        using (var scope = _services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var now = DateTime.UtcNow;
            var dueReminders = await db.Reminders
                .Include(r => r.UserPlant)
                .Where(r => r.Enabled && r.NextDueAt <= now)
                .ToListAsync(stoppingToken);

            if (dueReminders.Any())
            {
                _logger.LogInformation("Found {Count} due reminders for care operations.", dueReminders.Count);
                foreach (var r in dueReminders)
                {
                    _logger.LogInformation("Reminder: Plant ID {PlantId} ('{Nickname}') requires {Type} (due: {DueAt})",
                        r.UserPlantId, r.UserPlant?.Nickname ?? "Unknown", r.Type, r.NextDueAt);
                }
            }
        }
    }
}
