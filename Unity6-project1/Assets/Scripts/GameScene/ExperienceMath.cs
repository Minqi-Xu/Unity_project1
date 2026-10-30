using UnityEngine;

public static class ExperienceMath
{
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
