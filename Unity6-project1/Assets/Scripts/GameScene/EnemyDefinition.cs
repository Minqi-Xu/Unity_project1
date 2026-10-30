using System;
using UnityEngine;

public enum EnemyKind { Chaser, Runner, Tank, Ranged }
public enum EnemyRank { Normal, Elite, MiniBoss, FinalBoss }
[Flags]
public enum EnemyAffix { None = 0, Swift = 1, Healthy = 2, HasteAura = 4, DeathBlast = 8, PeriodicDash = 16 }

[CreateAssetMenu(fileName = "EnemyDefinition", menuName = "Roguelite/Enemy Definition")]
public class EnemyDefinition : ScriptableObject
{
    public string displayName = "Chaser";
    public EnemyKind kind;
    [Min(0f)] public float unlockTime;
    [Min(.01f)] public float weight = 1f;
    [Min(.01f)] public float healthMultiplier = 1f;
    [Min(.01f)] public float speedMultiplier = 1f;
    [Min(.01f)] public float damageMultiplier = 1f;
    [Min(.1f)] public float sizeMultiplier = 1f;
    public Color tint = new Color(.85f, .28f, .28f);
    [Min(1f)] public float preferredRange = 6f;
    [Min(.2f)] public float attackInterval = 3f;
    [Min(.1f)] public float projectileSpeed = 5f;
}

[Serializable]
public class EnemyEncounter
{
    [Min(0f)] public float time;
    public string title;
    public EnemyRank rank = EnemyRank.Elite;
    public EnemyKind kind;
    public EnemyAffix affixes;
    public bool IsDue(float elapsed) => elapsed >= time;
    public bool IsWarning(float elapsed, float warningTime) => elapsed >= time - Mathf.Max(0f, warningTime) && elapsed < time;
    public static EnemyEncounter[] Defaults() => new[]
    {
        At(120, "Elite: Swift Hunter", EnemyRank.Elite, EnemyKind.Runner, EnemyAffix.Swift),
        At(300, "Elite: War Banner", EnemyRank.Elite, EnemyKind.Tank, EnemyAffix.Healthy | EnemyAffix.HasteAura),
        At(480, "Mini Boss: Ravager", EnemyRank.MiniBoss, EnemyKind.Chaser, EnemyAffix.PeriodicDash),
        At(600, "Elite: Volatile Archer", EnemyRank.Elite, EnemyKind.Ranged, EnemyAffix.DeathBlast),
        At(900, "Mini Boss: Siege Beast", EnemyRank.MiniBoss, EnemyKind.Tank, EnemyAffix.Healthy | EnemyAffix.DeathBlast),
        At(1080, "Elite: Storm Leader", EnemyRank.Elite, EnemyKind.Runner, EnemyAffix.Swift | EnemyAffix.HasteAura | EnemyAffix.PeriodicDash),
        At(1200, "Final Boss: Overlord", EnemyRank.FinalBoss, EnemyKind.Ranged, EnemyAffix.Healthy | EnemyAffix.PeriodicDash)
    };
    private static EnemyEncounter At(float time, string title, EnemyRank rank, EnemyKind kind, EnemyAffix affixes) =>
        new EnemyEncounter { time = time, title = title, rank = rank, kind = kind, affixes = affixes };
}
