using System.Collections.Generic;
using UnityEngine;

// Owns the primary weapon; bombs on RMB and movement stay on PlayerController.
[DisallowMultipleComponent]
public partial class PlayerWeaponSystem : MonoBehaviour
{
    public WeaponDefinition Definition { get; private set; }
    public int WeaponLevel { get; private set; } = 1;
    public float LevelDamageMultiplier => 1f + .15f * (WeaponLevel - 1);
    public void UpgradeLevel() { if (Definition != null) WeaponLevel++; }
    private PlayerController owner;
    private float nextAttack;
    private float lightningSearchRetryUntil;
    private float activeCooldownDuration;
    private float axeRecoveryEnds;
    private readonly List<SummonedUnit> summons = new List<SummonedUnit>();

    public void Configure(PlayerController player, WeaponDefinition definition)
    {
        StopAllCoroutines();
        axeBusy = false;
        foreach (SummonedUnit unit in summons) if (unit != null) Destroy(unit.gameObject);
        summons.Clear();
        owner = player;
        Definition = definition;
        WeaponLevel = 1;
        buffs.Clear();
        CharacterDamageMultiplier = CharacterCooldownMultiplier = 1f;
        configuredAt = Time.time;
        consecutiveShots = pistolHits = 0;
        lastShot = -100f;
        drawStarted = -1f;
        nextAttack = Time.time;
        lightningSearchRetryUntil = Time.time;
        activeCooldownDuration = 0f;
        axeRecoveryEnds = Time.time;
    }

    public float Cooldown => GetWeaponCooldown();
    public bool IsDrawing => Definition != null && Definition.kind == WeaponKind.Bow && drawStarted >= 0f;
    public float DrawProgress => IsDrawing ? Mathf.Clamp01((Time.time - drawStarted) / .35f) : 0f;
    public bool UsesSummonAttacks => Definition != null && (Definition.kind == WeaponKind.Drone || Definition.kind == WeaponKind.Creature);
    public int ActiveSummonCount
    {
        get { int count = 0; foreach (SummonedUnit unit in summons) if (unit != null && !unit.IsTemporary) count++; return count; }
    }
    public int ReadySummonCount
    {
        get { int count = 0; foreach (SummonedUnit unit in summons) if (unit != null && !unit.IsTemporary && unit.AttackCooldownRemaining <= 0f) count++; return count; }
    }
    public float CooldownRemaining
    {
        get
        {
            if (!UsesSummonAttacks) return Mathf.Max(0f, Mathf.Max(nextAttack, axeBusy ? axeRecoveryEnds : 0f) - Time.time);
            SummonedUnit unit = NextReadySummon();
            return unit != null ? unit.AttackCooldownRemaining : 0f;
        }
    }
    public float CooldownDuration
    {
        get
        {
            if (!UsesSummonAttacks) return Mathf.Max(.02f, activeCooldownDuration);
            SummonedUnit unit = NextReadySummon();
            return unit != null ? unit.AttackCooldownDuration : Cooldown;
        }
    }
    private SummonedUnit NextReadySummon()
    {
        SummonedUnit next = null;
        foreach (SummonedUnit unit in summons)
            if (unit != null && !unit.IsTemporary && (next == null || unit.AttackCooldownRemaining < next.AttackCooldownRemaining)) next = unit;
        return next;
    }

    private void Update()
    {
        if (Definition != null && Definition.kind == WeaponKind.Bow && !Input.GetMouseButton(0)) CancelDraw();
        if (Definition == null || owner == null || owner.Stats == null || Time.timeScale <= 0f) return;
        PlayerHealth health = owner.GetComponent<PlayerHealth>();
        if (health != null && health.currentHealth <= 0f) return;
        if (Definition.kind == WeaponKind.Drone || Definition.kind == WeaponKind.Creature)
        {
            summons.RemoveAll(unit => unit == null);
            int count = SummonCount;
            int permanent = 0;
            foreach (SummonedUnit unit in summons) if (!unit.IsTemporary) permanent++;
            while (permanent < count) { SpawnSummon(transform.position, permanent); permanent++; }
        }
        else if (Definition.Automatic) TryAttack(Vector2.right);
    }

