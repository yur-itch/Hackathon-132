using System.Text.Json.Serialization;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PlantCare.Api.Data;
using PlantCare.Api.Services.Implementations;
using PlantCare.Api.Services.Interfaces;
using PlantCare.Api.Services.PlantNet;
using PlantCare.Api.Services.Recognition;
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
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secret = builder.Configuration["Jwt:Secret"] ?? "super_secret_key_plantcare_hackathon_2026_antigravity";
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

// Распознавание растений по фото (Pl@ntNet). Без ключа работает на фикстурах (мок).
builder.Services.Configure<PlantNetOptions>(
    builder.Configuration.GetSection(PlantNetOptions.SectionName));
builder.Services.AddHttpClient<IPlantNetClient, PlantNetClient>(c =>
    c.Timeout = TimeSpan.FromSeconds(10));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRecognitionService, RecognitionService>();
builder.Services.AddScoped<IReminderService, ReminderService>();

// CORS: открыто для дев-фронта (Vite на 5173). На проде сузить.
const string DevCors = "dev";
builder.Services.AddCors(o => o.AddPolicy(DevCors, p =>
    p.WithOrigins("http://localhost:5173")
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()));

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
