using Android.Content;
using MauiNotifications.Services;
using Microsoft.Extensions.Logging;

namespace MauiNotifications.Platforms
{
    [BroadcastReceiver(
        Name = ReceiverName,
        Enabled = true,
        Exported = false,
        Label = "WindScore motion scheduler")]
    internal class WindScoreMotionReceiver : BroadcastReceiver
    {
        public const string ReceiverName = "plugin.LocalNotification." + nameof(WindScoreMotionReceiver);
                                
        private ILogger<AlarmSchedulerWatchDogReceiver> logger;
        private UserFileDataService debugService;
        private PreferenceService preferenceService;
        private IAlarmService alarmService;
        
        public override async void OnReceive(Context context, Intent intent)
        {
            int intervalInMinutes = -1;

            try
            {
                BuildServices();

                ArgumentNullException.ThrowIfNull(intent);

                var actionName = intent.Action;
                                
                if (!actionName.Equals(AlarmService.WindScoreMotionAlarmActionIntent)) throw new InvalidOperationException("Unknow action intent");

                intervalInMinutes = intent.GetIntExtra(AlarmService.WINDSCOREMOTION_INTERVAL_EXTRA, -1);

                if (intervalInMinutes == -1) throw new InvalidOperationException("Interval is not valid");
                                
                await WindScoreMotion(context, intervalInMinutes);

            }
            catch (Exception ex)
            {
                await debugService.WriteDataAsync($"An error occured when executing windScore motion receiver : {ex}");

                if (intervalInMinutes > 0)
                {
                    try
                    {
                        await RescheduleNext(intervalInMinutes);
                    }
                    catch(Exception iex)
                    {
                        await debugService.WriteDataAsync($"An error occured when rescheduling wind score motion notification: {iex}");
                    }
                }

            }
            finally
            {
                logger.LogInformation($"Executed wind score motion receiver at : {DateTime.Now}");
            }
        }
                
        private async Task WindScoreMotion(Context context, int intervalInMinutes)
        {
            var preference = preferenceService.GetWindScoreMotionPreference();

            try
            {
                var windScoreServiceIntent = new Intent(context, typeof(WindScoreService));               
                
                context.StartService(windScoreServiceIntent);
            }
            catch (Exception ex)
            {
                await debugService.WriteDataAsync($"WindScoreMotion processing failed : {ex}");
            }
            
            await RescheduleNext(intervalInMinutes, preference);
        }

        private async Task RescheduleNext(int intervalInMinutes, WindScoreMotionPreference windScoreMotionPreference = default)
        {
            var preference = windScoreMotionPreference ?? preferenceService.GetWindScoreMotionPreference();

            if (!preference.currentlyActive)
            {
                await debugService.WriteDataAsync($"Reschedule is not possible, because motion was stopped");

                return;
            }

            var now = DateTime.Now;

            var nextCall = now.AddMinutes(intervalInMinutes);

            alarmService.RegisterWindScoreService(new(nextCall), intervalInMinutes);
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


