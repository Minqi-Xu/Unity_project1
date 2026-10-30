using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAttackContext
{
    public readonly Dictionary<Enemy, int> hits = new Dictionary<Enemy, int>();
    public float healing;
}

public partial class PlayerWeaponSystem
{
    private readonly Dictionary<WeaponBuffEffect, float> buffs = new Dictionary<WeaponBuffEffect, float>();
    private int consecutiveShots;
    private int pistolHits;
    private float lastShot = -100f;
    private float drawStarted = -1f;
    private float lastDrawInput = -100f;
    private float configuredAt;
    public float CharacterDamageMultiplier { get; set; } = 1f;
    public float CharacterCooldownMultiplier { get; set; } = 1f;
    public bool IsPrimaryWeapon => owner != null && gameObject == owner.gameObject;

    public float Get(WeaponBuffEffect effect) => buffs.TryGetValue(effect, out float value) ? value : 0f;
    public float Radius => Definition.radius * (1f + Get(WeaponBuffEffect.Radius));
    public float SummonSpeed => Definition.summonSpeed * (1f + Get(WeaponBuffEffect.CreatureSpeed));
    public float SummonReach => Definition.radius + Get(WeaponBuffEffect.CreatureReach);
    public float OrbitRadius => 1.6f + Get(WeaponBuffEffect.DroneOrbit);
    public float TurretLifetime => Definition.summonLifetime + Get(WeaponBuffEffect.TurretLifetime);
    public bool Overloaded => Get(WeaponBuffEffect.DroneOverload) > 0f && (Time.time - configuredAt) % 10f >= 7f;

    public void ApplyBuff(WeaponBuffEffect effect, float amount)
    {
        buffs[effect] = Get(effect) + amount;
        if (effect == WeaponBuffEffect.DroneFocus) buffs[WeaponBuffEffect.DroneIndependent] = 0f;
        if (effect == WeaponBuffEffect.DroneIndependent) buffs[WeaponBuffEffect.DroneFocus] = 0f;
    }

    private float GetWeaponCooldown()
    {
        if (Definition == null || owner == null || owner.Stats == null) return .3f;
        float cooldown = IsPrimaryWeapon
            ? owner.Stats.GetPrimaryWeaponCooldown(Definition.cooldown * CharacterCooldownMultiplier)
            : owner.Stats.GetAuxiliaryWeaponCooldown(Definition.cooldown);
        if (Definition.kind == WeaponKind.Bomb) cooldown *= owner.Stats.bombCooldownMultiplier;
        cooldown *= Mathf.Max(.4f, 1f - Get(WeaponBuffEffect.Cooldown));
        if (Definition.kind == WeaponKind.Pistol && Time.time - lastShot <= 1f)
            cooldown *= 1f - Mathf.Min(.4f, consecutiveShots * Get(WeaponBuffEffect.PistolRamp));
        if (Definition.kind == WeaponKind.Drone && Overloaded) cooldown *= .6f;
        return Mathf.Max(.02f, cooldown);
    }

    private void RecordShot()
    {
        consecutiveShots = Time.time - lastShot > 1f ? 1 : consecutiveShots + 1;
        lastShot = Time.time;
        drawStarted = -1f;
    }

    private bool ReadyToDraw()
    {
        if (Definition.kind != WeaponKind.Bow || Get(WeaponBuffEffect.BowCharge) <= 0f) return true;
        if (drawStarted < 0f || Time.time - lastDrawInput > .1f) drawStarted = Time.time;
        lastDrawInput = Time.time;
        return Time.time - drawStarted >= .35f;
    }

    public void CancelDraw() { drawStarted = -1f; lastDrawInput = -100f; }

