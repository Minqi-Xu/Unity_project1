using System;

public enum UpgradeRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

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

[Serializable]
public struct UpgradeEffect
{
    public UpgradeEffectType effectType;
    public float value;

    public UpgradeEffect(UpgradeEffectType effectType, float value)
    {
        this.effectType = effectType;
        this.value = value;
    }
}
