using UnityEngine;

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

    public void Initialize(PlayerController player, PlayerWeaponSystem system, int index)
    {
        owner = player;
        weapon = system;
        definition = system.Definition;
        slot = index;
        expires = Time.time + definition.summonLifetime;
        WeaponVisuals.AddUnitSprite(gameObject, player, definition.tint, definition.kind);
    }

    private void Update()
    {
        if (owner == null || weapon == null || definition == null) { Destroy(gameObject); return; }
        if (Time.timeScale <= 0f || owner.Stats == null) return;
        PlayerHealth health = owner.GetComponent<PlayerHealth>();
        if (health != null && health.currentHealth <= 0f) return;
        if (definition.kind == WeaponKind.Turret && Time.time >= expires) { Destroy(gameObject); return; }
        if (Time.time >= nextSearch)
        {
            target = FindTarget(transform.position, definition.range);
            nextSearch = Time.time + 0.2f;
        }
        if (definition.kind != WeaponKind.Turret)
        {
            float angle = slot * 2.4f + Time.time * 0.7f;
            Vector2 follow = (Vector2)owner.transform.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 1.6f;
            bool canChase = definition.kind == WeaponKind.Creature && target != null &&
                Vector2.Distance(owner.transform.position, target.transform.position) < definition.range;
            Vector2 destination = canChase ? (Vector2)target.transform.position : follow;
            transform.position = Vector2.MoveTowards(transform.position, destination, definition.summonSpeed * Time.deltaTime);
            if (Vector2.Distance(transform.position, owner.transform.position) > definition.range * 2f)
                transform.position = follow;
        }
        if (target == null || Time.time < nextAttack) return;
        float distance = Vector2.Distance(transform.position, target.transform.position);
        if (distance > definition.range) return;
        if (definition.kind == WeaponKind.Creature)
        {
            if (distance > definition.radius) return;
            target.TakeDamage(owner.Stats.CalculateDamage(definition.damage, owner.damageMultiplier, true));
            WeaponVisuals.Pulse(transform.position, definition.radius, definition.tint);
        }
        else weapon.FireProjectiles(transform.position, target.transform.position - transform.position);
        if (slot == 0) owner.bulletCooldownIndicator?.StartCooldown(weapon.Cooldown);
        nextAttack = Time.time + weapon.Cooldown;
    }

    public static Enemy FindTarget(Vector2 position, float range)
    {
        Enemy nearest = null;
        float best = range * range;
        foreach (Collider2D collider in Physics2D.OverlapCircleAll(position, range))
        {
            Enemy enemy = collider.GetComponentInParent<Enemy>();
            if (enemy == null || !enemy.IsAlive) continue;
            float distance = ((Vector2)enemy.transform.position - position).sqrMagnitude;
            if (distance > best) continue;
            best = distance;
            nearest = enemy;
        }
        return nearest;
    }
}
