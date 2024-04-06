
using MauiNotifications.Model.WeatherCurrent;
using MauiNotifications.Model.WeatherForecastSevereAlerts;

namespace MauiNotifications.Model.WeatherDaily;

public class WeatherDailyResponse
{
    public Summary summary { get; set; }
    public Forecast[] forecasts { get; set; }

    public CurrentResult Current { get; set; } = new CurrentResult();

    public List<HotTopicMessage> HotTopicMessages { get; set; } = new();

    public static Microsoft.Maui.Graphics.Color TileBackground => Forecast.TileBackgroundColor(80);

    public static void UpdateBackground(ContentPage page, int currWeatherIcon)
    {
        var theme = App.Current.RequestedTheme;

        if (theme == AppTheme.Light)
        {
            if (Weather.WeatherForecastResponse.weatherIconMapColors.TryGetValue(currWeatherIcon, out Color stopColor)) { }
            else stopColor = new Color(161, 181, 255);

            var col = new GradientStopCollection
            {
                new GradientStop(Color.FromArgb("#a1c7ff"), 0),
                new GradientStop(stopColor, 1)
            };

            page.Background = new LinearGradientBrush(col, new Point(0, 0), new Point(0, 1));
        }
    }
}

public class HotTopicMessage
{
    public string Message { get; set; }
    public AlertResult Alert { get; set; }

}

public class Summary
{
    public DateTime startDate { get; set; }
    public DateTime endDate { get; set; }
    public int severity { get; set; }
    public string phrase { get; set; }
    public string category { get; set; }
}

public class Forecast
{
    public object Nr { get; set; } = 0;
    public DateTime date { get; set; }
    public Temperature temperature { get; set; }
    public Realfeeltemperature realFeelTemperature { get; set; }
    public Realfeeltemperatureshade realFeelTemperatureShade { get; set; }
    public float hoursOfSun { get; set; }
    public Degreedaysummary degreeDaySummary { get; set; }
    public Airandpollen[] airAndPollen { get; set; }
    public Day day { get; set; }
    public Night night { get; set; }
    public string[] sources { get; set; }

    public Color DayColor => date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday ? Color.FromRgba("#fc5656") : Color.FromRgb(0, 0, 0);

    private float CloudCoverAverage => ((day.cloudCover + night.cloudCover) / 2);

    public Color BackgroundColor => TileBackgroundColor(CloudCoverAverage);

    public static Color TileBackgroundColor(float cloudCover) => Application.Current.RequestedTheme == AppTheme.Light
        ? Color.FromRgba((byte)255, (byte)255, (byte)255, (byte)Math.Max(15, 255f / 100 * Math.Max(1, cloudCover)))
        : Color.FromRgba((byte)97, (byte)97, (byte)97, (byte)Math.Max(15, 255f / 100 * Math.Max(1, cloudCover)));

    public int ThunderStormProbability => (day.thunderstormProbability + night.thunderstormProbability) / 2;

    public int AvgRainProbability => (day.rainProbability + night.rainProbability) / 2;
    public double AvgRainVolume => Math.Round((day.rain.value + night.rain.value), 1);
    public double AvgRainHours => Math.Round((day.hoursOfRain + night.hoursOfRain), 1);

    public int AvgSnowProbability => (day.snowProbability + night.snowProbability) / 2;
    public double AvgSnowVolume => Math.Round((day.snow.value + night.snow.value), 1);
    public double AvgSnowHours => Math.Round((day.hoursOfSnow + night.hoursOfSnow), 1);

    public int AvgIceProbability => (day.iceProbability + night.iceProbability) / 2;
    public double AvgIceVolume => Math.Round((day.ice.value + night.ice.value), 1);
    public double AvgIceHours => Math.Round((day.hoursOfIce + night.hoursOfIce), 1);

