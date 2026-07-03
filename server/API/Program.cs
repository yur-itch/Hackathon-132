using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Data;
using PlantCare.Api.Services.Implementations;
using PlantCare.Api.Services.Interfaces;
using PlantCare.Api.Services.PlantNet;
using PlantCare.Api.Services.Recognition;
using PlantCare.Api.Services.Interfaces;
using PlantCare.Api.Services.Implementations;
using PlantCare.Api.Services.Background;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// БД: PostgreSQL
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddControllers().AddJsonOptions(o =>
{
    // enum'ы отдаём строками ("Watering"), а не числами — удобнее для фронта
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddOpenApi();
// Регистрация сервисов бизнес-логики Core / DataAccess
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPlantsService, PlantsService>();
builder.Services.AddScoped<IUserPlantsService, UserPlantsService>();
builder.Services.AddScoped<IReminderService, ReminderService>();
builder.Services.AddScoped<IExchangeService, ExchangeService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<IFavoritesService, FavoritesService>();
builder.Services.AddHostedService<ReminderBackgroundService>();

// Распознавание растений по фото (Pl@ntNet). Без ключа работает на фикстурах (мок).
builder.Services.Configure<PlantNetOptions>(
    builder.Configuration.GetSection(PlantNetOptions.SectionName));
builder.Services.AddHttpClient<IPlantNetClient, PlantNetClient>(c =>
    c.Timeout = TimeSpan.FromSeconds(10));
builder.Services.AddScoped<IRecognitionService, RecognitionService>();
builder.Services.AddScoped<IReminderService, ReminderService>();

// CORS: открыто для дев-фронта (Vite на 5173). На проде сузить.
const string DevCors = "dev";
builder.Services.AddCors(o => o.AddPolicy(DevCors, p =>
    p.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
     .AllowAnyHeader()
     .AllowAnyMethod()));

var app = builder.Build();

// Авто-миграция + сид справочника при старте (удобно для хакатона).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    SeedData.EnsureSeeded(db);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                 // JSON контракт: /openapi/v1.json
    app.MapScalarApiReference();      // интерактивные доки: /scalar/v1
}

app.UseCors(DevCors);
app.MapControllers();

app.Run();
