using UnityEngine;

// Holds runtime player stat modifiers and converts base values into final gameplay values.
[DisallowMultipleComponent]
public class PlayerStats : MonoBehaviour
{
    [Header("Offense")]
    // Outgoing damage multiplier applied on top of the legacy player damage multiplier.
    public float damageMultiplier = 1f;

    // Probability of a critical hit; 0.25 means a 25% chance.
    [Range(0f, 1f)]
    public float criticalChance = 0f;

    // Total damage multiplier on a critical hit; 2 means double damage.
    [Min(1f)]
    public float criticalDamageMultiplier = 2f;

    // Multiplier applied to experience rewards before they are granted.
    public float experienceGainMultiplier = 1f;

    [Header("Movement")]
    // Multiplier applied to the player's base movement speed.
    public float moveSpeedMultiplier = 1f;

    // Multiplier applied to the player's base dash speed.
    public float dashSpeedMultiplier = 1f;

    // Multiplier applied to the player's base dash cooldown.
    public float dashCooldownMultiplier = 1f;

    [Header("Weapons")]
    // Total projectiles fired per attack, including the original projectile.
    [Min(1)]
    public int projectileCount = 1;

    // Additional enemies a projectile can hit after its first target; 0 means no piercing.
    [Min(0)]
    public int pierceCount = 0;

    // Multiplier applied to the player's base bullet fire cooldown.
    public float fireCooldownMultiplier = 1f;

    // Multiplier applied to the player's base bomb cooldown.
    public float bombCooldownMultiplier = 1f;

    [Header("Defense")]
    // Fraction of incoming damage prevented before health is reduced.
    [Range(0f, 0.9f)]
    public float damageReduction = 0f;

    [Header("Utility")]
    // Multiplier applied to pickup attraction radius.
    public float pickupRadiusMultiplier = 1f;

    // Combines legacy damage scaling with the new stat-system damage multiplier.
    public float GetDamageMultiplier(float legacyDamageMultiplier)
    {
        return Mathf.Max(0f, legacyDamageMultiplier * damageMultiplier);
    }

    // Returns the projectile count with a minimum of one, including runtime changes.
    public int GetProjectileCount()
    {
        return Mathf.Max(1, projectileCount);
    }

    // Returns the nonnegative number of additional targets a projectile can hit.
    public int GetPierceCount()
    {
        return Mathf.Max(0, pierceCount);
    }

    // Returns critical hit probability clamped between zero and one.
    public float GetCriticalChance()
    {
        return Mathf.Clamp01(criticalChance);
    }

    // Returns the total critical damage multiplier, never below normal damage.
    public float GetCriticalDamageMultiplier()
    {
        return Mathf.Max(1f, criticalDamageMultiplier);
    }

    // Returns final movement speed after stat modifiers are applied.
    public float GetMoveSpeed(float baseMoveSpeed)
    {
        return Mathf.Max(0f, baseMoveSpeed * moveSpeedMultiplier);
    }

    // Returns final dash speed after stat modifiers are applied.
    public float GetDashSpeed(float baseDashSpeed)
    {
        return Mathf.Max(0f, baseDashSpeed * dashSpeedMultiplier);
    }

    // Returns final bullet cooldown after stat modifiers are applied.
    public float GetFireCooldown(float baseFireCooldown)
    {
        return Mathf.Max(0.02f, baseFireCooldown * fireCooldownMultiplier);
    }

    // Returns final bomb cooldown after stat modifiers are applied.
    public float GetBombCooldown(float baseBombCooldown)
    {
        return Mathf.Max(0.05f, baseBombCooldown * bombCooldownMultiplier);
    }

    // Returns final dash cooldown after stat modifiers are applied.
    public float GetDashCooldown(float baseDashCooldown)
    {
        return Mathf.Max(0.05f, baseDashCooldown * dashCooldownMultiplier);
    }

    // Returns the multiplier that remains after damage reduction is applied.
    public float GetIncomingDamageMultiplier()
    {
        return Mathf.Max(0f, 1f - Mathf.Clamp01(damageReduction));
    }

    // Returns final pickup attraction radius after stat modifiers are applied.
    public float GetPickupRadius(float basePickupRadius)
    {
        return Mathf.Max(0f, basePickupRadius * pickupRadiusMultiplier);
    }

    // Converts base experience into whole experience points after experience modifiers.
    public int ApplyExperienceGain(int baseExperience)
    {
        return Mathf.Max(0, Mathf.FloorToInt(GetExperienceAmount(baseExperience)));
    }

    // Returns scaled experience as a float so RewardManager can preserve fractional remainders.
    public float GetExperienceAmount(float baseExperience)
    {
        return Mathf.Max(0f, baseExperience * experienceGainMultiplier);
    }

    // Applies all effects from one upgrade definition to this player.
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

    // Applies one upgrade effect to its matching stat bucket.
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

    // Converts a cooldown reduction percentage into a bounded cooldown multiplier.
    private float ApplyCooldownReduction(float currentMultiplier, float reduction)
    {
        return Mathf.Clamp(currentMultiplier * (1f - Mathf.Clamp01(reduction)), 0.1f, 5f);
    }
}
