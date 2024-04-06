namespace MauiNotifications.Model.WeatherForecastSevereAlerts;


public class WeatherForecastAlertResult
{
    public AlertResult[] results { get; set; }
}

public class AlertResult
{
    public string countryCode { get; set; }
    public int alertId { get; set; }
    public Description description { get; set; }
    public string category { get; set; }
    public int priority { get; set; }
    public string source { get; set; }
    public int sourceId { get; set; }
    public Alertarea[] alertAreas { get; set; }

    public override string ToString() => $"{description?.localized} - {string.Join('.', alertAreas.Select(a => $"{a.name}, {a.summary}, OD : {a.startTime}, DO : {a.endTime}"))}";
}

public class Description
{
    public string localized { get; set; }
    public string english { get; set; }
}

public class Alertarea
{
    public string name { get; set; }
    public string summary { get; set; }
    public DateTime startTime { get; set; }
    public string startTimeWithIcon => $"Od : {Utils.IconFont.Calendar} {startTime}";
    public DateTime endTime { get; set; }
    public string endTimeWithIcon => $"Do : {Utils.IconFont.Calendar} {endTime}";
    public Lateststatus latestStatus { get; set; }
    public string alertDetails { get; set; }
    public string alertDetailsLanguageCode { get; set; }
}

public class Lateststatus
{
    public string localized { get; set; }
    public string english { get; set; }
}




