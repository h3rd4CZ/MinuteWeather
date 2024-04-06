using Android.App;
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
        Label = "Alarm service watchdog sheduler")]
    internal class AlarmSchedulerWatchDogReceiver : BroadcastReceiver
    {
        public const string ReceiverName = "plugin.LocalNotification." + nameof(AlarmSchedulerWatchDogReceiver);
        private const int NEXTTRYWHENFAIL = 10;
                
        private ILogger<AlarmSchedulerWatchDogReceiver> logger;
        private UserFileDataService debugService;
        private PreferenceService preferenceService;
        private IAlarmService alarmService;
        private WeatherForecastService weatherForecastService;
                
        public override async void OnReceive(Context context, Intent intent)
        {
            try
            {
                BuildServices();
                                
                var actionName = intent.Action;

                if (!actionName.Equals(AlarmService.WatchdogAlarmActionIntent)) throw new InvalidOperationException("Unknow action intent");

                await Watchdog(context);

            }
            catch (Exception ex)
            {
                await debugService.WriteDataAsync($"An error occured when executing watchdog receiver : {ex}");
            }
            finally
            {
                logger.LogInformation($"Executed watchdog receiver for weather notification service at : {DateTime.Now}");
            }
        }

        private async Task Watchdog(Context context)
        {
            var preference = preferenceService.GetCommonPreference();

            if (preference is not null && preference.notificationsOn && preference.nextScheduleTime != default)
            {
                var nextSchedule = preference.nextScheduleTime;

                var delay = 5;

                var now = DateTime.Now;

                if (nextSchedule.AddMinutes(delay * 2) < now)
                {
                    await debugService.WriteDataAsync("Schedule is broken, reschedule needed");

                    var nextRepeatAt = DateTime.Now.AddSeconds(NEXTTRYWHENFAIL);

                    if (WeatherNotificationService.CanSchedule(nextRepeatAt, preference, out DateTime nextRescheduleTime))
                    {
                        alarmService.ScheduleAlarm(new AlarmSchedule(nextRepeatAt));
                    }
                    else
                    {
                        alarmService.ScheduleAlarm(new AlarmSchedule(nextRescheduleTime));
                    }

                    await debugService.WriteDataAsync($"Schedule has been rescheduled to : {(nextRescheduleTime == default ? nextRepeatAt : nextRescheduleTime)}");
                }
            }
        }        

        private void BuildServices()
        {
            logger = ResolveService<ILogger<AlarmSchedulerWatchDogReceiver>>();

            debugService = ResolveService<UserFileDataService>();

            preferenceService = ResolveService<PreferenceService>();

            alarmService = ResolveService<IAlarmService>();
        }

        T ResolveService<T>() => MauiProgram.Services.GetRequiredService<T>();
    }
}


