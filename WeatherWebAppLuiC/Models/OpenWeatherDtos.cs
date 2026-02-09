using System.Text.Json.Serialization;

namespace WeatherWebAppLuiC.Models
{
    public class GeoLocation
    {
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("lat")] public double Lat { get; set; }
        [JsonPropertyName("lon")] public double Lon { get; set; }
        [JsonPropertyName("country")] public string? Country { get; set; }
    }

    public class OneCallResponse
    {
        [JsonPropertyName("current")] public Current? Current { get; set; }
        [JsonPropertyName("hourly")] public List<Hourly>? Hourly { get; set; }
        [JsonPropertyName("timezone_offset")] public int TimezoneOffset { get; set; }
    }

    public class Current
    {
        [JsonPropertyName("dt")] public long Dt { get; set; }
        [JsonPropertyName("temp")] public double Temp { get; set; }
        [JsonPropertyName("feels_like")] public double FeelsLike { get; set; }
        [JsonPropertyName("weather")] public List<WeatherInfo>? Weather { get; set; }
    }

    public class Hourly
    {
        [JsonPropertyName("dt")] public long Dt { get; set; }
        [JsonPropertyName("temp")] public double Temp { get; set; }
        [JsonPropertyName("pop")] public double? Pop { get; set; }
        [JsonPropertyName("weather")] public List<WeatherInfo>? Weather { get; set; }
    }

    public class WeatherInfo
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("main")] public string? Main { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
        [JsonPropertyName("icon")] public string? Icon { get; set; }
    }
}
