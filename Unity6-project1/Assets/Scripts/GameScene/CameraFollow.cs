using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // Assign the player's transform in the inspector
    public Vector3 offset;   // Offset from the player position (e.g., (0, 0, -10) for a 2D game)
    public float smoothTime = 0.3f; // Time taken to smooth the camera movement
    private Vector3 velocity = Vector3.zero; // Velocity of the camera

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if(playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.Log("Camera cannot find player when start!");
        }
        transform.position = offset;    // Set the initial position to starting position even though player is not found
    }

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
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if(playerObject != null)
            {
                player = playerObject.transform;
            }
            else
            {
                Debug.Log("Camera cannot find player in LateUpdate!");
            }
        }
    }
}