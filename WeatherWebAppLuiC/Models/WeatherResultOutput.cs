namespace WeatherWebAppLuiC.Models
// Used to gather Weather Results
// Results will display City Name, some text description and since double data type is used, the weather will have a decimal.

{
    public class WeatherResultOutput
    {
        public string? City { get; set; }
        public string? WeatherDescription { get; set; }

        public int Temps { get; set; }
        
        public string? WeatherNow { get; set; }
    }

}