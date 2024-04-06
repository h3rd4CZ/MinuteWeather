using MauiNotifications.Model;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;

namespace MauiNotifications.Services
{
    public class GeoLocationService : AzureMapsServiceBase
    {
        private const string API_URL = "https://atlas.microsoft.com/search/address";

        public async Task<ReverseGeolocationResult> Reverse(Position latLon)
        {
            var client = GetInitClient();

            var uri = $"{API_URL}/reverse/json?api-version=1.0&subscription-key={SUBSCRIPTION_KEY}&language=cs-CZ&query={latLon.lat.ToString(new CultureInfo(1033))},{latLon.lon.ToString(new CultureInfo(1033))}";

            var response = await client.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ReverseGeolocationResult>();
            }
            else throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
        }

        public async Task<GeoLocationResults> Search(string searchFor)
        {
            var client = GetInitClient();

            var uri = $"{API_URL}/json?api-version=1.0&subscription-key={SUBSCRIPTION_KEY}&language=cs-CZ&query={searchFor}";

            var response = await client.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<GeoLocationResults>();
            }
            else throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
        }

        
    }
}
