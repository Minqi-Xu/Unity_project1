using System;

public static class WeaponWaveChecks
{
    private static void Check(bool ok, string label) { if (!ok) throw new Exception(label); }
    private static void Near(float actual, float expected, string label) => Check(Math.Abs(actual - expected) < .00001f, label);
    public static void Main()
    {
        Check(WeaponWaveRules.Count(1, 0f) == 1, "One default emission");
        Check(WeaponWaveRules.Count(2, 0f) == 2, "Basic upgrade adds a second wave");
        Check(WeaponWaveRules.Count(2, 1f) == 3, "Dedicated and basic wave upgrades add");
        Check(WeaponWaveRules.Count(100, 20f) == 32, "Wave safety cap");
        Near(WeaponWaveRules.Interval(.12f, .95f, 2), .12f, "Shotgun wave spacing");
        Near(WeaponWaveRules.Interval(.12f, .2f, 2), .1f, "High attack speed fits waves into one cycle");
        for (int count = 2; count <= 32; count++)
            foreach (float cooldown in new[] { .02f, .2f, .65f, .95f, 1.96f })
                Check(WeaponWaveRules.Interval(.12f, cooldown, count) * (count - 1) < cooldown, "Burst completes before next attack");
        foreach (WeaponKind kind in Enum.GetValues(typeof(WeaponKind)))
            Check(WeaponWaveRules.Supports(kind) == (kind == WeaponKind.Shotgun), "Only shotgun repeats volleys: " + kind);
        Check((int)UpgradeEffectType.ProjectileCount == 12, "Serialized basic buff enum ID preserved");
        float time = 0f;
        bool paused = false, alive = true;
        int shots = 0;
        var emission = WeaponWaveRules.Emit(2, .12f, () => alive, () => shots++, () => time, () => paused);
        Check(emission.MoveNext() && shots == 1, "First wave fires immediately, second waits");
        time = .1f;
        Check(emission.MoveNext() && shots == 1, "No early second wave");
        time = .12f; paused = true;
        Check(emission.MoveNext() && shots == 1, "Pause prevents a due wave");
        paused = false;
        Check(!emission.MoveNext() && shots == 2, "Unpause emits the second wave exactly once");
        time = 0f; shots = 0;
        emission = WeaponWaveRules.Emit(3, .1f, () => alive, () => shots++, () => time, () => paused);
        Check(emission.MoveNext() && shots == 1, "Three-wave start");
        time = .4f;
        Check(!emission.MoveNext() && shots == 3, "Late frames catch up remaining waves without dropping emissions");
        time = 0f; shots = 0;
        emission = WeaponWaveRules.Emit(3, .1f, () => alive, () => shots++, () => time, () => paused);
        Check(emission.MoveNext() && shots == 1, "Cancellation test start");
        alive = false;
        Check(!emission.MoveNext() && shots == 1, "Death/destruction cancels remaining waves");
        Console.WriteLine("PASS: shotgun-only waves, cadence and emitter: immediate first wave, delayed repeats, pause, catch-up, cancellation and stable enum ID.");
    }
}
