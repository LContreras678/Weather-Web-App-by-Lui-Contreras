using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WeatherWebAppLuiC.Models;
using Newtonsoft.Json.Linq;

namespace WeatherWebAppLuiC.Pages;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    public WeatherResultOutput? Weather { get; set; }
    [BindProperty]
    public string? City { get; set; }
        [BindProperty]
        public double? Lat { get; set; }
        [BindProperty]
        public double? Lon { get; set; }

    public IndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(City)) return Page();
        var apiKey = _configuration["OpenWeather:ApiKey"];
        var client = _httpClientFactory.CreateClient();

        // If lat/lon were provided by the autocomplete selection, skip server geocoding
        if (Lat.HasValue && Lon.HasValue)
        {
            return RedirectToPage("WeatherResult", new { city = City, lat = Lat, lon = Lon });
        }

        // Resolve city to coordinates using geocoding
        var geoUrl = $"https://api.openweathermap.org/geo/1.0/direct?q={Uri.EscapeDataString(City)}&limit=1&appid={apiKey}";
        var geoResp = await client.GetAsync(geoUrl);
        if (!geoResp.IsSuccessStatusCode) return Page();

        var geoJson = await geoResp.Content.ReadAsStringAsync();
        JArray? geoArray;
        try
        {
            geoArray = JArray.Parse(geoJson);
        }
        catch
        {
            return Page();
        }

        if (geoArray == null || geoArray.Count == 0) return Page();

        double lat = (double?)geoArray[0]["lat"] ?? 0.0;
        double lon = (double?)geoArray[0]["lon"] ?? 0.0;

        // Call One Call API (v3) to get current + hourly forecast
        var url = $"https://api.openweathermap.org/data/3.0/onecall?lat={lat}&lon={lon}&exclude=minutely,daily,alerts&appid={apiKey}&units=metric";
        var response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            JObject dataObj;
            try
            {
                dataObj = JObject.Parse(json);
            }
            catch
            {
                return Page();
            }

            var current = dataObj["current"];
            if (current == null) return Page();

            var weatherDesc = current["weather"]?.First?["description"]?.ToString();
            double temp = (double?)current["temp"] ?? 0.0;

            Weather = new WeatherResultOutput
            {
                City = City,
                WeatherDescription = weatherDesc,
                Temps = temp
            };

            return RedirectToPage("WeatherResult", new { city = City, lat = lat, lon = lon });
        }

        return Page();
    }

    // Server-side proxy for the OpenWeather geocoding API to keep the API key secret
    public async Task<IActionResult> OnGetGeocode(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return Content("[]", "application/json");

        var apiKey = _configuration["OpenWeather:ApiKey"];
        var url = $"https://api.openweathermap.org/geo/1.0/direct?q={Uri.EscapeDataString(query)}&limit=5&appid={apiKey}";
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();
        return Content(json, "application/json");
    }

}
