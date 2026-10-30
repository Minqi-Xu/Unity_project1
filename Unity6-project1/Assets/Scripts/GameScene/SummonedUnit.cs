using UnityEngine;
using System.Collections.Generic;

// Turrets hold position, drones follow, creatures roam and chase within a leash.
public class SummonedUnit : MonoBehaviour
{
    private PlayerController owner;
    private PlayerWeaponSystem weapon;
    private WeaponDefinition definition;
    private Enemy target;
    private int slot;
    private float nextSearch;
    private float nextAttack;
    private float expires;
    private float born;
    private float nextPounce;
    private float pounceUntil;
    public bool IsTemporary { get; private set; }
    public Enemy Target => target;
    public float AttackCooldownRemaining => Mathf.Max(0f, nextAttack - Time.time);
    public float AttackCooldownDuration { get; private set; }
    public void MakeTemporary(float lifetime) { IsTemporary = true; expires = Time.time + lifetime; }

    public void Initialize(PlayerController player, PlayerWeaponSystem system, int index)
    {
        owner = player;
        weapon = system;
        definition = system.Definition;
        slot = index;
        expires = Time.time + definition.summonLifetime;
        born = Time.time;
        AttackCooldownDuration = weapon.Cooldown;
        WeaponVisuals.AddUnitSprite(gameObject, player, definition.tint, definition.kind);
    }

    private void Update()
    {
        if (owner == null || weapon == null || definition == null) { Destroy(gameObject); return; }
        if (Time.timeScale <= 0f || owner.Stats == null) return;
        PlayerHealth health = owner.GetComponent<PlayerHealth>();
        if (health != null && health.currentHealth <= 0f) return;
        if (IsTemporary && Time.time >= expires) { Destroy(gameObject); return; }
        if (definition.kind == WeaponKind.Turret && Time.time >= born + weapon.TurretLifetime)
        {
            weapon.TurretExpired(transform.position);
            Destroy(gameObject);
            return;
        }
        if (Time.time >= nextSearch)
        {
            target = weapon.PickSummonTarget(this);
            nextSearch = Time.time + 0.2f;
        }
        if (definition.kind != WeaponKind.Turret)
        {
            float angle = slot * 2.4f + Time.time * 0.7f;
            Vector2 follow = (Vector2)owner.transform.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * weapon.OrbitRadius;
            bool canChase = definition.kind == WeaponKind.Creature && target != null &&
                Vector2.Distance(owner.transform.position, target.transform.position) < definition.range;
            Vector2 destination = canChase ? (Vector2)target.transform.position : follow;
            if (canChase && weapon.Get(WeaponBuffEffect.CreaturePounce) > 0f && Time.time >= nextPounce &&
                Vector2.Distance(transform.position, destination) > weapon.SummonReach && Vector2.Distance(transform.position, destination) <= 5f)
            {
                nextPounce = Time.time + 4f;
                pounceUntil = Time.time + .2f;
            }
            transform.position = Vector2.MoveTowards(transform.position, destination,
                weapon.SummonSpeed * (Time.time < pounceUntil ? 3f : 1f) * Time.deltaTime);
            if (Vector2.Distance(transform.position, owner.transform.position) > definition.range * 2f)
                transform.position = follow;
        }
        if (target == null || !target.IsAlive || Time.time < nextAttack) return;
        float distance = Vector2.Distance(transform.position, target.transform.position);
        if (distance > definition.range) return;
        if (definition.kind == WeaponKind.Creature)
        {
            if (distance > weapon.SummonReach) return;
            weapon.Hit(target, definition.damage, transform.position, 0, null, Time.time < pounceUntil ? 1.5f : 1f, !IsTemporary);
            WeaponVisuals.Pulse(transform.position, weapon.SummonReach, definition.tint);
        }
        else weapon.FireSummon(this, target.transform.position - transform.position);
        AttackCooldownDuration = weapon.Cooldown;
        nextAttack = Time.time + AttackCooldownDuration;
    }

    public static Enemy FindTarget(Vector2 position, float range, HashSet<Enemy> excluded = null)
    {
        Enemy nearest = null;
        float best = range * range;
        foreach (Collider2D collider in Physics2D.OverlapCircleAll(position, range))
        {
            Enemy enemy = collider.GetComponentInParent<Enemy>();
            if (enemy == null || !enemy.IsAlive || (excluded != null && excluded.Contains(enemy))) continue;
            float distance = ((Vector2)enemy.transform.position - position).sqrMagnitude;
            if (distance > best) continue;
            best = distance;
            nearest = enemy;
        }
        return nearest;
    }
}
