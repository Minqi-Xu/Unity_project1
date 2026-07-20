using UnityEngine;

/// <summary>
/// Moves a thrown bomb and applies area damage when it hits an enemy or reaches max range.
/// </summary>
public class Bomb: MonoBehaviour
{
    /// <summary>Bomb movement speed before camera scaling.</summary>
    public float bombSpeed = 10f;   // speed of throwing the bomb

    /// <summary>Radius used to find enemies damaged by the explosion.</summary>
    public float explosionRadius = 3f;  // explosion radius

    /// <summary>Optional visual effect prefab spawned when the bomb explodes.</summary>
    public GameObject explossionEffectprefab; // link to explosion effect prefab in the inspector

    /// <summary>Maximum travel distance before auto-explosion.</summary>
    public float maxDistance = 10f;     // max distance flying before explode

    /// <summary>Base explosion damage before player damage modifiers.</summary>
    public float basedamage = 50f;      // damage dealt when explode

    // Runtime projectile state.
    private Vector2 direction;
    private Vector3 spawnPosition;
    private PlayerController playerController;  // Reference to playerController

    // Camera references used for resolution-aware projectile movement.
    private float cameraSizeFactor;     // Since camera size changed due to resolution change, the speed related should also changed accordingly
    private Camera mainCamera;
    private CameraScaler cameraScaler;

    /// <summary>
    /// Initializes projectile direction and owner after instantiation.
    /// </summary>
    public void Initialize(Vector2 dir, PlayerController controller)
    {
        direction = dir.normalized;
        spawnPosition = transform.position;
        playerController = controller;
        mainCamera = Camera.main;
        cameraScaler = mainCamera != null ? mainCamera.GetComponent<CameraScaler>() : null;
    }

    /// <summary>
    /// Moves the bomb and triggers explosion once max travel distance is reached.
    /// </summary>
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

    /// <summary>
    /// Explodes immediately when colliding with an enemy.
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Enemy"))
        {
            // Explode if hits an enemy
            Explode();
        }
    }

    /// <summary>
    /// Spawns explosion effects, damages enemies in radius, and destroys the bomb.
    /// </summary>
    void Explode()
    {
        // Instantiate the explosion effect
        if (explossionEffectprefab != null)
        {
            Instantiate(explossionEffectprefab, transform.position, Quaternion.identity);
        }

        // Find all colliders in the explosion radius
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach(var hitCollider in hitColliders)
        {
            // Deal damage to each enemy within the explosion radius
            Enemy enemy = hitCollider.GetComponent<Enemy>();
            if(enemy != null)
            {
                // calculate damage with the damage multiplier
                float currentDamage = basedamage * (playerController != null ? playerController.GetDamageMultiplier() : 1f);
                enemy.TakeDamage(currentDamage);
            }
        }
        // Destroy bomb after explode
        Destroy(gameObject);
    }

    /// <summary>
    /// Returns projectile movement scale based on current camera size.
    /// </summary>
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

    /// <summary>
    /// Draws explosion radius in the Unity Scene view.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // Show the explosion radius in the scene view
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
