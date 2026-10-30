using UnityEngine;

// Moves toward the player, scales contact damage over time, takes damage, and drops experience on death.
public class Enemy : MonoBehaviour
{
    // Enemy movement speed before camera scaling.
    public float speed = 3f;  // Speed of the enemy

    // Maximum enemy health at spawn time.
    public float maxHealth = 100f; // Max health of the enemy

    // Current enemy health. Hidden because it is runtime state.
    [HideInInspector]
    public float currentHealth;

    // Experience pickup prefab spawned when this enemy dies.
    public GameObject experiencePrefab; // link to the exp prefab in the inspector

    // Starting contact damage before survival-time scaling.
    public float baseDamage = 10f;  // Base damage of enemy

    // Damage added per second while this enemy remains alive.
    public float damageIncreaseRate = 0.1f; // Rate of damage increase within the time enemy survive

    // Maximum contact damage this enemy can deal in one hit.
    public float maxDamage = 100 * 0.4f;   // 100 -> player max hp, 0.4f -> 40% of player's max hp

    // Runtime target and combat state.
    private Transform player; // Reference to the player's position
    private float currentDamage;
    private float damageMultiplier = 1f;  // damageMultiplier, currently not use, but may be used for buff

    // Camera references used for resolution-aware movement scaling.
    private float cameraSizeFactor; // Since camera size changed due to resolution change, the speed related should also changed accordingly
    private Camera mainCamera;
    private CameraScaler cameraScaler;
    private bool loggedMissingPlayer;
    // Prevent multiple projectiles or explosions from granting duplicate death rewards.
    private bool dead;
    public bool IsAlive => !dead && currentHealth > 0f;
    public EnemyRank Rank { get; private set; }
    public EnemyAffix Affixes { get; private set; }
    public string DisplayName { get; private set; } = "Chaser";
    private EnemyDefinition definition;
    private float nextAttack;
    private float nextDash = 5f;
    private float dashUntil;
    private float dashStarts;
    private Vector2 dashDirection;
    private float auraUntil;
    private float nextAura;
    private float nextContact;
    private float nextBossAttack = 4f;
    private bool initialized;
    private SpriteRenderer sprite;

    public void Configure(EnemyDefinition data, EnemyRank rank, EnemyAffix affixes, float extraHealth)
    {
        definition = data;
        Rank = rank;
        Affixes = affixes;
        DisplayName = data.displayName;
        float rankHealth = rank == EnemyRank.FinalBoss ? 35f : rank == EnemyRank.MiniBoss ? 16f : rank == EnemyRank.Elite ? 7f : 1f;
        float rankSize = rank == EnemyRank.FinalBoss ? 3.5f : rank == EnemyRank.MiniBoss ? 2.7f : rank == EnemyRank.Elite ? 1.8f : 1f;
        maxHealth = (maxHealth + Mathf.Max(0f, extraHealth)) * data.healthMultiplier * rankHealth * (Has(EnemyAffix.Healthy) ? 1.8f : 1f);
        currentHealth = maxHealth;
        speed *= data.speedMultiplier * (Has(EnemyAffix.Swift) ? 1.5f : 1f);
        baseDamage *= data.damageMultiplier * (rank == EnemyRank.Normal ? 1f : 1.5f);
        transform.localScale *= data.sizeMultiplier * rankSize;
        sprite = GetComponent<SpriteRenderer>();
        if (sprite != null) sprite.color = rank == EnemyRank.Normal ? data.tint : rank == EnemyRank.Elite ? new Color(1f,.5f,.1f) : new Color(1f,.15f,.5f);
        nextAttack = Time.time + 2f;
        nextDash = Time.time + 5f;
        nextBossAttack = Time.time + 4f;
        initialized = true;
    }

    private bool Has(EnemyAffix affix) => (Affixes & affix) != 0;
    public void SetEncounterTitle(string title)
    {
        if (!string.IsNullOrWhiteSpace(title)) DisplayName = gameObject.name = title;
    }

    // Initializes target lookup, health, damage, and camera references.
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
        if (!initialized) currentHealth = maxHealth;

        // Set initial damage
        currentDamage = baseDamage;

