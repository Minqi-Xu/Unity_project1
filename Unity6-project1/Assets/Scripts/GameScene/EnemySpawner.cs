using UnityEngine;
using System.Collections;

// Spawns enemies outside the camera view and increases pressure over time.
public class EnemySpawner : MonoBehaviour
{
    // Enemy prefab to spawn.
    public GameObject enemyPrefab;  // The enemy prefab to spawn

    // Delay before the first enemy spawn.
    public float initialSpawnDelay = 2f; // Initial delay between spawns

    // Initial interval between enemy spawns.
    public float spawnRate = 4f; // Seconds between early-game enemy spawns

    // Amount subtracted from spawn interval after each spawn.
    public float spawnIncreaseRate = 0.1f; // How fast the spawn rate increases

    [Min(.1f)] public float minimumSpawnInterval = .8f;

    // Health added to new enemies at each health-scaling step.
    public float healIncreaseRate = 20f; // How much enemy heal increases over time

    // Seconds between enemy health-scaling steps.
    public float healIncreaseTime = 30f; // How long will it cost for enemy increase their heal to next level

    // Runtime spawn pacing state.
    private float currentSpawnRate;
    private float gameTime = 0f;    // Game timer records game time (exclude pause)
    private Camera mainCamera;
    [Header("Enemy roster and timed encounters")]
    public EnemyDefinition[] enemyTypes;
    public EnemyEncounter[] encounters = EnemyEncounter.Defaults();
    [Min(0f)] public float encounterWarningTime = 2f;
    public bool finalBossRequired = true;
    private bool[] encounterSpawned;
    private bool ownsCatalog;
    public float StartingCharacterAverageSpeed { get; private set; } = 5f;
    public float ElapsedTime => gameTime;
    public bool RequiresFinalBoss => enabled && finalBossRequired && encounters != null &&
        System.Array.Exists(encounters, item => item != null && item.rank == EnemyRank.FinalBoss);

    // Validates required references and starts the spawn loop.
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

        currentSpawnRate = Mathf.Max(Mathf.Max(.1f, minimumSpawnInterval), spawnRate);
        CharacterDefinition[] roster = GameManager.Instance != null ? GameManager.Instance.characterDefinitions : null;
        if (roster == null || roster.Length == 0) roster = CharacterCatalog.Load();
        StartingCharacterAverageSpeed = CharacterCatalog.GetStartingAverageMoveSpeed(roster);
        if (enemyTypes == null || enemyTypes.Length == 0)
        {
            enemyTypes = Resources.LoadAll<EnemyDefinition>("Enemies");
            if (enemyTypes.Length == 0)
            {
                enemyTypes = EnemyCatalog.CreateDefaults();
                ownsCatalog = true;
            }
        }
        if (encounters == null) encounters = EnemyEncounter.Defaults();
        encounterSpawned = new bool[encounters.Length];
        StartCoroutine(SpawnEnemies());
    }

    // Tracks elapsed game time for enemy scaling.
    void Update()
    {
        gameTime += Time.deltaTime;
        for (int i = 0; i < encounters.Length; i++)
        {
            EnemyEncounter entry = encounters[i];
            if (entry == null || encounterSpawned[i] || !entry.IsDue(gameTime)) continue;
            encounterSpawned[i] = true;
            SpawnEnemy(entry);
        }
    }

    // Repeatedly spawns enemies and reduces spawn interval down to a minimum.
    IEnumerator SpawnEnemies()
    {
        yield return new WaitForSeconds(initialSpawnDelay);

        while (true)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(currentSpawnRate);
            currentSpawnRate = Mathf.Max(Mathf.Max(.1f, minimumSpawnInterval), currentSpawnRate - spawnIncreaseRate);
        }
    }

    // Chooses an off-screen position, spawns one enemy, and applies health scaling.
    void SpawnEnemy(EnemyEncounter encounter = null)
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

            EnemyDefinition type = encounter == null ? PickNormalType(gameTime) : FindType(encounter.kind);
            if (type != null)
                enemyScript.Configure(type, encounter != null ? encounter.rank : EnemyRank.Normal,
                    encounter != null ? encounter.affixes : EnemyAffix.None, additionalHealth, StartingCharacterAverageSpeed,
                    encounter != null ? encounter.bossMovementMultiplier : 1f, encounter != null ? encounter.bossRelativeSpeedLimit : 1.05f);
            if (encounter != null) enemyScript.SetEncounterTitle(encounter.title);
            //Debug.Log($"Spawned enemy with {enemyScript.maxHealth} HP");
        }
    }

    public EnemyDefinition PickNormalType(float elapsed)
    {
        float total = 0f;
        foreach (EnemyDefinition type in enemyTypes)
            if (type != null && type.unlockTime <= elapsed) total += Mathf.Max(0f, type.weight);
        float roll = Random.value * total;
        foreach (EnemyDefinition type in enemyTypes)
        {
            if (type == null || type.unlockTime > elapsed || type.weight <= 0f) continue;
            roll -= type.weight;
            if (roll <= 0f) return type;
        }
        return FindType(EnemyKind.Chaser);
    }

    public bool SpawnZoneWave(Vector2 center, float radius, int count)
    {
        if (!isActiveAndEnabled || enemyPrefab == null || enemyTypes == null || enemyTypes.Length == 0) return false;
        for (int i = 0; i < Mathf.Clamp(count, 1, 40); i++)
        {
            float angle = i * Mathf.PI * 2f / Mathf.Clamp(count, 1, 40);
            Vector2 position = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            GameObject obj = Instantiate(enemyPrefab, position, Quaternion.identity);
            Enemy enemy = obj.GetComponent<Enemy>();
            EnemyDefinition type = PickNormalType(gameTime);
            if (enemy != null && type != null)
                enemy.Configure(type, EnemyRank.Normal, EnemyAffix.None,
                    healIncreaseTime > 0f ? Mathf.Floor(gameTime / healIncreaseTime) * healIncreaseRate : 0f,
                    StartingCharacterAverageSpeed);
        }
        return true;
    }

    private EnemyDefinition FindType(EnemyKind kind)
    {
        foreach (EnemyDefinition type in enemyTypes) if (type != null && type.kind == kind) return type;
        foreach (EnemyDefinition type in enemyTypes) if (type != null) return type;
        return null;
    }

    private void OnGUI()
    {
        if (encounterSpawned == null) return;
        for (int i = 0; i < encounters.Length; i++)
        {
            EnemyEncounter entry = encounters[i];
            if (entry == null || encounterSpawned[i] || !entry.IsWarning(gameTime, encounterWarningTime)) continue;
            GUI.Box(new Rect(Screen.width * .2f, 65f, Screen.width * .6f, 45f),
                entry.title + " incoming: " + Mathf.CeilToInt(entry.time - gameTime) + "s");
            break;
        }
    }

    private void OnDestroy()
    {
        if (ownsCatalog && enemyTypes != null)
            foreach (EnemyDefinition type in enemyTypes) if (type != null) Destroy(type);
    }
}
