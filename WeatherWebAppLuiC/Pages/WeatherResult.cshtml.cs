using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WeatherWebAppLuiC.Models;
using System.Net.Http;
using System.Linq;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WeatherWebAppLuiC.Pages
{
    public class WeatherResultModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        public WeatherResultOutput? Weather { get; set; }
        public GeoLocation[]? Candidates { get; set; }
        public string? QueryCity { get; set; }

        public WeatherResultModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task OnGetAsync(string city, double? lat, double? lon)
        {
            QueryCity = city;

            if (string.IsNullOrWhiteSpace(city))
            {
                Weather = null;
                return;
            }

            var apiKey = _configuration["OpenWeather:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey)) return;

            var client = _httpClientFactory.CreateClient();

            // If lat/lon provided (user selected a candidate), skip geocoding
            if (lat.HasValue && lon.HasValue)
            {
                var data = await FetchOneCallDataAsync(client, apiKey, lat.Value, lon.Value);
                if (data == null) return;
                Weather = BuildWeatherResult(city, data);
                return;
            }

            // Otherwise fetch possible candidates for the search term
            var candidates = await ResolveCityCandidatesAsync(client, apiKey, city);
            if (candidates == null || candidates.Length == 0) return;

            if (candidates.Length == 1)
            {
                var c = candidates[0];
                var data = await FetchOneCallDataAsync(client, apiKey, c.Lat, c.Lon);
                if (data == null) return;
                Weather = BuildWeatherResult(city, data);
                return;
            }

            // Multiple matches — show selection to user
            Candidates = candidates;
            Weather = null;
            return;
        }

        private async Task<GeoLocation[]?> ResolveCityCandidatesAsync(HttpClient client, string apiKey, string city)
        {
            var geoUrl = $"https://api.openweathermap.org/geo/1.0/direct?q={Uri.EscapeDataString(city)}&limit=5&appid={apiKey}";
            var geoResp = await client.GetAsync(geoUrl);
            if (!geoResp.IsSuccessStatusCode) return null;

            var geoJson = await geoResp.Content.ReadAsStringAsync();
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var geoArray = JsonSerializer.Deserialize<GeoLocation[]>(geoJson, options);
                return geoArray;
            }
            catch
            {
                return null;
            }
        }

        private async Task<OneCallResponse?> FetchOneCallDataAsync(HttpClient client, string apiKey, double lat, double lon)
        {
            var url = $"https://api.openweathermap.org/data/3.0/onecall?lat={lat.ToString(CultureInfo.InvariantCulture)}&lon={lon.ToString(CultureInfo.InvariantCulture)}&exclude=minutely,daily,alerts&appid={apiKey}&units=metric";
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<OneCallResponse>(json, options);
            }
            catch
            {
                return null;
            }
        }

        private WeatherResultOutput? BuildWeatherResult(string city, OneCallResponse data)
        {
            var current = data.Current;
            if (current == null) return null;

            var weatherItem = current.Weather?.FirstOrDefault();
            var weatherMain = weatherItem?.Main;
            var weatherDesc = weatherItem?.Description;
            double temp = current.Temp;

            var result = new WeatherResultOutput
            {
                City = city,
                WeatherDescription = weatherDesc,
                Temps = temp,
                WeatherNow = weatherMain,
                HourlyForecast = new List<HourlyWeather>()
            };

            int timezoneOffsetSeconds = data.TimezoneOffset;
            var hourly = data.Hourly;
            if (hourly == null) return result;

            for (int i = 0; i < 12 && i < hourly.Count; i++)
            {
                var hourData = hourly[i];
                var hourWeather = hourData.Weather?.FirstOrDefault();

                DateTime time;
                try
                {
                    var dto = DateTimeOffset.FromUnixTimeSeconds(hourData.Dt);
                    time = dto.ToOffset(TimeSpan.FromSeconds(timezoneOffsetSeconds)).DateTime;
                }
                catch
                {
                    time = DateTime.UtcNow;
                }

                double hourTemp = hourData.Temp;

                result.HourlyForecast.Add(new HourlyWeather
                {
                    Time = time,
                    Temperature = hourTemp,
                    IconName = MapIcon(hourWeather),
                    WeatherDescription = hourWeather?.Description
                });
            }

            // Set feels-like (from current data if available)
            try
            {
                result.TempsFeelsLikeCelsius = current.FeelsLike;
            }
            catch { result.TempsFeelsLikeCelsius = null; }

            // Derive min/max from hourly forecast when available
            if (result.HourlyForecast != null && result.HourlyForecast.Count > 0)
            {
                var min = result.HourlyForecast.Min(h => h.Temperature);
                var max = result.HourlyForecast.Max(h => h.Temperature);
                result.MinTempsCelsius = Math.Round(min, 1);
                result.MaxTempsCelsius = Math.Round(max, 1);
            }

            return result;
        }

        private string MapIcon(WeatherInfo? weatherItem)
        {
            try
            {
                var iconCode = weatherItem?.Icon;
                if (!string.IsNullOrWhiteSpace(iconCode))
                {
                    return $"https://openweathermap.org/img/wn/{iconCode}@2x.png";
                }
            }
            catch { }

            try
            {
                var main = weatherItem?.Main?.ToLower() ?? string.Empty;
                return main switch
                {
                    "clouds" => "https://openweathermap.org/img/wn/03d@2x.png",
                    "clear" => "https://openweathermap.org/img/wn/01d@2x.png",
                    "few clouds" => "https://openweathermap.org/img/wn/02d@2x.png",
                    "scattered clouds" => "https://openweathermap.org/img/wn/03d@2x.png",
                    "broken clouds" => "https://openweathermap.org/img/wn/04d@2x.png",
                    "shower rain" => "https://openweathermap.org/img/wn/09d@2x.png",
                    "rain" => "https://openweathermap.org/img/wn/10d@2x.png",
                    "thunderstorm" => "https://openweathermap.org/img/wn/11d@2x.png",
                    "snow" => "https://openweathermap.org/img/wn/13d@2x.png",
                    "mist" => "https://openweathermap.org/img/wn/50d@2x.png",
                    _ => "https://openweathermap.org/img/wn/03d@2x.png",
                };
            }
            catch
            {
                return "https://openweathermap.org/img/wn/03d@2x.png";
            }
        }
    }
}