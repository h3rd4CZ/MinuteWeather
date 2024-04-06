using Google.Android.Material.Snackbar;
using Android.App;
using Microsoft.Maui.Controls;

namespace MauiNotifications
{
    public class MessageTest : IMessage
    {
        public void ShowMessage(string Message)
        {            
            var Activity =(Activity)MainActivity.ActivityCurrent;
            //var view = Activity.FindViewById(Resource.Id.content);
            var view = Activity.FindViewById(global::Android.Resource.Id.Content);
                        
            Snackbar snackbar = Snackbar.Make(view, Message, Snackbar.LengthLong);
                        
            snackbar.Show();
        }
    }
}
