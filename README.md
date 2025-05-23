# Weather Data Application

An ASP.NET Core application that integrates with OpenWeatherMap API to retrieve weather data. Users can search for weather data by city name or zip code. The application persists weather data to a MySQL database and periodically refreshes it.

## Features

- Search weather data by city name or zip code
- Store weather data in MySQL database
- Automatic periodic refresh of weather data
- RESTful API for accessing weather data

## Technologies Used

- ASP.NET Core 8.0
- Entity Framework Core 8.0
- MySQL
- Hangfire (for task scheduling)
- Serilog (for logging)
- xUnit (for unit testing)
- Moq (for mocking in tests)

## Setup and Installation

### Prerequisites

- .NET 8.0 SDK
- MySQL Server

### Steps to Run

1. Clone the repository
2. Navigate to the root directory
3. Update the connection string in `appsettings.json` to point to your MySQL instance
4. Set your OpenWeatherMap API key in `appsettings.json` or as an environment variable:
 - export OpenWeatherMapApiKey=your_api_key_here
5. Run the application:
   -cd WeatherApp.API dotnet run
6. Access the API at `https://localhost:7274/`

### API Endpoints

- `GET /api/weather/city/{cityName}` - Get weather by city name
- `GET /api/weather/zip/{zipCode}` - Get weather by zip code
- `GET /api/weather/history/{cityName}` - Get historical weather data for a city

## Running Tests
- cd WeatherApp.Tests dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

## Short Feedback
Q : Was it easy to complete the task using AI?
- A : Yes, the AI provided clear instructions and code snippets that were easy to follow.

Q : How long did task take you to complete?
- A : Approximately 90 minutes.

Q : Was the code ready to run after generation?
- A : Yes, the code was ready to run after making minor adjustments to the connection string, API key, adding references, instaling nuget packages.

Q : What did you have to change to make it usable?
- A : Get my API key from provider.

Q : Which challenges did you face during completion of the task?
- A : The main challenge was ensuring that the Hangfire setup steps. I've never used it before.

Q : Which specific prompts you learned as a good practice to complete the task?
- A : Providing clear and specific requirements for the Hanfire setup and dependency injection. Saying "Provide me" and "Step by step" is a good practice."


## License

MIT
