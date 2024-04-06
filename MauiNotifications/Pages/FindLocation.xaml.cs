using MauiNotifications.Model;
using MauiNotifications.Services;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace MauiNotifications.Pages;

public partial class FindLocation : ContentPage
{
    private readonly GeoLocationService geoLocationService;
    private readonly ILogger<FindLocation> logger;
    private readonly UserFileDataService userFileDataService;
    private readonly LocationService locationService;
    private readonly IMessage message;

    public FindLocation(
        GeoLocationService geoLocationService,
        ILogger<FindLocation> logger,
        UserFileDataService userFileDataService,
        LocationService locationService,
        IMessage message
        )
    {
        this.geoLocationService = geoLocationService;
        this.logger = logger;
        this.userFileDataService = userFileDataService;
        this.locationService = locationService;
        this.message = message;
        InitializeComponent();
    }

    private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is GeoLocationResult geoLocation)
        {
            var freeFormAddress = geoLocation.address.freeformAddress;

            Shell.Current.GoToAsync("..", new Dictionary<string, object>
            {
                { "data", geoLocation.position },
                { "address", freeFormAddress},
            });
        }
    }

    private async void ListViewReverse_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is ReverseAddress address)
        {
            try
            {
                var position = address.position.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                var freeFormAddress = address.address.municipality;

                await Shell.Current.GoToAsync("..", new Dictionary<string, object>
            {
                { "data", new Position{ lat = float.Parse( position[0], new CultureInfo(1033)), lon = float.Parse(position[1], new CultureInfo(1033)) }},
                { "address", freeFormAddress},
            });
            }
            catch (Exception ex)
            {
                message.ShowMessage(ex.Message);

                await userFileDataService.WriteDataAsync(ex.Message);
            }
        }
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        var searchFor = locationText.Text;

        try
        {
            var places = await geoLocationService.Search(searchFor);

            list.ItemsSource = places.results;

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "En arror occured when looking for places");
        }
    }

    private async void Button_CurrPosition_Clicked(object sender, EventArgs e)
    {

        btnCurrLoc.IsEnabled = false;

        var currLoc = await locationService.GetCurrentLocation();

        if (currLoc is not null)
        {
            message.ShowMessage($"lat: {currLoc.Latitude}, Lon : {currLoc.Longitude}");

            try
            {
                var reversedPlace = await geoLocationService.Reverse(new Position { lat = (float)currLoc.Latitude, lon = (float)currLoc.Longitude });

                listReverse.ItemsSource = reversedPlace.addresses;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Err when reversing location");
            }
        }
        else
        {
            message.ShowMessage($"ERR");
        }

        btnCurrLoc.IsEnabled = true;
    }
}