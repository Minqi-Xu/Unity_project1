using UnityEngine;

public class ExperienceDrop: MonoBehaviour
{
    public float lifespan = 60f;    // time before it disappears
    public float moveSpeed = 1f;    // Speed of moving towards the player
    private Transform player;
    private PlayerController playerController;
    private float cameraSizeFactor; // Since camera size changed due to resolution change, the speed related should also changed accordingly
    private Camera mainCamera;
    private CameraScaler cameraScaler;

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

        // destory after lifespan
        Destroy(gameObject, lifespan);
    }

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
                Vector2 direction = (player.position - transform.position).normalized;
                transform.position += (Vector3)direction * moveSpeed * Time.deltaTime * cameraSizeFactor;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Call method on player increase their exp here
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
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
