using System;

// Standalone checks against the production float boundary/progress helpers.
public static class ExperienceMathChecks
{
    private static void Check(bool ok, string label) { if (!ok) throw new Exception(label); }
    private static void Near(float actual, float expected, string label) => Check(Math.Abs(actual - expected) < .00001f, label);

    public static void Main()
    {
        Near(ExperienceMath.Progress(1.15f, 5f), .23f, "Fractional grant percentage");
        Near(ExperienceMath.Progress(1.25f, 2.5f), .5f, "Fractional threshold percentage");
        Near(ExperienceMath.Progress(-1f, 5f), 0f, "Lower bound");
        Near(ExperienceMath.Progress(9f, 5f), 1f, "Upper bound");
        Near(ExperienceMath.Progress(1f, 0f), 0f, "Zero threshold");
        Near(ExperienceMath.Progress(float.NaN, 5f), 0f, "Invalid current XP");
        Near(ExperienceMath.Progress(1f, float.PositiveInfinity), 0f, "Invalid threshold");
        float current = 0f;
        for (int i = 0; i < 50; i++) current += .1f;
        Check(ExperienceMath.CanLevelUp(current, 5f), "Fifty 0.1-point pickups must reach five");
        Check(!ExperienceMath.CanLevelUp(4.99f, 5f), "Meaningful missing XP must not grant a level");
        Check(!ExperienceMath.CanLevelUp(float.NaN, 5f), "NaN cannot grant a level");
        Check(!ExperienceMath.CanLevelUp(float.PositiveInfinity, 5f), "Infinity cannot grant a level");
        Check(ExperienceMath.CanLevelUp(2.5f, 2.5f), "Exact fractional threshold");
        Check(!ExperienceMath.CanLevelUp(0f, float.Epsilon), "Zero XP must never grant levels even at a tiny threshold");
        Check(!ExperienceMath.IsPositiveFinite(-1f) && !ExperienceMath.IsPositiveFinite(float.NaN) &&
            !ExperienceMath.IsPositiveFinite(float.PositiveInfinity), "Invalid grants are rejected");
        Console.WriteLine("PASS: production float XP progress, fractional thresholds, rounding boundaries and invalid-value guards.");
    }
}
