using WeatherApp.Core.Models;

namespace WeatherApp.Core.Interfaces
{
    public interface IWeatherRepository
    {
        Task<WeatherData?> GetByCityNameAsync(string cityName);
        
        Task<WeatherData?> GetByZipCodeAsync(string zipCode);
        
        Task<IEnumerable<WeatherData>> GetHistoricalDataAsync(string cityName, int limit = 10);
        
        Task AddAsync(WeatherData weatherData);
        
        Task UpdateAsync(WeatherData weatherData);
        
        Task<IEnumerable<string>> GetAllCityNamesAsync();
        
        Task<IEnumerable<string>> GetAllZipCodesAsync();
    }
}
