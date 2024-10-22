using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float bulletSpeed = 25f; // Speed of the bullet
    public float baseDamage = 8f;
    private Vector2 direction;
    private Vector3 spawnPosition;
    private PlayerController playerController;  // Reference to the PlayerController
    private float cameraSizeFactor;


    // This method is called when the bullet is instantiated
    public void Initialize(Vector2 dir, PlayerController controller)
    {
        direction = dir.normalized;
        spawnPosition = transform.position;
        playerController = controller;  // Store the reference
    }

    void Update()
    {
        // get the camera's orthographic size
        cameraSizeFactor = Camera.main.orthographicSize / FindFirstObjectByType<CameraScaler>().baseOrthographicSize;
        // Move the bullet in the specified direction
        transform.position += (Vector3)direction * bulletSpeed * Time.deltaTime * cameraSizeFactor;

        // Optionally, destroy the bullet after a certain distance (e.g., 100 units)
        if (Vector2.Distance(transform.position, spawnPosition) > 100f) // Adjust as needed
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();

            if(enemy != null)
            {
                // increase damage over time
                float currentDamage = baseDamage * playerController.damageMultiplier;

                // Deal damage to enemy hit
                enemy.TakeDamage(currentDamage);
                // Debug.Log($"Deal {currentDamage} damage to enemy!");
            }
            Destroy(gameObject);
        }
    }
}
