using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.API.Controllers;
using WeatherApp.Core.Interfaces;
using WeatherApp.Core.Models;
using Xunit;

namespace WeatherApp.Tests
{
    public class WeatherControllerTests
    {
        private readonly Mock<IWeatherService> _mockWeatherService;
        private readonly Mock<ILogger<WeatherController>> _mockLogger;
        private readonly WeatherController _controller;

        public WeatherControllerTests()
        {
            _mockWeatherService = new Mock<IWeatherService>();
            _mockLogger = new Mock<ILogger<WeatherController>>();
            _controller = new WeatherController(_mockWeatherService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetByCity_WithValidCity_ReturnsOkResult()
        {
            // Arrange
            var cityName = "London";
            var weatherData = new WeatherData { CityName = cityName };
            _mockWeatherService.Setup(s => s.GetWeatherByCityAsync(cityName))
                .ReturnsAsync(weatherData);

            // Act
            var result = await _controller.GetByCity(cityName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<WeatherData>(okResult.Value);
            Assert.Equal(weatherData, returnValue);
        }

        [Fact]
        public async Task GetByCity_WithNonExistentCity_ReturnsNotFound()
        {
            // Arrange
            var cityName = "NonExistentCity";
            _mockWeatherService.Setup(s => s.GetWeatherByCityAsync(cityName))
                .ReturnsAsync((WeatherData?)null);

            // Act
            var result = await _controller.GetByCity(cityName);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetByCity_WithServiceException_ReturnsInternalServerError()
        {
            // Arrange
            var cityName = "London";
            _mockWeatherService.Setup(s => s.GetWeatherByCityAsync(cityName))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetByCity(cityName);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetByZipCode_WithValidZipCode_ReturnsOkResult()
        {
            // Arrange
            var zipCode = "10001";
            var weatherData = new WeatherData { ZipCode = zipCode };
            _mockWeatherService.Setup(s => s.GetWeatherByZipAsync(zipCode))
                .ReturnsAsync(weatherData);

            // Act
            var result = await _controller.GetByZipCode(zipCode);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<WeatherData>(okResult.Value);
            Assert.Equal(weatherData, returnValue);
        }

        [Fact]
        public async Task GetHistory_WithValidCity_ReturnsOkResult()
        {
            // Arrange
            var cityName = "London";
            var historicalData = new List<WeatherData>
            {
                new WeatherData { CityName = cityName },
                new WeatherData { CityName = cityName }
            };
            _mockWeatherService.Setup(s => s.GetHistoricalWeatherAsync(cityName, 10))
                .ReturnsAsync(historicalData);

            // Act
            var result = await _controller.GetHistory(cityName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<WeatherData>>(okResult.Value);
            Assert.Equal(historicalData, returnValue);
        }

        [Fact]
        public void RefreshWeatherData_ReturnsAccepted()
        {
            // Act
            var result = _controller.RefreshWeatherData();

            // Assert
            Assert.IsType<AcceptedResult>(result);
        }
    }
}
