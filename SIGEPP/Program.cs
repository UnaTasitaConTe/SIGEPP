using System.Text;
using Application.Academics;
using Application.Auth;
using Application.Users;
using Infrastructure;
using Infrastructure.Persistence;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SIGEPP.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Infrastructure services (DbContext, Repositories, Security)
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

// Application services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserAppService>();

// Application services - Módulo académico
builder.Services.AddScoped<AcademicPeriodsAppService>();
builder.Services.AddScoped<SubjectsAppService>();
builder.Services.AddScoped<TeacherAssignmentsAppService>();

// Application services - Módulo PPA
builder.Services.AddScoped<Application.Ppa.PpaAppService>();
builder.Services.AddScoped<Application.Ppa.PpaAttachmentsAppService>();

// Configurar autenticación JWT
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Configuración de JWT no encontrada en appsettings.json");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // En producción cambiar a true
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
        ClockSkew = TimeSpan.Zero // Eliminar tolerancia de tiempo (por defecto son 5 minutos)
    };
});

// Configurar autorización basada en permisos
builder.Services.AddSigeppAuthorizationPolicies();

// Configurar CORS para permitir peticiones desde el frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173") // Ajustar según el puerto del frontend
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Run database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Running database migrations...");
        dbContext.Database.Migrate();
        logger.LogInformation("Database migrations completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database");
        throw;
    }
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// CORS debe ir antes de autenticación
app.UseCors();

// Middleware de autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
