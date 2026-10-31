using System;

// Pure base-relative stat rules shared by runtime calculations and standalone checks.
public static class AdditiveStatMath
{
    public static float DamageMultiplier(float legacy, float global, float primary, float weaponBonus) =>
        Math.Max(0f, 1f + (legacy - 1f) + (global - 1f) + (primary - 1f) + weaponBonus);

    public static float CooldownMultiplier(float specific, float global, float extraReduction = 0f) =>
        Math.Min(5f, Math.Max(.1f, specific + global - 1f - extraReduction));

    public static float AddCooldownReduction(float current, float reduction) =>
        Math.Min(5f, Math.Max(.1f, current - Math.Min(1f, Math.Max(0f, reduction))));

    public static float AttackSpeedMultiplier(float permanent, float temporary) =>
        Math.Max(.01f, permanent + Math.Max(0f, temporary));
}
