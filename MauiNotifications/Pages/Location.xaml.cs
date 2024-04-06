using MauiNotifications.Model;
using MauiNotifications.Services;
using System.Runtime.Serialization;
using CommunityToolkit.Maui.Views;
using MauiNotifications.Pages.Popups;

namespace MauiNotifications.Pages;

public partial class Location : ContentPage, IQueryAttributable
{
    private const int NOTIF_SCHEDULE_DELAY = 10;
    private readonly IMessage message;
    private readonly IAlarmService alarmService;
    private readonly PreferenceService preferenceService;
    private readonly UserFileDataService userFileDataService;
    private LocationPreference preference;
    private WindScoreMotionPreference windScoreMotionPreference;
        
    public Location(
        IMessage message,
        IAlarmService alarmService,
        PreferenceService preferenceService,
        UserFileDataService userFileDataService)
    {
        InitializeComponent();
        this.message = message;
        this.alarmService = alarmService;
        this.preferenceService = preferenceService;
        this.userFileDataService = userFileDataService;
    }
        
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await RequestResources();

            if (preference is null) preference = preferenceService.GetCommonPreference();

            if (windScoreMotionPreference is null) windScoreMotionPreference = preferenceService.GetWindScoreMotionPreference();

            var notificationsEnabled = await alarmService.AreNotificationsEnabled();

            if (notificationsEnabled)
            {
                preference.AreNotificationEnabled = true;
                sysNotificationsGranted.IsToggled = true;
                sysNotificationsGranted.IsEnabled = false;
            }

            if (windScoreMotionPreference is not null) windScore.IsToggled = windScoreMotionPreference.currentlyActive;
            
            RefreshBindingContext();            
        }
        catch (Exception ex)
        {
            message.ShowMessage($"Err loading preferencies : {ex.Message}");

            await userFileDataService.WriteDataAsync(ex.Message);
        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("data", out object latLon) && latLon is Position position)
        {
            query.TryGetValue("address", out object address);

            var freeFormAddress = address is string sAddress ? sAddress : string.Empty;
            
            preference = preference with { lat = position.lat, lon = position.lon, addressName = freeFormAddress };

            SavePreference(this, new EventArgs { });

            RefreshBindingContext();
        }
    }

    private void RefreshBindingContext()
    {
        BindingContext = preference;
    }

    private async Task RequestResources()
    {
        var enabled = await alarmService.AreNotificationsEnabled();

        if (!enabled)
        {
            enabled = await alarmService.RequestNotificationsEnabled();
        }

        //var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                        
        var statusAlwaysLocation = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
        if (statusAlwaysLocation != PermissionStatus.Granted)
        {
            statusAlwaysLocation = await Permissions.RequestAsync<Permissions.LocationAlways>();
        }
    }

    private async void SetLocationImageButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("findloc");
    }

    private async void sysNotifications_Toggled(object sender, ToggledEventArgs e)
    {
        if (preference is null) return;

        if (preference.AreNotificationEnabled == e.Value) return;

        if (e.Value)
        {
            var enabled = await alarmService.AreNotificationsEnabled();

            if (!enabled)
            {
                enabled = await alarmService.RequestNotificationsEnabled();
            }

            message.ShowMessage($"Notification granted : {enabled}");
        }
    }

    private void notificationOn_Toggled(object sender, ToggledEventArgs e)
    {
        preference = preferenceService.GetCommonPreference();

        if (preference.notificationsOn && preference.notificationsOn == e.Value) return;

        preference = preference with { notificationsOn = e.Value };

        if (e.Value)
        {
            var model = new AlarmSchedule(DateTime.Now.AddSeconds(NOTIF_SCHEDULE_DELAY));

            alarmService.ScheduleAlarm(model, true);

            message.ShowMessage("Notifikace zapnuty");
        }
        else
        {
            alarmService.CancelUpcommingIntent();

            message.ShowMessage("Notifikace vypnuty");
        }

        SavePreference(this, new EventArgs { });
    }

    private void useCurrLocation_Toggled(object sender, ToggledEventArgs e)
    {
        if (preference is null) return;

        preference = preference with { isTrackingPosition = e.Value };

        SavePreference(this, new EventArgs { });
    }

    private void TimePicker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (preference is null) return;

        if (sender is TimePicker timePicker)
        {
            var clsId = timePicker.ClassId;

            if (clsId == "1") preference = preference with { notificationFrom = timePicker.Time };
            else if (clsId == "2") preference = preference with { notificationTo = timePicker.Time };
            else if (clsId == "3") preference = preference with { notificationFromWeekend = timePicker.Time };
            else if (clsId == "4") preference = preference with { notificationToWeekend = timePicker.Time };
        }

        SavePreference(this, new EventArgs { });
    }

    private void SavePreference(object sender, EventArgs e)
    {
        preferenceService.SaveCommonPreference(preference);
    }

    private async void Geofencing_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("geofence");
    }

    private void ImageButton_Clicked(object sender, EventArgs e)
    {
        var canSchedule = alarmService.CanScheduleExactAlarms();

        message.ShowMessage($"Can schedule exact : {canSchedule}");
    }

    private async void windScore_Toggled(object sender, ToggledEventArgs e)
    {
        if (windScoreMotionPreference is null) return;

        if (windScoreMotionPreference.currentlyActive == e.Value) return;

        if (e.Value)
        {
            var result = await this.ShowPopupAsync(new WindScoreInputPopup());

            if (result is int intervalInMinutes)
            {
                var now = DateTime.Now;

                alarmService.RegisterWindScoreService(new(now.AddMinutes(intervalInMinutes)), intervalInMinutes);

                var newPref = windScoreMotionPreference with { currentlyActive = true, lastActivatedAt = now, interval = intervalInMinutes, passedPoints = new() };

                preferenceService.SaveWindScoreMotionPreference(newPref);

                windScoreMotionPreference = newPref;
            }
            else message.ShowMessage("Unknown interval");
        }
        else
        {
            alarmService.UnRegisterWindScoreService();
            var newPref = windScoreMotionPreference with { currentlyActive = false };
            preferenceService.SaveWindScoreMotionPreference(newPref);
            windScoreMotionPreference = newPref;
        }
    }
}