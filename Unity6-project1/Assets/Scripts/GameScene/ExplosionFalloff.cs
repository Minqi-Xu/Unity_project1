using UnityEngine;

// Distance is measured to enemy centers. The core is flat, the outer ring is linear.
public static class ExplosionFalloff
{
    public static float Evaluate(float distance, float radius, float coreFraction, float edgeMultiplier)
    {
        if (radius <= 0f || distance > radius) return 0f;
        float normalized = Mathf.Max(0f, distance) / radius;
        float core = Mathf.Clamp(coreFraction, 0f, .95f);
        if (normalized <= core) return 1f;
        float progress = (normalized - core) / (1f - core);
        return Mathf.Lerp(1f, Mathf.Clamp01(edgeMultiplier), progress);
    }
}
