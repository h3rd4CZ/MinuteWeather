using MauiNotifications.Model;
using MauiNotifications.Services;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using System.Windows.Input;
#if ANDROID
using Plugin.LocalNotification.Platforms;
#endif
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace MauiNotifications;

public partial class MainPage : ContentPage
{
    int count = 0;
    IServicesTest Services;
    private readonly UserFileDataService userFileDataService;
    private readonly IMessage message;
    private readonly INotificationService notificationService;
    private readonly IAlarmService alarmService;
    private readonly LocationService locationService;
    private readonly ILogger logger;
    private Timer timer;

    public ICommand completed;

    public string MessageToast { get; set; }
    public int CounterSnack { get; set; }
    public MainPage(IServicesTest Services_,
                    UserFileDataService userFileDataService,
                    IMessage message,
                    INotificationService notificationService,
                    IAlarmService alarmService,
                    LocationService locationService)
    {
        InitializeComponent();
        Services = Services_;
        this.userFileDataService = userFileDataService;
        this.message = message;
        this.notificationService = notificationService;
        this.alarmService = alarmService;
        this.locationService = locationService;
        var file = FileSystem.AppDataDirectory;
    }


    private void Button_Clicked_AlarmCancel(object sender, EventArgs e)
    {
        alarmService.CancelUpcommingIntent();
    }
    
    private void Button_Clicked_Alarm(object sender, EventArgs e)
    {
        var model = new AlarmSchedule(DateTime.Now.AddSeconds(5));

        alarmService.ScheduleAlarm(model);
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        count++;

        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);

        timer = new Timer(o =>
        {
            var now = DateTime.Now;
            Services.Start(now);
        }, default, 1000, 30000);
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        timer.Dispose();

        Services.Stop();
    }

    private void Button_Clicked_1(object sender, EventArgs e)
    {
        var canScheduleExactAlarms = alarmService.CanScheduleExactAlarms();

        message.ShowMessage($"Can schedule exact alarms : {canScheduleExactAlarms}");
    }

    private async void Button_Clicked_3(object sender, EventArgs e)
    {
        //var status = await CheckAndRequestLocationPermission();

        //permLbl.Text = status.ToString();

        await userFileDataService.WriteDataAsync("Permission requested");

        var enabled = await alarmService.AreNotificationsEnabled();
      
        if(!enabled)
        {
            enabled = await alarmService.RequestNotificationsEnabled(); 
        }

        if (enabled)
        {
            permLbl.Text = "Granted";
        }
        else
        {
            permLbl.Text = "NotGranted";
        }
    }

    public async Task<PermissionStatus> CheckAndRequestLocationPermission()
    {
        PermissionStatus status = await CheckStatusAsync<Camera>();
                
        if (status == PermissionStatus.Granted)
            return status;

        if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
        {
            // Prompt the user to turn on in settings
            // On iOS once a permission has been denied it may not be requested again from the application
            return status;
        }

        if (ShouldShowRationale<Camera>())
        {
            // Prompt the user with additional information as to why the permission is needed
        }

        status = await RequestAsync<Camera>();

        return status;
    }

    private async void Button_Clicked_2(object sender, EventArgs e)
    {

        string loc = default;

        try
        {

            LocLbl.Text = "Waiting...";
            locBtn.IsEnabled = false;

            Location location = await locationService.GetCurrentLocation();
                        
            locBtn.IsEnabled = true;

            if (location != null)
                loc = $"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}";
            else loc = "None";
        }
        catch (FeatureNotSupportedException fnsEx)
        {
            // Handle not supported on device exception
        }
        catch (FeatureNotEnabledException fneEx)
        {
            // Handle not enabled on device exception
        }
        catch (PermissionException pEx)
        {
            // Handle permission exception
        }
        catch (Exception ex)
        {
            // Unable to get location
        }

        LocLbl.Text = loc;
    }

    private async void ToolbarItem_Clicked(object sender, EventArgs e)
    {
        if (sender is ToolbarItem toolbarItem)
        {
            var task = toolbarItem.ClassId switch
            {
                "1" => Shell.Current.GoToAsync("debug"),
                "2" => Shell.Current.GoToAsync("loc"),
                "3" => Shell.Current.GoToAsync("weather"),
                "4" => Shell.Current.GoToAsync("weatherdaily"),
                _ => throw new InvalidOperationException("Unknown class ID")
            };

            await task;
        }
    }
}

