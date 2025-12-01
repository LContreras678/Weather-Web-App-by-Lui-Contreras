using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WeatherWebAppLuiC.Models;

namespace WeatherWebAppLuiC.Pages
{
    public class WeatherResultModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public WeatherResultOutput? Weather { get; set; }

        public WeatherResultModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task OnGetAsync(string city)
        {
            var apiKey = "337ae1ad117f2462b426a91fe7943fd5";
            var url = $"https://api.openweathermap.org/data/2.5/forecast?q={city}&appid={apiKey}&units=metric";
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json)!;
                Weather = new WeatherResultOutput
                {
                    City = data.city.name,
                    WeatherDescription = data.list[0].weather[0].description,
                    Temps = (int)data.list[0].main.temp,
                    WeatherNow = data.list[0].weather[0].main,
                    HourlyForecast = new List<HourlyWeather>()
                };


                Func<string, string> mapIcon = main =>
                {
                    main = main.ToLower();
                    if (main == "clouds") return "03d.png";
                    if (main == "clear") return "01d.png";
                    if (main == "rain") return "05d.png";
                    if (main == "snow") return "13d.png";
                    return "Default";
                };

                for (int i = 0; i < 12 && i < ((IEnumerable<dynamic>)data.list).Count(); i++)
                {
                    var hourData = data.list[i];
                    string main = (string)hourData.weather[0].main;
                    Weather.HourlyForecast.Add(new HourlyWeather
                    {
                        Time = DateTime.Parse((string)hourData.dt_txt),
                        Temperature = (int)hourData.main.temp,
                        IconName = mapIcon(main)
                    });
                }
            }
        }
    }
}