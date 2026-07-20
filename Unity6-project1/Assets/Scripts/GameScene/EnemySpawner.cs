using UnityEngine;
using System.Collections;

/// <summary>
/// Spawns enemies outside the camera view and increases pressure over time.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    /// <summary>Enemy prefab to spawn.</summary>
    public GameObject enemyPrefab;  // The enemy prefab to spawn

    /// <summary>Delay before the first enemy spawn.</summary>
    public float initialSpawnDelay = 2f; // Initial delay between spawns

    /// <summary>Initial interval between enemy spawns.</summary>
    public float spawnRate = 5f; // Rate at which enemies spawn

    /// <summary>Amount subtracted from spawn interval after each spawn.</summary>
    public float spawnIncreaseRate = 0.1f; // How fast the spawn rate increases

    /// <summary>Health added to new enemies at each health-scaling step.</summary>
    public float healIncreaseRate = 20f; // How much enemy heal increases over time

    /// <summary>Seconds between enemy health-scaling steps.</summary>
    public float healIncreaseTime = 30f; // How long will it cost for enemy increase their heal to next level

    // Runtime spawn pacing state.
    private float currentSpawnRate;
    private float gameTime = 0f;    // Game timer records game time (exclude pause)
    private Camera mainCamera;

    /// <summary>
    /// Validates required references and starts the spawn loop.
    /// </summary>
    void Start()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemySpawner has no enemy prefab assigned.");
            enabled = false;
            return;
        }

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("EnemySpawner cannot find a camera tagged MainCamera.");
            enabled = false;
            return;
        }

        currentSpawnRate = Mathf.Max(0.1f, spawnRate);
        StartCoroutine(SpawnEnemies());
    }

    /// <summary>
    /// Tracks elapsed game time for enemy scaling.
    /// </summary>
    void Update()
    {
        gameTime += Time.deltaTime;
    }

    /// <summary>
    /// Repeatedly spawns enemies and reduces spawn interval down to a minimum.
    /// </summary>
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

    /// <summary>
    /// Chooses an off-screen position, spawns one enemy, and applies health scaling.
    /// </summary>
    void SpawnEnemy()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }
        }

        // Get screen bounds in world coordinates
        Vector3 minScreenBounds = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 maxScreenBounds = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));

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
            float additionalHealth = healIncreaseTime > 0f
                ? Mathf.Floor(gameTime / healIncreaseTime) * healIncreaseRate
                : 0f; // increase heal every setting time

            enemyScript.maxHealth += additionalHealth;
            enemyScript.currentHealth = enemyScript.maxHealth;
            //Debug.Log($"Spawned enemy with {enemyScript.maxHealth} HP");
        }
    }
}
