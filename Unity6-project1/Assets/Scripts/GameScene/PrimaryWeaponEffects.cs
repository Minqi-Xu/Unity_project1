using System.Collections.Generic;
using UnityEngine;

public partial class PlayerWeaponSystem
{
    private readonly Collider2D[] specializationHits = new Collider2D[128];
    private readonly HashSet<Enemy> specializationTargets = new HashSet<Enemy>();
    private readonly HashSet<Enemy> ricochetVisited = new HashSet<Enemy>();

    private int QuerySpecialization(Vector2 center, float radius) => Physics2D.OverlapCircle(center, radius,
        new ContactFilter2D { useTriggers = true }, specializationHits);

    // Resolved damage already includes the original crit and weapon multipliers. Only target bonuses apply here.
    public int SecondaryArea(Vector2 center, float radius, float resolvedDamage, Enemy exclude = null, HashSet<Enemy> excludedTargets = null)
    {
        if (owner == null || owner.Stats == null || resolvedDamage <= 0f || (Application.isPlaying && Time.timeScale <= 0f)) return 0;
        int count = QuerySpecialization(center, radius);
        specializationTargets.Clear();
        int struck = 0;
        for (int i = 0; i < count && struck < 8; i++)
        {
            Enemy target = specializationHits[i].GetComponentInParent<Enemy>();
            if (target == null || target == exclude || !target.IsAlive || !specializationTargets.Add(target)) continue;
            // The main blast used collider overlap, so exclude its exact set, including large edge-straddling enemies.
            if (excludedTargets != null && excludedTargets.Contains(target)) continue;
            target.TakeDamage(resolvedDamage, owner.Stats);
            struck++;
        }
        if (Application.isPlaying) WeaponVisuals.Pulse(center, radius, Definition.tint);
        return struck;
    }

    private void ApplyPrimarySpecialization(Enemy target, float damage, int pierced, WeaponAttackContext context, bool projectileHit, bool killed)
    {
        context = context ?? new WeaponAttackContext();
        switch (Definition.kind)
        {
            case WeaponKind.Pistol:
                if (projectileHit && !context.ricochetTriggered && Get(WeaponBuffEffect.PistolRicochet) > 0f)
                {
                    context.ricochetTriggered = true;
                    Ricochet(target, damage);
                }
                break;
            case WeaponKind.Turret:
                if (projectileHit && Get(WeaponBuffEffect.TurretSplash) > 0f)
                    SecondaryArea(target.transform.position, 1.5f, damage * Get(WeaponBuffEffect.TurretSplash), target);
                break;
            case WeaponKind.Shotgun:
                if (projectileHit && !context.primaryAreaTriggered && Get(WeaponBuffEffect.ShotgunShockwave) > 0f)
                {
                    context.primaryAreaTriggered = true;
                    SecondaryArea(target.transform.position, 1.5f,
                        damage * ProjectileCount * Get(WeaponBuffEffect.ShotgunShockwave), target);
                }
                break;
            case WeaponKind.Bow:
                if (projectileHit && (pierced + 1) % 2 == 0 && Get(WeaponBuffEffect.BowNova) > 0f)
                    SecondaryArea(target.transform.position, 1.6f, damage * Get(WeaponBuffEffect.BowNova), target);
                break;
            case WeaponKind.Axe:
                if (!context.primaryAreaTriggered && Get(WeaponBuffEffect.AxeShockwave) > 0f)
                {
                    context.primaryAreaTriggered = true;
                    SecondaryArea(target.transform.position, 1.8f, damage * Get(WeaponBuffEffect.AxeShockwave), target);
                }
                if (killed && !context.primaryKillBurstTriggered && Get(WeaponBuffEffect.AxeKillBurst) > 0f)
                {
                    context.primaryKillBurstTriggered = true;
                    SecondaryArea(target.transform.position, 2f, damage * Get(WeaponBuffEffect.AxeKillBurst), target);
                }
                break;
        }
    }

    private void Ricochet(Enemy first, float damage)
    {
        ricochetVisited.Clear();
        ricochetVisited.Add(first);
        Enemy previous = first;
        int bounces = Mathf.Clamp(Mathf.RoundToInt(Get(WeaponBuffEffect.PistolRicochet)), 0, 4);
        for (int bounce = 0; bounce < bounces; bounce++)
        {
            Vector2 position = previous.transform.position;
            int count = QuerySpecialization(position, 3f);
            Enemy nearest = null;
            float distance = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                Enemy candidate = specializationHits[i].GetComponentInParent<Enemy>();
                if (candidate == null || !candidate.IsAlive || ricochetVisited.Contains(candidate)) continue;
                float squared = ((Vector2)candidate.transform.position - position).sqrMagnitude;
                if (squared < distance) { nearest = candidate; distance = squared; }
            }
            if (nearest == null) break;
            ricochetVisited.Add(nearest);
            damage *= .6f;
            nearest.TakeDamage(damage, owner.Stats);
            if (Application.isPlaying)
            {
                LineRenderer trace = WeaponVisuals.Stroke("Ricochet", null, Definition.tint, .07f, 2);
                trace.SetPosition(0, position); trace.SetPosition(1, nearest.transform.position);
                Destroy(trace.gameObject, .15f);
            }
            previous = nearest;
        }
    }

    public int PullBombTargets(Vector2 center, float blastRadius)
    {
        float distance = Get(WeaponBuffEffect.BombVacuum);
        if (distance <= 0f) return 0;
        int count = QuerySpecialization(center, blastRadius * 1.5f);
        specializationTargets.Clear();
        int moved = 0;
        for (int i = 0; i < count && moved < 16; i++)
        {
            Enemy target = specializationHits[i].GetComponentInParent<Enemy>();
            if (target == null || !target.IsAlive || target.Rank >= EnemyRank.MiniBoss || !specializationTargets.Add(target)) continue;
            target.transform.position = Vector2.MoveTowards(target.transform.position, center,
                distance * (target.Rank == EnemyRank.Elite ? .5f : 1f));
            moved++;
        }
        Physics2D.SyncTransforms();
        return moved;
    }
}
