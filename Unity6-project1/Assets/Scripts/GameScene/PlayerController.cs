using System.Collections;
using TMPro;
using UnityEngine;

// Handles player input, movement, dash, weapon firing, experience, level-up rewards, and status debug output.
public class PlayerController : MonoBehaviour
{
    // Base movement speed before camera scaling and stat modifiers.
    public float moveSpeed = 5f;  // Speed of the player

    // Base dash movement speed before stat modifiers.
    public float dashSpeed = 20f; // Speed during dash

    // How long one dash lasts in seconds.
    public float dashDuration = 0.2f; // How long the dash lasts

    [Tooltip("Seconds of invulnerability after the dash ends. The player is also invulnerable throughout the dash.")]
    [Min(0f)] public float postDashInvulnerabilityDuration = 0.1f;

    // Base dash cooldown in seconds before stat modifiers.
    public float dashCooldown = 5f; // Dash cooldown time

    // Bullet prefab spawned when the player fires with left mouse button.
    public GameObject bulletPrefab; // link with the bullet prefab in the Inspector

    // Bomb prefab spawned when the player fires with right mouse button.
    public GameObject bombPrefab; // link with the bomb prefab in the Inspector

    // Base bullet fire cooldown in seconds before stat modifiers.
    public float fireCooldown = 0.2f; // Bullet fire cooldown time

    // Base bomb fire cooldown in seconds before stat modifiers.
    public float bombCooldown = 10f; // Bomb cooldown time

    // Current player level.
    public int level = 1; // player level

    // Legacy outgoing damage multiplier kept for compatibility with existing gameplay.
    public float damageMultiplier = 1f; // damage multiplier

    // Legacy level label; the experience HUD replaces its numeric progress display.
    public TextMeshProUGUI levelText; // Link to a UI text element to display level

    // Experience required to gain one level.
    [Min(.01f)] public float experienceThreshold = 5f;

    // Cooldown indicator for bullet firing.
    public CooldownIndicator bulletCooldownIndicator; // Bullet cooldown indicator

    // Cooldown indicator for dash.
    public CooldownIndicator dashCooldownIndicator; // Dash cooldown indicator

    // Cooldown indicator for bomb firing.
    public CooldownIndicator bombCooldownIndicator; // Bomb cooldown indicator

    // Elapsed gameplay time for this player. Stops advancing when timeScale is paused.
    public float gameTime = 0f;    // gameTimer, will stop when pause

    // Runtime stat component used for roguelite upgrades and derived player values.
    public PlayerStats Stats { get; private set; }

    // Cached movement and combat state.
    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 lastMoveDirection = Vector2.right;
    private bool isDashing = false;
    private float dashInvulnerableUntil;
    public bool IsDashInvulnerable => isDashing || Time.time < dashInvulnerableUntil;
    private float dashCooldownTimer = 0f;

    // Cooldown timestamps and experience bookkeeping.
    private float lastFireTime = 0f; // Timestamp of the last shot
    private float lastBombTime = -100f; // Timestamp of the last bomb
    private float lastDashCooldownDuration;
    public float DashCooldownRemaining => Mathf.Max(0f, dashCooldownTimer);
    public float DashCooldownDuration => lastDashCooldownDuration > 0f ? lastDashCooldownDuration : GetDashCooldown();
    public float BombCooldownRemaining => Mathf.Max(0f, lastBombTime + GetBombCooldown() - Time.time);
    public float BombCooldownDuration => GetBombCooldown();
    public float PrimaryCooldownRemaining => Mathf.Max(0f, lastFireTime + GetPrimaryWeaponCooldown() - Time.time);
    public float PrimaryCooldownDuration => GetPrimaryWeaponCooldown();
    private float currentExperience = 0f;
    public float CurrentExperience => currentExperience;
    public float ExperienceProgress => ExperienceMath.Progress(currentExperience, experienceThreshold);

    // Camera references used for screen bounds and resolution-aware movement scaling.
    private float cameraSizeFactor; // Since camera size changed due to resolution change, the speed related should also changed accordingly
    private Camera mainCamera;
    private CameraScaler cameraScaler;

