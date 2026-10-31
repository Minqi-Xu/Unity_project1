using System;

// Reference walking distance; actual player buffs and dash do not move map events farther away.
public static class ZoneTravelDistance
{
    public static float Estimate(float seconds, float referenceMoveSpeed, float worldScale)
    {
        return Math.Max(0f, seconds) * Math.Max(0f, referenceMoveSpeed) * Math.Max(0f, worldScale);
    }
}
