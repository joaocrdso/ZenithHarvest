using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using ZenithHarvest.Api.Middleware;
using ZenithHarvest.Application.Security;
using ZenithHarvest.Application.UseCases;
using ZenithHarvest.Domain.Interfaces;
using ZenithHarvest.Infrastructure.Persistence;
using ZenithHarvest.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// --- SERVICES ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// OPENAPI
builder.Services.AddOpenApi();

// REPOSITORIES (Dependency Injection — DIP)
builder.Services.AddScoped<IInsurerRepository, InsurerRepository>();
builder.Services.AddScoped<IPolicyRepository, PolicyRepository>();
builder.Services.AddScoped<IClaimRepository, ClaimRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// HANDLERS
builder.Services.AddScoped<GetPoliciesByInsurerHandler>();
builder.Services.AddScoped<CreateClaimHandler>();
builder.Services.AddScoped<AuthenticationHandler>();

// JWT SERVICE
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? Environment.GetEnvironmentVariable("Jwt__Secret") 
    ?? throw new InvalidOperationException("JWT Secret não configurado");
builder.Services.AddScoped<IJwtService>(sp => new JwtService(jwtSecret));

// AUTHENTICATION
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

// HEALTH CHECKS
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ZenithContext>("oracle-db");

// LOGGING
builder.Services.AddLogging();

// DEBUG CONNECTION STRING

builder.Services.AddDbContext<ZenithContext>(opt =>
    opt.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// --- SEED ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ZenithHarvest.Infrastructure.Persistence.ZenithContext>();
    db.Database.Migrate();

    if (!db.Insurers.Any())
    {
        var insurer = new ZenithHarvest.Domain.Entities.Insurer
        {
            CNPJ = "12.345.678/0001-99",
            Nome = "Zenith Seguros",
            CodigoSUSEP = "123456"
        };
        db.Insurers.Add(insurer);
        db.SaveChanges();

        db.Users.Add(new ZenithHarvest.Domain.Entities.User
        {
            InsurerId = insurer.Id,
            Email = "admin@zenith.com",
            PasswordHash = ZenithHarvest.Application.Security.PasswordHasher.Hash("123456"),
            Role = "Admin",
            Ativo = true
        });
        db.SaveChanges();
    }
}

// --- MIDDLEWARE ---
app.UseMiddleware<ExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
