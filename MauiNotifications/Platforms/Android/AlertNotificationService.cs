using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using MauiNotifications.Model.WeatherForecastSevereAlerts;
using MauiNotifications.Services;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;

namespace MauiNotifications.Platforms.Android
{

    [Service]
    public class AlertNotificationService : Service
    {
        private UserFileDataService debugService;
        private INotificationService notificationService;
        private WeatherForecastService weatherForecastService;
        private PreferenceService preferenceService;

        private const int NOTIF_ID = 8888;
                
        public override IBinder OnBind(Intent intent) => default;

        public AlertNotificationService()
        {
            BuildServices();
        }
                
        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            Task.Run(async () =>
            {
                try
                {
                    await CheckForAlerts(NOTIF_ID);
                }
                catch (Exception ex)
                {
                    try
                    {
                        await debugService.WriteDataAsync($"ERR when processing alert job service handler : {ex}");
                    }
                    catch { }
                }
                finally
                {
                    StopSelf();
                }
            });

            return StartCommandResult.NotSticky;
        }

        private async Task CheckForAlerts(int notifId)
        {
            var location = await preferenceService.GetPreferenceLocation(30);

            var preference = preferenceService.GetCommonPreference();

            var alerts = await weatherForecastService.GetSevereAlerts(location);

            if(alerts is not null && alerts.results?.Length > 0)
            {
                var alertResults = alerts.results;

                var currProcessed = preference.processedAlerts ?? new();

                foreach (var alert in alertResults.Where(a => !currProcessed.Contains( a.alertId)))
                {
                    try
                    {
                        await ShowAlertNotification(alert, notifId++);
                    }
                    finally
                    {
                        currProcessed.Add(alert.alertId);
                    }
                }

                preference = preference with { processedAlerts = currProcessed };

                preferenceService.SaveCommonPreference(preference);
            }
        }

        private async Task ShowAlertNotification(AlertResult weatherForecastSummary, int notifId)
        {
            var descr = weatherForecastSummary.ToString();

            var scheduleAt = DateTime.Now.AddMinutes(1);

            var request = new NotificationRequest
            {
                Schedule = new NotificationRequestSchedule { NotifyTime = scheduleAt },
                NotificationId = notifId,
                Title = "Nová informace o počasí",
                Subtitle = "Varování",
                Description = descr,
                CategoryType = NotificationCategoryType.Status,
                Android = new AndroidOptions
                {
                    IconSmallName = new AndroidIcon("notificonalert"),
                    IconLargeName = new AndroidIcon($"notificonalertbig")
                }
            };
                                    
            await notificationService.Show(request);
        }
                
        private void BuildServices()
        {
            debugService = ResolveService<UserFileDataService>();

            notificationService = ResolveService<INotificationService>();

            weatherForecastService = ResolveService<WeatherForecastService>();

            preferenceService = ResolveService<PreferenceService>();
        }

        T ResolveService<T>() => MauiProgram.Services.GetRequiredService<T>();
    }
}
