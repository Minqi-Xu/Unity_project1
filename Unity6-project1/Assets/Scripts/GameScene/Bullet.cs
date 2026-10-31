using UnityEngine;
using System.Collections.Generic;

// Moves a bullet projectile and applies player-scaled damage when it hits an enemy.
public class Bullet : MonoBehaviour
{
    // Projectile movement speed before camera scaling.
    public float bulletSpeed = 25f; // Speed of the bullet

    // Base damage before player damage modifiers.
    public float baseDamage = 8f;

    // Runtime projectile state.
    private Vector2 direction;
    private Vector3 spawnPosition;
    private PlayerController playerController;  // Reference to the PlayerController
    // Snapshot the penetration budget at launch; each enemy can be hit only once.
    private int remainingHits = 1;
    private bool spent;
    private PlayerWeaponSystem weapon;
    private WeaponAttackContext attackContext;
    private float weaponScale = 1f;
    private bool allowProcs = true;
    private int hitIndex;
    private int initialHits;
    private float launched;
    private bool returning;

    public void SetWeapon(PlayerWeaponSystem source, WeaponAttackContext context, float scale, bool procs)
    {
        weapon = source; attackContext = context; weaponScale = scale; allowProcs = procs;
    }

    private void BeginReturn()
    {
        returning = true;
        hitEnemies.Clear();
        remainingHits = initialHits;
        hitIndex = 0;
    }
    private readonly HashSet<Enemy> hitEnemies = new HashSet<Enemy>();
    private readonly HashSet<ZoneSeal> hitSeals = new HashSet<ZoneSeal>();

    // Camera references used for resolution-aware projectile movement.
    private float cameraSizeFactor; // Since camera size changed due to resolution change, the speed related should also changed accordingly
    private Camera mainCamera;
    private CameraScaler cameraScaler;


    // Initializes projectile direction and owner after instantiation.
    // This method is called when the bullet is instantiated
    public void Initialize(Vector2 dir, PlayerController controller, int weaponPierce = 0)
    {
        direction = dir.normalized;
        spawnPosition = transform.position;
        playerController = controller;  // Store the reference
        remainingHits = 1 + Mathf.Max(0, weaponPierce) + (controller != null && controller.Stats != null ? controller.Stats.GetPierceCount() : 0);
        spent = false;
        initialHits = remainingHits;
        launched = Time.time;
        hitEnemies.Clear();
        hitSeals.Clear();
        mainCamera = Camera.main;
        cameraScaler = mainCamera != null ? mainCamera.GetComponent<CameraScaler>() : null;
    }

    // Moves the bullet forward and destroys it after it travels too far.
    void Update()
    {
        if (weapon != null && weapon.Get(WeaponBuffEffect.BowReturn) > 0f && !returning && Time.time - launched >= .45f)
            BeginReturn();
        if (returning)
        {
            if (playerController == null || Vector2.Distance(transform.position, playerController.transform.position) < .4f)
            { Destroy(gameObject); return; }
            direction = (playerController.transform.position - transform.position).normalized;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        }
        // get the camera's orthographic size
        cameraSizeFactor = GetCameraSizeFactor();
        // Move the bullet in the specified direction
        transform.position += (Vector3)direction * bulletSpeed * Time.deltaTime * cameraSizeFactor;

        // Optionally, destroy the bullet after a certain distance (e.g., 100 units)
        if (Vector2.Distance(transform.position, spawnPosition) > 100f) // Adjust as needed
        {
            Destroy(gameObject);
        }
    }

    // Damages an enemy on contact and then destroys the bullet.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (spent) return;
        ZoneSeal seal = other.GetComponentInParent<ZoneSeal>();
        if (seal != null)
        {
            if (!hitSeals.Add(seal)) return;
            PlayerStats sealStats = playerController != null ? playerController.Stats : null;
            float raw = baseDamage * weaponScale * (weapon != null ? weapon.LevelDamageMultiplier *
                (1f + weapon.Get(WeaponBuffEffect.Damage)) * weapon.CharacterDamageMultiplier : 1f);
            seal.TakeDamage(sealStats != null ? sealStats.CalculateDamage(raw, playerController.damageMultiplier,
                weapon == null || weapon.IsPrimaryWeapon) : raw);
            remainingHits--;
            if (remainingHits <= 0) { spent = true; Destroy(gameObject); }
            return;
        }
        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy == null || !hitEnemies.Add(enemy)) return;
        PlayerStats stats = playerController != null ? playerController.Stats : null;
        if (weapon != null) weapon.Hit(enemy, baseDamage, spawnPosition, hitIndex++, attackContext, weaponScale, allowProcs, true);
        else
        {
            float damage = stats != null ? stats.CalculateDamage(baseDamage, playerController.damageMultiplier, true)
                : baseDamage * (playerController != null ? playerController.GetDamageMultiplier() : 1f);
            enemy.TakeDamage(damage, stats, true, true);
        }
        remainingHits--;
        if (remainingHits > 0) return;

        if (!returning && weapon != null && weapon.Get(WeaponBuffEffect.BowReturn) > 0f)
        { BeginReturn(); return; }

        spent = true;
        if (stats != null && stats.finalPierceExplosion > 0f)
        {
            // This secondary hit cannot create another projectile or recursively explode.
            float radius = 2f * Mathf.Max(0f, stats.explosionRadiusMultiplier);
            if (weapon != null) weapon.DamageArea(transform.position, radius,
                baseDamage * stats.finalPierceExplosion * weaponScale, null, false);
            else Bomb.DamageArea(transform.position, radius, baseDamage * stats.finalPierceExplosion, playerController, true, false);
            Bomb bomb = playerController.bombPrefab != null ? playerController.bombPrefab.GetComponent<Bomb>() : null;
            if (bomb != null && bomb.explossionEffectprefab != null)
                Instantiate(bomb.explossionEffectprefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    // Returns projectile movement scale based on current camera size.
    private float GetCameraSizeFactor()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (cameraScaler == null && mainCamera != null)
        {
            cameraScaler = mainCamera.GetComponent<CameraScaler>();
        }

        return mainCamera != null && cameraScaler != null
            ? mainCamera.orthographicSize / cameraScaler.baseOrthographicSize
            : 1f;
    }
}
