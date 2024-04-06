using MauiNotifications.Services;
using Microsoft.Maui.Graphics;

namespace MauiNotifications.Model.Weather;
public class WeatherForecastResponse
{
    public static Dictionary<int, string> weatherIconMap =

    new Dictionary<int, string>
    {
            {1, Utils.IconFont.WeatherSunny },
            {2, Utils.IconFont.WeatherPartlyCloudy },
            {3, Utils.IconFont.WeatherPartlyCloudy },
            {4, Utils.IconFont.WeatherPartlyCloudy },
            {5, Utils.IconFont.WeatherPartlyCloudy },
            {6, Utils.IconFont.WeatherPartlyCloudy },
            {7, Utils.IconFont.WeatherCloudy },
            {8, Utils.IconFont.WeatherCloudy },
            {11, Utils.IconFont.WeatherFog },
            {12, Utils.IconFont.WeatherRainy },
            {13, Utils.IconFont.WeatherPartlyRainy },
            {14, Utils.IconFont.WeatherPartlyRainy },
            {15, Utils.IconFont.WeatherLightning },
            {16, Utils.IconFont.WeatherLightning },
            {17, Utils.IconFont.WeatherLightning },
            {18, Utils.IconFont.WeatherPouring },
            {19, Utils.IconFont.WeatherSnowy },
            {20, Utils.IconFont.WeatherPartlySnowy },
            {21, Utils.IconFont.WeatherPartlySnowy },
            {22, Utils.IconFont.WeatherSnowyHeavy },
            {23, Utils.IconFont.WeatherPartlySnowy },
            {24, Utils.IconFont.Skate },
            {25, Utils.IconFont.WeatherSnowyRainy },
            {26, Utils.IconFont.WeatherSnowyRainy },
            {29, Utils.IconFont.WeatherSnowyRainy },
            {30, Utils.IconFont.ThermometerAlert },
            {31, Utils.IconFont.ThermometerLow },
            {32, Utils.IconFont.WeatherWindy },
            {33, Utils.IconFont.WeatherNight },
            {34, Utils.IconFont.WeatherNightPartlyCloudy },
            {35, Utils.IconFont.WeatherNightPartlyCloudy },
            {36, Utils.IconFont.WeatherNightPartlyCloudy },
            {37, Utils.IconFont.WeatherNightPartlyCloudy },
            {38, Utils.IconFont.WeatherNightPartlyCloudy },
            {39, Utils.IconFont.WeatherRainy },
            {40, Utils.IconFont.WeatherRainy},
            {41, Utils.IconFont.WeatherLightning},
            {42, Utils.IconFont.WeatherLightning},
            {43, Utils.IconFont.WeatherSnowy},
            {44, Utils.IconFont.WeatherPartlySnowy},
    };

    public static Dictionary<int, Microsoft.Maui.Graphics.Color> weatherIconMapColors =

    new Dictionary<int, Microsoft.Maui.Graphics.Color>
    {
            {1, Microsoft.Maui.Graphics.Color.FromRgba("#ffd787")},
            {2, Microsoft.Maui.Graphics.Color.FromRgba("#ffd787") },
            {3, Microsoft.Maui.Graphics.Color.FromRgba("#ffd787") },
            {4, Microsoft.Maui.Graphics.Color.FromRgba("#ffd787") },
            {5, Microsoft.Maui.Graphics.Color.FromRgba("#ffd787") },
            {6, Microsoft.Maui.Graphics.Color.FromRgba("#fff5a6") },
            {7, Microsoft.Maui.Graphics.Color.FromRgba("#c5d1fa") },
            {8, Microsoft.Maui.Graphics.Color.FromRgba("#c5d1fa") },
            {11, Microsoft.Maui.Graphics.Color.FromRgba("#c5d1fa") },
            {12, Microsoft.Maui.Graphics.Color.FromRgba("#a6b8ff") },
            {13, Microsoft.Maui.Graphics.Color.FromRgba("#a6b8ff") },
            {14, Microsoft.Maui.Graphics.Color.FromRgba("#a6b8ff") },
            {15, Microsoft.Maui.Graphics.Color.FromRgba("#a6b8ff") },
            {16, Microsoft.Maui.Graphics.Color.FromRgba("#a6b8ff") },
            {17, Microsoft.Maui.Graphics.Color.FromRgba("#a6b8ff") },
            {18, Microsoft.Maui.Graphics.Color.FromRgba("#a6b8ff") },
            {19, Microsoft.Maui.Graphics.Color.FromRgba("#eaa6ff") },
            {20, Microsoft.Maui.Graphics.Color.FromRgba("#eaa6ff") },
            {21, Microsoft.Maui.Graphics.Color.FromRgba("#eaa6ff") },
            {22, Microsoft.Maui.Graphics.Color.FromRgba("#eaa6ff") },
            {23, Microsoft.Maui.Graphics.Color.FromRgba("#eaa6ff") },
            {24, Microsoft.Maui.Graphics.Color.FromRgba("#eaa6ff") },
            {25, Microsoft.Maui.Graphics.Color.FromRgba("#eaa6ff") },
            {26, Microsoft.Maui.Graphics.Color.FromRgba("#eaa6ff") },
            {29, Microsoft.Maui.Graphics.Color.FromRgba("#eaa6ff") },
            {30, Microsoft.Maui.Graphics.Color.FromRgba("#ffd787") },
            {31, Microsoft.Maui.Graphics.Color.FromRgba("#ffd787") },
            {32, Microsoft.Maui.Graphics.Color.FromRgba("#c5d1fa") }
    };

