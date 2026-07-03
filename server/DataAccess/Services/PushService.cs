using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PlantCare.Api.Data;
using PlantCare.Api.Services.Interfaces;
using WebPush;
using CoreModels = PlantCare.Api.Models;

namespace PlantCare.Api.Services.Implementations;

public class PushService : IPushService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<PushService> _log;
    private readonly WebPushClient _client = new();

    public PushService(AppDbContext db, IConfiguration config, ILogger<PushService> log)
    {
        _db = db;
        _config = config;
        _log = log;
    }

    public string GetPublicKey() => _config["Vapid:PublicKey"] ?? "";

    public async Task SubscribeAsync(string ownerId, string endpoint, string p256dh, string auth)
    {
        var existing = await _db.PushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == endpoint);
        if (existing is not null)
        {
            existing.OwnerId = ownerId;
            existing.P256dh = p256dh;
            existing.Auth = auth;
        }
        else
        {
            _db.PushSubscriptions.Add(new CoreModels.PushSubscription
            {
                OwnerId = ownerId,
                Endpoint = endpoint,
                P256dh = p256dh,
                Auth = auth
            });
        }

        await _db.SaveChangesAsync();
    }

    public async Task UnsubscribeAsync(string ownerId, string endpoint)
    {
        var subs = await _db.PushSubscriptions
            .Where(s => s.OwnerId == ownerId && s.Endpoint == endpoint)
            .ToListAsync();

        _db.PushSubscriptions.RemoveRange(subs);
        await _db.SaveChangesAsync();
    }

    public async Task NotifyOwnerAsync(string ownerId, string title, string body, CancellationToken ct = default)
    {
        var subject = _config["Vapid:Subject"] ?? "mailto:plantcare@example.com";
        var publicKey = _config["Vapid:PublicKey"];
        var privateKey = _config["Vapid:PrivateKey"];

        if (string.IsNullOrWhiteSpace(publicKey) || string.IsNullOrWhiteSpace(privateKey))
        {
            _log.LogWarning("VAPID-ключи не настроены — push не отправлен.");
            return;
        }

        var vapidDetails = new VapidDetails(subject, publicKey, privateKey);
        var subscriptions = await _db.PushSubscriptions.Where(s => s.OwnerId == ownerId).ToListAsync(ct);
        if (subscriptions.Count == 0) return;

        var payload = JsonSerializer.Serialize(new { title, body });
        var deadEndpoints = new List<string>();

        foreach (var sub in subscriptions)
        {
            var pushSubscription = new PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);
            try
            {
                await _client.SendNotificationAsync(pushSubscription, payload, vapidDetails, ct);
            }
            catch (WebPushException ex) when (ex.StatusCode is HttpStatusCode.Gone or HttpStatusCode.NotFound)
            {
                deadEndpoints.Add(sub.Endpoint);
            }
            catch (WebPushException ex)
            {
                _log.LogWarning(ex, "Не удалось отправить push на {Endpoint}", sub.Endpoint);
            }
        }

        if (deadEndpoints.Count > 0)
        {
            var dead = await _db.PushSubscriptions.Where(s => deadEndpoints.Contains(s.Endpoint)).ToListAsync(ct);
            _db.PushSubscriptions.RemoveRange(dead);
            await _db.SaveChangesAsync(ct);
        }
    }
}
