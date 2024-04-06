namespace MauiNotifications.Model.WeatherForecastHourly;
public class WeatherHourlyResponse
{
    public Forecast[] forecasts { get; set; }

    public Wind GetLastKnownWind()
    {
        return forecasts?.OrderBy(f => f.date)?.FirstOrDefault()?.wind;
    }
}

public class Forecast
{
    public DateTime date { get; set; }
    public int iconCode { get; set; }
    public string IconTitle => $"w{iconCode}";
    public string iconPhrase { get; set; }
    public bool hasPrecipitation { get; set; }
    public bool isDaylight { get; set; }
    public Temperature temperature { get; set; }
    public Realfeeltemperature realFeelTemperature { get; set; }
    public Wetbulbtemperature wetBulbTemperature { get; set; }
    public Dewpoint dewPoint { get; set; }
    public Wind wind { get; set; }
    public Windgust windGust { get; set; }
    public int relativeHumidity { get; set; }
    public Visibility visibility { get; set; }
    public int cloudCover { get; set; }
    public Ceiling ceiling { get; set; }
    public int uvIndex { get; set; }
    public string uvIndexPhrase { get; set; }
    public int precipitationProbability { get; set; }
    public int rainProbability { get; set; }
    public int snowProbability { get; set; }
    public int iceProbability { get; set; }
    public Totalliquid totalLiquid { get; set; }
    public Rain rain { get; set; }
    public Snow snow { get; set; }
    public Ice ice { get; set; }
}

public class Temperature
{
    public float value { get; set; }
    public string unit { get; set; }
    public int unitType { get; set; }
}

public class Realfeeltemperature
{
    public float value { get; set; }
    public string unit { get; set; }
    public int unitType { get; set; }
}

public class Wetbulbtemperature
{
    public float value { get; set; }
    public string unit { get; set; }
    public int unitType { get; set; }
}

public class Dewpoint
{
    public float value { get; set; }
    public string unit { get; set; }
    public int unitType { get; set; }
}

public class Wind
{
    public Direction direction { get; set; }
    public Speed speed { get; set; }
}

public class Direction
{
    public float degrees { get; set; }
    public string localizedDescription { get; set; }
}

public class Speed
{
    public float value { get; set; }
    public double valueMs => Math.Round(value / 3.6, 1);
    public string unit { get; set; }
    public int unitType { get; set; }
}

public class Windgust
{
    public Speed1 speed { get; set; }
}

public class Speed1
{
    public float value { get; set; }
    public string unit { get; set; }
    public int unitType { get; set; }
}

public class Visibility
{
    public float value { get; set; }
    public string unit { get; set; }
    public int unitType { get; set; }
}

public class Ceiling
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

