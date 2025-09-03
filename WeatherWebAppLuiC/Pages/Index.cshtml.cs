using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WeatherWebAppLuiC.Models;

namespace WeatherWebAppLuiC.Pages;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public WeatherResultOutput? Weather { get; set; }
    [BindProperty]
    public string? City { get; set; }

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(City)) return Page();

        var apiKey = "337ae1ad117f2462b426a91fe7943fd5";
        var url = $"https://api.openweathermap.org/data/2.5/weather?q={City}&appid={apiKey}&units=metric";
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json)!;
            Weather = new WeatherResultOutput
            {
                City = data.name,
                WeatherDescription = data.weather[0].description,
                Temps = data.main.temp
            };
            
            return RedirectToPage("WeatherResult", new { city = City });
        }
        return Page();
    }

    public void OnGet()
    {

    }
}
