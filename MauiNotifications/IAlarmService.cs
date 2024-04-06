using MauiNotifications.Model;
using MauiNotifications.Services;

namespace MauiNotifications
{
    public interface IAlarmService
    {
        bool CanScheduleExactAlarms();
        bool ScheduleAlarm(AlarmSchedule alarm, bool withWatchdog = false, string wellcomePlace = default);
        Task<bool> AreNotificationsEnabled();
        Task<bool> RequestNotificationsEnabled();
        bool CancelUpcommingIntent();
        void RegisterGeofence(GeofencePreferencePlace place);
        void UnregisterGeofence(GeofencePreferencePlace place);
        void RegisterWindScoreService(AlarmSchedule nextCall, int intervalInMinutes);
        void UnRegisterWindScoreService();

    }
}
