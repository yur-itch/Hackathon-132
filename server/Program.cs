using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// БД: SQLite для нулевого старта. Для перехода на PostgreSQL —
// замените UseSqlite на UseNpgsql и строку подключения в appsettings.json.
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=plantcare.db"));

builder.Services.AddControllers().AddJsonOptions(o =>
{
    // enum'ы отдаём строками ("Watering"), а не числами — удобнее для фронта
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddOpenApi();

// CORS: открыто для дев-фронта (Vite на 5173). На проде сузить.
const string DevCors = "dev";
builder.Services.AddCors(o => o.AddPolicy(DevCors, p =>
    p.WithOrigins("http://localhost:5173")
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
