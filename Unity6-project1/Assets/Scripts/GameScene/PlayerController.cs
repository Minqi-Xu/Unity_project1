using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Handles player input, movement, dash, weapon firing, experience, level-up rewards, and status debug output.
/// </summary>
public class PlayerController : MonoBehaviour
{
    /// <summary>Base movement speed before camera scaling and stat modifiers.</summary>
    public float moveSpeed = 5f;  // Speed of the player

    /// <summary>Base dash movement speed before stat modifiers.</summary>
    public float dashSpeed = 20f; // Speed during dash

    /// <summary>How long one dash lasts in seconds.</summary>
    public float dashDuration = 0.2f; // How long the dash lasts

    /// <summary>Base dash cooldown in seconds before stat modifiers.</summary>
    public float dashCooldown = 5f; // Dash cooldown time

    /// <summary>Bullet prefab spawned when the player fires with left mouse button.</summary>
    public GameObject bulletPrefab; // link with the bullet prefab in the Inspector

    /// <summary>Bomb prefab spawned when the player fires with right mouse button.</summary>
    public GameObject bombPrefab; // link with the bomb prefab in the Inspector

    /// <summary>Base bullet fire cooldown in seconds before stat modifiers.</summary>
    public float fireCooldown = 0.2f; // Bullet fire cooldown time

    /// <summary>Base bomb fire cooldown in seconds before stat modifiers.</summary>
    public float bombCooldown = 10f; // Bomb cooldown time

    /// <summary>Current player level.</summary>
    public int level = 1; // player level

    /// <summary>Legacy outgoing damage multiplier kept for compatibility with existing gameplay.</summary>
    public float damageMultiplier = 1f; // damage multiplier

    /// <summary>UI text that displays level and experience progress.</summary>
    public TextMeshProUGUI levelText; // Link to a UI text element to display level

    /// <summary>Experience required to gain one level.</summary>
    public int experienceThreshold = 5;    // How many exp to level up

    /// <summary>Cooldown indicator for bullet firing.</summary>
    public CooldownIndicator bulletCooldownIndicator; // Bullet cooldown indicator

    /// <summary>Cooldown indicator for dash.</summary>
    public CooldownIndicator dashCooldownIndicator; // Dash cooldown indicator

    /// <summary>Cooldown indicator for bomb firing.</summary>
    public CooldownIndicator bombCooldownIndicator; // Bomb cooldown indicator

    /// <summary>Elapsed gameplay time for this player. Stops advancing when timeScale is paused.</summary>
    public float gameTime = 0f;    // gameTimer, will stop when pause

    /// <summary>Runtime stat component used for roguelite upgrades and derived player values.</summary>
    public PlayerStats Stats { get; private set; }

    // Cached movement and combat state.
    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 lastMoveDirection = Vector2.right;
    private bool isDashing = false;
    private float dashCooldownTimer = 0f;

    // Cooldown timestamps and experience bookkeeping.
    private float lastFireTime = 0f; // Timestamp of the last shot
    private float lastBombTime = -100f; // Timestamp of the last bomb
    private int currentExperience = 0;  // current experience collected

    // Camera references used for screen bounds and resolution-aware movement scaling.
    private float cameraSizeFactor; // Since camera size changed due to resolution change, the speed related should also changed accordingly
    private Camera mainCamera;
    private CameraScaler cameraScaler;

    /// <summary>
    /// Initializes player components, camera references, cooldown UI, and level UI.
    /// </summary>
    void Start()
    {
        Stats = GetComponent<PlayerStats>();
        if (Stats == null)
        {
            Stats = gameObject.AddComponent<PlayerStats>();
        }

        // Get the Rigidbody2D component attached to the player
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        cameraScaler = mainCamera != null ? mainCamera.GetComponent<CameraScaler>() : null;

        levelText = FindComponentByName<TextMeshProUGUI>("LevelText");
        bulletCooldownIndicator = FindComponentByName<CooldownIndicator>("BulletCooldownIndicator");
        dashCooldownIndicator = FindComponentByName<CooldownIndicator>("DashCooldownIndicator");
        bombCooldownIndicator = FindComponentByName<CooldownIndicator>("BombCooldownIndicator");
        UpdateLevelText();
    }

    /// <summary>
    /// Reads input, advances timers, and triggers dash or weapon actions.
    /// </summary>
    void Update()
    {
        // get the camera's orthographic size
        cameraSizeFactor = GetCameraSizeFactor();
        gameTime += Time.deltaTime;
        // Get input from the player (arrow keys or WASD)
        movement.x = Input.GetAxisRaw("Horizontal");  // Left/Right movement
        movement.y = Input.GetAxisRaw("Vertical");    // Up/Down movement
        if (movement.sqrMagnitude > 0f)
        {
            lastMoveDirection = movement.normalized;
        }

        // Detect dash input
        if(Input.GetKeyDown(KeyCode.Space) && !isDashing && dashCooldownTimer <= 0f)
        {
            StartDash();
        }

        // Detect fire input
        if(Input.GetMouseButton(0))
        {
            FireBullet();
        }

        // Detect bomb input
        if(Input.GetMouseButton(1))
        {
            FireBomb();
        }

        // Reduce dash cooldown timer
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

    }

