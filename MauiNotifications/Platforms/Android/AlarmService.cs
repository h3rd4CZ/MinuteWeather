using Android.App;
using Android.Content;
using Android.Gms.Location;
using Android.OS;
using Java.Lang;
using MauiNotifications.Model;
using MauiNotifications.Platforms;
using MauiNotifications.Platforms.Android;
using MauiNotifications.Services;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using Plugin.LocalNotification.Platforms;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Application = Android.App.Application;

namespace MauiNotifications
{
    public class AlarmService : IAlarmService
    {
        private readonly ILogger logger;

        private readonly AlarmManager MyAlarmManager;
        private readonly NotificationManager NotificationManager;

        protected readonly GeofencingClient MyGeofencingClient;

        public static string AlarmActionIntent = $"{Assembly.GetExecutingAssembly().GetName().Name}_Alarm";
        public static string WatchdogAlarmActionIntent = $"{Assembly.GetExecutingAssembly().GetName().Name}_WatchdogAlarm";
        public static string WindScoreMotionAlarmActionIntent = $"{Assembly.GetExecutingAssembly().GetName().Name}_WindScoreMotion";


        public const int ALARM_INTENT_NOTIFID = 895632147;
        public const int ALARM_WATCHDOGINTENT_NOTIFID = 458965258;
        public const int ALARM_WINDSCORE_NOTIFID = 589632587;

        public const string WEATHER_GEOFENCE_PLACEID = "WeatherGeofence.PlaceId";
        public const string WELLCOME_PLACE_EXTRA = "WellcomePlace";

        public const string WINDSCOREMOTION_INTERVAL_EXTRA = "WindScoreMotionInterval";

        public AlarmService(ILogger<AlarmService> logger)
        {
            if (!OperatingSystem.IsAndroidVersionAtLeast(21))
            {
                return;
            }

            MyAlarmManager = AlarmManager.FromContext(Application.Context);

            NotificationManager = NotificationManager.FromContext(Application.Context);

            MyGeofencingClient = LocationServices.GetGeofencingClient(Application.Context);

            this.logger = logger;
        }

        public bool CanScheduleExactAlarms()
        {
#if MONOANDROID
            if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            {
                return Task.FromResult(true);
            }
#elif ANDROID
            if (OperatingSystem.IsAndroidVersionAtLeast(31))
            {
                return MyAlarmManager.CanScheduleExactAlarms();
            }
#endif

            return false;
        }


        public async Task<bool> AreNotificationsEnabled()
        {
#if MONOANDROID
            if (Build.VERSION.SdkInt < BuildVersionCodes.N)
            {
                return Task.FromResult(true);
            }
#elif ANDROID
            if (!OperatingSystem.IsAndroidVersionAtLeast(24))
            {
                return await Task.FromResult(true);
            }
#endif
            return await Task.FromResult(NotificationManager.AreNotificationsEnabled());
        }

        public async Task<bool> RequestNotificationsEnabled()
        {
#if ANDROID
            if (!OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                return await Task.FromResult(false);
            }

            var status = await Permissions.RequestAsync<NotificationPerms>();

            return status == PermissionStatus.Granted;
#else
            return result == PermissionStatus.Granted;
#endif
        }

        public bool CancelUpcommingIntent()
        {
#if MONOANDROID
            if (Build.VERSION.SdkInt < BuildVersionCodes.Kitkat)
            {
                return false;
            }
#elif ANDROID
            if (!OperatingSystem.IsAndroidVersionAtLeast(21))
            {
                return false;
            }
#endif

            var alarmPendingIntent = CreateAlarmIntent(ALARM_INTENT_NOTIFID, null);
            MyAlarmManager?.Cancel(alarmPendingIntent);
            alarmPendingIntent?.Cancel();

            var watchdogPendingIntent = CreateWatchdogAlarmIntent(ALARM_WATCHDOGINTENT_NOTIFID);
            MyAlarmManager?.Cancel(watchdogPendingIntent);
            watchdogPendingIntent?.Cancel();

            return true;
        }

        public void RegisterWindScoreService(AlarmSchedule nextCall, int intervalInMinutes)
        {
            PendingIntent pendingIntent = GetWindScorePendingIntent(intervalInMinutes);

            var onTime = nextCall.date;

            var utcAlarmTimeInMillis =
                (onTime.ToUniversalTime() - DateTime.UtcNow)
                .TotalMilliseconds;

            var triggerTime = (long)utcAlarmTimeInMillis;

            var alarmType = AlarmType.RtcWakeup;

            var triggerAtTime = GetBaseCurrentTime(alarmType) + triggerTime;

            MyAlarmManager.SetAlarmClock(new AlarmManager.AlarmClockInfo(triggerAtTime, pendingIntent), pendingIntent);
        }
                
