using WeatherApp.Core.Models;

namespace WeatherApp.Core.Interfaces
{
    public interface IExternalWeatherService
    {
        Task<WeatherData?> GetWeatherByCityAsync(string cityName);
        
        Task<WeatherData?> GetWeatherByZipAsync(string zipCode);
    }
}
