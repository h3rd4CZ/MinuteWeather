using MauiNotifications.Model;
using MauiNotifications.Model.WeatherCurrent;

namespace MauiNotifications.Services
{
    public record class LastKnownWind(DateTime knownAt, Wind wind);
        
    public class WindScoreMotionPass
    {
        public float WindDir { get; set; }
        public float WindSpeed { get; set; }
        public double Course { get; set; }
        public decimal Score { get; set; }
        public Position Position { get; set; }
        public DateTime DateTime { get; set; }
    }

    public record class WindScoreMotionPreference(bool currentlyActive, int interval, DateTime lastActivatedAt, List<WindScoreMotionPass> passedPoints, LastKnownWind lastKnownWind)
    {
        public LastKnownWind lastKnownWind { get; set; } = lastKnownWind;
    }
}
