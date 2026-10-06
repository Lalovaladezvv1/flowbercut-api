using System.Text;

using Flowbercut.Api.Data;
using Flowbercut.Api.Middleware;
using Flowbercut.Api.Options;
using Flowbercut.Api.Repositories;
using Flowbercut.Api.Security;
using Flowbercut.Api.Services;
using Flowbercut.Api.Services.Interfaces;
using Flowbercut.Api.Repositories;
using Flowbercut.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<FlowbercutDbContext>(
    options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("Flowbercut")));

// --------------------------------------------------
// JWT
// --------------------------------------------------

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(
        JwtOptions.SectionName));

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        "No se configuró Jwt.");

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtOptions.SecretKey)),
                ClockSkew = TimeSpan.FromSeconds(30)
            };
    });

builder.Services.AddAuthorization();

// --------------------------------------------------
// Authentication
// --------------------------------------------------

builder.Services.AddScoped<
    IAuthRepository,
    AuthRepository>();

builder.Services.AddScoped<
    IAuthService,
    AuthService>();

builder.Services.AddSingleton<
    IPasswordService,
    PasswordService>();

builder.Services.AddSingleton<
    IJwtService,
    JwtService>();

builder.Services.AddScoped<
    IPlatformTenantRepository,
    PlatformTenantRepository>();

builder.Services.AddScoped<
    IPlatformTenantService,
    PlatformTenantService>();

// --------------------------------------------------
// Payload Encryption
// --------------------------------------------------

builder.Services.Configure<PayloadEncryptionOptions>(
    builder.Configuration.GetSection(
        PayloadEncryptionOptions.SectionName));

var encryptionOptions = builder.Configuration
    .GetSection(PayloadEncryptionOptions.SectionName)
    .Get<PayloadEncryptionOptions>()
    ?? throw new InvalidOperationException(
        "No se configuró PayloadEncryption.");

builder.Services.AddSingleton<ISecuritySessionService>(
    new SecuritySessionService(
        TimeSpan.FromMinutes(
            encryptionOptions.SessionLifetimeMinutes),
        TimeSpan.FromSeconds(
            encryptionOptions.AllowedClockSkewSeconds)));

builder.Services.AddSingleton<
    IPayloadEncryptionService,
    AesGcmPayloadEncryptionService>();

// --------------------------------------------------
// Application
// --------------------------------------------------

builder.Services.AddCors(options =>
{
    options.AddPolicy("FlowbercutWeb", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("FlowbercutWeb", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "https://flowbercut.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("FlowbercutWeb");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<PayloadEncryptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
