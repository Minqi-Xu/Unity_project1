using System;

// Percent bonuses affect the resolved hit; fixed damage is added once afterwards, without crit/scaling.
public static class EnemyDamageRules
{
    public static float Resolve(float damage, EnemyRank rank, float healthFraction,
        float normalBonus, float eliteBonus, float bossBonus, float executeBonus,
        float flatHitDamage, float projectileFlatDamage, bool directHit, bool projectileHit)
    {
        if (damage <= 0f) return 0f;
        float rankBonus = rank == EnemyRank.Normal ? normalBonus : rank == EnemyRank.Elite ? eliteBonus : bossBonus;
        float multiplier = 1f + Math.Max(0f, rankBonus) + (healthFraction <= .3f ? Math.Max(0f, executeBonus) : 0f);
        float extra = directHit ? Math.Max(0f, flatHitDamage) + (projectileHit ? Math.Max(0f, projectileFlatDamage) : 0f) : 0f;
        return damage * multiplier + extra;
    }
}
