using UnityEngine;

/// <summary>
/// Moves a bullet projectile and applies player-scaled damage when it hits an enemy.
/// </summary>
public class Bullet : MonoBehaviour
{
    /// <summary>Projectile movement speed before camera scaling.</summary>
    public float bulletSpeed = 25f; // Speed of the bullet

    /// <summary>Base damage before player damage modifiers.</summary>
    public float baseDamage = 8f;

    // Runtime projectile state.
    private Vector2 direction;
    private Vector3 spawnPosition;
    private PlayerController playerController;  // Reference to the PlayerController

    // Camera references used for resolution-aware projectile movement.
    private float cameraSizeFactor; // Since camera size changed due to resolution change, the speed related should also changed accordingly
    private Camera mainCamera;
    private CameraScaler cameraScaler;


    /// <summary>
    /// Initializes projectile direction and owner after instantiation.
    /// </summary>
    // This method is called when the bullet is instantiated
    public void Initialize(Vector2 dir, PlayerController controller)
    {
        direction = dir.normalized;
        spawnPosition = transform.position;
        playerController = controller;  // Store the reference
        mainCamera = Camera.main;
        cameraScaler = mainCamera != null ? mainCamera.GetComponent<CameraScaler>() : null;
    }

    /// <summary>
    /// Moves the bullet forward and destroys it after it travels too far.
    /// </summary>
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

    /// <summary>
    /// Damages an enemy on contact and then destroys the bullet.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();

            if(enemy != null)
            {
                // increase damage over time
                float currentDamage = baseDamage * (playerController != null ? playerController.GetDamageMultiplier() : 1f);

                // Deal damage to enemy hit
                enemy.TakeDamage(currentDamage);
                // Debug.Log($"Deal {currentDamage} damage to enemy!");
            }
            Destroy(gameObject);
        }
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
}
