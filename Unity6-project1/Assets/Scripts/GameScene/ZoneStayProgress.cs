using System;

// Independent of physics: leaving grants at most grace seconds of continued progress.
public sealed class ZoneStayProgress
{
    public float Elapsed { get; private set; }
    public float OutsideTime { get; private set; }
    private bool entered;

    public bool Tick(bool inside, float deltaTime, float requiredTime, float graceTime)
    {
        if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime <= 0f) return false;
        float credited = 0f;
        if (inside)
        {
            entered = true;
            OutsideTime = 0f;
            credited = deltaTime;
        }
        else if (entered)
        {
            credited = Math.Min(deltaTime, Math.Max(0f, graceTime - OutsideTime));
            OutsideTime += deltaTime;
        }
        Elapsed = Math.Min(Math.Max(0f, requiredTime), Elapsed + credited);
        return entered && Elapsed >= Math.Max(0f, requiredTime);
    }

    public void Reset() { Elapsed = OutsideTime = 0f; entered = false; }
}
