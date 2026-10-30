using UnityEngine;
using UnityEngine.Serialization;

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

    // Primary-weapon bonus; auxiliaries and RMB bombs do not inherit it.
    [FormerlySerializedAs("bulletDamageMultiplier")]
    public float primaryWeaponDamageMultiplier = 1f;
    // Radius multiplier for bombs and final-pierce explosions.
    public float explosionRadiusMultiplier = 1f;
    // Multiplier shared by bullet, bomb, and dash cooldowns.
    public float globalCooldownMultiplier = 1f;
    // Health restored every five seconds of active gameplay.
    public float regeneration = 0f;
    // Additional seconds of immunity after a successful incoming hit.
    public float hitInvulnerability = 0f;
    // Multiplier for the speed of attracted experience pickups.
    public float pickupSpeedMultiplier = 1f;
    // Fraction of maximum health restored on each future level-up.
    public float levelUpHealing = 0f;
    // Global damage multiplier added on each future level-up.
    public float levelUpDamage = 0f;
    // Character-specific scaling for future per-level damage growth.
    public float levelDamageGrowthMultiplier = 1f;
    // Fraction of bullet damage dealt by an explosion on the final allowed hit.
    public float finalPierceExplosion = 0f;
    // Attack-speed bonus after dashing, scaled by movement-speed bonuses.
    public float dashAttackSpeed = 0f;
    // Maximum additional critical chance at zero health, scaled by missing health.
    public float lowHealthCriticalChance = 0f;
    // Scaled-time regeneration accumulator and temporary attack-speed expiry.
    private float regenerationTimer;
    private float dashAttackSpeedUntil;

    // Regeneration stops during pauses and never revives a dead player.
    private void Update()
    {
        if (regeneration <= 0f) { regenerationTimer = 0f; return; }
        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health == null || health.currentHealth <= 0f) return;
        regenerationTimer += Time.deltaTime;
        while (regenerationTimer >= 5f)
        {
            regenerationTimer -= 5f;
            HealWithLog(health, regeneration);
        }
    }

    // Applies growth bonuses owned before this level was earned.
    public void OnLevelUp(PlayerHealth health)
    {
        damageMultiplier += Mathf.Max(0f, levelUpDamage) * Mathf.Max(0f, levelDamageGrowthMultiplier);
        if (health != null) HealWithLog(health, health.maxHealth * Mathf.Clamp01(levelUpHealing));
    }

    // Refreshes, rather than stacks, a three-second post-dash attack-speed window.
    public void OnDashCompleted()
    {
        dashAttackSpeedUntil = Time.time + 3f;
    }

    // Rolls one critical hit per target; callers supply their weapon base damage.
    public float CalculateDamage(float baseDamage, float legacyMultiplier, bool primaryWeapon)
    {
        float result = Mathf.Max(0f, baseDamage) * GetDamageMultiplier(legacyMultiplier);
        if (primaryWeapon) result *= Mathf.Max(0f, primaryWeaponDamageMultiplier);
        float chance = GetCriticalChance();
        if (chance >= 1f || (chance > 0f && Random.value < chance)) result *= GetCriticalDamageMultiplier();
        return result;
    }

    // Logs actual passive healing without emitting messages at full health.
    private void HealWithLog(PlayerHealth health, float amount)
    {
        if (health.currentHealth <= 0f || amount <= 0f || health.currentHealth >= health.maxHealth) return;
        health.Heal(amount);
        GetComponent<PlayerController>()?.DebugLogPlayerStatus("Buff Healing", 0f);
    }

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

    // Retain serialized legacy cooldown multipliers when loading existing prefabs.
    [FormerlySerializedAs("fireCooldownMultiplier")]
    public float primaryWeaponCooldownMultiplier = 1f;
    public float primaryWeaponAttackSpeedMultiplier = 1f;

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
        PlayerHealth health = GetComponent<PlayerHealth>();
        float missingHealth = health != null && health.maxHealth > 0f
            ? 1f - Mathf.Clamp01(health.currentHealth / health.maxHealth) : 0f;
        return Mathf.Clamp01(criticalChance + Mathf.Max(0f, lowHealthCriticalChance) * missingHealth);
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

    // Returns final primary cooldown after its attack-speed and global modifiers.
    public float GetPrimaryWeaponCooldown(float baseFireCooldown)
    {
        float attackSpeed = Time.time < dashAttackSpeedUntil
            ? 1f + Mathf.Max(0f, dashAttackSpeed) * Mathf.Max(1f, moveSpeedMultiplier) : 1f;
        return Mathf.Max(0.02f, baseFireCooldown * primaryWeaponCooldownMultiplier * globalCooldownMultiplier /
            (Mathf.Max(.01f, primaryWeaponAttackSpeedMultiplier) * attackSpeed));
    }

    // Auxiliary weapons inherit global cooldown, not primary attack-speed buffs.
    public float GetAuxiliaryWeaponCooldown(float baseCooldown)
    {
        return Mathf.Max(.02f, baseCooldown * globalCooldownMultiplier);
    }

    // Returns final bomb cooldown after stat modifiers are applied.
    public float GetBombCooldown(float baseBombCooldown)
    {
        return Mathf.Max(0.05f, baseBombCooldown * bombCooldownMultiplier * globalCooldownMultiplier);
    }

    // Returns final dash cooldown after stat modifiers are applied.
    public float GetDashCooldown(float baseDashCooldown)
    {
        return Mathf.Max(0.05f, baseDashCooldown * dashCooldownMultiplier * globalCooldownMultiplier);
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
            case UpgradeEffectType.PrimaryWeaponDamageMultiplier:
                primaryWeaponDamageMultiplier = Mathf.Max(0f, primaryWeaponDamageMultiplier + effect.value); break;
            case UpgradeEffectType.ProjectileCount:
                projectileCount = Mathf.Max(1, projectileCount + Mathf.RoundToInt(effect.value)); break;
            case UpgradeEffectType.PierceCount:
                pierceCount = Mathf.Max(0, pierceCount + Mathf.RoundToInt(effect.value)); break;
            case UpgradeEffectType.CriticalChance:
                criticalChance = Mathf.Clamp01(criticalChance + effect.value); break;
            case UpgradeEffectType.CriticalDamageMultiplier:
                criticalDamageMultiplier = Mathf.Max(1f, criticalDamageMultiplier + effect.value); break;
            case UpgradeEffectType.ExplosionRadiusMultiplier:
                explosionRadiusMultiplier = Mathf.Max(0f, explosionRadiusMultiplier + effect.value); break;
            case UpgradeEffectType.Regeneration:
                regeneration = Mathf.Max(0f, regeneration + effect.value); break;
            case UpgradeEffectType.HitInvulnerability:
                hitInvulnerability = Mathf.Max(0f, hitInvulnerability + effect.value); break;
            case UpgradeEffectType.PickupSpeedMultiplier:
                pickupSpeedMultiplier = Mathf.Max(0f, pickupSpeedMultiplier + effect.value); break;
            case UpgradeEffectType.LevelUpHealing:
                levelUpHealing = Mathf.Clamp01(levelUpHealing + effect.value); break;
            case UpgradeEffectType.LevelUpDamage:
                levelUpDamage = Mathf.Max(0f, levelUpDamage + effect.value); break;
            case UpgradeEffectType.GlobalCooldownReduction:
                globalCooldownMultiplier = ApplyCooldownReduction(globalCooldownMultiplier, effect.value); break;
            case UpgradeEffectType.FinalPierceExplosion:
                finalPierceExplosion = Mathf.Max(0f, finalPierceExplosion + effect.value); break;
            case UpgradeEffectType.DashAttackSpeed:
                dashAttackSpeed = Mathf.Max(0f, dashAttackSpeed + effect.value); break;
            case UpgradeEffectType.LowHealthCriticalChance:
                lowHealthCriticalChance = Mathf.Clamp01(lowHealthCriticalChance + effect.value); break;
            case UpgradeEffectType.DamageMultiplier:
                damageMultiplier = Mathf.Max(0f, damageMultiplier + effect.value);
                break;
            case UpgradeEffectType.MoveSpeedMultiplier:
                moveSpeedMultiplier = Mathf.Max(0f, moveSpeedMultiplier + effect.value);
                break;
            case UpgradeEffectType.DashSpeedMultiplier:
                dashSpeedMultiplier = Mathf.Max(0f, dashSpeedMultiplier + effect.value);
                break;
            case UpgradeEffectType.PrimaryWeaponAttackSpeed:
                primaryWeaponAttackSpeedMultiplier = Mathf.Max(.01f, primaryWeaponAttackSpeedMultiplier + effect.value);
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
