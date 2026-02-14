using System.Text;
using GunnyApi.Infrastructure.Context;
using GunnyApi.Infrastructure.Database;
using GunnyApi.Infrastructure.Http;
using GunnyApi.Infrastructure.Middleware;
using GunnyApi.Infrastructure.Security;
using GunnyApi.Infrastructure.Settings;
using GunnyApi.Repositories;
using GunnyApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Load appsettings.Local.json if exists (for local secrets)
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Kestrel Configuration from appsettings.json
var kestrelSettings = builder.Configuration.GetSection("KestrelSettings").Get<KestrelSettings>();
builder.Services.Configure<KestrelSettings>(builder.Configuration.GetSection("KestrelSettings"));

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    if (kestrelSettings!.ListenOnAllInterfaces)
    {
        // Listen on all network interfaces (0.0.0.0)
        serverOptions.ListenAnyIP(kestrelSettings.HttpPort); // HTTP
        
        if (kestrelSettings.EnableHttps)
        {
            serverOptions.ListenAnyIP(kestrelSettings.HttpsPort, listenOptions =>
            {
                listenOptions.UseHttps(); // HTTPS
            });
        }
    }
    else
    {
        // Listen only on localhost (127.0.0.1)
        serverOptions.ListenLocalhost(kestrelSettings.HttpPort); // HTTP
        
        if (kestrelSettings.EnableHttps)
        {
            serverOptions.ListenLocalhost(kestrelSettings.HttpsPort, listenOptions =>
            {
                listenOptions.UseHttps(); // HTTPS
            });
        }
    }
});

// Add services to the container.

// Database Connection Factory
builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();

// JWT Configuration
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Game Settings Configuration
builder.Services.Configure<GameSettings>(builder.Configuration.GetSection("GameSettings"));

// CORS Settings Configuration
builder.Services.Configure<CorsSettings>(builder.Configuration.GetSection("CorsSettings"));

// PaymentTiers Settings Configuration
builder.Services.Configure<PaymentTiersSettings>(builder.Configuration.GetSection("PaymentTiersSettings"));

// Localization Settings Configuration
builder.Services.Configure<LocalizationSettings>(builder.Configuration.GetSection("LocalizationSettings"));
builder.Services.AddScoped<GunnyApi.Infrastructure.Services.ILocalizationService, GunnyApi.Infrastructure.Services.LocalizationService>();

// Email Settings Configuration
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<GunnyApi.Infrastructure.Services.IEmailService, GunnyApi.Infrastructure.Services.EmailService>();

// HttpClient Factory và Service
builder.Services.AddHttpClient();
builder.Services.AddScoped<IHttpClientService, HttpClientService>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
var key = Encoding.UTF8.GetBytes(jwtSettings!.SecretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Register Repositories and Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IServerRepository, ServerRepository>();
builder.Services.AddScoped<IServerService, ServerService>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IChargeMoneyService, ChargeMoneyService>();

// Register UserContext
builder.Services.AddScoped<GunnyApi.Infrastructure.Context.IUserContext, GunnyApi.Infrastructure.Context.UserContext>();

builder.Services.AddControllers();

// CORS Configuration from appsettings.json
var corsSettings = builder.Configuration.GetSection("CorsSettings").Get<CorsSettings>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsSettings!.PolicyName,
        policyBuilder =>
        {
            policyBuilder
                .WithOrigins(corsSettings.AllowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
            
            if (corsSettings.AllowCredentials)
            {
                policyBuilder.AllowCredentials();
            }
        });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(corsSettings!.PolicyName); // <--- Đặt trước UseAuthentication

// Language Detection Middleware - đặt trước UseAuthentication
app.UseLanguageDetection();

app.UseAuthentication();

// User Context Middleware - phải đặt sau UseAuthentication
app.UseUserContext();

app.UseAuthorization();

app.MapControllers();

app.Run();
