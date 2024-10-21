using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // Assign the player's transform in the inspector
    public Vector3 offset;   // Offset from the player position (e.g., (0, 0, -10) for a 2D game)
    public float smoothTime = 0.3f; // Time taken to smooth the camera movement
    private Vector3 velocity = Vector3.zero; // Velocity of the camera

    void LateUpdate()
    {
        if (player != null)
        {
            // Define the target position of the camera
            Vector3 targetPosition = player.position + offset;

            // Smoothly move the camera to the target position using SmoothDamp
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }
}