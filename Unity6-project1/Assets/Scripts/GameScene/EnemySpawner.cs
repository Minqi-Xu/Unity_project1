using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;  // The enemy prefab to spawn
    public float initialSpawnDelay = 2f; // Initial delay between spawns
    public float spawnRate = 5f; // Rate at which enemies spawn
    public float spawnIncreaseRate = 0.1f; // How fast the spawn rate increases
    public float healIncreaseRate = 20f; // How much enemy heal increases over time
    public float healIncreaseTime = 30f; // How long will it cost for enemy increase their heal to next level

    private float currentSpawnRate;
    private float gameStartTime;
    private float gameTime = 0f;    // Game timer records game time (exclude pause)

    void Start()
    {
        currentSpawnRate = spawnRate;
        gameStartTime = Time.time; // get the start time of the game
        StartCoroutine(SpawnEnemies());
    }

    void Update()
    {
        gameTime += Time.deltaTime;
    }

    IEnumerator SpawnEnemies()
    {
        yield return new WaitForSeconds(initialSpawnDelay);

        while (true)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(currentSpawnRate);
            currentSpawnRate = Mathf.Max(1f, currentSpawnRate - spawnIncreaseRate); // Decrease spawn time, limit it to 1 second minimum
        }
    }

    void SpawnEnemy()
    {
        // Get screen bounds in world coordinates
        Vector3 minScreenBounds = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 maxScreenBounds = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));

        Vector2 spawnPosition;

        // Randomly pick a position outside the screen bounds
        if (Random.value > 0.5f)
        {
            // Spawn along the horizontal sides (left or right)
            float xPos = Random.value > 0.5f ? maxScreenBounds.x + 1f : minScreenBounds.x - 1f;
            float yPos = Random.Range(minScreenBounds.y - 1f, maxScreenBounds.y + 1f);
            spawnPosition = new Vector2(xPos, yPos);
        }
        else
        {
            // Spawn along the vertical sides (top or bottom)
            float yPos = Random.value > 0.5f ? maxScreenBounds.y + 1f : minScreenBounds.y - 1f;
            float xPos = Random.Range(minScreenBounds.x - 1f, maxScreenBounds.x + 1f);
            spawnPosition = new Vector2(xPos, yPos);
        }

        // Spawn the enemy outside the screen
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        // Log spawn position for debugging
        //Debug.Log($"Spawned enemy at {spawnPosition}.");

        // Get Enemy component and increase base health based on time.
        Enemy enemyScript = enemy.GetComponent<Enemy>();

        if(enemyScript != null)
        {
            // calculate the base health
            float additionalHealth = Mathf.Floor(gameTime / healIncreaseTime) * healIncreaseRate; // increase heal every setting time

            enemyScript.maxHealth += additionalHealth;
            enemyScript.currentHealth = enemyScript.maxHealth;
            //Debug.Log($"Spawned enemy with {enemyScript.maxHealth} HP");
        }
    }
}
