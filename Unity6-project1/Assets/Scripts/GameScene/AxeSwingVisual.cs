using UnityEngine;

// Draws the committed aim, the elliptical reach, and a moving axe-shaped stroke.
public class AxeSwingVisual : MonoBehaviour
{
    private Transform owner;
    private Vector2 aim;
    private float reach, sideReach, arc, windup, active, recovery, elapsed;
    private Color tint;
    private float started;
    private LineRenderer boundary, haft, blade;

    public void Initialize(Transform target, Vector2 direction, float range, float width, float sweep,
        float preparation, float swing, float rest, Color color)
    {
        owner = target; aim = direction.normalized; reach = range; sideReach = width; arc = sweep;
        windup = preparation; active = Mathf.Max(.001f, swing); recovery = rest; tint = color;
        started = Time.time;
        boundary = WeaponVisuals.Stroke("Reach", transform, color, .045f, 34);
        haft = WeaponVisuals.Stroke("Haft", transform, new Color(.75f, .55f, .3f), .09f, 2);
        blade = WeaponVisuals.Stroke("Blade", transform, color, .16f, 7);
        Draw();
    }

    private void Update()
    {
        if (owner == null) { Destroy(gameObject); return; }
        if (Time.timeScale <= 0f) return;
        elapsed = Time.time - started;
        if (elapsed > windup + active + recovery) { Destroy(gameObject); return; }
        Draw();
    }

    private Vector3 Point(float degrees, float length)
    {
        Vector2 direction = Quaternion.Euler(0f, 0f, degrees) * aim;
        return (Vector3)direction * length;
    }

    private void Draw()
    {
        if (owner == null) return;
        transform.position = owner.position;
        float fade = elapsed < windup ? .35f : elapsed < windup + active ? .9f
            : Mathf.Lerp(.7f, 0f, (elapsed - windup - active) / Mathf.Max(.001f, recovery));
        Color color = new Color(tint.r, tint.g, tint.b, fade);
        boundary.startColor = boundary.endColor = color;
        blade.startColor = blade.endColor = color;
        haft.startColor = haft.endColor = new Color(.75f, .55f, .3f, fade);
        boundary.SetPosition(0, Vector3.zero);
        for (int i = 1; i < 33; i++)
        {
            float angle = Mathf.Lerp(-arc * .5f, arc * .5f, (i - 1) / 31f);
            boundary.SetPosition(i, Point(angle, MeleeSweepShape.ReachAtAngle(angle, reach, sideReach)));
        }
        boundary.SetPosition(33, Vector3.zero);
        float progress = Mathf.Clamp01((elapsed - windup) / active);
        float sweep = Mathf.Lerp(-arc * .5f, arc * .5f, progress);
        float length = MeleeSweepShape.ReachAtAngle(sweep, reach, sideReach);
        haft.SetPosition(0, Point(sweep, .25f));
        haft.SetPosition(1, Point(sweep, length * .82f));
        for (int i = 0; i < 7; i++)
        {
            float angle = sweep + Mathf.Lerp(-13f, 13f, i / 6f);
            blade.SetPosition(i, Point(angle, length * (.85f + .13f * Mathf.Sin(i / 6f * Mathf.PI))));
        }
    }
}