    public double totalLiquid
    {
        get
        {
            var devWidth = DeviceDisplay.Current.MainDisplayInfo.Width - 50;

            var precipVolume = (double)(day?.totalLiquid?.value + night?.totalLiquid?.value);

            double maxPrecipPerDay = 100;

            var precipPerDayPerc = Math.Min(maxPrecipPerDay, precipVolume) / maxPrecipPerDay * 100;

            return devWidth / 100 * precipPerDayPerc;
        }
    }

    public Thickness Margin => date.DayOfWeek == DayOfWeek.Sunday
        ? new Thickness(5, 5, 5, 25)
        : new Thickness(5, 5, 5, 5);


}

public class Temperature
{
    public Minimum minimum { get; set; }
    public Maximum maximum { get; set; }

    public static Color GetColor(float value) => (value, App.Current.RequestedTheme) switch
    {
        ( > 0, AppTheme.Light) => Color.FromArgb("ffff3636"),
        ( <= 0, AppTheme.Light) => Color.FromArgb("ff00b8f1"),
        ( > 0, AppTheme.Dark) => Color.FromArgb("ffff6666"),
        ( <= 0, AppTheme.Dark) => Color.FromArgb("ff00b8f1"),
        _ => Color.FromArgb("ffffffff")
    };

}

public class Minimum
{
    public float value { get; set; }
    public string unit { get; set; }
    public int unitType { get; set; }

    public override string ToString() => $"{Math.Round(value, 0)}°";

    public Color Color => Temperature.GetColor(value);


}

public class Maximum
{
    public float value { get; set; }
    public string unit { get; set; }
    public int unitType { get; set; }

    public override string ToString() => $"{Math.Round(value, 0)}°";

    public Color Color => Temperature.GetColor(value);
}

public class Realfeeltemperature
{
    public Minimum1 minimum { get; set; }
    public Maximum1 maximum { get; set; }
}

public class Minimum1
{
    public float value { get; set; }
    public string unit { get; set; }
    public int unitType { get; set; }
}

public class Maximum1
{
    public float value { get; set; }
    public string unit { get; set; }
    public int unitType { get; set; }
}

public class Realfeeltemperatureshade
{
    public Minimum2 minimum { get; set; }
    public Maximum2 maximum { get; set; }
}

public class Minimum2
{
    public float value { get; set; }
    public string unit { get; set; }
    public int unitType { get; set; }
}

public class Maximum2
{
    public float value { get; set; }
    public string unit { get; set; }
    public int unitType { get; set; }
}

public class Degreedaysummary
{
    public Heating heating { get; set; }
    public Cooling cooling { get; set; }
}

public class Heating
{
    public float value { get; set; }
    public string unit { get; set; }
    public int unitType { get; set; }
}

public class Cooling
{
    public float value { get; set; }
    public string unit { get; set; }
    public int unitType { get; set; }
}

public class Day
{
    public int iconCode { get; set; }
    public string iconPhrase { get; set; }
    public bool hasPrecipitation { get; set; }
    public string precipitationType { get; set; }
    public string precipitationIntensity { get; set; }
    public string shortPhrase { get; set; }
    public string longPhrase { get; set; }
    public int precipitationProbability { get; set; }
    public int thunderstormProbability { get; set; }
    public int rainProbability { get; set; }
    public int snowProbability { get; set; }
    public int iceProbability { get; set; }
    public Wind wind { get; set; }
    public Windgust windGust { get; set; }
    public Totalliquid totalLiquid { get; set; }
    public Rain rain { get; set; }
    public Snow snow { get; set; }
    public Ice ice { get; set; }
    public float hoursOfPrecipitation { get; set; }
    public float hoursOfRain { get; set; }
    public float hoursOfSnow { get; set; }
    public float hoursOfIce { get; set; }
    public int cloudCover { get; set; }
    public string Icon => $"w{iconCode}";
}

public class Wind
{
    public Direction direction { get; set; }
    public Speed speed { get; set; }

