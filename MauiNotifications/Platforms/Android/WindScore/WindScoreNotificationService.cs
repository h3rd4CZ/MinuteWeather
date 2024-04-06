using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using MauiNotifications.Model;
using MauiNotifications.Model.WeatherCurrent;
using MauiNotifications.Services;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;

namespace MauiNotifications
{
    [Service]
    public class WindScoreService : Service
    {
        const int LASTKNOWNWIND_VALIDITY_INIMINUTES = 60;

        private const int WS_NOTIF_ID = 47896;
        private const string WS_CHANNEL_ID = "WindScoreChannel";
        public const string WS_INTERVAL_PARAM = "WS_INTERVAL";
        public const string WINDSCORE_NOTIF_TITLE = "Minute Weather - WindScore©";

        private readonly WeatherForecastService weatherForecastService;
        private readonly LocationService locationService;
        private readonly UserFileDataService userFileDataService;
        private readonly PreferenceService preferenceService;
                
        public static bool ServiceIsRunning { get; private set; }

        public WindScoreService()
        {
            weatherForecastService = MauiProgram.Services.GetRequiredService<WeatherForecastService>();

            locationService = MauiProgram.Services.GetRequiredService<LocationService>();

            userFileDataService = MauiProgram.Services.GetRequiredService<UserFileDataService>();

            preferenceService = MauiProgram.Services.GetRequiredService<PreferenceService>();
        }

        public override IBinder OnBind(Intent intent) => default;


        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            if (intent is null)
            {
                userFileDataService.WriteData($"Intent is null when OnStartCommand called");

                return StartCommandResult.Sticky;
            }

            var intervalInSeconds = intent.GetIntExtra(WS_INTERVAL_PARAM, -1);

            Task.Run(async () =>
            {
                await PushWindScore();
            });

            return StartCommandResult.Sticky;
        }

        public override void OnCreate()
        {
            try
            {
                base.OnCreate();

                //RegisterForeground();
            }
            catch (System.Exception ex)
            {
                userFileDataService.WriteData($"ERR WindScore OnCreate : {ex}");
            }
        }

        public override void OnDestroy()
        {
            try
            {
                base.OnDestroy();

                ServiceIsRunning = false;
            }
            catch (Exception ex)
            {
                userFileDataService.WriteData($"ERR WindScore OnDestroy : {ex}");
            }
        }

        private async Task PushWindScore()
        {
            var score = await ProcessWindScore();

            if (score.HasValue)
            {
                var scoreValue = score.Value;

                var request = new NotificationRequest
                {
                    NotificationId = WS_NOTIF_ID,
                    Title = scoreValue.ToString(),
                    Description = WINDSCORE_NOTIF_TITLE,
                    CategoryType = NotificationCategoryType.Status,
                    Android = new AndroidOptions
                    {
                        IconSmallName = new AndroidIcon("windscore_small"),
                        IconLargeName = new AndroidIcon("windscore_big")
                    }
                };

                await LocalNotificationCenter.Current.Show(request);
            }
        }

        private async Task<decimal?> ProcessWindScore()
        {
            var motionPreference = preferenceService.GetWindScoreMotionPreference();

            WindScoreMotionPass newPassedPoint = default;

            try
            {
                var position = await locationService.GetCurrentLocation(10);

                if (position == default) throw new InvalidOperationException("Location has not arrived");

                newPassedPoint = new WindScoreMotionPass()
                {
                    Position = new Position { lat = (float)position.Latitude, lon = (float)position.Longitude },
                    DateTime = DateTime.Now
                };

                double course = default;
                                
                var lastKnownLocation
                    = motionPreference.passedPoints?.LastOrDefault()?.Position ?? new Position { lat = (float)position.Latitude, lon = (float)position.Longitude };

                if(position.Course.HasValue)
                {
                    await userFileDataService.WriteDataAsync($"WS: Position has valid course : {position.Course.Value}");
                }

                course = position.Course.HasValue ? position.Course.Value : Services.LocationService.GetTrueNorthBearing(
                    lastKnownLocation,
                    new Position { lat = (float)position.Latitude, lon = (float)position.Longitude }
                );
                                
                newPassedPoint.Course = course;

                var loc = new Position() { lat = (float)position.Latitude, lon = (float)position.Longitude };

                var lastKnownWind = await GetLastKnownWind(loc, motionPreference);

                if (lastKnownWind is not null)
                {
                    newPassedPoint.WindDir = lastKnownWind.direction.degrees;
                    newPassedPoint.WindSpeed = lastKnownWind.speed.value;

                    var score = WindScoreProcessor.ComputeScore((decimal)lastKnownWind.direction.degrees, (decimal)course, (decimal)lastKnownWind.speed.value);

                    newPassedPoint.Score = score;

                    return score;
                }
                else throw new InvalidOperationException("No last known wind found");

            }
            catch (Exception ex)
            {
                await userFileDataService.WriteDataAsync($"WS:ERR at : {DateTime.Now} : {ex}");

                return null;
            }
            finally
            {
                if (newPassedPoint is not null)
                {
                    motionPreference.passedPoints.Add(newPassedPoint);

                    preferenceService.SaveWindScoreMotionPreference(motionPreference);
                }
            }
        }

        private async Task<Wind> GetLastKnownWind(Position position, WindScoreMotionPreference preference)
        {
            var preferenceWind = preference?.lastKnownWind;
            var windStamp = preferenceWind?.knownAt;

            var now = DateTime.Now;

            if(windStamp is null || windStamp.Value.AddMinutes(LASTKNOWNWIND_VALIDITY_INIMINUTES) < now)
            {
                try
                {
                    var newWind = await weatherForecastService.GetCurrentForecast(position);

                    var currentWeatherResults = newWind?.results;

                    if (currentWeatherResults is not null && currentWeatherResults.Length > 0 && currentWeatherResults[0].wind is not null)
                    {
                        var lastWind = currentWeatherResults[0].wind;

                        preference.lastKnownWind = new(now, lastWind);

                        return lastWind;
                    }
                    else return default;
                }
                catch(Exception ex)
                {
                    await userFileDataService.WriteDataAsync($"Updating wind failed : {ex}");
                }

                return preferenceWind?.wind;
            }
            else
            {
                return preferenceWind?.wind;
            }
        }
    }
}

