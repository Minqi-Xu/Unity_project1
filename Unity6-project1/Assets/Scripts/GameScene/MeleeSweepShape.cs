using UnityEngine;

// Polar boundary of an ellipse, clipped to a forward-facing angular sector.
public static class MeleeSweepShape
{
    public static float ReachAtAngle(float angleDegrees, float forwardReach, float sideReach)
    {
        float angle = angleDegrees * Mathf.Deg2Rad;
        float x = Mathf.Cos(angle) / Mathf.Max(.01f, forwardReach);
        float y = Mathf.Sin(angle) / Mathf.Max(.01f, sideReach);
        return 1f / Mathf.Sqrt(x * x + y * y);
    }

    public static bool Contains(Vector2 offset, Vector2 aim, float forwardReach, float sideReach, float arc, float progress = 1f)
    {
        if (forwardReach <= 0f || sideReach <= 0f || progress < 0f) return false;
        if (offset.sqrMagnitude < .000001f) return true;
        if (aim.sqrMagnitude < .000001f) aim = Vector2.right;
        float angle = Vector2.SignedAngle(aim, offset);
        float half = Mathf.Clamp(arc, 0f, 170f) * .5f;
        if (angle < -half || angle > Mathf.Lerp(-half, half, Mathf.Clamp01(progress))) return false;
        float reach = ReachAtAngle(angle, forwardReach, sideReach);
        return offset.sqrMagnitude <= reach * reach + .0001f;
    }
}
