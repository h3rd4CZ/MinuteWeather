using MauiNotifications.Model;
using MauiNotifications.Model.WeatherForecastHourly;
using System.Text.Json;

namespace MauiNotifications.Services
{
    public class PreferenceService
    {
        private const string WINDSCOREMOTION_PREFERENCE_KEY = "windscoremotion";
        private const string GEOFENCE_PREFERENCE_KEY = "geofence";
        private const string LOCATION_PREFERENCE_KEY = "location";

        private readonly LocationService locationService;
        private readonly GeoLocationService geoLocationService;

        public PreferenceService(LocationService locationService, GeoLocationService geoLocationService)
        {
            this.locationService = locationService;
            this.geoLocationService = geoLocationService;
        }

        public void SaveWindScoreMotionPreference(WindScoreMotionPreference windScoreMotionPreference)
        {
            var preference = JsonSerializer.Serialize(windScoreMotionPreference);

            Preferences.Default.Set(WINDSCOREMOTION_PREFERENCE_KEY, preference);
        }

        public WindScoreMotionPreference GetWindScoreMotionPreference()
        {
            var preference = Preferences.Default.Get(WINDSCOREMOTION_PREFERENCE_KEY, string.Empty);

            return string.IsNullOrWhiteSpace(preference)
                ? new WindScoreMotionPreference(false, 1, default, new(), new(default, default))
                : JsonSerializer.Deserialize<WindScoreMotionPreference>(preference);
        }

        public void SaveGeofencePreference(GeofencePreference geofencePreference)
        {
            var preference = JsonSerializer.Serialize(geofencePreference);

            Preferences.Default.Set(GEOFENCE_PREFERENCE_KEY, preference);
        }

        public GeofencePreference GetGeofencePreference()
        {
            var preference = Preferences.Default.Get(GEOFENCE_PREFERENCE_KEY, string.Empty);

            if (string.IsNullOrWhiteSpace(preference))
            {
                return new GeofencePreference(
                    false,
                    default,
                    new()
                    );
            }
            else
            {
                return JsonSerializer.Deserialize<GeofencePreference>(preference);
            }
        }

        public void SaveCommonPreference(LocationPreference locationPreference)
        {
            var preference = JsonSerializer.Serialize(locationPreference);

            Preferences.Default.Set(LOCATION_PREFERENCE_KEY, preference);
        }

        public LocationPreference GetCommonPreference()
        {
            var preference = Preferences.Default.Get(LOCATION_PREFERENCE_KEY, string.Empty);

            if (string.IsNullOrWhiteSpace(preference))
            {
                return new LocationPreference(
                    0,
                    0,
                    string.Empty,
                    true,
                    false,
                    DateTime.MinValue,
                    TimeSpan.Parse("08:00"),
                    TimeSpan.Parse("16:00"),
                    TimeSpan.Parse("10:00"),
                    TimeSpan.Parse("22:00"),
                    new());
            }
            else
            {
                return JsonSerializer.Deserialize<LocationPreference>(preference);
            }
        }

        public async Task<Position> GetPreferenceLocation(int timeout = 10)
        {
            var preference = GetCommonPreference();

            var geofencePreference = GetGeofencePreference();

            if (preference.isTrackingPosition)
            {
                var loc = await locationService.GetCurrentLocation(timeout);

                if (loc is null) throw new InvalidOperationException("Current device location unavailable");

                var position = new Position();

                position.lat = (float)loc.Latitude;
                position.lon = (float)loc.Longitude;
                var addressName = await geoLocationService.Reverse(position);
                position.address = addressName?.addresses?.FirstOrDefault()?.address?.municipality ?? "?";

                return position;
            }
            else if (geofencePreference.active && geofencePreference.currentPlace > 0)
            {
                var currentGeofencePlace = geofencePreference.places.FirstOrDefault(p => p.Id == geofencePreference.currentPlace);

                if (currentGeofencePlace is null) throw new InvalidOperationException("Geofence current place was not found");

                return new Position
                {
                    lat = currentGeofencePlace.Lat,
                    lon = currentGeofencePlace.Lon,
                    address = currentGeofencePlace.AddressName
                };
            }
            else
            {
                return new Position
                {
                    lat = preference.lat,
                    lon = preference.lon,
                    address = preference.addressName
                };
            }
        }
    }
}
