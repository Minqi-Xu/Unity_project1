using UnityEngine;

// Experience pickup that moves toward the player when close and grants experience on collection.
public class ExperienceDrop: MonoBehaviour
{
    // Seconds before the pickup is destroyed if not collected.
    public float lifespan = 60f;    // time before it disappears

    // Movement speed while attracted toward the player.
    public float moveSpeed = 1f;    // Speed of moving towards the player

    // Cached player references used for attraction and reward delivery.
    private Transform player;
    private PlayerController playerController;
    // Prevent duplicate collection when the player has multiple colliders.
    private bool collected;

    // Camera references used for resolution-aware pickup movement.
    private float cameraSizeFactor; // Since camera size changed due to resolution change, the speed related should also changed accordingly
    private Camera mainCamera;
    private CameraScaler cameraScaler;

    // Finds the player, caches camera references, and schedules pickup despawn.
    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerController = playerObject.GetComponent<PlayerController>();
        }

        mainCamera = Camera.main;
        cameraScaler = mainCamera != null ? mainCamera.GetComponent<CameraScaler>() : null;

        // Destroy after lifespan.
        Destroy(gameObject, lifespan);
    }

    // Moves toward the player when inside the current pickup attraction radius.
    void Update()
    {
        // get the camera's orthographic size
        cameraSizeFactor = GetCameraSizeFactor();

        // Move towards the player if within a certain distance
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            float pickupRadius = playerController != null ? playerController.GetPickupRadius(3f) : 3f;
            if (distance <= pickupRadius * cameraSizeFactor)
            {
                float speedMultiplier = playerController != null && playerController.Stats != null
                    ? Mathf.Max(0f, playerController.Stats.pickupSpeedMultiplier) : 1f;
                transform.position = Vector3.MoveTowards(transform.position, player.position,
                    moveSpeed * speedMultiplier * Time.deltaTime * cameraSizeFactor);
            }
        }
    }

    // Grants experience to the player and destroys the pickup on collection.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!collected && other.CompareTag("Player"))
        {
            // Call method on player increase their exp here
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                collected = true;
                if (RewardManager.Instance != null)
                {
                    RewardManager.Instance.GrantExperience(playerController, 1);
                }
                else
                {
                    playerController.CollectExperience(1);
                }
            }
            Destroy(gameObject);
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
