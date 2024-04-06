using MauiNotifications.Pages;

namespace MauiNotifications;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

        Routing.RegisterRoute("main", typeof(MainPage));
        Routing.RegisterRoute("debug", typeof(Debug));
        Routing.RegisterRoute("loc", typeof(Pages.Location));
        Routing.RegisterRoute("findloc", typeof(FindLocation));
        Routing.RegisterRoute("weather", typeof(WeatherForecastPage));
        Routing.RegisterRoute("weatherdaily", typeof(WeatherForecastDaily));
        Routing.RegisterRoute("weatherhourly", typeof(WeatherForecastHourly));
        Routing.RegisterRoute("close", typeof(Close));
        Routing.RegisterRoute("geofence", typeof(GeofenceSettings));
    }
}
