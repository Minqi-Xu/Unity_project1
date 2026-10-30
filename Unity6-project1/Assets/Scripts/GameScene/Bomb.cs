using UnityEngine;
using System.Collections.Generic;

// Moves a thrown bomb and applies area damage when it hits an enemy or reaches max range.
public class Bomb: MonoBehaviour
{
    // Bomb movement speed before camera scaling.
    public float bombSpeed = 10f;   // speed of throwing the bomb

    // Radius used to find enemies damaged by the explosion.
    public float explosionRadius = 3f;  // explosion radius

    // Optional visual effect prefab spawned when the bomb explodes.
    public GameObject explossionEffectprefab; // link to explosion effect prefab in the inspector

    // Maximum travel distance before auto-explosion.
    public float maxDistance = 10f;     // max distance flying before explode

    // Base explosion damage before player damage modifiers.
    public float basedamage = 50f;      // damage dealt when explode

    // Runtime projectile state.
    private Vector2 direction;
    private Vector3 spawnPosition;
    private PlayerController playerController;  // Reference to playerController
    // Unity destroys objects at frame end, so guard against repeated explosion callbacks.
    private bool exploded;

    // Camera references used for resolution-aware projectile movement.
    private float cameraSizeFactor;     // Since camera size changed due to resolution change, the speed related should also changed accordingly
    private Camera mainCamera;
    private CameraScaler cameraScaler;

    // Initializes projectile direction and owner after instantiation.
    public void Initialize(Vector2 dir, PlayerController controller)
    {
        direction = dir.normalized;
        spawnPosition = transform.position;
        playerController = controller;
        mainCamera = Camera.main;
        cameraScaler = mainCamera != null ? mainCamera.GetComponent<CameraScaler>() : null;
    }

    // Moves the bomb and triggers explosion once max travel distance is reached.
    void Update()
    {
        // get the camera's orthographic size
        cameraSizeFactor = GetCameraSizeFactor();

        // Move the bomb in the direction set
        transform.position += (Vector3)direction * bombSpeed * Time.deltaTime * cameraSizeFactor;

        // Check if the bomb has traveled too far to auto exploded
        if(Vector2.Distance(spawnPosition, transform.position) >= maxDistance)
        {
            // Auto explode when reach maxDistance
            Explode();
        }
    }

    // Explodes immediately when colliding with an enemy.
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Enemy"))
        {
            // Explode if hits an enemy
            Explode();
        }
    }

    // Spawns explosion effects, damages enemies in radius, and destroys the bomb.
    void Explode()
    {
        if (exploded) return;
        exploded = true;
        // Instantiate the explosion effect
        if (explossionEffectprefab != null)
        {
            Instantiate(explossionEffectprefab, transform.position, Quaternion.identity);
        }

        float radius = explosionRadius * (playerController != null && playerController.Stats != null
            ? Mathf.Max(0f, playerController.Stats.explosionRadiusMultiplier) : 1f);
        DamageArea(transform.position, radius, basedamage, playerController, false);
        // Destroy bomb after explode
        Destroy(gameObject);
    }

    // Applies one independently rolled hit per enemy, even when it has multiple colliders.
    public static void DamageArea(Vector2 center, float radius, float baseDamage, PlayerController owner, bool bullet)
    {
        HashSet<Enemy> damaged = new HashSet<Enemy>();
        foreach (Collider2D collider in Physics2D.OverlapCircleAll(center, radius))
        {
            Enemy enemy = collider.GetComponentInParent<Enemy>();
            if (enemy == null || !damaged.Add(enemy)) continue;
            float damage = owner != null && owner.Stats != null
                ? owner.Stats.CalculateDamage(baseDamage, owner.damageMultiplier, bullet)
                : baseDamage * (owner != null ? owner.GetDamageMultiplier() : 1f);
            enemy.TakeDamage(damage);
        }
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

    // Draws explosion radius in the Unity Scene view.
    void OnDrawGizmosSelected()
    {
        // Show the explosion radius in the scene view
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
