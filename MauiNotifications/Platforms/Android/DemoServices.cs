using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;

//namespace MauiNotifications
//{
//    [Service(ForegroundServiceType = Android.Content.PM.ForegroundService.TypeDataSync)]
//    public class DemoServices : Service, IServicesTest
//    {
//        Random random;
//        bool firstNotifFired;
//        int prevNotifIdSent;
//        public override IBinder OnBind(Intent intent)
//        {
//            throw new NotImplementedException();
//        }

//        public DemoServices()
//        {
//            random  =new Random();
//        }

//        [return: GeneratedEnum]
//        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
//        {
//            if (intent.Action == "START_SERVICE")
//            {
//                System.Diagnostics.Debug.WriteLine("Se ha iniciado el servicio");

//                var dtm = DateTime.Now;
                
//                var notifId = random.Next(1, int.MaxValue);

//                if (prevNotifIdSent > 0)
//                {
//                    LocalNotificationCenter.Current.Clear(prevNotifIdSent);
//                }
                
//                var request = new NotificationRequest
//                {
                    
//                    NotificationId = notifId,
//                    Title = "New weather notification",
//                    Subtitle = "New data has arrived",
//                    Description = $"Current date : {dtm}",
//                    BadgeNumber = 42,
//                    Schedule = new NotificationRequestSchedule
//                    {
//                        NotifyTime = DateTime.Now.AddSeconds(2),
//                        RepeatType = NotificationRepeat.No
//                    },
//                    Android = new AndroidOptions
//                    {
//                        IconSmallName = new AndroidIcon("dotnet_bot"),
//                        IconLargeName = new AndroidIcon("dotnet_bot")
//                    }
//                };

//                LocalNotificationCenter.Current.Show(request);

//                prevNotifIdSent = notifId;

//                if (!firstNotifFired)
//                {
//                    RegisterNotification();
//                    firstNotifFired = true;
//                }
//            }
//            else if (intent.Action == "STOP_SERVICE")
//            {
//                System.Diagnostics.Debug.WriteLine("Se ha detenido el servicio");
//                StopForeground(true);
//                StopSelfResult(startId);
//            }

//            return StartCommandResult.NotSticky;
//        }

//        public void Start(DateTime now)
//        {
//            Intent startService = new Intent(MainActivity.ActivityCurrent, typeof(DemoServices));
//            startService.SetAction("START_SERVICE");
//            var svc = MainActivity.ActivityCurrent.StartService(startService);
//        }

//        public void Stop()
//        {
//            Intent stopIntent = new Intent(MainActivity.ActivityCurrent, this.Class);
//            stopIntent.SetAction("STOP_SERVICE");
//            MainActivity.ActivityCurrent.StartService(stopIntent);
//        }

//        private void RegisterNotification()
//        {
//            NotificationChannel channel = new NotificationChannel("ServicioChannel1", "Demo de servicio", NotificationImportance.Low);
//            NotificationManager manager = (NotificationManager)MainActivity.ActivityCurrent.GetSystemService(Context.NotificationService);
                        
//            var intent = new Intent(this.ApplicationContext, typeof(MainActivity));
//            intent.AddFlags(ActivityFlags.SingleTop);
//            intent.PutExtra("Title", "Message");
//            var pendingAppIntent = PendingIntent.GetActivity(ApplicationContext, 0, intent, PendingIntentFlags.Mutable);
                        
//            manager.CreateNotificationChannel(channel);
//            Notification notification = new Notification.Builder(this, "ServicioChannel1")
//                .SetContentTitle("Weather - notification running")
//                .SetSmallIcon(Resource.Drawable.dotnet_bot)
//                .SetOngoing(false)
//                .SetContentIntent(pendingAppIntent)
//                .Build();

//            //StartForeground(100, notification, Android.Content.PM.ForegroundService.TypeDataSync);
//            //StartForeground(100, notification);

//        }
//    }

//    //[Service(ForegroundServiceType = Android.Content.PM.ForegroundService.TypeDataSync)]
//    //public class DemoServices : Service, IServicesTest
//    //{
//    //    private static Context context = global::Android.App.Application.Context;

