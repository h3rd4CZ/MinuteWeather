using CommunityToolkit.Maui.Views;
using MauiNotifications.Model.WeatherForecastSevereAlerts;

namespace MauiNotifications.Pages.Popups;

public partial class AlertPopup : Popup
{
	public AlertPopup(AlertResult alert)
	{
		InitializeComponent();
				
		SetupView(alert);
    }

    private void SetupView(AlertResult alert)
    {
		lblSummary.Text = alert.description.localized;

		alertAreas.ItemsSource = alert.alertAreas;
    }
}