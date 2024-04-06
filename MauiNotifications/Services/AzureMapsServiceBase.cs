namespace MauiNotifications.Services
{
    public class AzureMapsServiceBase
    {
        protected const string SUBSCRIPTION_KEY = "2P-dULMV40mSm9D-mZIKNIjwG652KgSoyoYlR08EL84";
        protected HttpClient GetInitClient()
        {
            var client = new HttpClient();

            return client;
        }
    }
}
