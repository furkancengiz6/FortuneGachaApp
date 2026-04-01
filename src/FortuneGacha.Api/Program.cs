using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FortuneGacha.Api.Data;
using FortuneGacha.Api.Models;
using FortuneGacha.Api.Hubs;
using FortuneGacha.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IImageStorageService, LocalImageStorageService>();
builder.Services.AddScoped<IGachaService, GachaServiceV2>();
builder.Services.AddHttpClient<INotificationService, ExpoNotificationService>();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:19006", "http://10.0.2.2:8081", "http://localhost:8081")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Database
builder.Services.AddDbContext<GachaDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? "SUPER_SECRET_KEY_FOR_TESTING_PURPOSES_ONLY");

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };

    // SignalR: URL Query string'den token okuma (WebSockets header desteklemez)
    x.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Enable serving static files from wwwroot
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<GachaDbContext>();
    context.Database.EnsureCreated();

    if (!context.Achievements.Any())
    {
        context.Achievements.AddRange(
            new Achievement { Name = "İlk Kehanet", Description = "İlk falını çektin!", GpReward = 20, IconKey = "first_draw" },
            new Achievement { Name = "Şanslı Yıldız", Description = "Bir Legendary kart buldun!", GpReward = 100, IconKey = "legendary" },
            new Achievement { Name = "Sosyal Kelebek", Description = "5 arkadaş edindin!", GpReward = 50, IconKey = "social" },
            new Achievement { Name = "Koleksiyoner", Description = "10 farklı fal biriktirdin!", GpReward = 40, IconKey = "collector" }
        );
        context.SaveChanges();
    }

    if (!context.Decorations.Any())
    {
        context.Decorations.AddRange(
            new Decoration { Name = "Mistik Mor", Type = "AvatarFrame", Price = 50, ImageUrl = "frame_purple", Rarity = "Common" },
            new Decoration { Name = "Altın Şafak", Type = "AvatarFrame", Price = 250, ImageUrl = "frame_gold", Rarity = "Rare" },
            new Decoration { Name = "Efsanevi Ejder", Type = "AvatarFrame", Price = 1000, ImageUrl = "frame_dragon", Rarity = "Legendary" }
        );
        context.SaveChanges();
    }
}

app.Run();
