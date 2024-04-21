namespace MauiNotifications.Services
{
    public class AzureMapsServiceBase
    {
        protected const string SUBSCRIPTION_KEY = "<---azure maps subscription key here--->";
        protected HttpClient GetInitClient()
        {
            var client = new HttpClient();

            return client;
        }
    }
}
