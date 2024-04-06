using Android.App;
using Android.App.Job;
using Android.Content;
using MauiNotifications.Model;
using MauiNotifications.Platforms.Android;
using MauiNotifications.Services;
using Microsoft.Extensions.Logging;

namespace MauiNotifications.Platforms
{
    [BroadcastReceiver(
        Name = ReceiverName,
        Enabled = true,
        Exported = false,
        Label = "Test alarm service sheduler")]
    [IntentFilter(
        new[]
        {
            Intent.ActionBootCompleted,
            Intent.ActionMyPackageReplaced,
            "android.intent.action.QUICKBOOT_POWERON",
            "com.htc.intent.action.QUICKBOOT_POWERON"
        },
        Categories = new[]
        {
            Intent.CategoryHome
        })]
    internal class AlarmSchedulerReceiver : BroadcastReceiver
    {
        /// <summary>
        ///
        /// </summary>
        public const string ReceiverName = "plugin.LocalNotification." + nameof(AlarmSchedulerReceiver);
                        
        private ILogger<AlarmSchedulerReceiver> logger;
        private UserFileDataService debugService;
        private PreferenceService preferenceService;
        private IAlarmService alarmService;
                
        public override async void OnReceive(Context context, Intent intent)
        {
            try
            {
                BuildServices();

                var actionName = intent.Action;

                if (actionName == Intent.ActionBootCompleted ||
                    actionName == Intent.ActionMyPackageReplaced ||
                    actionName == "android.intent.action.QUICKBOOT_POWERON" ||
                    actionName == "com.htc.intent.action.QUICKBOOT_POWERON")
                { 
                    if(!actionName.Equals(Intent.ActionMyPackageReplaced))
                    {
                        await debugService.WriteDataAsync("Device rebooted");

                        var preference = preferenceService.GetCommonPreference();
                        var geofencePreference = preferenceService.GetGeofencePreference();

                        if(preference.notificationsOn)
                        {
                            alarmService.ScheduleAlarm(new AlarmSchedule(DateTime.Now.AddMinutes(1)), true);

                            await debugService.WriteDataAsync("Weather alarm scheduler successfully scheduled");
                        }
                                                
                        if (geofencePreference.places.Count > 0)
                        {
                            foreach (var geofencePlace in geofencePreference.places) alarmService.RegisterGeofence(geofencePlace);

                            await debugService.WriteDataAsync("Geofence places successfully registered");
                        }
                    }
                    else 
                    {
                        await debugService.WriteDataAsync("Package replaced");
                    }
                }
                else
                {
                    if (!actionName.Equals(AlarmService.AlarmActionIntent)) throw new InvalidOperationException("Unknow action intent");

                    logger.LogInformation($"Executing receiver for weather notification service at : {DateTime.Now}");
                                                            
                    var notificationServiceIntent = new Intent(context, typeof(WeatherNotificationService));

                    var wellcomeExtra = intent.GetStringExtra(AlarmService.WELLCOME_PLACE_EXTRA);

                    if (!string.IsNullOrWhiteSpace(wellcomeExtra)) notificationServiceIntent.PutExtra(AlarmService.WELLCOME_PLACE_EXTRA, wellcomeExtra);

                    context.StartService(notificationServiceIntent);

                    //await ShowNotificationAndScheduleNext(NOTIF_ID);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    await debugService.WriteDataAsync(ex.ToString());
                }
                catch { }

                logger.LogError(ex, "An error occured when processing alarm receiver");
            }
            finally
            {
                logger.LogInformation($"Executed receiver for weather notification service at : {DateTime.Now}");
            }
        }
                        

        private void BuildServices()
        {
            logger = ResolveService<ILogger<AlarmSchedulerReceiver>>();

            debugService = ResolveService<UserFileDataService>();
                        
            preferenceService = ResolveService<PreferenceService>();

            alarmService = ResolveService<IAlarmService>();
        }
               
        T ResolveService<T>() => MauiProgram.Services.GetRequiredService<T>();

        static DateTime ToDateTime(DateTime date, TimeSpan span) => new DateTime(date.Year, date.Month, date.Day, span.Hours, span.Minutes, span.Seconds);
    }
}


