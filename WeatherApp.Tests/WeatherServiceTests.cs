using Microsoft.Extensions.Logging;
using Moq;
using WeatherApp.Core.Interfaces;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;
using Xunit;

namespace WeatherApp.Tests
{
    public class WeatherServiceTests
    {
        private readonly Mock<IWeatherRepository> _mockRepository;
        private readonly Mock<IExternalWeatherService> _mockExternalService;
        private readonly Mock<ILogger<WeatherService>> _mockLogger;
        private readonly WeatherService _weatherService;

        public WeatherServiceTests()
        {
            _mockRepository = new Mock<IWeatherRepository>();
            _mockExternalService = new Mock<IExternalWeatherService>();
            _mockLogger = new Mock<ILogger<WeatherService>>();
            _weatherService = new WeatherService(_mockRepository.Object, _mockExternalService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetWeatherByCityAsync_WithFreshData_ReturnsFromRepository()
        {
            // Arrange
            var cityName = "London";
            var weatherData = new WeatherData
            {
                CityName = cityName,
                Temperature = 20,
                RetrievedAt = DateTime.UtcNow.AddMinutes(-10) // Fresh data (less than 30 minutes old)
            };

            _mockRepository.Setup(r => r.GetByCityNameAsync(cityName))
                .ReturnsAsync(weatherData);

            // Act
            var result = await _weatherService.GetWeatherByCityAsync(cityName);

            // Assert
            Assert.Equal(weatherData, result);
            _mockExternalService.Verify(s => s.GetWeatherByCityAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetWeatherByCityAsync_WithStaleData_FetchesFromExternalService()
        {
            // Arrange
            var cityName = "London";
            var staleData = new WeatherData
            {
                CityName = cityName,
                Temperature = 20,
                RetrievedAt = DateTime.UtcNow.AddMinutes(-40) // Stale data (more than 30 minutes old)
            };

            var freshData = new WeatherData
            {
                CityName = cityName,
                Temperature = 22,
                RetrievedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.GetByCityNameAsync(cityName))
                .ReturnsAsync(staleData);
            _mockExternalService.Setup(s => s.GetWeatherByCityAsync(cityName))
                .ReturnsAsync(freshData);

            // Act
            var result = await _weatherService.GetWeatherByCityAsync(cityName);

            // Assert
            Assert.Equal(freshData, result);
            _mockExternalService.Verify(s => s.GetWeatherByCityAsync(cityName), Times.Once);
            _mockRepository.Verify(r => r.UpdateAsync(freshData), Times.Once);
        }

        [Fact]
        public async Task GetWeatherByCityAsync_WithNoData_FetchesFromExternalService()
        {
            // Arrange
            var cityName = "London";
            WeatherData? nullData = null;
            var freshData = new WeatherData
            {
                CityName = cityName,
                Temperature = 22,
                RetrievedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.GetByCityNameAsync(cityName))
                .ReturnsAsync(nullData);
            _mockExternalService.Setup(s => s.GetWeatherByCityAsync(cityName))
                .ReturnsAsync(freshData);

            // Act
            var result = await _weatherService.GetWeatherByCityAsync(cityName);

            // Assert
            Assert.Equal(freshData, result);
            _mockExternalService.Verify(s => s.GetWeatherByCityAsync(cityName), Times.Once);
            _mockRepository.Verify(r => r.AddAsync(freshData), Times.Once);
        }

        [Fact]
        public async Task GetWeatherByCityAsync_WithEmptyCity_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _weatherService.GetWeatherByCityAsync(string.Empty));
        }

        [Fact]
        public async Task GetWeatherByZipAsync_WithFreshData_ReturnsFromRepository()
        {
            // Arrange
            var zipCode = "10001";
            var weatherData = new WeatherData
            {
                ZipCode = zipCode,
                Temperature = 20,
                RetrievedAt = DateTime.UtcNow.AddMinutes(-10) // Fresh data (less than 30 minutes old)
            };

            _mockRepository.Setup(r => r.GetByZipCodeAsync(zipCode))
                .ReturnsAsync(weatherData);

            // Act
            var result = await _weatherService.GetWeatherByZipAsync(zipCode);

            // Assert
            Assert.Equal(weatherData, result);
            _mockExternalService.Verify(s => s.GetWeatherByZipAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetHistoricalWeatherAsync_ReturnsDataFromRepository()
        {
            // Arrange
            var cityName = "London";
            var historicalData = new List<WeatherData>
            {
                new WeatherData { CityName = cityName, RetrievedAt = DateTime.UtcNow.AddDays(-1) },
                new WeatherData { CityName = cityName, RetrievedAt = DateTime.UtcNow.AddDays(-2) }
            };

            _mockRepository.Setup(r => r.GetHistoricalDataAsync(cityName, 10))
                .ReturnsAsync(historicalData);

            // Act
            var result = await _weatherService.GetHistoricalWeatherAsync(cityName);

            // Assert
            Assert.Equal(historicalData, result);
        }

        [Fact]
        public async Task RefreshAllWeatherDataAsync_RefreshesAllCitiesAndZipCodes()
        {
            // Arrange
            var cities = new List<string> { "London", "Paris" };
            var zipCodes = new List<string> { "10001", "90210" };

            _mockRepository.Setup(r => r.GetAllCityNamesAsync())
                .ReturnsAsync(cities);
            _mockRepository.Setup(r => r.GetAllZipCodesAsync())
                .ReturnsAsync(zipCodes);

            foreach (var city in cities)
            {
                _mockExternalService.Setup(s => s.GetWeatherByCityAsync(city))
                    .ReturnsAsync(new WeatherData { CityName = city });
            }

            foreach (var zipCode in zipCodes)
            {
                _mockExternalService.Setup(s => s.GetWeatherByZipAsync(zipCode))
                    .ReturnsAsync(new WeatherData { ZipCode = zipCode });
            }

            // Act
            await _weatherService.RefreshAllWeatherDataAsync();

            // Assert
            foreach (var city in cities)
            {
                _mockExternalService.Verify(s => s.GetWeatherByCityAsync(city), Times.Once);
            }

            foreach (var zipCode in zipCodes)
            {
                _mockExternalService.Verify(s => s.GetWeatherByZipAsync(zipCode), Times.Once);
            }

            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<WeatherData>()), Times.Exactly(cities.Count + zipCodes.Count));
        }
    }
}
