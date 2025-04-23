using Microsoft.Extensions.Logging;
using WeatherApp.Core.Interfaces;
using WeatherApp.Core.Models;

namespace WeatherApp.Core.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly IWeatherRepository _weatherRepository;
        private readonly IExternalWeatherService _externalWeatherService;
        private readonly ILogger<WeatherService> _logger;

        public WeatherService(
            IWeatherRepository weatherRepository,
            IExternalWeatherService externalWeatherService,
            ILogger<WeatherService> logger)
        {
            _weatherRepository = weatherRepository ?? throw new ArgumentNullException(nameof(weatherRepository));
            _externalWeatherService = externalWeatherService ?? throw new ArgumentNullException(nameof(externalWeatherService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<WeatherData?> GetWeatherByCityAsync(string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
                throw new ArgumentException("City name cannot be empty", nameof(cityName));

            try
            {
                // Try to get from database first
                var weatherData = await _weatherRepository.GetByCityNameAsync(cityName);

                // If data is older than 30 minutes, refresh it
                if (weatherData == null || (DateTime.UtcNow - weatherData.RetrievedAt).TotalMinutes > 30)
                {
                    _logger.LogInformation("Fetching fresh weather data for city: {CityName}", cityName);
                    weatherData = await _externalWeatherService.GetWeatherByCityAsync(cityName);

                    if (weatherData != null)
                    {
                        var existingData = await _weatherRepository.GetByCityNameAsync(cityName);
                        if (existingData != null)
                            await _weatherRepository.UpdateAsync(weatherData);
                        else
                            await _weatherRepository.AddAsync(weatherData);
                    }
                }

                return weatherData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting weather data for city: {CityName}", cityName);
                throw;
            }
        }

        public async Task<WeatherData?> GetWeatherByZipAsync(string zipCode)
        {
            if (string.IsNullOrWhiteSpace(zipCode))
                throw new ArgumentException("Zip code cannot be empty", nameof(zipCode));

            try
            {
                // Try to get from database first
                var weatherData = await _weatherRepository.GetByZipCodeAsync(zipCode);

                // If data is older than 30 minutes, refresh it
                if (weatherData == null || (DateTime.UtcNow - weatherData.RetrievedAt).TotalMinutes > 30)
                {
                    _logger.LogInformation("Fetching fresh weather data for zip code: {ZipCode}", zipCode);
                    weatherData = await _externalWeatherService.GetWeatherByZipAsync(zipCode);

                    if (weatherData != null)
                    {
                        var existingData = await _weatherRepository.GetByZipCodeAsync(zipCode);
                        if (existingData != null)
                            await _weatherRepository.UpdateAsync(weatherData);
                        else
                            await _weatherRepository.AddAsync(weatherData);
                    }
                }

                return weatherData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting weather data for zip code: {ZipCode}", zipCode);
                throw;
            }
        }

        public async Task<IEnumerable<WeatherData>> GetHistoricalWeatherAsync(string cityName, int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(cityName))
                throw new ArgumentException("City name cannot be empty", nameof(cityName));

            try
            {
                return await _weatherRepository.GetHistoricalDataAsync(cityName, limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting historical weather data for city: {CityName}", cityName);
                throw;
            }
        }

        public async Task RefreshAllWeatherDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting scheduled weather data refresh");

                // Get all cities and zip codes that we have in the database
                var cities = await _weatherRepository.GetAllCityNamesAsync();
                var zipCodes = await _weatherRepository.GetAllZipCodesAsync();

                // Refresh data for all cities
                foreach (var city in cities)
                {
                    try
                    {
                        var weatherData = await _externalWeatherService.GetWeatherByCityAsync(city);
                        if (weatherData != null)
                        {
                            await _weatherRepository.UpdateAsync(weatherData);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error refreshing weather data for city: {CityName}", city);
                        // Continue with other cities even if one fails
                    }
                }

                // Refresh data for all zip codes
                foreach (var zipCode in zipCodes)
                {
                    try
                    {
                        var weatherData = await _externalWeatherService.GetWeatherByZipAsync(zipCode);
                        if (weatherData != null)
                        {
                            await _weatherRepository.UpdateAsync(weatherData);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error refreshing weather data for zip code: {ZipCode}", zipCode);
                        // Continue with other zip codes even if one fails
                    }
                }

                _logger.LogInformation("Completed scheduled weather data refresh");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during scheduled weather data refresh");
                throw;
            }
        }
    }
}
