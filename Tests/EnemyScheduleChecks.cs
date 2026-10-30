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
        Console.WriteLine("PASS: encounter schedule, ranks, warning and spawn boundaries, all five affixes.");
    }
    private static void Check(bool condition, string label) { if (!condition) throw new Exception(label); }
}
