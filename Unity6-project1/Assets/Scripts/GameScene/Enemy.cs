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

    void Start()
    {
        // Find the player by tag (assuming you tagged the player as "Player")
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Set initial health
        currentHealth = maxHealth;

        // Set initial damage
        currentDamage = baseDamage;

        if (player == null)
        {
            Debug.LogError("Player not found! Make sure the player is tagged as 'Player'.");
        }
    }

    void Update()
    {
        // get the camera's orthographic size
        cameraSizeFactor = Camera.main.orthographicSize / FindFirstObjectByType<CameraScaler>().baseOrthographicSize;
        if (player != null)
        {
            // Move towards the player's position
            Vector2 direction = (player.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime * cameraSizeFactor);

            // Debug.Log($"Enemy moving towards player. Position: {transform.position}, Direction: {direction}");

            // Increase damage overtime
            currentDamage += damageIncreaseRate * Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player")) // if the player is "touch" by enemy
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            PlayerController playerController = collision.GetComponent<PlayerController>();
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
        Instantiate(experiencePrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
