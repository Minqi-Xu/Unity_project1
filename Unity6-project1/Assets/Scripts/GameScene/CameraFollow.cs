using UnityEngine;

// Smoothly follows the player once a Player-tagged object exists in the scene.
public class CameraFollow : MonoBehaviour
{
    // Player transform to follow. Found by tag when not assigned.
    public Transform player; // Assign the player's transform in the inspector

    // Offset from the player position.
    public Vector3 offset;   // Offset from the player position (e.g., (0, 0, -10) for a 2D game)

    // Smoothing time used by Vector3.SmoothDamp.
    public float smoothTime = 0.3f; // Time taken to smooth the camera movement

    // SmoothDamp velocity cache and missing-player log guard.
    private Vector3 velocity = Vector3.zero; // Velocity of the camera
    private bool loggedMissingPlayer;

    // Attempts initial player lookup and places the camera at its base offset.
    void Start()
    {
        TryFindPlayer(logWhenMissing: true);
        transform.position = offset;    // Set the initial position to starting position even though player is not found
    }

    // Follows the player after movement updates have completed.
    void LateUpdate()
    {
        if (player != null)
        {
            // Define the target position of the camera
            Vector3 targetPosition = player.position + offset;

            // Smoothly move the camera to the target position using SmoothDamp
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
        else
        {
            TryFindPlayer(logWhenMissing: false);
        }
    }

    // Finds the player by tag and optionally logs once if no player exists yet.
    private void TryFindPlayer(bool logWhenMissing)
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if(playerObject != null)
        {
            player = playerObject.transform;
            loggedMissingPlayer = false;
            return;
        }

        if (logWhenMissing && !loggedMissingPlayer)
        {
            Debug.LogWarning("Camera cannot find player yet. It will retry until a Player-tagged object exists.");
            loggedMissingPlayer = true;
        }
    }
}
