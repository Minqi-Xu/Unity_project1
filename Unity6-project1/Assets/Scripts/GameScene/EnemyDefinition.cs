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
    // Legacy serialized data; normal spawn timing and weights now live on EnemySpawner.
    [HideInInspector, Min(0f)] public float unlockTime;
    [HideInInspector, Min(.01f)] public float weight = 1f;
    [Min(.01f)] public float healthMultiplier = 1f;
    [Min(.01f)] public float speedMultiplier = 1f;
    [Header("Elite / Boss movement")]
    [Range(.1f, 1f)] public float eliteSpeedMultiplier = .65f;
    [Range(.1f, 1f)] public float bossSpeedMultiplier = .6f;
    [Min(1f)] public float bossDashSpeedMultiplier = 2f;
    [Min(.01f)] public float damageMultiplier = 1f;
    [Min(.1f)] public float sizeMultiplier = 1f;
    public Color tint = new Color(.85f, .28f, .28f);
    [Min(1f)] public float preferredRange = 6f;
    [Min(.2f)] public float attackInterval = 3f;
    [Min(.1f)] public float projectileSpeed = 5f;
}

[Serializable]
public class NormalEnemySpawnRule
{
    public EnemyKind kind;
    [Min(0f), Tooltip("Seconds from run start. Pause time is excluded.")]
    public float unlockTime;
    [Min(0f), Tooltip("Relative spawn weight after unlocking. Zero disables this kind.")]
    public float weight = 1f;
    public bool IsAvailable(float elapsed) => weight > 0f && elapsed >= unlockTime;
    public static NormalEnemySpawnRule[] Defaults() => new[]
    {
        new NormalEnemySpawnRule { kind = EnemyKind.Chaser, unlockTime = 0f },
        new NormalEnemySpawnRule { kind = EnemyKind.Runner, unlockTime = 60f },
        new NormalEnemySpawnRule { kind = EnemyKind.Tank, unlockTime = 120f },
        new NormalEnemySpawnRule { kind = EnemyKind.Ranged, unlockTime = 300f }
    };
}

[Serializable]
public class EnemyEncounter
{
    [Min(0f)] public float time;
    public string title;
    public EnemyRank rank = EnemyRank.Elite;
    public EnemyKind kind;
    public EnemyAffix affixes;
    public bool guaranteePrimaryWeaponBuff;
    [Header("Per-encounter Boss movement")]
    [Min(.1f)] public float bossMovementMultiplier = 1f;
    [Range(.1f, 1.2f)] public float bossRelativeSpeedLimit = 1.05f;
    public bool IsDue(float elapsed) => elapsed >= time;
    public bool IsWarning(float elapsed, float warningTime) => elapsed >= time - Mathf.Max(0f, warningTime) && elapsed < time;
    public static EnemyEncounter[] Defaults() => new[]
    {
        At(120, "Elite: Swift Hunter", EnemyRank.Elite, EnemyKind.Runner, EnemyAffix.Swift),
        At(240, "Elite: War Banner", EnemyRank.Elite, EnemyKind.Tank, EnemyAffix.Healthy | EnemyAffix.HasteAura),
        At(360, "Mini Boss: Ravager", EnemyRank.MiniBoss, EnemyKind.Chaser, EnemyAffix.PeriodicDash, primaryReward: true),
        At(420, "Elite: Volatile Archer", EnemyRank.Elite, EnemyKind.Ranged, EnemyAffix.DeathBlast),
        At(540, "Mini Boss: Siege Beast", EnemyRank.MiniBoss, EnemyKind.Tank, EnemyAffix.Healthy | EnemyAffix.DeathBlast, 1.25f, 1.15f),
        At(600, "Elite: Storm Leader", EnemyRank.Elite, EnemyKind.Runner, EnemyAffix.Swift | EnemyAffix.HasteAura | EnemyAffix.PeriodicDash),
        At(720, "Final Boss: Overlord", EnemyRank.FinalBoss, EnemyKind.Ranged, EnemyAffix.Healthy | EnemyAffix.PeriodicDash, 1.35f, 1.2f)
    };
    private static EnemyEncounter At(float time, string title, EnemyRank rank, EnemyKind kind, EnemyAffix affixes,
        float movementMultiplier = 1f, float relativeSpeedLimit = 1.05f, bool primaryReward = false) =>
        new EnemyEncounter { time = time, title = title, rank = rank, kind = kind, affixes = affixes,
            bossMovementMultiplier = movementMultiplier, bossRelativeSpeedLimit = relativeSpeedLimit, guaranteePrimaryWeaponBuff = primaryReward };
}
