using SkiaSharp;

namespace epj.RadialDial.Maui;

public static class Utils
{
    public static float ToAngle(this SKPoint point, SKPoint center)
    {
        var rad = (float)Math.Atan2(center.Y - point.Y, center.X - point.X) % 360.0f;
        var deg = rad.RadianToDegree();

        if (deg < 0)
        {
            deg += 360.0f;
        }

        return deg;
    }

    public static SKPoint PointOnCircle(SKPoint point, SKPoint center, float radius)
    {
        var dX = point.X - center.X;
        var dY = point.Y - center.Y;
        var dist = (float)Math.Sqrt(Math.Pow(dX, 2) + Math.Pow(dY, 2));
        return new SKPoint(center.X + radius * (dX / dist), center.Y + radius * (dY / dist));
    }

    public static float RadianToDegree(this float radians) => (float)(180.0 / Math.PI) * radians;

    public static float NormalizeAngleTo360(this float angle) => angle - 360.0f * (float)Math.Floor(angle / 360.0f);
}