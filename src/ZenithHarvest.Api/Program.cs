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

// DBCONTEXT + ORACLE
builder.Services.AddDbContext<ZenithContext>(opt =>
    opt.UseOracle(builder.Configuration.GetConnectionString("Oracle")));

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

var app = builder.Build();

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
