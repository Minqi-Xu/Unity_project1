using UnityEngine;

public class ExperienceDrop: MonoBehaviour
{
    public float lifespan = 60f;    // time before it disappears
    public float moveSpeed = 1f;    // Speed of moving towards the player
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // destory after lifespan
        Destroy(gameObject, lifespan);
    }

    void Update()
    {
        // Move towards the player if within a certain distance
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= 3f)
            {
                Vector2 direction = (player.position - transform.position).normalized;
                transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
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
                playerController.CollectExperience(1);
            }
            Destroy(gameObject);
        }
    }

}