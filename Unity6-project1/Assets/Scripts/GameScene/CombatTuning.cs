using UnityEngine;

// Shared Inspector settings loaded automatically for every player and enemy.
[CreateAssetMenu(menuName = "Game/Combat Tuning")]
public class CombatTuning : ScriptableObject
{
    [Header("Global Combat Testing")]
    [Tooltip("Final damage dealt to enemies by all weapons and effects. 1 = normal, 2 = double, 0 = no damage.")]
    [Min(0f)] public float playerDamageMultiplier = 1f;

    [Tooltip("Fraction of final incoming player damage prevented, after the per-hit cap. 0 = normal, 0.5 = 50% reduction, 1 = no damage.")]
    [Range(0f, 1f)] public float playerDamageMitigation = 0f;

    private static CombatTuning shared;
    private static CombatTuning Shared
    {
        get
        {
            if (shared == null) shared = Resources.Load<CombatTuning>("CombatTuning");
            return shared;
        }
    }

    public static float ApplyPlayerDamage(float damage)
    {
        CombatTuning settings = Shared;
        return settings != null ? damage * Mathf.Max(0f, settings.playerDamageMultiplier) : damage;
    }

    public static float ApplyIncomingDamage(float damage)
    {
        CombatTuning settings = Shared;
        return settings != null ? damage * (1f - Mathf.Clamp01(settings.playerDamageMitigation)) : damage;
    }
}
