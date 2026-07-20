using UnityEngine;

/// <summary>
/// Smoothly follows the player once a Player-tagged object exists in the scene.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    /// <summary>Player transform to follow. Found by tag when not assigned.</summary>
    public Transform player; // Assign the player's transform in the inspector

    /// <summary>Offset from the player position.</summary>
    public Vector3 offset;   // Offset from the player position (e.g., (0, 0, -10) for a 2D game)

    /// <summary>Smoothing time used by Vector3.SmoothDamp.</summary>
    public float smoothTime = 0.3f; // Time taken to smooth the camera movement

    // SmoothDamp velocity cache and missing-player log guard.
    private Vector3 velocity = Vector3.zero; // Velocity of the camera
    private bool loggedMissingPlayer;

    /// <summary>
    /// Attempts initial player lookup and places the camera at its base offset.
    /// </summary>
    void Start()
    {
        TryFindPlayer(logWhenMissing: true);
        transform.position = offset;    // Set the initial position to starting position even though player is not found
    }

    /// <summary>
    /// Follows the player after movement updates have completed.
    /// </summary>
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

    /// <summary>
    /// Finds the player by tag and optionally logs once if no player exists yet.
    /// </summary>
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