    // All proc-generated damage uses procs=false to prevent recursive rewards/attacks.
    public void Hit(Enemy enemy, float baseDamage, Vector2 origin, int pierced = 0,
        WeaponAttackContext context = null, float scale = 1f, bool procs = true)
    {
        if (enemy == null || !enemy.IsAlive || owner == null || owner.Stats == null) return;
        float multiplier = 1f + Get(WeaponBuffEffect.Damage);
        if (Definition.kind == WeaponKind.Pistol && procs)
        {
            pistolHits++;
            if (pistolHits % 5 == 0) multiplier += Get(WeaponBuffEffect.PistolCombo);
        }
        if (Definition.kind == WeaponKind.Shotgun)
        {
            if (Vector2.Distance(origin, enemy.transform.position) <= 3f) multiplier += Get(WeaponBuffEffect.ShotgunClose);
            if (context != null)
            {
                context.hits.TryGetValue(enemy, out int hits);
                context.hits[enemy] = hits + 1;
                if (hits + 1 == 3) multiplier += Get(WeaponBuffEffect.ShotgunConcentrated);
            }
        }
        if (Definition.kind == WeaponKind.Bow)
            multiplier += Get(WeaponBuffEffect.BowPierceDamage) * pierced + Get(WeaponBuffEffect.BowCharge);
        if (Definition.kind == WeaponKind.Axe && enemy.currentHealth <= enemy.maxHealth * .3f)
            multiplier += Get(WeaponBuffEffect.AxeExecute);
        float characterScale = Definition.kind == WeaponKind.Bomb ? 1f : CharacterDamageMultiplier;
        float damage = owner.Stats.CalculateDamage(baseDamage * multiplier * scale * characterScale, owner.damageMultiplier,
            IsPrimaryWeapon);
        enemy.TakeDamage(damage);
        bool killed = !enemy.IsAlive;
        if ((Definition.kind == WeaponKind.Melee || Definition.kind == WeaponKind.Axe) && !killed)
        {
            Vector2 away = (Vector2)(enemy.transform.position - owner.transform.position);
            if (away.sqrMagnitude < .0001f) away = Vector2.right;
            enemy.transform.position += (Vector3)away.normalized * (Definition.knockback + Get(WeaponBuffEffect.MeleeKnockback));
        }
        if (!procs) return;
        if (Definition.kind == WeaponKind.Creature && !killed && Get(WeaponBuffEffect.CreatureBleed) > 0f)
        {
            WeaponBleed bleed = enemy.GetComponent<WeaponBleed>();
            if (bleed == null) bleed = enemy.gameObject.AddComponent<WeaponBleed>();
            bleed.Apply(owner, Definition.damage * Get(WeaponBuffEffect.CreatureBleed) * (1f + Get(WeaponBuffEffect.Damage)));
        }
        if (!killed) return;
        if (Definition.kind == WeaponKind.Pistol && Get(WeaponBuffEffect.PistolKillShot) > 0f)
        {
            Enemy next = SummonedUnit.FindTarget(enemy.transform.position, 12f);
            if (next != null)
                for (int i = 0; i < Mathf.RoundToInt(Get(WeaponBuffEffect.PistolKillShot)); i++)
                    FireProjectiles(enemy.transform.position, next.transform.position - enemy.transform.position, 1f, false, 1);
        }
        if (Definition.kind == WeaponKind.Melee && context != null && context.healing < 5f)
        {
            float heal = Mathf.Min(5f - context.healing, Get(WeaponBuffEffect.MeleeHeal));
            owner.GetComponent<PlayerHealth>()?.Heal(heal);
            context.healing += heal;
        }
        if (Definition.kind == WeaponKind.Creature && Get(WeaponBuffEffect.CreatureBreed) > 0f)
        {
            summons.RemoveAll(unit => unit == null);
            int temporary = 0;
            foreach (SummonedUnit unit in summons) if (unit.IsTemporary) temporary++;
            if (temporary < 2)
            {
                SpawnSummon(enemy.transform.position, summons.Count);
                summons[summons.Count - 1].MakeTemporary(8f);
            }
        }
    }

    public void DamageArea(Vector2 center, float radius, float damage, WeaponAttackContext context = null,
        bool procs = true, bool centerBonus = false, bool falloff = false)
    {
        HashSet<Enemy> targets = new HashSet<Enemy>();
        foreach (Collider2D collider in Physics2D.OverlapCircleAll(center, radius))
        {
            Enemy enemy = collider.GetComponentInParent<Enemy>();
            if (enemy == null || !targets.Add(enemy)) continue;
            float distance = Vector2.Distance(center, enemy.transform.position);
            float scale = centerBonus && distance <= radius * Definition.explosionCoreFraction
                ? 1f + Get(WeaponBuffEffect.BombCenter) : 1f;
            if (falloff) scale *= ExplosionFalloff.Evaluate(distance, radius,
                Definition.explosionCoreFraction, Definition.explosionEdgeDamageMultiplier);
            if (scale <= 0f) continue;
            Hit(enemy, damage, center, 0, context, scale, procs);
        }
    }

