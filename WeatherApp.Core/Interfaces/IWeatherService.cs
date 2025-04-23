using WeatherApp.Core.Models;

namespace WeatherApp.Core.Interfaces
{
    public interface IWeatherService
    {
        Task<WeatherData?> GetWeatherByCityAsync(string cityName);
        
        Task<WeatherData?> GetWeatherByZipAsync(string zipCode);
        
        Task<IEnumerable<WeatherData>> GetHistoricalWeatherAsync(string cityName, int limit = 10);
        
        Task RefreshAllWeatherDataAsync();
    }
}