        public void UnRegisterWindScoreService()
        {
            PendingIntent pendingIntent = GetWindScorePendingIntent();
            MyAlarmManager?.Cancel(pendingIntent);
            pendingIntent?.Cancel();
        }

        public bool ScheduleAlarm(AlarmSchedule alarm, bool withWatchdog = false, string wellcomePlace = default)
        {
            logger.LogInformation("Entered schedule alarm intent");

            var serializedRequest = JsonSerializer.Serialize(alarm);

            var alarmIntent = CreateAlarmIntent(ALARM_INTENT_NOTIFID, serializedRequest, wellcomePlace);

            var onTime = alarm.date;

            var utcAlarmTimeInMillis =
                (onTime.ToUniversalTime() - DateTime.UtcNow)
                .TotalMilliseconds;

            var triggerTime = (long)utcAlarmTimeInMillis;

            var alarmType = AlarmType.RtcWakeup;

            var triggerAtTime = GetBaseCurrentTime(alarmType) + triggerTime;

            if (
#if MONOANDROID
                Build.VERSION.SdkInt >= BuildVersionCodes.BuildVersionCodes.Lollipop
#elif ANDROID
                OperatingSystem.IsAndroidVersionAtLeast(21)
#endif
            )
            {
                //MyAlarmManager.SetExactAndAllowWhileIdle(alarmType, triggerAtTime, alarmIntent);

                MyAlarmManager.SetAlarmClock(new AlarmManager.AlarmClockInfo(triggerAtTime, alarmIntent), alarmIntent);
            }
            else
            {
                MyAlarmManager.SetExact(alarmType, triggerAtTime, alarmIntent);
            }

            if (withWatchdog) ScheduleWatchdog();

            return true;
        }

        public void RegisterGeofence(GeofencePreferencePlace place)
        {
            var geofenceBuilder = new GeofenceBuilder()
            .SetRequestId(place.Id.ToString())
            .SetExpirationDuration(-1)
            .SetNotificationResponsiveness(120000)
            .SetCircularRegion(
                place.Lat,
                place.Lon,
                Convert.ToSingle(place.Radius)
            );

            var transitionType = 0;

            transitionType |= Geofence.GeofenceTransitionEnter;
            transitionType |= Geofence.GeofenceTransitionExit;

            geofenceBuilder.SetTransitionTypes(transitionType);

            var geofence = geofenceBuilder.Build();

            var geoRequest = new GeofencingRequest.Builder()
                .SetInitialTrigger(GeofencingRequest.InitialTriggerEnter)
                .AddGeofence(geofence)
                .Build();

            PendingIntent pendingIntent = CreateGeoPendingIntent(place);

            MyGeofencingClient
                .AddGeofences(
                    geoRequest,
                    pendingIntent
                );
        }

        public void UnregisterGeofence(GeofencePreferencePlace place)
        {
            var geoPendingIntent = CreateGeoPendingIntent(place);

            MyGeofencingClient?.RemoveGeofences(geoPendingIntent);
        }

        private static PendingIntent GetWindScorePendingIntent(int? intervalInMinutes = default)
        {
            var broadcastIntent = new Intent(WindScoreMotionAlarmActionIntent, default, Application.Context, typeof(WindScoreMotionReceiver));

            if (intervalInMinutes.HasValue)
                broadcastIntent.PutExtra(WINDSCOREMOTION_INTERVAL_EXTRA, intervalInMinutes.Value);

            broadcastIntent.AddFlags(ActivityFlags.SingleTop)
                .AddFlags(ActivityFlags.IncludeStoppedPackages)
                .PutExtra(LocalNotificationCenter.ReturnRequestActionId, ALARM_WINDSCORE_NOTIFID);

            var requestCode = new Random(ALARM_WINDSCORE_NOTIFID).Next();

            var pendingIntent =
                PendingIntent.GetBroadcast(
                    Application.Context,
                    requestCode,
                    broadcastIntent,
                    AndroidPendingIntentFlags.UpdateCurrent.ToNative()
                );
            return pendingIntent;
        }
                          
