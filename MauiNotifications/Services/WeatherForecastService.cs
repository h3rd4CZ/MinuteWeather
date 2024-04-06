using MauiNotifications.Model;
using MauiNotifications.Model.Test;
using MauiNotifications.Model.Weather;
using MauiNotifications.Model.WeatherCurrent;
using MauiNotifications.Model.WeatherDaily;
using MauiNotifications.Model.WeatherForecastHourly;
using MauiNotifications.Model.WeatherForecastSevereAlerts;
using System.Globalization;
using System.Net.Http.Json;

namespace MauiNotifications.Services
{
    public class WeatherForecastService : AzureMapsServiceBase
    {
        private const string API_URL = "https://atlas.microsoft.com/weather/forecast/minute";
        private const string API_URL_DAILY = "https://atlas.microsoft.com/weather/forecast/daily";
        private const string API_URL_CURRENT = "https://atlas.microsoft.com/weather/currentConditions";
        private const string API_URL_HOURLY = "https://atlas.microsoft.com/weather/forecast/hourly";
        private const string API_URL_SEVEREALERTS = "https://atlas.microsoft.com/weather/severe/alerts";

        public const int FORECAST_SNAPSHOT_DURATION_MINUTES = 120;

        public const int FORECAST_SNAPSHOT_FRAME_SIZE = 5;

        public const int FORECAST_DAILY_DUR = 25;

        public const int FORECAST_HOURLY_DUR_MAX = 240;

        public const int FORECAST_HOURLY_DUR = FORECAST_HOURLY_DUR_MAX;

        public async Task<WeatherForecastAlertResult> GetSevereAlerts(Position latLon, int timeoutInSeconds = 30)
        {
            //return System.Text.Json.JsonSerializer.Deserialize<WeatherForecastAlertResult>(Const.SEVEREALERT_TESTDATA);

            var client = GetInitClient();

            client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);

            var uri
                = $"{API_URL_SEVEREALERTS}/json?api-version=1.0&subscription-key={SUBSCRIPTION_KEY}&language=cs-CZ&query={latLon.lat.ToString(new CultureInfo(1033))},{latLon.lon.ToString(new CultureInfo(1033))}";

            var response = await client.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<WeatherForecastAlertResult>();
            }
            else throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
        }

        public async Task<WeatherHourlyResponse> GetHourlyForecast(Position latLon, int timeoutInSeconds = 60)
        {
            var client = GetInitClient();

            var uri
                = $"{API_URL_HOURLY}/json?api-version=1.0&subscription-key={SUBSCRIPTION_KEY}&language=cs-CZ&query={latLon.lat.ToString(new CultureInfo(1033))},{latLon.lon.ToString(new CultureInfo(1033))}&duration={FORECAST_HOURLY_DUR}";

            client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);

            var response = await client.GetAsync(uri);
                        
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<WeatherHourlyResponse>();
            }
            else throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
        }

        public async Task<WeatherCurrentResult> GetCurrentForecast(Position latLon, int timeoutInSeconds = 60)
        {
            var client = GetInitClient();

            var uri
                = $"{API_URL_CURRENT}/json?api-version=1.0&subscription-key={SUBSCRIPTION_KEY}&language=cs-CZ&query={latLon.lat.ToString(new CultureInfo(1033))},{latLon.lon.ToString(new CultureInfo(1033))}&duration=0&details=true";

            client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);

            var response = await client.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<WeatherCurrentResult>();
            }
            else throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
        }

        public async Task<WeatherDailyResponse> GetDailyForecast(Position latLon)
        {
            var client = GetInitClient();

            var uri
                = $"{API_URL_DAILY}/json?api-version=1.0&subscription-key={SUBSCRIPTION_KEY}&language=cs-CZ&query={latLon.lat.ToString(new CultureInfo(1033))},{latLon.lon.ToString(new CultureInfo(1033))}&duration={FORECAST_DAILY_DUR}";

            var response = await client.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                var resp = await response.Content.ReadAsStringAsync();

                return await response.Content.ReadFromJsonAsync<WeatherDailyResponse>();
            }
            else throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
        }

        public async Task<WeatherForecastResponse> GetMinuteForecast(Position latLon, int timeoutInSeconds = 5)
        {
            var client = GetInitClient();

            var uri
                = $"{API_URL}/json?api-version=1.0&subscription-key={SUBSCRIPTION_KEY}&language=cs-CZ&query={latLon.lat.ToString(new CultureInfo(1033))},{latLon.lon.ToString(new CultureInfo(1033))}&interval={FORECAST_SNAPSHOT_FRAME_SIZE}";

            client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);

            var response = await client.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<WeatherForecastResponse>();
            }
            else throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
        }      
    }
}
