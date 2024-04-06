using MauiNotifications.Model;
using MauiNotifications.Services;

namespace MauiNotifications.Pages;

public partial class WeatherForecastPage : ContentPage
{
    private WeatherForecastService weatherForecastService;
    private IMessage message;
    private PreferenceService preferenceService;
    private readonly GeoLocationService geoLocationService;
    private readonly UserFileDataService userFileDataService;
    private LocationService locationService;
    
    public WeatherForecastPage(
        WeatherForecastService weatherForecastService,
        IMessage message,
        PreferenceService preferenceService,
        GeoLocationService geoLocationService,
        UserFileDataService userFileDataService,
        LocationService locationService)
    {
        this.weatherForecastService = weatherForecastService;
        this.message = message;
        this.preferenceService = preferenceService;
        this.geoLocationService = geoLocationService;
        this.userFileDataService = userFileDataService;
        this.locationService = locationService;

        InitializeComponent();
    }
        
    private async void list_Loaded(object sender, EventArgs e)
    {
        try
        {
            var location = await preferenceService.GetPreferenceLocation();

            Title = location.address;

            //var reversedPlace = await geoLocationService.Reverse(location);
            //Title = reversedPlace.addresses?.FirstOrDefault().address.municipality ?? "?";

            var weatherForecast = await weatherForecastService.GetMinuteForecast(location, 30);

            lblSummary.Text = weatherForecast?.summary?.briefPhrase ?? "[NO SUMMARY INFO]";
                        
            //lblInterval.Text = $"{weatherForecast.intervals?.FirstOrDefault()?.DateDisplay} - {weatherForecast.intervals?.LastOrDefault()?.DateDisplay}";

            lblPrecip.Text = $"{weatherForecast.ForecastPrecipInMm} mm";

            BindingContext = weatherForecast;
        }
        catch (Exception ex)
        {
            message.ShowMessage($"Err loading weather : {ex.Message}");

            await userFileDataService.WriteDataAsync(ex.Message);
        }

        loader.IsRunning = false;
        loader.IsVisible = false;
    }
}