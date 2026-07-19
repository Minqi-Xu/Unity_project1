using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 3f;  // Speed of the enemy、
    public float maxHealth = 100f; // Max health of the enemy
    [HideInInspector]
    public float currentHealth;
    public GameObject experiencePrefab; // link to the exp prefab in the inspector
    public float baseDamage = 10f;  // Base damage of enemy
    public float damageIncreaseRate = 0.1f; // Rate of damage increase within the time enemy survive
    public float maxDamage = 100 * 0.4f;   // 100 -> player max hp, 0.4f -> 40% of player's max hp
    private Transform player; // Reference to the player’s position
    private float currentDamage;
    private float damageMultiplier = 1f;  // damageMultiplier, currently not use, but may be used for buff
    private float cameraSizeFactor; // Since camera size changed due to resolution change, the speed related should also changed accordingly
    private Camera mainCamera;
    private CameraScaler cameraScaler;
    private bool loggedMissingPlayer;

    void Start()
    {
        // Find the player by tag (assuming you tagged the player as "Player")
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player not found! Make sure the player is tagged as 'Player'.");
            loggedMissingPlayer = true;
        }

        // Set initial health
        currentHealth = maxHealth;

        // Set initial damage
        currentDamage = baseDamage;

        mainCamera = Camera.main;
        cameraScaler = mainCamera != null ? mainCamera.GetComponent<CameraScaler>() : null;
    }

    void Update()
    {
        // get the camera's orthographic size
        cameraSizeFactor = GetCameraSizeFactor();
        if (player != null)
        {
            // Move towards the player's position
            Vector2 direction = (player.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime * cameraSizeFactor);

            // Debug.Log($"Enemy moving towards player. Position: {transform.position}, Direction: {direction}");

            // Increase damage overtime
            currentDamage += damageIncreaseRate * Time.deltaTime;
        }
        else
        {
            TryFindPlayer();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player")) // if the player is "touch" by enemy
        {
            PlayerHealth playerHealth = collision.GetComponentInParent<PlayerHealth>();
            PlayerController playerController = collision.GetComponentInParent<PlayerController>();
            if(playerHealth != null)
            {
                //Debug.Log($"Enemy deal {Mathf.Min(currentDamage * damageMultiplier, maxDamage)} dmg");
                playerHealth.TakeDamage(Mathf.Min(currentDamage * damageMultiplier, maxDamage), playerController);  // Deal damage to player with damage multiplier applied
            }

            // Destroy the enemy (without exp drop)
            Destroy(gameObject);
        }
    }

    // Run when receive damage
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        //Debug.Log($"Enemy took {damage} damage, {currentHealth} health remaining");

        if(currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        //Debug.Log("Enemy die!");
        // Death effects, and destroy

        // Drop experience
        if (experiencePrefab != null)
        {
            Instantiate(experiencePrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private void TryFindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            return;
        }

        if (!loggedMissingPlayer)
        {
            Debug.LogError("Player not found! Make sure the player is tagged as 'Player'.");
            loggedMissingPlayer = true;
        }
    }

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
