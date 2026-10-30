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
    private readonly HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

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
        hitEnemies.Clear();
        mainCamera = Camera.main;
        cameraScaler = mainCamera != null ? mainCamera.GetComponent<CameraScaler>() : null;
    }

    // Moves the bullet forward and destroys it after it travels too far.
    void Update()
    {
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
        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy == null || !hitEnemies.Add(enemy)) return;
        PlayerStats stats = playerController != null ? playerController.Stats : null;
        float damage = stats != null ? stats.CalculateDamage(baseDamage, playerController.damageMultiplier, true)
            : baseDamage * (playerController != null ? playerController.GetDamageMultiplier() : 1f);
        enemy.TakeDamage(damage);
        remainingHits--;
        if (remainingHits > 0) return;

        spent = true;
        if (stats != null && stats.finalPierceExplosion > 0f)
        {
            // This secondary hit cannot create another projectile or recursively explode.
            float radius = 2f * Mathf.Max(0f, stats.explosionRadiusMultiplier);
            Bomb.DamageArea(transform.position, radius, baseDamage * stats.finalPierceExplosion, playerController, true);
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
