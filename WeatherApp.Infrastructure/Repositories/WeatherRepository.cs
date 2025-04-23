using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WeatherApp.Core.Interfaces;
using WeatherApp.Core.Models;
using WeatherApp.Infrastructure.Data;

namespace WeatherApp.Infrastructure.Repositories
{
    public class WeatherRepository : IWeatherRepository
    {
        private readonly WeatherDbContext _dbContext;
        private readonly ILogger<WeatherRepository> _logger;

        public WeatherRepository(WeatherDbContext dbContext, ILogger<WeatherRepository> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<WeatherData?> GetByCityNameAsync(string cityName)
        {
            try
            {
                return await _dbContext.WeatherData
                    .Where(w => w.CityName.ToLower() == cityName.ToLower())
                    .OrderByDescending(w => w.RetrievedAt)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving weather data for city: {CityName}", cityName);
                throw;
            }
        }

        public async Task<WeatherData?> GetByZipCodeAsync(string zipCode)
        {
            try
            {
                return await _dbContext.WeatherData
                    .Where(w => w.ZipCode == zipCode)
                    .OrderByDescending(w => w.RetrievedAt)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving weather data for zip code: {ZipCode}", zipCode);
                throw;
            }
        }

        public async Task<IEnumerable<WeatherData>> GetHistoricalDataAsync(string cityName, int limit = 10)
        {
            try
            {
                return await _dbContext.WeatherData
                    .Where(w => w.CityName.ToLower() == cityName.ToLower())
                    .OrderByDescending(w => w.RetrievedAt)
                    .Take(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving historical weather data for city: {CityName}", cityName);
                throw;
            }
        }

        public async Task AddAsync(WeatherData weatherData)
        {
            try
            {
                await _dbContext.WeatherData.AddAsync(weatherData);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding weather data");
                throw;
            }
        }

        public async Task UpdateAsync(WeatherData weatherData)
        {
            try
            {
                // We're adding a new record for historical tracking rather than updating existing
                weatherData.Id = 0; // Ensure we're inserting a new record
                weatherData.RetrievedAt = DateTime.UtcNow;
                await _dbContext.WeatherData.AddAsync(weatherData);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating weather data");
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetAllCityNamesAsync()
        {
            try
            {
                return await _dbContext.WeatherData
                    .Select(w => w.CityName)
                    .Distinct()
                    .Where(c => !string.IsNullOrEmpty(c))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all city names");
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetAllZipCodesAsync()
        {
            try
            {
                return await _dbContext.WeatherData
                    .Select(w => w.ZipCode)
                    .Distinct()
                    .Where(z => !string.IsNullOrEmpty(z))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all zip codes");
                throw;
            }
        }
    }
}
