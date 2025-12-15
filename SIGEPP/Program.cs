using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Infrastructure services (DbContext, Repositories)
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

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
}


app.UseAuthorization();

app.MapControllers();

app.Run();
