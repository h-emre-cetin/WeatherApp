namespace WeatherApp.Core.Models
{
    public class WeatherData
    {
        public int Id { get; set; }

        public string CityName { get; set; } = string.Empty;
        
        public string? ZipCode { get; set; }
        
        public double Temperature { get; set; }
        
        public double FeelsLike { get; set; }
        
        public double MinTemperature { get; set; }
        
        public double MaxTemperature { get; set; }
        
        public int Humidity { get; set; }
        
        public string Description { get; set; } = string.Empty;
        
        public string Icon { get; set; } = string.Empty;
        
        public double WindSpeed { get; set; }
        
        public DateTime LastUpdated { get; set; }
        
        public DateTime RetrievedAt { get; set; } = DateTime.UtcNow;
    }
}
