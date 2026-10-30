using UnityEngine;

// Proximity pickup intentionally does not depend on the enemy/player collision layers.
public class RewardChest : MonoBehaviour
{
    public EnemyRank enemyRank = EnemyRank.Elite;
    public bool finalBossReward => enemyRank == EnemyRank.FinalBoss;
    public float pickupDistance = 1.25f;
    private bool claimed;
    private PlayerController player;

    private void Update()
    {
        if (claimed || Time.deltaTime <= 0f) return;
        if (player == null) player = FindAnyObjectByType<PlayerController>();
        if (player == null || Vector2.Distance(player.transform.position, transform.position) > pickupDistance) return;
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health == null || health.currentHealth <= 0f || RewardManager.Instance == null) return;
        claimed = true;
        if (!RewardManager.Instance.GrantEnemyChest(player, enemyRank)) { claimed = false; return; }
        if (finalBossReward && GameManager.Instance != null) GameManager.Instance.NotifyFinalRewardClaimed();
        Destroy(gameObject);
    }

    private void OnGUI()
    {
        if (claimed || Camera.main == null) return;
        Vector3 point = Camera.main.WorldToScreenPoint(transform.position);
        if (point.z > 0f) GUI.Box(new Rect(point.x - 70f, Screen.height - point.y - 40f, 140f, 25f), "Rare Treasure");
    }
}
