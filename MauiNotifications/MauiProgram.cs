using CommunityToolkit.Maui;
using MauiNotifications.Pages;
using MauiNotifications.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Compatibility.Hosting;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;

namespace MauiNotifications;

public static class MauiProgram
{
    public static IServiceProvider Services { get; private set; }

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
#if ANDROID
        //builder.Services.AddTransient<IServicesTest, DemoServices>();
        builder.Services.AddTransient<IMessage, MessageTest>();
        builder.Services.AddSingleton<IMessage, MessageTest>();
        builder.Services.AddSingleton<IAlarmService, AlarmService>();
#elif IOS
        builder.Services.AddTransient<IServicesTest, DemoServices>();
        builder.Services.AddTransient<IMessage, MessageTest>();
#endif
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddTransient<Pages.Location>();
        builder.Services.AddTransient<WeatherForecastPage>();
        builder.Services.AddTransient<WeatherForecastDaily>();
        builder.Services.AddTransient<WeatherForecastHourly>();
        builder.Services.AddTransient<FindLocation>();
        builder.Services.AddTransient<Debug>();
        builder.Services.AddSingleton<UserFileDataService>(); 
        builder.Services.AddTransient<PreferenceService>();
        builder.Services.AddTransient<LocationService>();
        builder.Services.AddTransient<WeatherForecastCacheService>();
        builder.Services.AddSingleton<GeoLocationService>();
        builder.Services.AddTransient<WeatherForecastService>();
        builder.Services.AddTransient<GeofenceSettings>();

        builder.Services.AddTransient<Model.MVVM.GeofencePreferenceModel>();
        builder.Services.AddTransient<Model.MVVM.WeatherHourlyModel>();

        builder.Services.AddSingleton(typeof(IAsynchronousCache<>), typeof(AsynchronousExpirationDictionaryCache<>));

#if ANDROID
        builder.UseLocalNotification();
#endif

#if IOS
        builder.UseLocalNotification();
#endif

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit(options =>
            {
                options.SetShouldSuppressExceptionsInConverters(false);
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("OpenSans-Light.ttf", "OpenSansLight");
                fonts.AddFont("materialdesignicons-webfont.ttf", "Material");
            })

        .UseLocalNotification(config =>
         {
             config.AddCategory(new NotificationCategory(NotificationCategoryType.Status)
             {
                 ActionList = new HashSet<NotificationAction>(new List<NotificationAction>()
                        {
                            new NotificationAction(100)
                            {
                                Title = "Otevřít aplikaci",
                                Android =
                                {
                                    LaunchAppWhenTapped = true,
                                    IconName =
                                    {
                                        ResourceName = "i2"
                                    }
                                }
                            },
                            //new NotificationAction(101)
                            //{
                            //    Title = "Close",
                            //    Android =
                            //    {
                            //        LaunchAppWhenTapped = false,
                            //        IconName =
                            //        {
                            //            ResourceName = "i3"
                            //        }
                            //    }
                            //}
                        })
             })
             .AddAndroid(android =>
             {
                 android.AddChannel(new NotificationChannelRequest
                 {
                     Name = "Weather info",
                     //Sound = "good_things_happen"
                 });
             })
             .AddiOS(iOS =>
             {
#if IOS
#endif
             });
         });


#if DEBUG
        builder.Services.AddLogging(logging =>
        {
            logging.AddDebug();
        });
#endif

        var mauiApp = builder.Build();

        var services = mauiApp.Services;

        Services = services;

        return mauiApp;
    }
}
