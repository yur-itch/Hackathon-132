using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PlantCare.Api.Data;
using PlantCare.Api.Services.Background;
using PlantCare.Api.Services.Implementations;
using PlantCare.Api.Services.Interfaces;
using PlantCare.Api.Services.PlantNet;
using PlantCare.Api.Services.Recognition;
using Scalar.AspNetCore;
using PlantCare.Api.Hubs;


AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Jwt:Secret не должен жить в appsettings.json (утечёт в git). Если не задан через
// user-secrets/env — генерируем случайный на время жизни процесса: авторизация продолжит
// работать в рамках запуска, но токены, выданные до рестарта, после него станут невалидны.
// Для стабильных сессий между рестартами задайте секрет явно (см. README).
if (string.IsNullOrWhiteSpace(builder.Configuration["Jwt:Secret"]))
{
    builder.Configuration["Jwt:Secret"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
}

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddOpenApi();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPlantsService, PlantsService>();
builder.Services.AddScoped<IUserPlantsService, UserPlantsService>();
builder.Services.AddScoped<IReminderService, ReminderService>();
builder.Services.AddScoped<IExchangeService, ExchangeService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<IFavoritesService, FavoritesService>();
builder.Services.AddScoped<IPushService, PushService>();
builder.Services.AddHostedService<ReminderBackgroundService>();

builder.Services.AddSignalR();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secret = builder.Configuration["Jwt:Secret"]!; // гарантирован выше (задан или сгенерирован)
        var issuer = builder.Configuration["Jwt:Issuer"] ?? "PlantCareApi";
        var audience = builder.Configuration["Jwt:Audience"] ?? "PlantCareClient";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue("access_token", out var token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.Configure<PlantNetOptions>(
    builder.Configuration.GetSection(PlantNetOptions.SectionName));
builder.Services.AddHttpClient<IPlantNetClient, PlantNetClient>(c =>
    c.Timeout = TimeSpan.FromSeconds(10));
builder.Services.AddScoped<IRecognitionService, RecognitionService>();

const string DevCors = "dev";
builder.Services.AddCors(o => o.AddPolicy(DevCors, p =>
    p.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var wasCreated = db.Database.EnsureCreated();
    if (!wasCreated)
    {
        // EnsureCreated() ничего не делает, если база уже существует — новые таблицы/колонки,
        // добавленные в модели после первого запуска (например, PushSubscriptions,
        // Reminder.NotifiedAt), НЕ появятся сами. Если после git pull видите ошибки вида
        // "column ... does not exist" — снесите локальный volume БД (docker compose down -v)
        // и поднимите заново. Полноценная замена — EF-миграции (dotnet ef migrations add).
        app.Logger.LogWarning(
            "БД уже существовала при старте — EnsureCreated() не применяет новые изменения " +
            "схемы к существующей базе. Если видите ошибки 'column/relation does not exist' " +
            "после обновления кода — выполните: docker compose down -v && docker compose up -d db");
    }
    SeedData.EnsureSeeded(db);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors(DevCors);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
