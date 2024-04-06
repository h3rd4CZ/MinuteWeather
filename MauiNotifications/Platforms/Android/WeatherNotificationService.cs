using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using MauiNotifications.Model;
using MauiNotifications.Model.Weather;
using MauiNotifications.Services;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;

namespace MauiNotifications.Platforms.Android
{

    [Service]
    public class WeatherNotificationService : Service
    {

        private ILogger<AlarmSchedulerReceiver> logger;
        private UserFileDataService debugService;
        private INotificationService notificationService;
        private WeatherForecastService weatherForecastService;
        private PreferenceService preferenceService;
        private IAlarmService alarmService;

        private const int NOTIF_ID = 9999;
        public const int RETRYAFTERINMINUTES = 5;
        private const int ENDEVENT_SAFE_INTERVAL_INMINUTES = 5;
        private const int STARTEVENT_SAFE_INTERVAL_INMINUTES = 5;
        private const int UPCOMMINGINTERVAL_SAFE_WINDOW_TO_CLARIFICATION = 15;
        private const int MAXTIMETONEXTSCHEDULEINMINUTES = 80;

        private const int ALERTCHECKING_FORENOON_HOUR_FROM = 9;
        private const int ALERTCHECKING_FORENOON_HOUR_TO = 10;

        private const int ALERTCHECKING_AFTERNOON_HOUR_FROM = 15;
        private const int ALERTCHECKING_AFTERNOON_HOUR_TO = 16;

        public override IBinder OnBind(Intent intent) => default;

        public WeatherNotificationService()
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
                    logger.LogInformation($"Job service run at : {DateTime.Now}");

                    var wellcomeExtra = intent?.GetStringExtra(MauiNotifications.AlarmService.WELLCOME_PLACE_EXTRA);

                    await ShowNotificationAndScheduleNext(NOTIF_ID, wellcomeExtra);

