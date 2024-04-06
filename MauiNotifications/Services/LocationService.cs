using MauiNotifications.Model;

using static System.Math;

namespace MauiNotifications.Services
{
    public class LocationService
    {
        private readonly UserFileDataService userFileDataService;

        public static double GetTrueNorthBearing(Position from, Position to)
        {
            ArgumentNullException.ThrowIfNull(nameof(from));
            ArgumentNullException.ThrowIfNull(nameof(to));

            var dl = to.lon - from.lon;
            var dphi = Log(Tan(to.lat / 2.0 + PI / 4.0) / Tan(from.lat / 2.0 + PI / 4.0));

            if (Abs(dl) > PI)
            {
                if (dl > 0.0)
                {
                    dl = -(2.0f * (float)PI - dl);
                }
                else
                {
                    dl = (2.0f * (float)PI + dl);
                }
            }

            return (Atan2(dl, dphi) * (180 / PI) + 360) % 360;            

        }

        public LocationService(UserFileDataService userFileDataService)
        {
            this.userFileDataService = userFileDataService;
        }

        public async Task<Location> GetCurrentLocation(int timeout = 10)
        {
            try
            {
                return await GetCurrentLocationAsync(timeout);

            }

            catch (FeatureNotSupportedException fnsEx)
            {
                await userFileDataService.WriteDataAsync(fnsEx.Message);
            }
            catch (FeatureNotEnabledException fneEx)
            {
                await userFileDataService.WriteDataAsync(fneEx.Message);
            }
            catch (PermissionException pEx)
            {
                await userFileDataService.WriteDataAsync(pEx.Message);
            }
            catch (Exception ex)
            {
                await userFileDataService.WriteDataAsync(ex.Message);
            }

            return default;
        }

        private async Task<Location> GetCurrentLocationAsync(int timeout = 10)
        {
            try
            {
                GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(timeout));

                var _cancelTokenSource = new CancellationTokenSource();

                return await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);

            }
            catch (Exception ex)
            {
                await userFileDataService.WriteDataAsync(ex.Message);
            }

            return default;
        }
    }
}