    private IEnumerator MeleeAttack()
    {
        WeaponAttackContext context = new WeaponAttackContext();
        int strikes = Mathf.Clamp(owner.Stats.GetProjectileCount() + Mathf.RoundToInt(Get(WeaponBuffEffect.MeleeCombo)), 1, 32);
        for (int i = 0; i < strikes; i++)
        {
            if (owner == null || owner.GetComponent<PlayerHealth>().currentHealth <= 0f) yield break;
            DamageArea(transform.position, Radius, Definition.damage, context);
            WeaponVisuals.Pulse(transform.position, Radius, Definition.tint);
            if (Get(WeaponBuffEffect.MeleeCombo) > 0f && i < strikes - 1) yield return new WaitForSeconds(.12f);
        }
    }

    public void Explode(Vector2 center, float baseRadius, float damage)
    {
        float radius = baseRadius * (1f + Get(WeaponBuffEffect.Radius)) * Mathf.Max(0f, owner.Stats.explosionRadiusMultiplier);
        DamageArea(center, radius, damage, null, true, true, Definition.kind == WeaponKind.Bomb);
        WeaponVisuals.Pulse(center, radius, Definition.tint);
        if (Definition.kind == WeaponKind.Bomb)
            WeaponVisuals.Pulse(center, radius * Definition.explosionCoreFraction, new Color(1f, .85f, .4f));
        if (Get(WeaponBuffEffect.BombCluster) > 0f) StartCoroutine(Cluster(center, damage));
        if (Get(WeaponBuffEffect.BombRepeat) > 0f) StartCoroutine(RepeatBlast(center, radius, damage));
        if (Get(WeaponBuffEffect.BombBurn) > 0f) StartCoroutine(BurningGround(center, radius, damage));
    }

    private IEnumerator Cluster(Vector2 center, float damage)
    {
        yield return new WaitForSeconds(.2f);
        int count = Mathf.RoundToInt(Get(WeaponBuffEffect.BombCluster));
        for (int i = 0; i < count; i++)
        {
            Vector2 point = center + new Vector2(Mathf.Cos(i * Mathf.PI * 2f / count), Mathf.Sin(i * Mathf.PI * 2f / count)) * 1.5f;
            DamageArea(point, 1f, damage * .2f, null, false, false, true);
            WeaponVisuals.Pulse(point, 1f, Definition.tint);
        }
    }

    private IEnumerator RepeatBlast(Vector2 center, float radius, float damage)
    {
        yield return new WaitForSeconds(.6f);
        DamageArea(center, radius, damage * Get(WeaponBuffEffect.BombRepeat), null, false, false, true);
        WeaponVisuals.Pulse(center, radius, Definition.tint);
    }

    private IEnumerator BurningGround(Vector2 center, float radius, float damage)
    {
        for (int i = 0; i < 6; i++)
        {
            yield return new WaitForSeconds(.5f);
            DamageArea(center, radius, damage * Get(WeaponBuffEffect.BombBurn), null, false);
            WeaponVisuals.Pulse(center, radius, new Color(1f, .3f, 0f, .5f));
        }
    }

