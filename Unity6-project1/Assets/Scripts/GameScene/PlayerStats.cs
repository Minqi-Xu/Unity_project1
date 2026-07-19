using UnityEngine;

[DisallowMultipleComponent]
public class PlayerStats : MonoBehaviour
{
    [Header("Offense")]
    public float damageMultiplier = 1f;
    public float experienceGainMultiplier = 1f;

    [Header("Movement")]
    public float moveSpeedMultiplier = 1f;
    public float dashSpeedMultiplier = 1f;
    public float dashCooldownMultiplier = 1f;

    [Header("Weapons")]
    public float fireCooldownMultiplier = 1f;
    public float bombCooldownMultiplier = 1f;

    [Header("Defense")]
    [Range(0f, 0.9f)]
    public float damageReduction = 0f;

    [Header("Utility")]
    public float pickupRadiusMultiplier = 1f;

    public float GetDamageMultiplier(float legacyDamageMultiplier)
    {
        return Mathf.Max(0f, legacyDamageMultiplier * damageMultiplier);
    }

    public float GetMoveSpeed(float baseMoveSpeed)
    {
        return Mathf.Max(0f, baseMoveSpeed * moveSpeedMultiplier);
    }

    public float GetDashSpeed(float baseDashSpeed)
    {
        return Mathf.Max(0f, baseDashSpeed * dashSpeedMultiplier);
    }

    public float GetFireCooldown(float baseFireCooldown)
    {
        return Mathf.Max(0.02f, baseFireCooldown * fireCooldownMultiplier);
    }

    public float GetBombCooldown(float baseBombCooldown)
    {
        return Mathf.Max(0.05f, baseBombCooldown * bombCooldownMultiplier);
    }

    public float GetDashCooldown(float baseDashCooldown)
    {
        return Mathf.Max(0.05f, baseDashCooldown * dashCooldownMultiplier);
    }

    public float GetIncomingDamageMultiplier()
    {
        return Mathf.Max(0f, 1f - Mathf.Clamp01(damageReduction));
    }

    public float GetPickupRadius(float basePickupRadius)
    {
        return Mathf.Max(0f, basePickupRadius * pickupRadiusMultiplier);
    }

    public int ApplyExperienceGain(int baseExperience)
    {
        return Mathf.Max(0, Mathf.FloorToInt(GetExperienceAmount(baseExperience)));
    }

    public float GetExperienceAmount(float baseExperience)
    {
        return Mathf.Max(0f, baseExperience * experienceGainMultiplier);
    }

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

    private float ApplyCooldownReduction(float currentMultiplier, float reduction)
    {
        return Mathf.Clamp(currentMultiplier * (1f - Mathf.Clamp01(reduction)), 0.1f, 5f);
    }
}
