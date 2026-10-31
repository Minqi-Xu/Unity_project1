using UnityEngine;

public static class ExperienceMath
{
    // Base + linear growth + quadratic growth; level one keeps the original threshold.
    public static float ThresholdForLevel(float baseThreshold, int level, float linearGrowth, float quadraticGrowth)
    {
        if (!IsPositiveFinite(baseThreshold)) baseThreshold = 1f;
        if (!IsPositiveFinite(linearGrowth)) linearGrowth = 0f;
        if (!IsPositiveFinite(quadraticGrowth)) quadraticGrowth = 0f;
        double steps = System.Math.Max(0d, (double)level - 1d);
        double threshold = baseThreshold + System.Math.Ceiling(linearGrowth * steps + quadraticGrowth * steps * steps);
        return (float)System.Math.Min(threshold, float.MaxValue);
    }

    public static bool IsPositiveFinite(float value) => value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);

    // Tolerate float accumulation error, e.g. fifty 0.1-point pickups reaching 5 XP.
    public static bool CanLevelUp(float current, float threshold) => IsPositiveFinite(threshold) && IsPositiveFinite(current) &&
        (current >= threshold || Mathf.Approximately(current, threshold));

    public static float Progress(float current, float threshold)
    {
        if (!IsPositiveFinite(threshold) || float.IsNaN(current) || float.IsInfinity(current)) return 0f;
        return Mathf.Clamp01(current / threshold);
    }
}
