using Hangfire;
using Hangfire.Logging;
using Hangfire.MySql;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WeatherApp.Core.Interfaces;
using WeatherApp.Core.Services;
using WeatherApp.Infrastructure.Data;
using WeatherApp.Infrastructure.Repositories;
using WeatherApp.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Setup Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(Path.Combine("logs", "log-.txt"), rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<WeatherDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddHttpClient();

//Register Services
builder.Services.AddScoped<IWeatherRepository, WeatherRepository>();
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<IExternalWeatherService, OpenWeatherMapService>();

// Configure Hangfire
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseStorage(new MySqlStorage(connectionString, new MySqlStorageOptions
    {
        TablesPrefix = "Hangfire_"
    })));

builder.Services.AddHangfireServer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Configure Hangfire dashboard
app.UseHangfireDashboard();

// Schedule recurring jobs
RecurringJob.AddOrUpdate<IWeatherService>(
    "refresh-weather-data",
    service => service.RefreshAllWeatherDataAsync(),
    Cron.Hourly);

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
