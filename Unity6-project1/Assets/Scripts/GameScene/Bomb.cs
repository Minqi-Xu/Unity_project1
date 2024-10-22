using System;
using UnityEditor;
using UnityEngine;

public class Bomb: MonoBehaviour
{
    public float bombSpeed = 10f;   // speed of throwing the bomb
    public float explosionRadius = 3f;  // explosion radius
    public GameObject explossionEffectprefab; // link to explosion effect prefab in the inspector
    public float maxDistance = 10f;     // max distance flying before explode
    public float basedamage = 50f;      // damage dealt when explode

    private Vector2 direction;
    private Vector3 spawnPosition;
    private PlayerController playerController;  // Reference to playerController
    private float cameraSizeFactor;

    public void Initialize(Vector2 dir, PlayerController controller)
    {
        direction = dir.normalized;
        spawnPosition = transform.position;
        playerController = controller;
    }

    void Update()
    {
        // get the camera's orthographic size
        cameraSizeFactor = Camera.main.orthographicSize / FindFirstObjectByType<CameraScaler>().baseOrthographicSize;

        // Move the bomb in the direction set
        transform.position += (Vector3)direction * bombSpeed * Time.deltaTime * cameraSizeFactor;

        // Check if the bomb has traveled too far to auto exploded
        if(Vector2.Distance(spawnPosition, transform.position) >= maxDistance)
        {
            // Auto explode when reach maxDistance
            Explode();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Enemy"))
        {
            // Explode if hits an enemy
            Explode();
        }
    }

    void Explode()
    {
        // Instantiate the explosion effect
        Instantiate(explossionEffectprefab, transform.position, Quaternion.identity);

        // Find all colliders in the explosion radius
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach(var hitCollider in hitColliders)
        {
            // Deal damage to each enemy within the explosion radius
            Enemy enemy = hitCollider.GetComponent<Enemy>();
            if(enemy != null)
            {
                // calculate damage with the damage multiplier
                float currentDamage = basedamage * playerController.damageMultiplier;
                enemy.TakeDamage(currentDamage);
            }
        }
        // Destroy bomb after explode
        Destroy(gameObject);
    }

    void OnDrawGizmoSelected()
    {
        // Show the explosion radius in the scene view
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}