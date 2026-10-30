using System.Collections.Generic;
using UnityEngine;

// Owns the primary weapon; bombs on RMB and movement stay on PlayerController.
[DisallowMultipleComponent]
public class PlayerWeaponSystem : MonoBehaviour
{
    public WeaponDefinition Definition { get; private set; }
    private PlayerController owner;
    private float nextAttack;
    private readonly List<SummonedUnit> summons = new List<SummonedUnit>();

    public void Configure(PlayerController player, WeaponDefinition definition)
    {
        foreach (SummonedUnit unit in summons) if (unit != null) Destroy(unit.gameObject);
        summons.Clear();
        owner = player;
        Definition = definition;
        nextAttack = Time.time;
    }

    public float Cooldown => Definition == null ? 0.3f : Definition.kind == WeaponKind.Bomb
        ? owner.Stats.GetBombCooldown(Definition.cooldown)
        : owner.Stats.GetFireCooldown(Definition.cooldown);

    private void Update()
    {
        if (Definition == null || owner == null || owner.Stats == null || Time.timeScale <= 0f) return;
        PlayerHealth health = owner.GetComponent<PlayerHealth>();
        if (health != null && health.currentHealth <= 0f) return;
        if (Definition.kind == WeaponKind.Drone || Definition.kind == WeaponKind.Creature)
        {
            summons.RemoveAll(unit => unit == null);
            int count = Mathf.Clamp(Definition.summonCount + owner.Stats.GetProjectileCount() - 1, 1, 12);
            while (summons.Count < count) SpawnSummon(transform.position, summons.Count);
        }
        else if (Definition.Automatic) TryAttack(Vector2.right);
    }

    public bool TryAttack(Vector2 direction)
    {
        if (Definition == null || owner == null || owner.Stats == null || Time.timeScale <= 0f || Time.time < nextAttack)
            return false;
        if (Definition.kind == WeaponKind.Drone || Definition.kind == WeaponKind.Creature) return false;
        if (direction.sqrMagnitude < 0.001f) direction = Vector2.right;
        if (Definition.kind == WeaponKind.Melee)
        {
            int strikes = Mathf.Clamp(owner.Stats.GetProjectileCount(), 1, 32);
            for (int i = 0; i < strikes; i++)
                Bomb.DamageArea(transform.position, Definition.radius, Definition.damage, owner, true);
            WeaponVisuals.Pulse(transform.position, Definition.radius, Definition.tint);
        }
        else if (Definition.kind == WeaponKind.Turret)
        {
            summons.RemoveAll(unit => unit == null);
            int limit = Mathf.Clamp(Definition.summonCount + owner.Stats.GetProjectileCount() - 1, 1, 12);
            if (summons.Count >= limit)
            {
                SummonedUnit oldest = summons[0];
                summons.RemoveAt(0);
                Destroy(oldest.gameObject);
            }
            SpawnSummon((Vector2)transform.position + direction.normalized * 1.2f, summons.Count);
        }
        else if (Definition.kind == WeaponKind.Bomb)
        {
            if (owner.bombPrefab == null) return false;
            GameObject shot = Instantiate(owner.bombPrefab, transform.position, Quaternion.identity);
            Bomb bomb = shot.GetComponent<Bomb>();
            if (bomb == null) { Destroy(shot); return false; }
            bomb.basedamage = Definition.damage;
            bomb.explosionRadius = Definition.radius;
            bomb.maxDistance = Definition.range;
            bomb.Initialize(direction, owner);
            Destroy(shot, 10f);
        }
        else
        {
            if (owner.bulletPrefab == null) return false;
            FireProjectiles(transform.position, direction);
        }
        float actionCooldown = Definition.kind == WeaponKind.Turret
            ? owner.Stats.GetFireCooldown(Definition.deploymentCooldown) : Cooldown;
        nextAttack = Time.time + actionCooldown;
        owner.bulletCooldownIndicator?.StartCooldown(actionCooldown);
        return true;
    }

    public void FireProjectiles(Vector2 position, Vector2 direction)
    {
        if (owner.bulletPrefab == null) return;
        bool summon = Definition.kind == WeaponKind.Turret || Definition.kind == WeaponKind.Drone;
        int count = Mathf.Clamp(Definition.projectiles + (summon ? 0 : owner.Stats.GetProjectileCount() - 1), 1, 64);
        float fan = Definition.kind == WeaponKind.Shotgun ? Definition.spread : Mathf.Min(60f, Definition.spread * (count - 1));
        for (int i = 0; i < count; i++)
        {
            float angle = count <= 1 ? 0f : (i / (float)(count - 1) - 0.5f) * fan;
            Vector2 aim = Quaternion.Euler(0f, 0f, angle) * direction.normalized;
            GameObject shot = Instantiate(owner.bulletPrefab, position, Quaternion.Euler(0f, 0f, Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg));
            Bullet bullet = shot.GetComponent<Bullet>();
            if (bullet == null) { Destroy(shot); continue; }
            bullet.baseDamage = Definition.damage;
            bullet.bulletSpeed = Definition.projectileSpeed;
            bullet.Initialize(aim, owner, Definition.pierce);
            SpriteRenderer renderer = shot.GetComponent<SpriteRenderer>();
            if (renderer != null) renderer.color = Definition.tint;
            if (Definition.kind == WeaponKind.Bow) shot.transform.localScale = Vector3.Scale(shot.transform.localScale, new Vector3(1.8f, 0.55f, 1f));
            Destroy(shot, 5f);
        }
    }

    private void SpawnSummon(Vector2 position, int slot)
    {
        GameObject unitObject = new GameObject(Definition.displayName + " " + (slot + 1));
        unitObject.transform.position = position;
        SummonedUnit unit = unitObject.AddComponent<SummonedUnit>();
        unit.Initialize(owner, this, slot);
        summons.Add(unit);
    }

    private void OnDestroy()
    {
        foreach (SummonedUnit unit in summons) if (unit != null) Destroy(unit.gameObject);
    }
}
