using Android.Content;
using Android.Gms.Location;
using MauiNotifications.Services;

namespace MauiNotifications.Platforms.Android
{
    /// <summary>
    /// 
    /// </summary>
    [BroadcastReceiver(
        Name = ReceiverName,
        Enabled = true,
        Exported = false,
        Label = "Weather geofence place transition receiver detector"
    )]
    public class GeofenceSchedulerReceiver : BroadcastReceiver
    {
        /// <summary>
        ///
        /// </summary>
        public const string ReceiverName = "MauiNotifications.Platforms.Android." + nameof(GeofenceSchedulerReceiver);

        private UserFileDataService debugService;
        private PreferenceService preferenceService;
        private IAlarmService alarmService;

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <param name="intent"></param>
        public override async void OnReceive(Context context, Intent intent)
        {
            try
            {
                BuildServices();

                var now = DateTime.Now;

                var placeId = intent.GetLongExtra(AlarmService.WEATHER_GEOFENCE_PLACEID, -1);

                if (placeId == -1) throw new InvalidOperationException("No valid place id included in intent");

                var geofencePreference = preferenceService.GetGeofencePreference();

                var geofencePlace = geofencePreference.places?.FirstOrDefault(p => p.Id == placeId);

                if (geofencePlace is null) throw new InvalidOperationException($"Geofence place with id : {placeId} was not found");

                GeofencingEvent geofencingEvent = GeofencingEvent.FromIntent(intent);

                if (geofencingEvent is null || geofencingEvent.HasError)
                {
                    throw new InvalidOperationException($"Geofencing event has errors : {GeofenceStatusCodes.GetStatusCodeString(geofencingEvent.ErrorCode)}");
                }

                IList<IGeofence> geofenceList = geofencingEvent.TriggeringGeofences;

                int? transition = geofencingEvent.GeofenceTransition;

                if (!transition.HasValue) throw new InvalidOperationException("Geofence transition does not have a value");

                if (geofenceList is null) throw new InvalidOperationException("Empty geofence list");

                var enter = false;

                foreach (var geofence in geofenceList)
                {
                    if (transition == Geofence.GeofenceTransitionEnter)
                    {
                        geofencePlace.EnterAt = now;
                        geofencePreference.currentPlace = (uint)placeId;

                        enter = true;
                    }

                    if (transition == Geofence.GeofenceTransitionExit)
                    {
                        geofencePlace.LeftAt = now;

                        if (geofencePreference.currentPlace == (uint)placeId)
                        {
                            geofencePreference.currentPlace = 0;
                        }
                    }
                }

                preferenceService.SaveGeofencePreference(geofencePreference);

                if (geofencePreference.active && enter) WellcomePlace(geofencePlace.AddressName);
            }
            catch (Exception ex)
            {
                await debugService.WriteDataAsync($"An error occured when handling geofence receiver : {ex}");
            }
        }

        private void WellcomePlace(string addressName)
            => alarmService.ScheduleAlarm(new(DateTime.Now.AddSeconds(10)), wellcomePlace: addressName);

        private void BuildServices()
        {
            debugService = ResolveService<UserFileDataService>();

            preferenceService = ResolveService<PreferenceService>();

            alarmService = ResolveService<IAlarmService>();
        }

        T ResolveService<T>() => MauiProgram.Services.GetRequiredService<T>();
    }
}
