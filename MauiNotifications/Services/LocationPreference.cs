namespace MauiNotifications.Services
{
    public record class LocationPreference(
        float lat,
        float lon,
        string addressName,
        bool isTrackingPosition,
        bool notificationsOn,
        DateTime nextScheduleTime,
        TimeSpan notificationFrom,
        TimeSpan notificationTo, 
        TimeSpan notificationFromWeekend,
        TimeSpan notificationToWeekend,
        List<int> processedAlerts)
    {
        public bool AreNotificationEnabled { get; set; }
    }
}
