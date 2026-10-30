using System;
using System.Linq;

// Runs against production encounter data without starting the Unity editor.
public static class EnemyScheduleChecks
{
    public static void Main()
    {
        EnemyEncounter[] entries = EnemyEncounter.Defaults();
        Check(entries.Select(x => x.time).SequenceEqual(new float[] { 120, 300, 480, 600, 900, 1080, 1200 }), "Exact seven encounter times");
        Check(entries.Count(x => x.rank == EnemyRank.Elite) == 4, "Four elites");
        Check(entries.Count(x => x.rank == EnemyRank.MiniBoss) == 2, "Two mini bosses");
        Check(entries.Last().rank == EnemyRank.FinalBoss, "Final boss at twenty minutes");
        foreach (EnemyEncounter entry in entries)
        {
            Check(!entry.IsWarning(entry.time - 2.01f, 2f), "No early warning");
            Check(entry.IsWarning(entry.time - 2f, 2f), "Warning starts exactly two seconds before spawn");
            Check(entry.IsWarning(entry.time - .01f, 2f), "Warning remains until spawn");
            Check(!entry.IsDue(entry.time - .01f), "Never spawn before scheduled time");
            Check(entry.IsDue(entry.time) && entry.IsDue(entry.time + 10f), "Due even when a frame crosses the boundary");
            Check(!entry.IsWarning(entry.time, 2f), "Warning ends at spawn");
            Check(!entry.IsWarning(entry.time - .1f, -2f), "Negative warning duration is clamped");
        }
        EnemyAffix covered = entries.Aggregate(EnemyAffix.None, (value, entry) => value | entry.affixes);
        Check(covered == (EnemyAffix.Swift | EnemyAffix.Healthy | EnemyAffix.HasteAura | EnemyAffix.DeathBlast | EnemyAffix.PeriodicDash), "All five affixes appear in encounters");
        foreach (float average in new[] { 4f, 4.7833333f, 6f })
            foreach (EnemyRank rank in new[] { EnemyRank.Elite, EnemyRank.MiniBoss, EnemyRank.FinalBoss })
            {
                float cap = average * 1.05f;
                foreach (float requested in new[] { 0f, 1f, 4.95f * 1.5f, 50f, 1000f })
                    Check(EnemyMovementBalance.LimitSpeed(requested, average, rank) <= cap, "Swift, dash and stacked multipliers respect starting-speed cap");
                Check(EnemyMovementBalance.LimitSpeed(1f, average, rank) == 1f, "Slow enemies are never sped up to match the cap");
                Check(EnemyMovementBalance.LimitSpeed(cap, average, rank) == cap, "Cap boundary is preserved");
            }
        Check(EnemyMovementBalance.LimitSpeed(8f, 4f, EnemyRank.Normal) == 8f, "Normal enemies retain their own movement tuning");
        Check(EnemyMovementBalance.LimitSpeed(50f, 0f, EnemyRank.Elite) == 5f * 1.05f, "Missing roster uses the baseline fallback");
        EnemyEncounter[] bosses = entries.Where(x => x.rank >= EnemyRank.MiniBoss).ToArray();
        Check(bosses[0].bossMovementMultiplier == 1f && bosses[0].bossRelativeSpeedLimit == 1.05f, "First boss retains current movement");
        Check(bosses[1].bossMovementMultiplier == 1.25f && bosses[1].bossRelativeSpeedLimit == 1.15f, "Second boss gains 25% movement and a 115% cap");
        Check(bosses[2].bossMovementMultiplier == 1.35f && bosses[2].bossRelativeSpeedLimit == 1.2f, "Final boss gains 35% movement and a 120% cap");
        Check(entries.Where(x => x.rank == EnemyRank.Elite).All(x => x.bossMovementMultiplier == 1f && x.bossRelativeSpeedLimit == 1.05f), "Elite configuration remains unchanged");
        Check(EnemyMovementBalance.LimitSpeed(100f, 5f, EnemyRank.MiniBoss, 1.15f) == 5f * 1.15f, "Later boss can exceed the first boss cap");
        Check(EnemyMovementBalance.LimitSpeed(100f, 5f, EnemyRank.FinalBoss, 10f) == 6f, "Boss cap never exceeds 120%, including dash");
        Check(EnemyMovementBalance.LimitSpeed(100f, 5f, EnemyRank.Elite, 1.2f) == 5f * 1.05f, "Elite cap cannot be raised by boss configuration");
        Console.WriteLine("PASS: encounter schedule, ranks, warning and spawn boundaries, all five affixes.");
        Console.WriteLine("PASS: elite and boss movement cap, Swift/dash multiplier limits, slower speeds preserved.");
    }
    private static void Check(bool condition, string label) { if (!condition) throw new Exception(label); }
}