//    //    private static string foregroundChannelId = "9001";


//    //    public override IBinder OnBind(Intent intent)
//    //    {
//    //        throw new NotImplementedException();
//    //    }
//    //    [return: GeneratedEnum]
//    //    public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
//    //    {
//    //        if (intent.Action == "START_SERVICE")
//    //        {
//    //            var data = intent.GetStringExtra("data");

//    //            System.Diagnostics.Debug.WriteLine("Se ha iniciado el servicio");

//    //            RegisterNotification(JsonSerializer.Deserialize<DateTime>(data));
//    //            //RegisterNotification();
//    //        }
//    //        else if (intent.Action == "STOP_SERVICE")
//    //        {
//    //            System.Diagnostics.Debug.WriteLine("Se ha detenido el servicio");
//    //            StopForeground(true);
//    //            StopSelfResult(startId);
//    //        }
//    //        return StartCommandResult.NotSticky;
//    //    }

//    //    public void Start(DateTime now)
//    //    {
//    //        Intent startService = new Intent(context, typeof(DemoServices));
//    //        startService.PutExtra("data", JsonSerializer.Serialize<DateTime>(now));
//    //        startService.SetAction("START_SERVICE");
//    //        context.StartService(startService);

//    //    }

//    //    public void Stop()
//    //    {
//    //        Intent stopIntent = new Intent(MainActivity.ActivityCurrent, this.Class);
//    //        stopIntent.SetAction("STOP_SERVICE");
//    //        MainActivity.ActivityCurrent.StartService(stopIntent);
//    //    }

//    //    private void RegisterNotification(DateTime data)
//    //    {
//    //        Intent alarmIntent = new Intent(AlarmClock.ActionSetAlarm);
//    //        alarmIntent.PutExtra(AlarmClock.ExtraMessage, "New Alarm");
//    //        alarmIntent.PutExtra(AlarmClock.ExtraHour, 10);
//    //        alarmIntent.PutExtra(AlarmClock.ExtraMinutes, 30);
//    //        var pendingAlarmIntent = PendingIntent.GetActivity(context, 0, alarmIntent, PendingIntentFlags.Immutable);

//    //        var intent = new Intent(context, typeof(MainActivity));
//    //        intent.AddFlags(ActivityFlags.SingleTop);
//    //        intent.PutExtra("Title", "Message");
//    //        var pendingAppIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.Mutable);

//    //        var notifBuilder = new NotificationCompat.Builder(context, foregroundChannelId)
//    //            .SetContentTitle("Nová notifikace")
//    //            .SetContentText($"Aktuálně je {data}")
//    //            .SetSmallIcon(Resource.Drawable.navigation_empty_icon)
//    //            .SetOngoing(true)
//    //            .AddAction(Resource.Drawable.mtrl_dialog_background, "Set alarm", pendingAlarmIntent)
//    //            .AddAction(Resource.Drawable.mtrl_dropdown_arrow, "Open app", pendingAppIntent)
//    //            .SetContentIntent(pendingAppIntent);

//    //        // Building channel if API verion is 26 or above
//    //        if (global::Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.O)
//    //        {
//    //            NotificationChannel notificationChannel = new NotificationChannel(foregroundChannelId, "Title", NotificationImportance.High);
//    //            notificationChannel.Importance = NotificationImportance.High;
//    //            notificationChannel.EnableLights(true);
//    //            notificationChannel.EnableVibration(true);
//    //            notificationChannel.SetShowBadge(true);
//    //            //notificationChannel.SetVibrationPattern(new long[] { 100, 200, 300, 400, 500, 400, 300, 200, 400 });
//    //            notificationChannel.SetVibrationPattern(new long[] { 100 });

//    //            var notifManager = context.GetSystemService(Context.NotificationService) as NotificationManager;
//    //            if (notifManager != null)
//    //            {
//    //                notifBuilder.SetChannelId(foregroundChannelId);
//    //                notifManager.CreateNotificationChannel(notificationChannel);
//    //            }
//    //        }

//    //        var notification = notifBuilder.Build();

//    //        //StartService(intent);
//    //        StartForeground(100, notification);
//    //    }
//    //}
//}