using MauiNotifications.Model.MVVM;
using MauiNotifications.Services;

namespace MauiNotifications.Pages;

public partial class GeofenceSettings : ContentPage
{
	public GeofenceSettings(GeofencePreferenceModel geofencePreferenceModel)
	{
		InitializeComponent();

		BindingContext = geofencePreferenceModel;
    }
}