using UnityEngine;

/// <summary>
/// Holds runtime player stat modifiers and converts base values into final gameplay values.
/// </summary>
[DisallowMultipleComponent]
public class PlayerStats : MonoBehaviour
{
    [Header("Offense")]
    /// <summary>Outgoing damage multiplier applied on top of the legacy player damage multiplier.</summary>
    public float damageMultiplier = 1f;

    /// <summary>Multiplier applied to experience rewards before they are granted.</summary>
    public float experienceGainMultiplier = 1f;

    [Header("Movement")]
    /// <summary>Multiplier applied to the player's base movement speed.</summary>
    public float moveSpeedMultiplier = 1f;

    /// <summary>Multiplier applied to the player's base dash speed.</summary>
    public float dashSpeedMultiplier = 1f;

    /// <summary>Multiplier applied to the player's base dash cooldown.</summary>
    public float dashCooldownMultiplier = 1f;

    [Header("Weapons")]
    /// <summary>Multiplier applied to the player's base bullet fire cooldown.</summary>
    public float fireCooldownMultiplier = 1f;

    /// <summary>Multiplier applied to the player's base bomb cooldown.</summary>
    public float bombCooldownMultiplier = 1f;

    [Header("Defense")]
    /// <summary>Fraction of incoming damage prevented before health is reduced.</summary>
    [Range(0f, 0.9f)]
    public float damageReduction = 0f;

    [Header("Utility")]
    /// <summary>Multiplier applied to pickup attraction radius.</summary>
    public float pickupRadiusMultiplier = 1f;

    /// <summary>
    /// Combines legacy damage scaling with the new stat-system damage multiplier.
    /// </summary>
    public float GetDamageMultiplier(float legacyDamageMultiplier)
    {
        return Mathf.Max(0f, legacyDamageMultiplier * damageMultiplier);
    }

    /// <summary>
    /// Returns final movement speed after stat modifiers are applied.
    /// </summary>
    public float GetMoveSpeed(float baseMoveSpeed)
    {
        return Mathf.Max(0f, baseMoveSpeed * moveSpeedMultiplier);
    }

    /// <summary>
    /// Returns final dash speed after stat modifiers are applied.
    /// </summary>
    public float GetDashSpeed(float baseDashSpeed)
    {
        return Mathf.Max(0f, baseDashSpeed * dashSpeedMultiplier);
    }

    /// <summary>
    /// Returns final bullet cooldown after stat modifiers are applied.
    /// </summary>
    public float GetFireCooldown(float baseFireCooldown)
    {
        return Mathf.Max(0.02f, baseFireCooldown * fireCooldownMultiplier);
    }

    /// <summary>
    /// Returns final bomb cooldown after stat modifiers are applied.
    /// </summary>
    public float GetBombCooldown(float baseBombCooldown)
    {
        return Mathf.Max(0.05f, baseBombCooldown * bombCooldownMultiplier);
    }

    /// <summary>
    /// Returns final dash cooldown after stat modifiers are applied.
    /// </summary>
    public float GetDashCooldown(float baseDashCooldown)
    {
        return Mathf.Max(0.05f, baseDashCooldown * dashCooldownMultiplier);
    }

    /// <summary>
    /// Returns the multiplier that remains after damage reduction is applied.
    /// </summary>
    public float GetIncomingDamageMultiplier()
    {
        return Mathf.Max(0f, 1f - Mathf.Clamp01(damageReduction));
    }

    /// <summary>
    /// Returns final pickup attraction radius after stat modifiers are applied.
    /// </summary>
    public float GetPickupRadius(float basePickupRadius)
    {
        return Mathf.Max(0f, basePickupRadius * pickupRadiusMultiplier);
    }

    /// <summary>
    /// Converts base experience into whole experience points after experience modifiers.
    /// </summary>
    public int ApplyExperienceGain(int baseExperience)
    {
        return Mathf.Max(0, Mathf.FloorToInt(GetExperienceAmount(baseExperience)));
    }

    /// <summary>
    /// Returns scaled experience as a float so RewardManager can preserve fractional remainders.
    /// </summary>
    public float GetExperienceAmount(float baseExperience)
    {
        return Mathf.Max(0f, baseExperience * experienceGainMultiplier);
    }

    /// <summary>
    /// Applies all effects from one upgrade definition to this player.
    /// </summary>
    public void ApplyUpgrade(UpgradeDefinition upgrade, PlayerHealth health)
    {
        if (upgrade == null || upgrade.effects == null)
        {
            return;
        }

        foreach (UpgradeEffect effect in upgrade.effects)
        {
            ApplyEffect(effect, health);
        }
    }

    /// <summary>
    /// Applies one upgrade effect to its matching stat bucket.
    /// </summary>
    private void ApplyEffect(UpgradeEffect effect, PlayerHealth health)
    {
        switch (effect.effectType)
        {
            case UpgradeEffectType.DamageMultiplier:
                damageMultiplier = Mathf.Max(0f, damageMultiplier + effect.value);
                break;
            case UpgradeEffectType.MoveSpeedMultiplier:
                moveSpeedMultiplier = Mathf.Max(0f, moveSpeedMultiplier + effect.value);
                break;
            case UpgradeEffectType.DashSpeedMultiplier:
                dashSpeedMultiplier = Mathf.Max(0f, dashSpeedMultiplier + effect.value);
                break;
            case UpgradeEffectType.FireCooldownReduction:
                fireCooldownMultiplier = ApplyCooldownReduction(fireCooldownMultiplier, effect.value);
                break;
            case UpgradeEffectType.BombCooldownReduction:
                bombCooldownMultiplier = ApplyCooldownReduction(bombCooldownMultiplier, effect.value);
                break;
            case UpgradeEffectType.DashCooldownReduction:
                dashCooldownMultiplier = ApplyCooldownReduction(dashCooldownMultiplier, effect.value);
                break;
            case UpgradeEffectType.MaxHealth:
                if (health != null)
                {
                    health.IncreaseMaxHealth(effect.value, true);
                }
                break;
            case UpgradeEffectType.Heal:
                if (health != null)
                {
                    health.Heal(effect.value);
                }
                break;
            case UpgradeEffectType.DamageReduction:
                damageReduction = Mathf.Clamp(damageReduction + effect.value, 0f, 0.9f);
                break;
            case UpgradeEffectType.PickupRadiusMultiplier:
                pickupRadiusMultiplier = Mathf.Max(0f, pickupRadiusMultiplier + effect.value);
                break;
            case UpgradeEffectType.ExperienceGainMultiplier:
                experienceGainMultiplier = Mathf.Max(0f, experienceGainMultiplier + effect.value);
                break;
        }
    }

    /// <summary>
    /// Converts a cooldown reduction percentage into a bounded cooldown multiplier.
    /// </summary>
    private float ApplyCooldownReduction(float currentMultiplier, float reduction)
    {
        return Mathf.Clamp(currentMultiplier * (1f - Mathf.Clamp01(reduction)), 0.1f, 5f);
    }
}