    public Summary summary { get; set; }
    public Intervalsummary[] intervalSummaries { get; set; }
    public Interval[] intervals { get; set; }
    public double ForecastPrecipInMm =>
         Math.Round((intervals.Sum(i => Interval.PrecipInMmPerHourFormula(i.dbz)) / intervals.Count() * WeatherForecastService.FORECAST_SNAPSHOT_DURATION_MINUTES / 60), 1);

    public double ForecastPrecipInMmForInterval(int startMinute, int endMinute)
    {
        var precipIntervals =
            intervals
            .Where(i => i.minute >= startMinute && i.minute + WeatherForecastService.FORECAST_SNAPSHOT_FRAME_SIZE - 1 <= endMinute);

        if (precipIntervals.Count() == 0) return default;

        var precipIntervalStart = precipIntervals.First().minute;
        var precipIntervalEnd = precipIntervals.Last().minute + WeatherForecastService.FORECAST_SNAPSHOT_FRAME_SIZE - 1;

        var intervalWindowDurationInMinutes = precipIntervalEnd - precipIntervalStart;


        var precipIntervalSum =
            precipIntervals.Sum(i => Interval.PrecipInMmPerHourFormula(i.dbz));

        var precipForInterval = precipIntervalSum / precipIntervals.Count() * ((float)intervalWindowDurationInMinutes / 60);

        return Math.Round(precipForInterval, 1);
    }
}

public class Summary
{
    public string briefPhrase60 { get; set; }
    public string shortPhrase { get; set; }
    public string briefPhrase { get; set; }
    public string longPhrase { get; set; }
    public int iconCode { get; set; }
}

public class Intervalsummary
{
    public int startMinute { get; set; }
    public int endMinute { get; set; }
    public int totalMinutes { get; set; }
    public string shortPhrase { get; set; }
    public string briefPhrase { get; set; }
    public string longPhrase { get; set; }
    public int iconCode { get; set; }
}

public class Interval
{
    public DateTime startTime { get; set; }
    public int minute { get; set; }
    public float dbz { get; set; }
    public string shortPhrase { get; set; }
    public int iconCode { get; set; }
    public int cloudCover { get; set; }
    public string threshold { get; set; }
    public Color color { get; set; } = new Color { hex = "#000" };
    public Simplifiedcolor simplifiedColor { get; set; }
    public string precipitationType { get; set; }
    public string DateDisplay => startTime.ToLocalTime().ToString("HH:mm");
    public Microsoft.Maui.Graphics.Color HexColor => Microsoft.Maui.Graphics.Color.FromRgb(color.red, color.green, color.blue);
    public string iconCodePath => $"w{iconCode}";
    public string PrecipInMmPerHour
    {
        get
        {
            var precip = Math.Round(PrecipInMmPerHourFormula(dbz), 1);

            return precip > 0 ? $"{precip} mm" : string.Empty;
        }
    }

    public static double PrecipInMmPerHourFormula(float dbz) => dbz == 0f ? 0 : Math.Pow((Math.Pow(10, dbz / 10) / 200), 0.625);
}

public class Color
{
    public int red { get; set; }
    public int green { get; set; }
    public int blue { get; set; }
    public string hex { get; set; }
}

public class Simplifiedcolor
{
    public int red { get; set; }
    public int green { get; set; }
    public int blue { get; set; }
    public string hex { get; set; }
}
