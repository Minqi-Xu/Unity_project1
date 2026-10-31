using System;

// Intersects the player-to-destination ray with the inset screen rectangle (GUI coordinates).
public static class ZoneNavigationGeometry
{
    public struct Point
    {
        public float x, y;
        public Point(float x, float y) { this.x = x; this.y = y; }
    }

    public static Point EdgePoint(float originX, float originY, float targetX, float targetY,
        float minX, float minY, float maxX, float maxY)
    {
        originX = Math.Max(minX, Math.Min(maxX, originX));
        originY = Math.Max(minY, Math.Min(maxY, originY));
        float dx = targetX - originX, dy = targetY - originY;
        if (Math.Abs(dx) + Math.Abs(dy) < .0001f) dy = -1f;
        float tx = Math.Abs(dx) < .0001f ? float.PositiveInfinity :
            ((dx > 0f ? maxX : minX) - originX) / dx;
        float ty = Math.Abs(dy) < .0001f ? float.PositiveInfinity :
            ((dy > 0f ? maxY : minY) - originY) / dy;
        float t = Math.Min(tx, ty);
        return new Point(Math.Max(minX, Math.Min(maxX, originX + dx * t)),
            Math.Max(minY, Math.Min(maxY, originY + dy * t)));
    }
}