    /// <summary>
    /// Applies physics movement when the player is not currently dashing.
    /// </summary>
    void FixedUpdate()
    {
        // Move the player based on input
        // Only move if not dash
        if(!isDashing)
        {
            // calculate the new position
            Vector2 newPosition = rb.position + movement * GetMoveSpeed() * Time.fixedDeltaTime * cameraSizeFactor;

            // Clamp position to screeen bounds
            newPosition = ClampToScreenBound(newPosition);

            // move to new position
            rb.MovePosition(newPosition);
        }
    }
    
    /// <summary>
    /// Starts a dash using current movement direction or the last valid movement direction.
    /// </summary>
    private void StartDash()
    {
        Vector2 dashDirection = movement.sqrMagnitude > 0f ? movement.normalized : lastMoveDirection;
        if (dashDirection == Vector2.zero)
        {
            return;
        }

        float currentDashCooldown = GetDashCooldown();
        dashCooldownTimer = currentDashCooldown;
        if (dashCooldownIndicator != null)
        {
            dashCooldownIndicator.StartCooldown(currentDashCooldown);
        }

        StartCoroutine(Dash(dashDirection));
    }

    /// <summary>
    /// Moves the player in dash direction for the configured dash duration.
    /// </summary>
    IEnumerator Dash(Vector2 dashDirection)
    {
        //start Dash
        isDashing = true;

        float elapsedTime = 0f;

        // Keep moving in dash direction for dash Duration
        while (elapsedTime < dashDuration)
        {
            // calculate the new position
            Vector2 newPosition = rb.position + dashDirection * GetDashSpeed() * Time.fixedDeltaTime * cameraSizeFactor;

            // Clamp position to screeen bounds
            newPosition = ClampToScreenBound(newPosition);

            // move to new position
            rb.MovePosition(newPosition);
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // End dashing
        isDashing = false;
    }

    /// <summary>
    /// Clamps the target player position to the current camera view.
    /// </summary>
    Vector2 ClampToScreenBound(Vector2 targetPosition)
    {
        // Get the screen bounds in world space
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            return targetPosition;
        }

        Vector3 minScreenBounds = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 maxScreenBounds = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));

        // Clamp thee position within the screen bounds
        targetPosition.x = Mathf.Clamp(targetPosition.x, minScreenBounds.x, maxScreenBounds.x);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minScreenBounds.y, maxScreenBounds.y);

        return targetPosition;
    }

    /// <summary>
    /// Fires a bullet toward the mouse when the bullet cooldown is ready.
    /// </summary>
    void FireBullet()
    {
        float currentFireCooldown = GetFireCooldown();
        if (Time.time >= lastFireTime + currentFireCooldown)
        {
            if (bulletCooldownIndicator != null)
            {
                bulletCooldownIndicator.StartCooldown(currentFireCooldown);
            }

            if (bulletPrefab == null || mainCamera == null)
            {
                return;
            }

            // Get the mouse position in the world
            Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;  // We only care about 2D positions

            //calculate the direction to the mouse
            Vector2 fireDirection = (mousePosition - transform.position).normalized;

            // Instantiate the bullet at the player's position
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            // Debug.Log($"Bullet Position: {transform.position}");

            // Set the bullet's direction
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            // Debug.Log($"Bullet Position: {bulletScript}");
            if (bulletScript != null)
            {
                bulletScript.Initialize(fireDirection, this);
            }


            // calculate the angle to rotate the bullet to face the direction of movement
            float angle = Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.Euler(0, 0, angle);

            // Destroy the bullet after a few seconds to avoid clutter
            Destroy(bullet, 10f);  // Destroy after 10 seconds

            // Update the last fire time
            lastFireTime = Time.time;
        }
    }

    /// <summary>
    /// Fires a bomb toward the mouse when the bomb cooldown is ready.
    /// </summary>
    void FireBomb()
    {
        float currentBombCooldown = GetBombCooldown();
        if(Time.time >= lastBombTime + currentBombCooldown)
        {
            if (bombCooldownIndicator != null)
            {
                bombCooldownIndicator.StartCooldown(currentBombCooldown);
            }

            if (bombPrefab == null || mainCamera == null)
            {
                return;
            }

            // Get the mouse poisition in the world
            Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;  // We only care about 2D positions

            //calculate the direction to the mouse
            Vector2 fireDirection = (mousePosition - transform.position).normalized;

            // Instantiate the bomb at the player's position
            GameObject bomb = Instantiate(bombPrefab, transform.position, Quaternion.identity);

            // Set the bomb's direction
            Bomb bombScript = bomb.GetComponent<Bomb>();
            if (bombScript != null)
            {
                bombScript.Initialize(fireDirection, this);
            }

            // Update the last bomb time
            lastBombTime = Time.time;
        }
        
    }

    /// <summary>
    /// Adds experience and triggers level-ups while the experience threshold is met.
    /// </summary>
    public void CollectExperience(int amount)
    {
        // increase player level
        currentExperience += Mathf.Max(0, amount);

        if (experienceThreshold <= 0)
        {
            Debug.LogWarning("Experience threshold must be greater than 0. Resetting to 1.");
            experienceThreshold = 1;
        }

        // check if current experience meets the threshold
        while(currentExperience >= experienceThreshold)
        {
            // Reset experience
            currentExperience -= experienceThreshold;
            LevelUp();
        }

        UpdateLevelText();
    }

    /// <summary>
    /// Prints current player damage, health, and incoming enemy damage for status-change debugging.
    /// </summary>
    public void DebugLogPlayerStatus(string reason, float enemyDamage)
    {
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        float currentHealth = playerHealth != null ? playerHealth.currentHealth : 0f;

        Debug.Log(
            $"[PlayerStatus] {reason} | PlayerDMG: {GetCurrentPlayerDamage():0.##} | PlayerHP: {currentHealth:0.##} | EnemyDMG: {enemyDamage:0.##}"
        );
    }

    /// <summary>
    /// Calculates the debug damage display using the current bullet prefab and player damage multiplier.
    /// </summary>
    private float GetCurrentPlayerDamage()
    {
        if (bulletPrefab != null)
        {
            Bullet bullet = bulletPrefab.GetComponent<Bullet>();
            if (bullet != null)
            {
                return bullet.baseDamage * GetDamageMultiplier();
            }
        }

        return GetDamageMultiplier();
    }

    /// <summary>
    /// Increases player level and applies either a roguelite reward or the legacy damage bonus fallback.
    /// </summary>
    private void LevelUp()
    {
        level++;
        bool rewardApplied = UpgradeManager.Instance != null && UpgradeManager.Instance.TryOfferLevelUpRewards(this);
        if (!rewardApplied)
        {
            damageMultiplier += 0.1f;
        }

        DebugLogPlayerStatus(rewardApplied ? "Level Up Reward" : "Level Up", 0f);
    }

    /// <summary>
    /// Returns final outgoing damage multiplier after PlayerStats modifiers are applied.
    /// </summary>
    public float GetDamageMultiplier()
    {
        return Stats != null ? Stats.GetDamageMultiplier(damageMultiplier) : damageMultiplier;
    }

    /// <summary>
    /// Returns final pickup attraction radius after PlayerStats modifiers are applied.
    /// </summary>
    public float GetPickupRadius(float basePickupRadius)
    {
        return Stats != null ? Stats.GetPickupRadius(basePickupRadius) : basePickupRadius;
    }

    /// <summary>
    /// Returns final movement speed after PlayerStats modifiers are applied.
    /// </summary>
    private float GetMoveSpeed()
    {
        return Stats != null ? Stats.GetMoveSpeed(moveSpeed) : moveSpeed;
    }

    /// <summary>
    /// Returns final dash speed after PlayerStats modifiers are applied.
    /// </summary>
    private float GetDashSpeed()
    {
        return Stats != null ? Stats.GetDashSpeed(dashSpeed) : dashSpeed;
    }

    /// <summary>
    /// Returns final bullet cooldown after PlayerStats modifiers are applied.
    /// </summary>
    private float GetFireCooldown()
    {
        return Stats != null ? Stats.GetFireCooldown(fireCooldown) : fireCooldown;
    }

    /// <summary>
    /// Returns final bomb cooldown after PlayerStats modifiers are applied.
    /// </summary>
    private float GetBombCooldown()
    {
        return Stats != null ? Stats.GetBombCooldown(bombCooldown) : bombCooldown;
    }

    /// <summary>
    /// Returns final dash cooldown after PlayerStats modifiers are applied.
    /// </summary>
    private float GetDashCooldown()
    {
        return Stats != null ? Stats.GetDashCooldown(dashCooldown) : dashCooldown;
    }

    /// <summary>
    /// Refreshes the level and experience UI text if it exists.
    /// </summary>
    private void UpdateLevelText()
    {
        if(levelText != null)
        {
            levelText.text = $"Level: {level} ({currentExperience}/{experienceThreshold})";
        }
    }

    /// <summary>
    /// Returns movement scale based on current camera size relative to the base camera size.
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
    /// Finds a component on a scene object by object name.
    /// </summary>
    private static T FindComponentByName<T>(string objectName) where T : Component
    {
        GameObject target = GameObject.Find(objectName);
        return target != null ? target.GetComponent<T>() : null;
    }
}