        mainCamera = Camera.main;
        cameraScaler = mainCamera != null ? mainCamera.GetComponent<CameraScaler>() : null;
    }

    // Moves toward the player and increases contact damage while alive.
    void Update()
    {
        if (dead || Time.deltaTime <= 0f) return;
        // get the camera's orthographic size
        cameraSizeFactor = GetCameraSizeFactor();
        if (player != null)
        {
            // Move towards the player's position
            MoveAndAttack();

            // Debug.Log($"Enemy moving towards player. Position: {transform.position}, Direction: {direction}");

            // Increase damage overtime
            currentDamage += damageIncreaseRate * Time.deltaTime;
        }
        else
        {
            TryFindPlayer();
        }
    }

    // Damages the player on contact and destroys this enemy without dropping experience.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Contact(collision);
    }

    private void OnTriggerStay2D(Collider2D collision) => Contact(collision);

    private void Contact(Collider2D collision)
    {
        if (dead || Time.timeScale <= 0f || Time.time < nextContact) return;
        if(collision.GetComponentInParent<PlayerHealth>() != null)
        {
            PlayerHealth playerHealth = collision.GetComponentInParent<PlayerHealth>();
            PlayerController playerController = collision.GetComponentInParent<PlayerController>();
            if(playerHealth != null)
            {
                //Debug.Log($"Enemy deal {Mathf.Min(currentDamage * damageMultiplier, maxDamage)} dmg");
                playerHealth.TakeDamage(Mathf.Min(currentDamage * damageMultiplier, maxDamage), playerController);  // Deal damage to player with damage multiplier applied
            }

            // Destroy the enemy (without exp drop)
            nextContact = Time.time + 1f;
            if (Rank == EnemyRank.Normal)
            {
                dead = true;
                Destroy(gameObject);
            }
        }
    }

    // Reduces enemy health and kills it when health reaches zero.
    // Run when receive damage
    public void TakeDamage(float damage)
    {
        if (dead || damage <= 0f) return;
        currentHealth -= damage;
        //Debug.Log($"Enemy took {damage} damage, {currentHealth} health remaining");

        if(currentHealth <= 0)
        {
            Die();
        }
    }

    // Drops experience if configured and destroys this enemy.
    void Die()
    {
        if (dead) return;
        dead = true;
        //Debug.Log("Enemy die!");
        // Death effects, and destroy

        // Drop experience
        if (experiencePrefab != null)
        {
            Instantiate(experiencePrefab, transform.position, Quaternion.identity);
        }

        if (Has(EnemyAffix.DeathBlast))
            EnemyHazard.Blast(transform.position, 2.8f * GetCameraSizeFactor(), baseDamage * 1.5f, .9f);
        if (Rank != EnemyRank.Normal && RewardManager.Instance != null)
            RewardManager.Instance.DropEnemyChest(transform.position, sprite != null ? sprite : GetComponent<SpriteRenderer>(), Rank);
        if (Rank == EnemyRank.FinalBoss && GameManager.Instance != null)
            GameManager.Instance.NotifyFinalBossDefeated();

        Destroy(gameObject);
    }

    private void MoveAndAttack()
    {
        Vector2 offset = player.position - transform.position;
        float scale = cameraSizeFactor;
        Vector2 direction = offset.normalized;
        float move = speed * (Time.time < auraUntil ? 1.35f : 1f) * scale;
        if (Has(EnemyAffix.PeriodicDash) && Time.time >= nextDash)
        {
            dashDirection = direction;
            dashStarts = Time.time + .7f;
            dashUntil = dashStarts + .5f;
            nextDash = Time.time + 6f;
            EnemyHazard.WarnDirection(transform.position, dashDirection, move * 3.5f * .5f, .7f);
        }
        if (Time.time < dashUntil)
        {
            if (Time.time >= dashStarts) transform.position += (Vector3)(dashDirection * move * 3.5f * Time.deltaTime);
        }
        else if (definition != null && definition.kind == EnemyKind.Ranged)
        {
            float range = definition.preferredRange * scale;
            if (offset.magnitude > range * 1.1f) transform.position += (Vector3)(direction * move * Time.deltaTime);
            else if (offset.magnitude < range * .7f) transform.position -= (Vector3)(direction * move * Time.deltaTime);
            if (Time.time >= nextAttack && offset.magnitude < range * 1.8f)
            {
                EnemyHazard.Shot(transform.position, direction, definition.projectileSpeed * scale, baseDamage, .45f);
                nextAttack = Time.time + definition.attackInterval;
            }
        }
        else transform.position = Vector2.MoveTowards(transform.position, player.position, move * Time.deltaTime);

        if (Has(EnemyAffix.HasteAura) && Time.time >= nextAura)
        {
            nextAura = Time.time + .5f;
            foreach (Collider2D hit in Physics2D.OverlapCircleAll(transform.position, 4f * scale))
            {
                Enemy ally = hit.GetComponentInParent<Enemy>();
                if (ally != null && ally != this && ally.Rank == EnemyRank.Normal && ally.IsAlive) ally.auraUntil = Time.time + .65f;
            }
            WeaponVisuals.Pulse(transform.position, 4f * scale, new Color(1f,.6f,.1f,.4f));
        }
        if (Rank >= EnemyRank.MiniBoss && Time.time >= nextBossAttack)
        {
            // Lock the target position during the warning, allowing the player to dodge.
            EnemyHazard.Blast(player.position, (Rank == EnemyRank.FinalBoss ? 3f : 2f) * scale, baseDamage * 1.5f, 1.2f);
            nextBossAttack = Time.time + (Rank == EnemyRank.FinalBoss ? 3.5f : 5f);
        }
    }

    private void OnGUI()
    {
        if (Rank == EnemyRank.Normal || !IsAlive || Camera.main == null) return;
        Vector3 screen = Camera.main.WorldToScreenPoint(transform.position);
        if (screen.z < 0f) return;
        float x = screen.x - 110f, y = Screen.height - screen.y - 65f;
        GUI.Box(new Rect(x, y, 220f, 24f), DisplayName + " | " + Rank);
        GUI.Box(new Rect(x, y + 24f, 220f, 20f), "");
        Color previous = GUI.color;
        GUI.color = Color.red;
        GUI.DrawTexture(new Rect(x + 2f, y + 26f, 216f * Mathf.Clamp01(currentHealth / maxHealth), 16f), Texture2D.whiteTexture);
        GUI.color = previous;
        if (Affixes != EnemyAffix.None) GUI.Label(new Rect(x, y + 45f, 300f, 24f), Affixes.ToString());
    }

    // Retries player lookup without logging every frame while the player is not spawned yet.
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

    // Returns movement scale based on current camera size.
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
