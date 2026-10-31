using System;

public static class AdditiveStatChecks
{
    private static void Near(float actual, float expected, string label)
    {
        if (Math.Abs(actual - expected) > .0001f) throw new Exception(label + ": " + actual + " != " + expected);
    }

    public static void Main()
    {
        Near(AdditiveStatMath.DamageMultiplier(1f, 1f, 1f, 0f), 1f, "No buffs");
        Near(AdditiveStatMath.DamageMultiplier(1.2f, 1.3f, 1.4f, .5f), 2.4f, "All outgoing damage bonuses add");
        Near(AdditiveStatMath.DamageMultiplier(1.2f, 1.3f, 1f, .5f), 2f, "Auxiliary excludes primary bonus");
        float reduction = 1f;
        for (int i = 0; i < 3; i++) reduction = AdditiveStatMath.AddCooldownReduction(reduction, .1f);
        Near(reduction, .7f, "Three ten-percent reductions total thirty percent");
        Near(AdditiveStatMath.CooldownMultiplier(reduction, .8f, .1f), .4f, "Specific, global and weapon cooldown reductions add");
        Near(AdditiveStatMath.CooldownMultiplier(.2f, .2f, .6f), .1f, "Combined reduction keeps ninety-percent cap");
        Near(AdditiveStatMath.AttackSpeedMultiplier(1.5f, .3f), 1.8f, "Permanent and dash attack speed add");
        Console.WriteLine("PASS: production additive damage, cooldown reduction, attack speed and cooldown caps.");
    }
}
