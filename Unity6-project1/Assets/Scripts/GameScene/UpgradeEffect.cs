using System;

/// <summary>
/// Rarity bucket for upgrades, intended for future weighting and UI styling.
/// </summary>
public enum UpgradeRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

/// <summary>
/// Gameplay stat bucket affected by an upgrade effect.
/// </summary>
public enum UpgradeEffectType
{
    DamageMultiplier,
    MoveSpeedMultiplier,
    DashSpeedMultiplier,
    FireCooldownReduction,
    BombCooldownReduction,
    DashCooldownReduction,
    MaxHealth,
    Heal,
    DamageReduction,
    PickupRadiusMultiplier,
    ExperienceGainMultiplier
}

/// <summary>
/// Serializable data entry describing one stat change in an upgrade.
/// </summary>
[Serializable]
public struct UpgradeEffect
{
    /// <summary>Stat bucket modified by this effect.</summary>
    public UpgradeEffectType effectType;

    /// <summary>Effect amount. Multipliers are additive deltas; cooldown reductions are percentages.</summary>
    public float value;

    /// <summary>
    /// Creates an effect with a target stat bucket and numeric value.
    /// </summary>
    public UpgradeEffect(UpgradeEffectType effectType, float value)
    {
        this.effectType = effectType;
        this.value = value;
    }
}
