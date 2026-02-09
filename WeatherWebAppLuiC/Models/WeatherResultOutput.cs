namespace WeatherWebAppLuiC.Models
// Used to gather Weather Results
// Results will display City Name, some text description and since double data type is used, the weather will have a decimal.
// Now updated to have Hourly Weather updates.

{
    public class WeatherResultOutput
    {
        public string? City { get; set; }
        public string? WeatherDescription { get; set; }

        // Temperature stored in Celsius (current)
        public double Temps { get; set; }

        // Nullable Celsius values for min/max/feels-like (may be absent)
        public double? MinTempsCelsius { get; set; }
        public double? MaxTempsCelsius { get; set; }
        public double? TempsFeelsLikeCelsius { get; set; }

        public string? WeatherNow { get; set; }

        public List<HourlyWeather> HourlyForecast { get; set; } = new();

        // Convert Celsius to Fahrenheit
        public static double CelsiusToFahrenheit(double c) => c * 9.0 / 5.0 + 32.0;

        // Fahrenheit rounded to 1 decimal place for display (current)
        public double TempsF => Math.Round(CelsiusToFahrenheit(Temps), 1);

        // Display string like as ab example: 9°C / 48.2 °F
        // The formula is: 	(32°F − 32) × 5/9 = 0°C


        public string TempsDisplay => $"{Math.Round(Temps, 0)}°C / {TempsF} °F";

        // Computed Fahrenheit and display helpers for nullable values
        public double? MinTempsFahrenheit => MinTempsCelsius.HasValue ? Math.Round(CelsiusToFahrenheit(MinTempsCelsius.Value), 1) : (double?)null;
        public double? MaxTempsFahrenheit => MaxTempsCelsius.HasValue ? Math.Round(CelsiusToFahrenheit(MaxTempsCelsius.Value), 1) : (double?)null;
        public double? TempsFeelsLikeFahrenheit => TempsFeelsLikeCelsius.HasValue ? Math.Round(CelsiusToFahrenheit(TempsFeelsLikeCelsius.Value), 1) : (double?)null;

        public string MinTempsDisplay => MinTempsCelsius.HasValue && MinTempsFahrenheit.HasValue
            ? $"{Math.Round(MinTempsCelsius.Value, 0)}°C / {MinTempsFahrenheit.Value} °F"
            : "N/A";

        public string MaxTempsDisplay => MaxTempsCelsius.HasValue && MaxTempsFahrenheit.HasValue
            ? $"{Math.Round(MaxTempsCelsius.Value, 0)}°C / {MaxTempsFahrenheit.Value} °F"
            : "N/A";

        public string TempsFeelsLikeDisplay => TempsFeelsLikeCelsius.HasValue && TempsFeelsLikeFahrenheit.HasValue
            ? $"{Math.Round(TempsFeelsLikeCelsius.Value, 0)}°C / {TempsFeelsLikeFahrenheit.Value} °F"
            : "N/A";
    }

    public class HourlyWeather
    {
        public DateTime Time { get; set; }

        // Temperature in Celsius
        public double Temperature { get; set; }
        public string? IconName { get; set; }

        public string? WeatherDescription { get; set; }

        public double TemperatureF => Math.Round(WeatherResultOutput.CelsiusToFahrenheit(Temperature), 1);

        public string TemperatureDisplay => $"{Math.Round(Temperature, 0)}°C / {TemperatureF} °F";
    }

}