    private void CastFireballs()
    {
        if (owner.bombPrefab == null) return;
        int count = Mathf.Clamp(Definition.projectiles + owner.Stats.GetProjectileCount() - 1 + Mathf.RoundToInt(Get(WeaponBuffEffect.Count)), 1, 32);
        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2f / count + Time.time;
            GameObject shot = Instantiate(owner.bombPrefab, transform.position, Quaternion.identity);
            Bomb bomb = shot.GetComponent<Bomb>();
            bomb.basedamage = Definition.damage;
            bomb.explosionRadius = Definition.radius;
            bomb.maxDistance = Definition.range;
            bomb.bombSpeed = Definition.projectileSpeed;
            bomb.Initialize(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)), owner, this);
            SpriteRenderer renderer = shot.GetComponent<SpriteRenderer>();
            if (renderer != null) renderer.color = Definition.tint;
            Destroy(shot, 8f);
        }
    }

    private bool CastLightning()
    {
        List<Enemy> targets = new List<Enemy>();
        Camera camera = Camera.main;
        if (camera == null) return false;
        foreach (Collider2D collider in Physics2D.OverlapCircleAll(transform.position, Definition.range))
        {
            Enemy enemy = collider.GetComponentInParent<Enemy>();
            if (enemy == null || !enemy.IsAlive || targets.Contains(enemy)) continue;
            Vector3 view = camera.WorldToViewportPoint(enemy.transform.position);
            if (view.z <= 0f || view.x < 0f || view.x > 1f || view.y < 0f || view.y > 1f) continue;
            targets.Add(enemy);
        }
        targets.Sort((a, b) => (a.transform.position - transform.position).sqrMagnitude.CompareTo((b.transform.position - transform.position).sqrMagnitude));
        int count = Mathf.Clamp(Definition.projectiles + owner.Stats.GetProjectileCount() - 1 + Mathf.RoundToInt(Get(WeaponBuffEffect.Count)), 1, 32);
        if (targets.Count > count) targets.RemoveRange(count, targets.Count - count);
        if (targets.Count == 0) return false;
        foreach (Enemy target in targets)
        {
            Hit(target, Definition.damage, transform.position);
            WeaponVisuals.Lightning(target.transform.position, Definition.tint);
        }
        if (Get(WeaponBuffEffect.LightningRepeat) > 0f) StartCoroutine(RepeatLightning(targets));
        return true;
    }

    private IEnumerator RepeatLightning(List<Enemy> targets)
    {
        yield return new WaitForSeconds(.3f);
        foreach (Enemy target in targets)
        {
            if (target == null || !target.IsAlive) continue;
            Hit(target, Definition.damage, transform.position, 0, null, Get(WeaponBuffEffect.LightningRepeat), false);
            WeaponVisuals.Lightning(target.transform.position, Definition.tint);
        }
    }

    public float TurretLinkScale(Vector2 position)
    {
        int neighbors = 0;
        foreach (SummonedUnit unit in summons)
            if (unit != null && Vector2.Distance(position, unit.transform.position) > .01f && Vector2.Distance(position, unit.transform.position) <= 4f) neighbors++;
        return 1f + Mathf.Min(3, neighbors) * Get(WeaponBuffEffect.TurretLink);
    }

    public void TurretExpired(Vector2 position)
    {
        if (Get(WeaponBuffEffect.TurretExpiry) <= 0f) return;
        DamageArea(position, 2f, Definition.damage * Get(WeaponBuffEffect.TurretExpiry), null, false);
        WeaponVisuals.Pulse(position, 2f, Definition.tint);
    }

    public Enemy PickSummonTarget(SummonedUnit unit)
    {
        if (Get(WeaponBuffEffect.DroneFocus) > 0f)
        {
            Enemy focus = SummonedUnit.FindTarget(owner.transform.position, Definition.range);
            if (focus != null && Vector2.Distance(unit.transform.position, focus.transform.position) <= Definition.range) return focus;
        }
        if (Get(WeaponBuffEffect.DroneIndependent) > 0f)
        {
            HashSet<Enemy> reserved = new HashSet<Enemy>();
            foreach (SummonedUnit other in summons) if (other != null && other != unit && other.Target != null) reserved.Add(other.Target);
            Enemy independent = SummonedUnit.FindTarget(unit.transform.position, Definition.range, reserved);
            if (independent != null) return independent;
        }
        return SummonedUnit.FindTarget(unit.transform.position, Definition.range);
    }

    public void FireSummon(SummonedUnit unit, Vector2 direction)
    {
        float scale = Definition.kind == WeaponKind.Turret ? TurretLinkScale(unit.transform.position) : Overloaded ? 1.25f : 1f;
        if (Get(WeaponBuffEffect.DroneBurst) > 0f) StartCoroutine(Burst(unit, direction, scale));
        else FireProjectiles(unit.transform.position, direction, scale);
    }

    private IEnumerator Burst(SummonedUnit unit, Vector2 direction, float scale)
    {
        for (int i = 0; i < 3; i++)
        {
            if (unit == null) yield break;
            FireProjectiles(unit.transform.position, direction, scale * .6f);
            if (i < 2) yield return new WaitForSeconds(.12f);
        }
    }
}
