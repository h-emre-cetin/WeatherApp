using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.Core.Models;
using WeatherApp.Infrastructure.Data;
using WeatherApp.Infrastructure.Repositories;
using Xunit;

namespace WeatherApp.Tests
{
    public class WeatherRepositoryTests
    {
        private readonly DbContextOptions<WeatherDbContext> _options;
        private readonly Mock<ILogger<WeatherRepository>> _mockLogger;

        public WeatherRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<WeatherDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _mockLogger = new Mock<ILogger<WeatherRepository>>();

            // Seed the database
            using var context = new WeatherDbContext(_options);
            context.WeatherData.AddRange(
                new WeatherData
                {
                    Id = 1,
                    CityName = "London",
                    Temperature = 20,
                    RetrievedAt = DateTime.UtcNow.AddHours(-1)
                },
                new WeatherData
                {
                    Id = 2,
                    CityName = "London",
                    Temperature = 22,
                    RetrievedAt = DateTime.UtcNow
                },
                new WeatherData
                {
                    Id = 3,
                    ZipCode = "10001",
                    CityName = "New York",
                    Temperature = 25,
                    RetrievedAt = DateTime.UtcNow
                }
            );
            context.SaveChanges();
        }

        [Fact]
        public async Task GetByCityNameAsync_ReturnsLatestWeatherData()
        {
            // Arrange
            using var context = new WeatherDbContext(_options);
            var repository = new WeatherRepository(context, _mockLogger.Object);

            // Act
            var result = await repository.GetByCityNameAsync("London");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("London", result.CityName);
            Assert.Equal(22, result.Temperature); // Should get the most recent one
        }

        [Fact]
        public async Task GetByCityNameAsync_WithNonExistentCity_ReturnsNull()
        {
            // Arrange
            using var context = new WeatherDbContext(_options);
            var repository = new WeatherRepository(context, _mockLogger.Object);

            // Act
            var result = await repository.GetByCityNameAsync("Paris");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByZipCodeAsync_ReturnsWeatherData()
        {
            // Arrange
            using var context = new WeatherDbContext(_options);
            var repository = new WeatherRepository(context, _mockLogger.Object);

            // Act
            var result = await repository.GetByZipCodeAsync("10001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("10001", result.ZipCode);
            Assert.Equal("New York", result.CityName);
        }

        [Fact]
        public async Task GetHistoricalDataAsync_ReturnsOrderedData()
        {
            // Arrange
            using var context = new WeatherDbContext(_options);
            var repository = new WeatherRepository(context, _mockLogger.Object);

            // Act
            var result = await repository.GetHistoricalDataAsync("London");

            // Assert
            var list = result.ToList();
            Assert.Equal(2, list.Count);
            Assert.Equal(22, list[0].Temperature); // Most recent first
            Assert.Equal(20, list[1].Temperature);
        }

        [Fact]
        public async Task AddAsync_AddsNewWeatherData()
        {
            // Arrange
            using var context = new WeatherDbContext(_options);
            var repository = new WeatherRepository(context, _mockLogger.Object);
            var newWeatherData = new WeatherData
            {
                CityName = "Paris",
                Temperature = 18,
                RetrievedAt = DateTime.UtcNow
            };

            // Act
            await repository.AddAsync(newWeatherData);

            // Assert
            var result = await context.WeatherData.FirstOrDefaultAsync(w => w.CityName == "Paris");
            Assert.NotNull(result);
            Assert.Equal(18, result.Temperature);
        }

        [Fact]
        public async Task UpdateAsync_AddsNewRecord()
        {
            // Arrange
            using var context = new WeatherDbContext(_options);
            var repository = new WeatherRepository(context, _mockLogger.Object);
            var updatedWeatherData = new WeatherData
            {
                CityName = "London",
                Temperature = 23,
                RetrievedAt = DateTime.UtcNow
            };

            // Act
            await repository.UpdateAsync(updatedWeatherData);

            // Assert
            var result = await context.WeatherData.Where(w => w.CityName == "London").ToListAsync();
            Assert.Equal(3, result.Count); // Should now have 3 records for London
            Assert.Contains(result, w => w.Temperature == 23);
        }

        [Fact]
        public async Task GetAllCityNamesAsync_ReturnsDistinctCityNames()
        {
            // Arrange
            using var context = new WeatherDbContext(_options);
            var repository = new WeatherRepository(context, _mockLogger.Object);

            // Act
            var result = await repository.GetAllCityNamesAsync();

            // Assert
            var list = result.ToList();
            Assert.Equal(2, list.Count);
            Assert.Contains("London", list);
            Assert.Contains("New York", list);
        }

        [Fact]
        public async Task GetAllZipCodesAsync_ReturnsDistinctZipCodes()
        {
            // Arrange
            using var context = new WeatherDbContext(_options);
            var repository = new WeatherRepository(context, _mockLogger.Object);

            // Act
            var result = await repository.GetAllZipCodesAsync();

            // Assert
            var list = result.ToList();
            Assert.Single(list);
            Assert.Contains("10001", list);
        }
    }
}
