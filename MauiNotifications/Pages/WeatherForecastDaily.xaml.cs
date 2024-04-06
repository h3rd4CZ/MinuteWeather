using CommunityToolkit.Maui.Views;
using MauiNotifications.Model;
using MauiNotifications.Model.Weather;
using MauiNotifications.Model.WeatherCurrent;
using MauiNotifications.Model.WeatherDaily;
using MauiNotifications.Pages.Popups;
using MauiNotifications.Services;

namespace MauiNotifications.Pages;

public partial class WeatherForecastDaily : ContentPage
{
    private readonly WeatherForecastService weatherForecastService;
    private readonly IMessage message;
    private readonly PreferenceService preferenceService;
    private readonly GeoLocationService geoLocationService;
    private readonly UserFileDataService userFileDataService;
    private readonly LocationService locationService;

    private IList<BoxView> allBoxes = new List<BoxView>();

    private Position location;

    public WeatherForecastDaily(
        WeatherForecastService weatherForecastService,
        IMessage message,
        PreferenceService preferenceService,
        GeoLocationService geoLocationService,
        UserFileDataService userFileDataService,
        LocationService locationService)
    {
        InitializeComponent();
        this.weatherForecastService = weatherForecastService;
        this.message = message;
        this.preferenceService = preferenceService;
        this.geoLocationService = geoLocationService;
        this.userFileDataService = userFileDataService;
        this.locationService = locationService;
    }
        
    private async void CollectionView_Loaded(object sender, EventArgs e)
    {
        try
        {
            location = await preferenceService.GetPreferenceLocation();

            Title = location.address;

            var forecast = await weatherForecastService.GetDailyForecast(location);

            var currentForecast = await weatherForecastService.GetCurrentForecast(location);

            var hotTopicCurrentWeather = new HotTopicMessage { Message = forecast.summary.phrase };
                                    
            var alerts = await weatherForecastService.GetSevereAlerts(location);
                        
            if(alerts is not null)
            {
                foreach (var alert in alerts.results)
                {
                    forecast.HotTopicMessages.Add(new HotTopicMessage { Message = alert.description.localized, Alert = alert });
                }
            }

            forecast.HotTopicMessages.Add(hotTopicCurrentWeather);

            if (currentForecast is not null && currentForecast.results?.Count() > 0)
            {
                forecast.Current = currentForecast.results.First();
                forecast.Current.Loaded = true;
            }

            BindingContext = forecast;

            WeatherDailyResponse.UpdateBackground(this, forecast.Current?.iconCode ?? 0);
        }
        catch (Exception ex)
        {
            message.ShowMessage($"Err loading weather : {ex.Message}");

            await userFileDataService.WriteDataAsync(ex.Message);
        }

        loader.IsRunning = false;
        loader.IsVisible = false;

        await Task.Yield();

        await AnimatePrecip();
    }
        
    private async Task AnimatePrecip()
    {
        var animateTasks = new List<Task>();

        foreach (var precipBox in allBoxes)
        {
            double desiredWidth = double.Parse(precipBox.ClassId);

            animateTasks.Add(precipBox.ScaleXTo(desiredWidth, 2000, Easing.Linear));
            animateTasks.Add(precipBox.ScaleYTo(1, 2000, Easing.Linear));
        }

        await Task.WhenAll(animateTasks);
    }

    private void BoxView_Loaded(object sender, EventArgs e)
    {
        if (sender is BoxView precipBox)
        {
            allBoxes.Add(precipBox);
        }
    }
        
    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        var frame = (Frame)sender;

        var ctx = (Forecast)frame.BindingContext;

        var date = ctx.date;

        var currDate = DateTime.Now;

        if(currDate.AddDays(WeatherForecastService.FORECAST_HOURLY_DUR_MAX / 24) < date)
        {
            return;
        }

        await Shell.Current.GoToAsync("weatherhourly", true, new Dictionary<string, object>() { 
            { "location", location },
            { "date", date },
            { "airAndPollen", ctx.airAndPollen.ToList()},
            { "iconCode", ctx.day.iconCode }
        });
    }

    private void TapGestureRecognizer_Tapped_Alert(object sender, TappedEventArgs e)
    {
        var frame = (Frame)sender;

        var hotTopicMsg = (HotTopicMessage)frame.BindingContext;

        var alert = hotTopicMsg.Alert;

        var popup = new AlertPopup(alert);

        this.ShowPopup(popup);
    }
}