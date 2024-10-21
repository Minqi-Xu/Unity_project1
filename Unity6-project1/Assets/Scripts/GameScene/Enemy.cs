using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 3f;  // Speed of the enemy、
    public float maxHealth = 100f; // Max health of the enemy
    [HideInInspector]
    public float currentHealth;
    public GameObject experiencePrefab; // link to the exp prefab in the inspector
    private Transform player; // Reference to the player’s position

    void Start()
    {
        // Find the player by tag (assuming you tagged the player as "Player")
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Set initial health
        currentHealth = maxHealth;

        if (player == null)
        {
            Debug.LogError("Player not found! Make sure the player is tagged as 'Player'.");
        }
    }

    void Update()
    {
        if (player != null)
        {
            // Move towards the player's position
            Vector2 direction = (player.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);

            // Debug.Log($"Enemy moving towards player. Position: {transform.position}, Direction: {direction}");
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
