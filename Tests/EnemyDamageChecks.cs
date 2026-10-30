using System;

public static class EnemyDamageChecks
{
    public static void Run()
    {
        Near(Hit(EnemyRank.Normal), 122f, "Normal vulnerability only applies to normal enemies");
        Near(Hit(EnemyRank.Elite), 127f, "Elite vulnerability excludes bosses");
        Near(Hit(EnemyRank.MiniBoss), 137f, "Mini bosses use boss vulnerability");
        Near(Hit(EnemyRank.FinalBoss), 137f, "Final bosses use boss vulnerability");
        Near(Hit(EnemyRank.FinalBoss, .3f), 172f, "Execute activates at exactly 30% health");
        Near(Hit(EnemyRank.FinalBoss, .3001f), 137f, "Execute does not activate above threshold");
        Near(Hit(EnemyRank.FinalBoss, 1f, false, true), 130f, "Secondary projectile effects do not reapply fixed damage");
        Near(Hit(EnemyRank.FinalBoss, .3f, false, false), 165f, "Damage over time benefits from vulnerability and execute without flat procs");
        Near(Hit(EnemyRank.Normal, 1f, true, false), 118f, "Non-projectile direct hits get only the general fixed bonus");
        Near(EnemyDamageRules.Resolve(0f, EnemyRank.Normal, 1f, 1, 1, 1, 1, 10, 10, true, true), 0f, "Zero damage does not create an enchantment proc");
        Near(EnemyDamageRules.Resolve(200f, EnemyRank.Normal, 1f, .15f, 0, 0, 0, 3, 4, true, true), 237f, "Fixed seven damage stays fixed when incoming damage doubles");
        Console.WriteLine("PASS: normal/elite/boss vulnerability, execute threshold, fixed additions and proc suppression.");
    }
    private static float Hit(EnemyRank rank, float health = 1f, bool direct = true, bool projectile = true) =>
        EnemyDamageRules.Resolve(100f, rank, health, .15f, .2f, .3f, .35f, 3f, 4f, direct, projectile);
    private static void Near(float actual, float expected, string label)
    { if (Math.Abs(actual - expected) > .001f) throw new Exception(label + ": " + actual + " != " + expected); }
}