        protected virtual PendingIntent CreateAlarmIntent(int notificationId, string serializedRequest, string wellcomePlace = default)
        {
            var broadcastIntent = CreateBroadCastIntent();

            if (!string.IsNullOrWhiteSpace(wellcomePlace)) broadcastIntent.PutExtra(WELLCOME_PLACE_EXTRA, wellcomePlace);

            var pendingIntent = CreateActionIntent(serializedRequest, new NotificationAction(notificationId)
            {
                Android =
                {
                    LaunchAppWhenTapped = false,
                    PendingIntentFlags = AndroidPendingIntentFlags.UpdateCurrent
                }
            }, broadcastIntent);

            return pendingIntent;
        }

        protected virtual PendingIntent CreateWatchdogAlarmIntent(int notificationId, AndroidPendingIntentFlags intentFlags = AndroidPendingIntentFlags.UpdateCurrent)
        {
            var pendingIntent = CreateActionIntent(default, new NotificationAction(notificationId)
            {
                Android =
                {
                    LaunchAppWhenTapped = false,
                    PendingIntentFlags = intentFlags
                }
            }, CreateWatchdogBroadCastIntent());

            return pendingIntent;
        }

        protected virtual PendingIntent CreateActionIntent(string serializedRequest, NotificationAction action, Android.Content.Intent broadcastIntent)
        {
            var notificationIntent = action.Android.LaunchAppWhenTapped
                ? (Application.Context.PackageManager?.GetLaunchIntentForPackage(Application.Context.PackageName ??
                                                                              string.Empty))
                : broadcastIntent;

            notificationIntent.AddFlags(ActivityFlags.SingleTop)
                .AddFlags(ActivityFlags.IncludeStoppedPackages)
                .PutExtra(LocalNotificationCenter.ReturnRequestActionId, action.ActionId)
                .PutExtra(LocalNotificationCenter.ReturnRequest, serializedRequest);

            var requestCode = new System.Random(action.ActionId).Next();

            var pendingIntent = action.Android.LaunchAppWhenTapped
                ? PendingIntent.GetActivity(
                    Application.Context,
                    requestCode,
                    notificationIntent,
                    action.Android.PendingIntentFlags.ToNative())
                : PendingIntent.GetBroadcast(
                    Application.Context,
                    requestCode,
                    notificationIntent,
                    action.Android.PendingIntentFlags.ToNative()
                );
            return pendingIntent;
        }

        protected virtual long GetBaseCurrentTime(AlarmType type)
        {
            return type switch
            {
                AlarmType.Rtc => JavaSystem.CurrentTimeMillis(),
                AlarmType.RtcWakeup => JavaSystem.CurrentTimeMillis(),
                AlarmType.ElapsedRealtime => SystemClock.ElapsedRealtime(),
                AlarmType.ElapsedRealtimeWakeup => SystemClock.ElapsedRealtime(),
                _ => throw new NotImplementedException(),
            };
        }

        private void ScheduleWatchdog()
        {
            var watchdogAlarmType = AlarmType.RtcWakeup;

            var watchdogIntent = CreateWatchdogAlarmIntent(ALARM_WATCHDOGINTENT_NOTIFID);

            MyAlarmManager.SetRepeating(watchdogAlarmType, JavaSystem.CurrentTimeMillis(), AlarmManager.IntervalFifteenMinutes, watchdogIntent);
        }

        private static PendingIntent CreateGeoPendingIntent(GeofencePreferencePlace place)
        {
            var requestCode = new Random((int)place.Id).Next();

            var intent = new Intent(Application.Context, typeof(GeofenceSchedulerReceiver));

            intent.AddFlags(ActivityFlags.SingleTop)
                .AddFlags(ActivityFlags.IncludeStoppedPackages)
                .PutExtra(WEATHER_GEOFENCE_PLACEID, place.Id);

            PendingIntentFlags flags = 0;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            {
                flags = PendingIntentFlags.Mutable | PendingIntentFlags.UpdateCurrent;
            }
            else
            {
                flags = PendingIntentFlags.UpdateCurrent;
            }

            var pendingIntent = PendingIntent.GetBroadcast(
                Application.Context,
                requestCode,
                 intent,
                 flags
            );
            return pendingIntent;
        }

        private Android.Content.Intent CreateBroadCastIntent()
            => new Android.Content.Intent(AlarmActionIntent, default, Application.Context, typeof(AlarmSchedulerReceiver));
        private Android.Content.Intent CreateWatchdogBroadCastIntent()
            => new Android.Content.Intent(WatchdogAlarmActionIntent, default, Application.Context, typeof(AlarmSchedulerWatchDogReceiver));
    }
}
