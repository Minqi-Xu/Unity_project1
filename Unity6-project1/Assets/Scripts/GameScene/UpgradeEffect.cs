using System;

// Rarity bucket for upgrades, intended for future weighting and UI styling.
public enum UpgradeRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

// Gameplay stat bucket affected by an upgrade effect.
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

// Serializable data entry describing one stat change in an upgrade.
[Serializable]
public struct UpgradeEffect
{
    // Stat bucket modified by this effect.
    public UpgradeEffectType effectType;

    // Effect amount. Multipliers are additive deltas; cooldown reductions are percentages.
    public float value;

    // Creates an effect with a target stat bucket and numeric value.
    public UpgradeEffect(UpgradeEffectType effectType, float value)
    {
        this.effectType = effectType;
        this.value = value;
    }
}
