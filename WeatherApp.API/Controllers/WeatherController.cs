using Microsoft.AspNetCore.Mvc;
using WeatherApp.Core.Interfaces;
using WeatherApp.Core.Models;

namespace WeatherApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;
        private readonly ILogger<WeatherController> _logger;

        public WeatherController(IWeatherService weatherService, ILogger<WeatherController> logger)
        {
            _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("city/{cityName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<WeatherData>> GetByCity(string cityName)
        {
            try
            {
                var result = await _weatherService.GetWeatherByCityAsync(cityName);
                if (result == null)
                    return NotFound($"Weather data for city '{cityName}' not found.");

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when getting weather by city");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving weather data for city: {CityName}", cityName);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving weather data.");
            }
        }

        [HttpGet("zip/{zipCode}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<WeatherData>> GetByZipCode(string zipCode)
        {
            try
            {
                var result = await _weatherService.GetWeatherByZipAsync(zipCode);
                if (result == null)
                    return NotFound($"Weather data for zip code '{zipCode}' not found.");

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when getting weather by zip code");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving weather data for zip code: {ZipCode}", zipCode);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving weather data.");
            }
        }

        [HttpGet("history/{cityName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<WeatherData>>> GetHistory(string cityName, [FromQuery] int limit = 10)
        {
            try
            {
                var result = await _weatherService.GetHistoricalWeatherAsync(cityName, limit);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when getting weather history");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving historical weather data for city: {CityName}", cityName);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving historical weather data.");
            }
        }

        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult RefreshWeatherData()
        {
            try
            {
                // We're not awaiting this task as it might take some time
                // and we want to return a response immediately
                _ = _weatherService.RefreshAllWeatherDataAsync();
                return Accepted("Weather data refresh has been scheduled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling weather data refresh");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while scheduling weather data refresh.");
            }
        }
    }
}
