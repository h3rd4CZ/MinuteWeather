using static System.Math;

namespace MauiNotifications.Services
{
    public static class WindScoreProcessor
    {
        public static decimal ComputeScore(decimal windDir, decimal moveDir, decimal windPower)
        {
            _ = windDir >= 0 ? true : throw new ArgumentException(nameof(windDir));
            _ = moveDir >= 0 ? true : throw new ArgumentException(nameof(moveDir));
            _ = windPower >= 0 ? true : throw new ArgumentException(nameof(windPower));

            moveDir = (moveDir + 180) % 360;

            var windMoveAngle = Abs(windDir - moveDir);

            if (windMoveAngle > 180) windMoveAngle = 360 - windMoveAngle;

            var angleCoeficient = windMoveAngle switch
            {
                >= 0 and <= 90 => 1 - windMoveAngle / 90,
                > 90 and <= 180 => 0 - (windMoveAngle - 90) / 90,
                _ => throw new InvalidOperationException("Angle is out of range, must be between 0 and 180")
            };

            return Round(angleCoeficient * windPower, 1);
        }
    }
}