    public bool TryAttack(Vector2 direction)
    {
        if (Definition == null || owner == null || owner.Stats == null || Time.timeScale <= 0f || Time.time < nextAttack || axeBusy ||
            (Definition.kind == WeaponKind.Lightning && Time.time < lightningSearchRetryUntil))
            return false;
        if (Definition.kind == WeaponKind.Drone || Definition.kind == WeaponKind.Creature) return false;
        if (!ReadyToDraw()) return false;
        if (direction.sqrMagnitude < 0.001f) direction = Vector2.right;
        if (Definition.kind == WeaponKind.Axe)
        {
            StartCoroutine(AxeAttack(direction.normalized));
        }
        else if (Definition.kind == WeaponKind.Melee)
        {
            StartCoroutine(MeleeAttack());
        }
        else if (Definition.kind == WeaponKind.Turret)
        {
            summons.RemoveAll(unit => unit == null);
            int limit = SummonCount;
            if (summons.Count >= limit)
            {
                SummonedUnit oldest = summons[0];
                summons.RemoveAt(0);
                Destroy(oldest.gameObject);
            }
            Vector2 placement = (Vector2)transform.position + direction.normalized * (1.2f + Get(WeaponBuffEffect.TurretDistance));
            Camera camera = Camera.main;
            if (camera != null && camera.orthographic)
            {
                Vector3 viewport = camera.WorldToViewportPoint(placement);
                viewport.x = Mathf.Clamp(viewport.x, .03f, .97f);
                viewport.y = Mathf.Clamp(viewport.y, .03f, .97f);
                placement = camera.ViewportToWorldPoint(viewport);
            }
            SpawnSummon(placement, summons.Count);
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
            bomb.Initialize(direction, owner, this);
            Destroy(shot, 10f);
        }
        else if (Definition.kind == WeaponKind.FireOrb) CastFireballs();
        else if (Definition.kind == WeaponKind.Lightning)
        {
            // Searching is throttled separately; only a successful strike consumes the visible cooldown.
            if (!CastLightning()) { lightningSearchRetryUntil = Time.time + .15f; return false; }
        }
        else
        {
            if (owner.bulletPrefab == null) return false;
            FireProjectiles(transform.position, direction);
        }
        RecordShot();
        float actionCooldown = Definition.kind == WeaponKind.Turret
            ? owner.Stats.GetPrimaryWeaponCooldown(Definition.deploymentCooldown * CharacterCooldownMultiplier * Mathf.Max(.4f, 1f - Get(WeaponBuffEffect.TurretDeployment))) : Cooldown;
        nextAttack = Time.time + actionCooldown;
        activeCooldownDuration = actionCooldown;
        if (gameObject == owner.gameObject) owner.bulletCooldownIndicator?.StartCooldown(actionCooldown);
        return true;
    }

    public void FireProjectiles(Vector2 position, Vector2 direction, float scale = 1f, bool procs = true, int shotCountOverride = 0)
    {
        if (owner.bulletPrefab == null) return;
        int count = ProjectileCount;
        if (shotCountOverride > 0) count = Mathf.Clamp(shotCountOverride, 1, 64);
        float fan = Definition.kind == WeaponKind.Shotgun ? Definition.spread : Mathf.Min(60f, Definition.spread * (count - 1));
        if (Definition.kind == WeaponKind.Shotgun) fan = Mathf.Max(10f, fan * (1f - Get(WeaponBuffEffect.ShotgunChoke)));
        WeaponAttackContext context = new WeaponAttackContext();
        for (int i = 0; i < count; i++)
        {
            float angle = count <= 1 ? 0f : (i / (float)(count - 1) - 0.5f) * fan;
            Vector2 aim = Quaternion.Euler(0f, 0f, angle) * direction.normalized;
            GameObject shot = Instantiate(owner.bulletPrefab, position, Quaternion.Euler(0f, 0f, Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg));
            Bullet bullet = shot.GetComponent<Bullet>();
            if (bullet == null) { Destroy(shot); continue; }
            bullet.baseDamage = Definition.damage;
            bullet.bulletSpeed = Definition.projectileSpeed;
            bullet.Initialize(aim, owner, Definition.pierce + Mathf.RoundToInt(Get(WeaponBuffEffect.BowPierce)));
            bullet.SetWeapon(this, context, scale, procs);
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