                    Alerts(this);

                }
                catch (Exception ex)
                {
                    try
                    {
                        await debugService.WriteDataAsync($"ERR when processing job service handler : {ex}");
                    }
                    catch { }

                    logger.LogError(ex, "An error occured when processing alarm receiver");

                    await ScheduleRetry();
                }
                finally
                {
                    StopSelf();
                }
            });

            return StartCommandResult.NotSticky;
        }

        private void Alerts(Context context)
        {
            var now = DateTime.Now;

            var isForenoon = now.Hour >= ALERTCHECKING_FORENOON_HOUR_FROM && now.Hour <= ALERTCHECKING_FORENOON_HOUR_TO;
            var isAfternoon = !isForenoon && now.Hour >= ALERTCHECKING_AFTERNOON_HOUR_FROM && now.Hour <= ALERTCHECKING_AFTERNOON_HOUR_TO;

            bool isTimeForAlerts = isForenoon || isAfternoon;

            if (isTimeForAlerts)
            {
                var notificationAlertServiceIntent = new Intent(context, typeof(AlertNotificationService));

                context.StartService(notificationAlertServiceIntent);
            }
        }

        private async Task ScheduleRetry()
        {
            try
            {
                var preference = preferenceService.GetCommonPreference();

                var nextTry = DateTime.Now.AddMinutes(RETRYAFTERINMINUTES);

                if (CanSchedule(nextTry, preference, out DateTime nextRescheduleTime))
                {
                    var pref = preference = preference with { nextScheduleTime = nextTry };
                    preferenceService.SaveCommonPreference(pref);

                    alarmService.ScheduleAlarm(new AlarmSchedule(nextTry));

                    await debugService.WriteDataAsync($"Weather notification schedule was rescheduled to : {nextTry}");
                }
                else
                {
                    var pref = preference = preference with { nextScheduleTime = nextRescheduleTime };
                    preferenceService.SaveCommonPreference(pref);

                    alarmService.ScheduleAlarm(new AlarmSchedule(nextRescheduleTime));

                    await debugService.WriteDataAsync($"Weather notification schedule was rescheduled to : {nextRescheduleTime}");
                }
            }
            catch (Exception ex)
            {
                try
                {
                    await debugService.WriteDataAsync($"An error occured when retriyng notification schedule : {ex}");
                }
                catch { }
            }
        }

        private bool ScheduleNext(Intervalsummary[] intervalSummaries, LocationPreference preference)
        {
            var intervalToSchedule = intervalSummaries.First();
            var currDtm = DateTime.Now;

            var isOverlaping = false;

            var overlapingInterval = intervalSummaries.FirstOrDefault(i => i != intervalToSchedule && i.startMinute <= intervalToSchedule.endMinute);

            if (overlapingInterval is not null)
            {
                isOverlaping = true;
                intervalToSchedule = overlapingInterval;
            }

            var totalMinutesToNextSchedule = isOverlaping
                ? intervalToSchedule.startMinute
                : intervalToSchedule.endMinute;

            totalMinutesToNextSchedule = Math.Min(MAXTIMETONEXTSCHEDULEINMINUTES, totalMinutesToNextSchedule);

            var interesetTime = currDtm.AddMinutes(totalMinutesToNextSchedule);

            if (interesetTime.Subtract(currDtm).TotalMinutes >= UPCOMMINGINTERVAL_SAFE_WINDOW_TO_CLARIFICATION)
            {
                interesetTime = interesetTime.AddMinutes(-ENDEVENT_SAFE_INTERVAL_INMINUTES);
            }
            else
            {
                interesetTime = interesetTime.AddMinutes(STARTEVENT_SAFE_INTERVAL_INMINUTES);
            }

            if (CanSchedule(interesetTime, preference, out DateTime rescheduleDateTime))
            {
                var pref = preference = preference with { nextScheduleTime = interesetTime };
                preferenceService.SaveCommonPreference(pref);

                return alarmService.ScheduleAlarm(new AlarmSchedule(interesetTime));
            }
            else
            {
                var pref = preference with { nextScheduleTime = rescheduleDateTime };
                preferenceService.SaveCommonPreference(pref);

                return alarmService.ScheduleAlarm(new AlarmSchedule(rescheduleDateTime));
            }
        }

        private async Task ShowNotificationAndScheduleNext(int notifId, string wellcomePlace = default)
        {
            var location = await preferenceService.GetPreferenceLocation(30);

            var preference = preferenceService.GetCommonPreference();

            var forecast = await weatherForecastService.GetMinuteForecast(location, 30);

            if (forecast is null) throw new InvalidOperationException("Forecast is null");

            if (forecast?.summary is not null)
            {
                var nextSummaryWindow = forecast.intervalSummaries?.FirstOrDefault();

                var expectedPrecip = nextSummaryWindow is not null ? forecast.ForecastPrecipInMmForInterval(nextSummaryWindow.startMinute, nextSummaryWindow.endMinute) : default;

                int timeWindow = default;

                if (expectedPrecip == default && forecast.intervalSummaries.Count() > 1)
                {
                    nextSummaryWindow = forecast.intervalSummaries[1];

                    expectedPrecip
                        = nextSummaryWindow is not null
                        ? forecast.ForecastPrecipInMmForInterval(nextSummaryWindow.startMinute, nextSummaryWindow.endMinute)
                        : default;

                    timeWindow = expectedPrecip > 0
                        ? nextSummaryWindow.endMinute - nextSummaryWindow.startMinute
                        : default;
                }

                await ShowNotification(forecast.summary, notifId, location, expectedPrecip, timeWindow, wellcomePlace);
            }

            else throw new InvalidOperationException("No forecast sumary found");

            if (forecast?.intervalSummaries?.Count() > 0)
            {
                var scheduled = ScheduleNext(forecast.intervalSummaries, preference);

                if (!scheduled) throw new InvalidOperationException("New alarm intent wasn´t scheduled");
            }
            else throw new InvalidOperationException("No interval summaries found");
        }

        private async Task ShowNotification(Model.Weather.Summary weatherForecastSummary, int notifId, Position position, double expectedPrecip, int timeWindowDur = default, string wellcomePlace = default)
        {
            var expectedPrecipText = expectedPrecip > 0 ? $" Očekávané srážky {expectedPrecip} mm." : string.Empty;

            var expectedPrecipWindowText = timeWindowDur > 0 ? $" (Trvání {timeWindowDur} min.)" : string.Empty;

            var descr = $"{weatherForecastSummary.longPhrase}.{expectedPrecipText}{expectedPrecipWindowText}";

            var request = new NotificationRequest
            {
                NotificationId = notifId,
                Title = string.IsNullOrWhiteSpace(wellcomePlace) 
                    ? "Nová informace o počasí" 
                    : $"Vítejte v {wellcomePlace}",
                Subtitle = string.IsNullOrWhiteSpace(wellcomePlace) ? position.address : string.Empty,
                Description = descr,
                CategoryType = NotificationCategoryType.Status,
                Android = new AndroidOptions
                {
                    IconSmallName = new AndroidIcon("notificonsmall"),
                    IconLargeName = new AndroidIcon($"w{weatherForecastSummary.iconCode}")
                }
            };

            await notificationService.Show(request);
        }

        public static bool CanSchedule(DateTime nextScheduleTime, LocationPreference preference, out DateTime nextReScheduleTime)
        {
            var isWeekend = nextScheduleTime.DayOfWeek == DayOfWeek.Sunday || nextScheduleTime.DayOfWeek == DayOfWeek.Saturday;

            var allowedFrom = ToDateTime(nextScheduleTime, isWeekend ? preference.notificationFromWeekend : preference.notificationFrom);
            var allowedTo = ToDateTime(nextScheduleTime, isWeekend ? preference.notificationToWeekend : preference.notificationTo);

            var canSchedule = nextScheduleTime >= allowedFrom && nextScheduleTime <= allowedTo;

            if (!canSchedule)
            {
                var isUnder = nextScheduleTime < allowedFrom;
                if (isUnder)
                {
                    nextReScheduleTime = allowedFrom;
                }
                else
                {
                    var nextDay = nextScheduleTime.AddDays(1);
                    var isNextDayWeekend = nextDay.DayOfWeek == DayOfWeek.Sunday || nextDay.DayOfWeek == DayOfWeek.Saturday;

                    nextReScheduleTime = isNextDayWeekend ? ToDateTime(nextDay, preference.notificationFromWeekend) : ToDateTime(nextDay, preference.notificationFrom);
                }

                return false;
            }

            nextReScheduleTime = DateTime.MinValue;

            return true;
        }

        private void BuildServices()
        {
            logger = ResolveService<ILogger<AlarmSchedulerReceiver>>();

            debugService = ResolveService<UserFileDataService>();

            notificationService = ResolveService<INotificationService>();

            weatherForecastService = ResolveService<WeatherForecastService>();

            preferenceService = ResolveService<PreferenceService>();

            alarmService = ResolveService<IAlarmService>();
        }

        T ResolveService<T>() => MauiProgram.Services.GetRequiredService<T>();

        static DateTime ToDateTime(DateTime date, TimeSpan span) => new DateTime(date.Year, date.Month, date.Day, span.Hours, span.Minutes, span.Seconds);
    }
}
