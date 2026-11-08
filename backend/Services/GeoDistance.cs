namespace backend.Services;

public static class GeoDistance
{
    // Haversine formula to calculate distance between two coordinates on the globe
    public static double StraightDistance(double latx, double lonx, double laty, double lony)
    {
        var a1 = Math.Pow(Math.Sin((latx - laty) * Math.PI / 360), 2);
        var a2 = Math.Cos(latx * Math.PI / 180) * Math.Cos(laty * Math.PI / 180);
        var a3 = Math.Pow(Math.Sin((lonx - lony) * Math.PI / 360), 2);
        var a = a1 + a2 * a3;
        var b = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return 6371 * b;
    }
}