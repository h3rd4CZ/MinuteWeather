using MauiNotifications.Model;
using MauiNotifications.Model.WeatherForecastHourly;
using MauiNotifications.Model.WeatherForecastSevereAlerts;
using MauiNotifications.Pages;

namespace MauiNotifications.Services
{
    public class WeatherForecastCacheService
    {
        private readonly IAsynchronousCache<WeatherForecastAlertResult> alertCache;
        private readonly IAsynchronousCache<WeatherHourlyResponse> hourlyForecastCache;
        private readonly WeatherForecastService weatherForecastService;

        public WeatherForecastCacheService(
            IAsynchronousCache<WeatherForecastAlertResult> alertCache,
            IAsynchronousCache<WeatherHourlyResponse> hourlyForecastCache,
            WeatherForecastService weatherForecastService)
        {
            this.alertCache = alertCache;
            this.hourlyForecastCache = hourlyForecastCache;
            this.weatherForecastService = weatherForecastService;
        }

        public async Task<WeatherForecastAlertResult> GetSevereAlerts(Position latLon)
        {
            return await alertCache.GetOrFetchValueAsync(
                CacheKey.ForValuesWhere("Position", $"{latLon.lat};{latLon.lon}"), 
                async () => await weatherForecastService.GetSevereAlerts(latLon));
        }

        public async Task<WeatherHourlyResponse> GetHourlyForecast(Position latLon, int timeoutInSeconds = 60)
        {
            return await hourlyForecastCache.GetOrFetchValueAsync(
                CacheKey.ForValuesWhere("Position", $"{latLon.lat};{latLon.lon}"),
                async () => await weatherForecastService.GetHourlyForecast(latLon, timeoutInSeconds));
        }
    }
}
