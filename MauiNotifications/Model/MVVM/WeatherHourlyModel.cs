
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiNotifications.Model.WeatherDaily;
using MauiNotifications.Model.WeatherForecastHourly;
using MauiNotifications.Services;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace MauiNotifications.Model.MVVM
{
    public partial class WeatherHourlyModel : ObservableObject, IQueryAttributable
    {
        private readonly WeatherForecastService weatherForecastService;
        private readonly IMessage message;

        private Airandpollen Empty => new Airandpollen { categoryValue = 0, name = "?", value = 0 };

        public ObservableCollection<WeatherForecastHourly.Forecast> forecast { get; } = new ObservableCollection<WeatherForecastHourly.Forecast>();

        [ObservableProperty]
        public float totalPrecip;

        [ObservableProperty]
        public Position place;

        [ObservableProperty]
        public DateOnly day;

        [ObservableProperty]
        public bool loading;

        public int IconCode { get; private set; }

        public IList<Airandpollen> AirAndPollen { get; private set; }

        [ObservableProperty]
        public Airandpollen airQuality;
        [ObservableProperty]
        public Airandpollen grass;
        [ObservableProperty]
        public Airandpollen mold;
        [ObservableProperty]
        public Airandpollen ragweed;
        [ObservableProperty]
        public Airandpollen tree;
        [ObservableProperty]
        public Airandpollen uVIndex;

        public WeatherHourlyModel(
            WeatherForecastService weatherForecastService,
            IMessage message)
        {
            this.weatherForecastService = weatherForecastService;
            this.message = message;
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("iconCode", out object iconCode) && iconCode is int ic) IconCode = ic;

            if (query.TryGetValue("airAndPollen", out object airAndPollen))
            {
                AirAndPollen = (List<Airandpollen>)airAndPollen;

                UpdatePollens();
            }

            if (query.TryGetValue("location", out object location) && location is Position position)
            {
                if (query.TryGetValue("date", out object date) && date is DateTime dateTime)
                {
                    Place = position;

                    Day = new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day);

                    await RefreshData();
                }
            }
        }

        private void UpdatePollens()
        {
            AirQuality = AirAndPollen.FirstOrDefault(a => a.name == "AirQuality") ?? Empty;
            Grass = AirAndPollen.FirstOrDefault(a => a.name == "Grass") ?? Empty;
            Mold = AirAndPollen.FirstOrDefault(a => a.name == "Mold") ?? Empty;
            Ragweed = AirAndPollen.FirstOrDefault(a => a.name == "Ragweed") ?? Empty;
            Tree = AirAndPollen.FirstOrDefault(a => a.name == "Tree") ?? Empty;
            UVIndex = AirAndPollen.FirstOrDefault(a => a.name == "UVIndex") ?? Empty;
        }

        [RelayCommand]
        public async Task RefreshData()
        {
            if (forecast.Count > 0) return;

            Loading = true;

            var forecastData = await weatherForecastService.GetHourlyForecast(Place);

            var forecasts = forecastData.forecasts?.Where(f => f.date.Year == Day.Year && f.date.Month == Day.Month && f.date.Day == Day.Day);

            foreach (var day in forecasts) forecast.Add(day);

            TotalPrecip = forecasts.Sum(f => f.totalLiquid.value);

            Loading = false;
        }
    }
}