    public string SpeedInMs => $"{Math.Round((speed.value / 3.6), 1)} m/s";
}

public class Direction
{
    public float degrees { get; set; }
    public string localizedDescription { get; set; }
}

public class Speed
{
    public float value { get; set; }
    public string unit { get; set; }
    public int unitType { get; set; }
}

public class Windgust
{
    public Direction1 direction { get; set; }
    public Speed1 speed { get; set; }
}

public class Direction1
{
    public float degrees { get; set; }
    public string localizedDescription { get; set; }
}

public class Speed1
{
    public float value { get; set; }
    public string unit { get; set; }
    public int unitType { get; set; }
}

public class Totalliquid
{
    public float value { get; set; }
    public string unit { get; set; }
    public int unitType { get; set; }
}

public class Rain
{
    public float value { get; set; }
    public string unit { get; set; }
    public int unitType { get; set; }
}

public class Snow
{
    public float value { get; set; }
    public string unit { get; set; }
    public int unitType { get; set; }
}

public class Ice
{
    public float value { get; set; }
    public string unit { get; set; }
    public int unitType { get; set; }
}

public class Night
{
    public int iconCode { get; set; }
    public string iconPhrase { get; set; }
    public bool hasPrecipitation { get; set; }
    public string precipitationType { get; set; }
    public string precipitationIntensity { get; set; }
    public string shortPhrase { get; set; }
    public string longPhrase { get; set; }
    public int precipitationProbability { get; set; }
    public int thunderstormProbability { get; set; }
    public int rainProbability { get; set; }
    public int snowProbability { get; set; }
    public int iceProbability { get; set; }
    public Wind1 wind { get; set; }
    public Windgust1 windGust { get; set; }
    public Totalliquid1 totalLiquid { get; set; }
    public Rain1 rain { get; set; }
    public Snow1 snow { get; set; }
    public Ice1 ice { get; set; }
    public float hoursOfPrecipitation { get; set; }
    public float hoursOfRain { get; set; }
    public float hoursOfSnow { get; set; }
    public float hoursOfIce { get; set; }
    public int cloudCover { get; set; }
    public string Icon => $"w{iconCode}";
}

public class Wind1
{
    public Direction2 direction { get; set; }
    public Speed2 speed { get; set; }
}

public class Direction2
{
    public float degrees { get; set; }
    public string localizedDescription { get; set; }
}

public class Speed2
{
    public float value { get; set; }
    public string unit { get; set; }
    public int unitType { get; set; }
}

public class Windgust1
{
    public Direction3 direction { get; set; }
    public Speed3 speed { get; set; }
}

public class Direction3
{
    public float degrees { get; set; }
    public string localizedDescription { get; set; }
}

public class Speed3
{
    public float value { get; set; }
    public string unit { get; set; }
    public int unitType { get; set; }
}

public class Totalliquid1
{
    public float value { get; set; }
    public string unit { get; set; }
    public int unitType { get; set; }
}

public class Rain1
{
    public float value { get; set; }
    public string unit { get; set; }
    public int unitType { get; set; }
}

public class Snow1
{
    public float value { get; set; }
    public string unit { get; set; }
    public int unitType { get; set; }
}

public class Ice1
{
    public float value { get; set; }
    public string unit { get; set; }
    public int unitType { get; set; }
}

public class Airandpollen
{
    public string name { get; set; }
    public int value { get; set; }
    public string category { get; set; }
    public int categoryValue { get; set; }
    public string type { get; set; }

    public Color Color => categoryValue switch
    {
        1 => Color.FromArgb("#4442f590"),
        2 => Color.FromArgb("#44cef542"),
        3 => Color.FromArgb("#44f5d442"),
        4 => Color.FromArgb("#44f5a742"),
        5 => Color.FromArgb("#44f57e42"),
        6 => Color.FromArgb("#44f54842"),
        _ => Color.FromArgb("#4442a4f5")
    };
}
