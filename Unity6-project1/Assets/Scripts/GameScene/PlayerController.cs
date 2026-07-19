using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;  // Speed of the player
    public float dashSpeed = 20f; // Speed during dash
    public float dashDuration = 0.2f; // How long the dash lasts
    public float dashCooldown = 5f; // Dash cooldown time
    public GameObject bulletPrefab; // link with the bullet prefab in the Inspector
    public GameObject bombPrefab; // link with the bomb prefab in the Inspector
    public float fireCooldown = 0.2f; // Bullet fire cooldown time
    public float bombCooldown = 10f; // Bomb cooldown time
    public int level = 1; // player level
    public float damageMultiplier = 1f; // damage multiplier
    public TextMeshProUGUI levelText; // Link to a UI text element to display level
    public int experienceThreshold = 5;    // How many exp to level up

    public CooldownIndicator bulletCooldownIndicator; // Bullet cooldown indicator
    public CooldownIndicator dashCooldownIndicator; // Dash cooldown indicator
    public CooldownIndicator bombCooldownIndicator; // Bomb cooldown indicator
    public float gameTime = 0f;    // gameTimer, will stop when pause

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool isDashing = false;
    private float dashCooldownTimer = 0f;
    private float lastFireTime = 0f; // Timestamp of the last shot
    private float lastBombTime = -100f; // Timestamp of the last bomb
    private int currentExperience = 0;  // current experience collected
    private float cameraSizeFactor; // Since camera size changed due to resolution change, the speed related should also changed accordingly
    private Camera mainCamera;
    private CameraScaler cameraScaler;

    void Start()
    {
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

    void Update()
    {
        // get the camera's orthographic size
        cameraSizeFactor = GetCameraSizeFactor();
        gameTime += Time.deltaTime;
        // Get input from the player (arrow keys or WASD)
        movement.x = Input.GetAxisRaw("Horizontal");  // Left/Right movement
        movement.y = Input.GetAxisRaw("Vertical");    // Up/Down movement

        // Detect dash input
        if(Input.GetKeyDown(KeyCode.Space) && !isDashing && dashCooldownTimer <= 0f)
        {
            StartCoroutine(Dash());
            if (dashCooldownIndicator != null)
            {
                dashCooldownIndicator.StartCooldown(dashCooldown);
            }
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

    void FixedUpdate()
    {
        // Move the player based on input
        // Only move if not dash
        if(!isDashing)
        {
            // calculate the new position
            Vector2 newPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime * cameraSizeFactor;

            // Clamp position to screeen bounds
            newPosition = ClampToScreenBound(newPosition);

            // move to new position
            rb.MovePosition(newPosition);
        }
    }
    
    IEnumerator Dash()
    {
        //start Dash
        isDashing = true;
        Vector2 dashDirection = movement.normalized; // Direction of the dash based on current movement
        if (dashDirection == Vector2.zero)
        {
            isDashing = false;
            dashCooldownTimer = dashCooldown;
            yield break;
        }

        float startTime = Time.time;

        // Keep moving in dash direction for dash Duration
        while (Time.time < startTime + dashDuration)
        {
            // calculate the new position
            Vector2 newPosition = rb.position + dashDirection * dashSpeed * Time.deltaTime * cameraSizeFactor;

            // Clamp position to screeen bounds
            newPosition = ClampToScreenBound(newPosition);

            // move to new position
            rb.MovePosition(newPosition);
            yield return null; // wait for the nextg frame
        }

        // End dashing and start cooldown
        isDashing = false;
        dashCooldownTimer = dashCooldown;
    }

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

    void FireBullet()
    {
        if (Time.time >= lastFireTime + fireCooldown)
        {
            if (bulletCooldownIndicator != null)
            {
                bulletCooldownIndicator.StartCooldown(fireCooldown);
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

    void FireBomb()
    {
        if(Time.time >= lastBombTime + bombCooldown)
        {
            if (bombCooldownIndicator != null)
            {
                bombCooldownIndicator.StartCooldown(bombCooldown);
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

    public void CollectExperience(int amount)
    {
        // increase player level
        currentExperience  += amount;

        // check if current experience meets the threshold
        if(currentExperience >= experienceThreshold)
        {
            // level up
            level++;
            damageMultiplier += 0.1f;   // increase damage multiplier

            // Reset experience
            currentExperience -= experienceThreshold;

        }

        UpdateLevelText();
    }

    private void UpdateLevelText()
    {
        if(levelText != null)
        {
            levelText.text = $"Level: {level} ({currentExperience}/{experienceThreshold})";
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

    private static T FindComponentByName<T>(string objectName) where T : Component
    {
        GameObject target = GameObject.Find(objectName);
        return target != null ? target.GetComponent<T>() : null;
    }
}