    // Initializes player components, camera references, cooldown UI, and level UI.
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
        if (GetComponent<AbilityCooldownHUD>() == null) gameObject.AddComponent<AbilityCooldownHUD>();
        if (GetComponent<ExperienceHUD>() == null) gameObject.AddComponent<ExperienceHUD>();
        UpdateLevelText();
    }

    // Reads input, advances timers, and triggers dash or weapon actions.
    void Update()
    {
        if (Time.timeScale <= 0f)
        {
            return;
        }

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

    // Applies physics movement when the player is not currently dashing.
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
    
    // Starts a dash using current movement direction or the last valid movement direction.
    private void StartDash()
    {
        Vector2 dashDirection = movement.sqrMagnitude > 0f ? movement.normalized : lastMoveDirection;
        if (dashDirection == Vector2.zero)
        {
            return;
        }

        float currentDashCooldown = GetDashCooldown();
        lastDashCooldownDuration = currentDashCooldown;
        dashCooldownTimer = currentDashCooldown;
        if (dashCooldownIndicator != null)
        {
            dashCooldownIndicator.StartCooldown(currentDashCooldown);
        }

        StartCoroutine(Dash(dashDirection));
    }

    // Moves the player in dash direction for the configured dash duration.
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
        dashInvulnerableUntil = Time.time + Mathf.Max(0f, postDashInvulnerabilityDuration);
        isDashing = false;
        Stats?.OnDashCompleted();
    }

    // Clamps the target player position to the current camera view.
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

    // Fires a bullet toward the mouse when the bullet cooldown is ready.
    void FireBullet()
    {
        PlayerWeaponSystem weapon = GetComponent<PlayerWeaponSystem>();
        if (weapon != null && weapon.Definition != null)
        {
            if (weapon.Definition.Automatic) return;
            if (weapon.Definition.kind == WeaponKind.Axe && !Input.GetMouseButtonDown(0)) return;
            Vector2 aim = mainCamera != null
                ? (Vector2)(mainCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position)
                : lastMoveDirection;
            weapon.TryAttack(aim);
            return;
        }
        float currentFireCooldown = GetPrimaryWeaponCooldown();
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

            int count = Stats != null ? Stats.GetProjectileCount() : 1;
            float step = count > 1 ? Mathf.Min(8f, 60f / (count - 1)) : 0f;
            for (int i = 0; i < count; i++)
            {
                Vector2 shotDirection = Quaternion.Euler(0f, 0f, (i - (count - 1) * .5f) * step) * fireDirection;
                GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                Bullet bulletScript = bullet.GetComponent<Bullet>();
                if (bulletScript != null) bulletScript.Initialize(shotDirection, this);
                float angle = Mathf.Atan2(shotDirection.y, shotDirection.x) * Mathf.Rad2Deg;
                bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                Destroy(bullet, 10f);
            }

            // Update the last fire time
            lastFireTime = Time.time;
        }
    }

    // Fires a bomb toward the mouse when the bomb cooldown is ready.
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

    // Adds experience and triggers level-ups while the experience threshold is met.
    public void CollectExperience(float amount)
    {
        if (!ExperienceMath.IsPositiveFinite(amount)) return;

        if (!ExperienceMath.IsPositiveFinite(experienceThreshold))
        {
            Debug.LogWarning("Experience threshold must be greater than 0. Resetting to 1.");
            experienceThreshold = 1f;
        }
        float total = currentExperience + amount;
        if (float.IsInfinity(total) || total - experienceThreshold == total) return;
        currentExperience = total;

        // check if current experience meets the threshold
        while (ExperienceMath.CanLevelUp(currentExperience, experienceThreshold))
        {
            // Reset experience
            currentExperience = Mathf.Max(0f, currentExperience - experienceThreshold);
            LevelUp();
        }

        UpdateLevelText();
    }

    // Prints current player damage, health, and incoming enemy damage for status-change debugging.
    public void DebugLogPlayerStatus(string reason, float enemyDamage)
    {
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        float currentHealth = playerHealth != null ? playerHealth.currentHealth : 0f;

        Debug.Log(
            $"[PlayerStatus] {reason} | PlayerDMG: {GetCurrentPlayerDamage():0.##} | PlayerHP: {currentHealth:0.##} | EnemyDMG: {enemyDamage:0.##}"
        );
    }

    // Calculates the debug damage display using the current bullet prefab and player damage multiplier.
    private float GetCurrentPlayerDamage()
    {
        PlayerWeaponSystem weapon = GetComponent<PlayerWeaponSystem>();
        if (weapon != null && weapon.Definition != null)
        {
            float primary = Stats != null ? Stats.primaryWeaponDamageMultiplier : 1f;
            return weapon.Definition.damage * GetDamageMultiplier() * primary *
                (weapon.Definition.kind == WeaponKind.Bomb ? 1f : weapon.CharacterDamageMultiplier);
        }
        if (bulletPrefab != null)
        {
            Bullet bullet = bulletPrefab.GetComponent<Bullet>();
            if (bullet != null)
            {
                return bullet.baseDamage * GetDamageMultiplier() * (Stats != null ? Stats.primaryWeaponDamageMultiplier : 1f);
            }
        }

        return GetDamageMultiplier();
    }

    // Increases player level and applies either a roguelite reward or the legacy damage bonus fallback.
    private void LevelUp()
    {
        level++;
        Stats?.OnLevelUp(GetComponent<PlayerHealth>());
        bool rewardApplied = UpgradeManager.Instance != null && UpgradeManager.Instance.TryOfferLevelUpRewards(this);
        if (!rewardApplied)
        {
            damageMultiplier += 0.1f * (Stats != null ? Stats.levelDamageGrowthMultiplier : 1f);
        }

        DebugLogPlayerStatus(rewardApplied ? "Level Up Reward" : "Level Up", 0f);
    }

    // Returns final outgoing damage multiplier after PlayerStats modifiers are applied.
    public float GetDamageMultiplier()
    {
        return Stats != null ? Stats.GetDamageMultiplier(damageMultiplier) : damageMultiplier;
    }

    // Returns final pickup attraction radius after PlayerStats modifiers are applied.
    public float GetPickupRadius(float basePickupRadius)
    {
        return Stats != null ? Stats.GetPickupRadius(basePickupRadius) : basePickupRadius;
    }

    // Returns final movement speed after PlayerStats modifiers are applied.
    private float GetMoveSpeed()
    {
        return Stats != null ? Stats.GetMoveSpeed(moveSpeed) : moveSpeed;
    }

    // Returns final dash speed after PlayerStats modifiers are applied.
    private float GetDashSpeed()
    {
        return Stats != null ? Stats.GetDashSpeed(dashSpeed) : dashSpeed;
    }

    // Returns final bullet cooldown after PlayerStats modifiers are applied.
    private float GetPrimaryWeaponCooldown()
    {
        return Stats != null ? Stats.GetPrimaryWeaponCooldown(fireCooldown) : fireCooldown;
    }

    // Returns final bomb cooldown after PlayerStats modifiers are applied.
    private float GetBombCooldown()
    {
        return Stats != null ? Stats.GetBombCooldown(bombCooldown) : bombCooldown;
    }

    // Returns final dash cooldown after PlayerStats modifiers are applied.
    private float GetDashCooldown()
    {
        return Stats != null ? Stats.GetDashCooldown(dashCooldown) : dashCooldown;
    }

    // Refreshes the level and experience UI text if it exists.
    private void UpdateLevelText()
    {
        if(levelText != null)
        {
            levelText.text = $"Level: {level}";
        }
    }

    // Returns movement scale based on current camera size relative to the base camera size.
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

    // Finds a component on a scene object by object name.
    private static T FindComponentByName<T>(string objectName) where T : Component
    {
        GameObject target = GameObject.Find(objectName);
        return target != null ? target.GetComponent<T>() : null;
    }
}
