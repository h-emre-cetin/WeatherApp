using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.Core.Interfaces;
using WeatherApp.Core.Models;

namespace WeatherApp.Infrastructure.Services
{
    public class OpenWeatherMapService : IExternalWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<OpenWeatherMapService> _logger;

        public OpenWeatherMapService(HttpClient httpClient, IConfiguration configuration, ILogger<OpenWeatherMapService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiKey = configuration["OpenWeatherMap:ApiKey"] ?? throw new ArgumentException("OpenWeatherMap API key is not configured");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<WeatherData?> GetWeatherByCityAsync(string cityName)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://api.openweathermap.org/data/2.5/weather?q={cityName}&appid={_apiKey}&units=metric");

                response.EnsureSuccessStatusCode();

                var weatherResponse = await response.Content.ReadFromJsonAsync<OpenWeatherMapResponse>();
                return MapToWeatherData(weatherResponse, cityName, null);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error fetching weather data for city: {CityName}", cityName);
                throw new ApplicationException($"Unable to retrieve weather data for {cityName}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching weather data for city: {CityName}", cityName);
                throw;
            }
        }

        public async Task<WeatherData?> GetWeatherByZipAsync(string zipCode)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://api.openweathermap.org/data/2.5/weather?zip={zipCode}&appid={_apiKey}&units=metric");

                response.EnsureSuccessStatusCode();

                var weatherResponse = await response.Content.ReadFromJsonAsync<OpenWeatherMapResponse>();
                return MapToWeatherData(weatherResponse, null, zipCode);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error fetching weather data for zip code: {ZipCode}", zipCode);
                throw new ApplicationException($"Unable to retrieve weather data for zip code {zipCode}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching weather data for zip code: {ZipCode}", zipCode);
                throw;
            }
        }

        private WeatherData? MapToWeatherData(OpenWeatherMapResponse? response, string? cityName, string? zipCode)
        {
            if (response == null)
                return null;

            return new WeatherData
            {
                CityName = cityName ?? response.Name,
                ZipCode = zipCode,
                Temperature = response.Main.Temp,
                FeelsLike = response.Main.FeelsLike,
                MinTemperature = response.Main.TempMin,
                MaxTemperature = response.Main.TempMax,
                Humidity = response.Main.Humidity,
                Description = response.Weather.Length > 0 ? response.Weather[0].Description : string.Empty,
                Icon = response.Weather.Length > 0 ? response.Weather[0].Icon : string.Empty,
                WindSpeed = response.Wind.Speed,
                LastUpdated = DateTimeOffset.FromUnixTimeSeconds(response.Dt).DateTime,
                RetrievedAt = DateTime.UtcNow
            };
        }

        // Classes to deserialize OpenWeatherMap API response
        private class OpenWeatherMapResponse
        {
            public WeatherInfo[] Weather { get; set; } = [];
            public MainInfo Main { get; set; } = new();
            public WindInfo Wind { get; set; } = new();
            public string Name { get; set; } = string.Empty;
            public long Dt { get; set; }
        }

        private class WeatherInfo
        {
            public string Main { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Icon { get; set; } = string.Empty;
        }

        private class MainInfo
        {
            public double Temp { get; set; }
            public double FeelsLike { get; set; }
            public double TempMin { get; set; }
            public double TempMax { get; set; }
            public int Humidity { get; set; }
        }

        private class WindInfo
        {
            public double Speed { get; set; }
        }
    }
}
