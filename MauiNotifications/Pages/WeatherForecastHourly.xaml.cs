using MauiNotifications.Model.MVVM;
using MauiNotifications.Model.WeatherDaily;

namespace MauiNotifications.Pages;

public partial class WeatherForecastHourly : ContentPage
{
	public WeatherForecastHourly(WeatherHourlyModel model)
    {
        InitializeComponent();

        BindingContext = model;

        Loaded += WeatherForecastHourly_Loaded;
    }
       
    
    private void WeatherForecastHourly_Loaded(object sender, EventArgs e)
    {
        WeatherDailyResponse.UpdateBackground(this, ((WeatherHourlyModel)BindingContext).IconCode);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }
}