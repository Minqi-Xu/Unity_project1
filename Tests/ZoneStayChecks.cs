using System;

public static class ZoneStayChecks
{
    public static void Main()
    {
        var stay = new ZoneStayProgress();
        stay.Tick(false, 30f, 10f, .5f); Near(stay.Elapsed, 0f, "No credit before first entry");
        Check(!stay.Tick(true, 2f, 10f, .5f), "Not completed early");
        stay.Tick(false, .3f, 10f, .5f); Near(stay.Elapsed, 2.3f, "Grace continues progress");
        stay.Tick(true, 1f, 10f, .5f); Near(stay.Elapsed, 3.3f, "Return within grace preserves progress");
        stay.Tick(false, 5f, 10f, .5f); Near(stay.Elapsed, 3.8f, "Large frame only credits remaining grace");
        stay.Tick(false, 10f, 10f, .5f); Near(stay.Elapsed, 3.8f, "Long absence pauses without clearing");
        stay.Tick(true, 1f, 10f, .5f); Near(stay.Elapsed, 4.8f, "Return after grace resumes");
        stay.Tick(true, 0f, 10f, .5f); stay.Tick(true, -1f, 10f, .5f);
        stay.Tick(true, float.NaN, 10f, .5f); stay.Tick(true, float.PositiveInfinity, 10f, .5f);
        Near(stay.Elapsed, 4.8f, "Pause and invalid delta cannot advance or corrupt");
        Check(stay.Tick(true, 100f, 10f, .5f), "Crossing completion succeeds");
        Near(stay.Elapsed, 10f, "Completion is clamped");
        stay.Reset(); Near(stay.Elapsed, 0f, "Repeatable event reset");
        stay.Tick(true, 1f, 2f, 0f); stay.Tick(false, 8f, 2f, 0f);
        Near(stay.Elapsed, 1f, "Zero grace pauses immediately");
        Check(stay.Tick(true, 1f, 2f, 0f), "Exact completion boundary");
        Console.WriteLine("PASS: entry, re-entry, grace boundary, long absence, large frames, pause, reset and completion.");
        Near(ZoneTravelDistance.Estimate(10f, 4.5f, 1f), 45f, "Ten-second minimum at reference speed");
        Near(ZoneTravelDistance.Estimate(25f, 4.5f, 1f), 112.5f, "Twenty-five-second maximum at reference speed");
        Near(ZoneTravelDistance.Estimate(10f, 4.5f, .5f), 22.5f, "Camera scaling matches player movement");
        Near(ZoneTravelDistance.Estimate(25f, 4.5f, 2f), 225f, "Scaled upper boundary");
        var right = ZoneNavigationGeometry.EdgePoint(640f, 360f, 2000f, 360f, 28f, 70f, 1252f, 655f);
        Near(right.x, 1252f, "Right edge"); Near(right.y, 360f, "Right direction");
        var top = ZoneNavigationGeometry.EdgePoint(300f, 200f, 300f, -1000f, 28f, 70f, 1252f, 655f);
        Near(top.x, 300f, "Origin follows player, not screen center"); Near(top.y, 70f, "Top edge");
        foreach (float width in new[] { 720f, 1280f, 2560f })
            for (int i = 0; i < 360; i++)
            {
                double angle = i * Math.PI / 180;
                float x = width * .4f, y = 300f;
                float dx = (float)Math.Cos(angle), dy = (float)Math.Sin(angle);
                var edge = ZoneNavigationGeometry.EdgePoint(x, y, x + dx * 10000f, y + dy * 10000f, 28f, 70f, width - 28f, 655f);
                Check(edge.x >= 28f && edge.x <= width - 28f && edge.y >= 70f && edge.y <= 655f, "All headings remain on-screen");
                Check(Math.Abs(edge.x - 28f) < .01f || Math.Abs(edge.x - (width - 28f)) < .01f ||
                    Math.Abs(edge.y - 70f) < .01f || Math.Abs(edge.y - 655f) < .01f, "Marker remains on an edge");
                Check(Math.Abs((edge.x - x) * dy - (edge.y - y) * dx) < .01f, "Marker lies on the player-to-target ray");
            }
        Console.WriteLine("PASS: reference speed travel bounds, camera scaling, and 1080 edge positions across aspect ratios.");
    }
    private static void Near(float a, float b, string label) { Check(Math.Abs(a - b) < .0001f, label); }
    private static void Check(bool value, string label) { if (!value) throw new Exception(label); }
}
