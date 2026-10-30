using UnityEngine;

// Enemy-only attacks. Fixed warning locations and swept projectile collision keep attacks dodgeable.
public class EnemyHazard : MonoBehaviour
{
    private Vector2 direction;
    private float speed, damage, radius, activateAt, expiresAt;
    private bool projectile;
    private LineRenderer ring;

    public static void Shot(Vector2 origin, Vector2 direction, float speed, float damage, float warning)
    {
        EnemyHazard hazard = Create(origin, .18f, damage, warning);
        hazard.direction = direction.normalized;
        hazard.speed = speed;
        hazard.projectile = true;
        hazard.expiresAt = hazard.activateAt + 8f;
        WarnDirection(origin, direction, speed * .6f, warning);
    }

    public static void Blast(Vector2 origin, float radius, float damage, float warning) => Create(origin, radius, damage, warning);

    public static void WarnDirection(Vector2 origin, Vector2 direction, float distance, float duration)
    {
        LineRenderer line = WeaponVisuals.Stroke("EnemyAttackDirection", null, Color.yellow, .07f, 2);
        line.transform.position = origin;
        line.SetPosition(0, Vector3.zero);
        line.SetPosition(1, direction.normalized * distance);
        Destroy(line.gameObject, duration);
    }

    private static EnemyHazard Create(Vector2 origin, float radius, float damage, float warning)
    {
        GameObject obj = new GameObject("EnemyAttackWarning");
        obj.transform.position = origin;
        EnemyHazard result = obj.AddComponent<EnemyHazard>();
        result.radius = radius;
        result.damage = damage;
        result.activateAt = Time.time + warning;
        result.ring = WeaponVisuals.Stroke("WarningRing", obj.transform, Color.yellow, .08f, 40);
        result.ring.loop = true;
        for (int i = 0; i < 40; i++)
        {
            float angle = i * Mathf.PI * 2f / 40f;
            result.ring.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
        }
        return result;
    }

    private void Update()
    {
        if (Time.deltaTime <= 0f || Time.time < activateAt) return;
        if (!projectile)
        {
            PlayerHealth victim = null;
            foreach (Collider2D collider in Physics2D.OverlapCircleAll(transform.position, radius))
            {
                victim = collider.GetComponentInParent<PlayerHealth>();
                if (victim != null) break;
            }
            if (victim != null) victim.TakeDamage(damage, victim.GetComponent<PlayerController>());
            WeaponVisuals.Pulse(transform.position, radius, Color.red);
            Destroy(gameObject);
            return;
        }
        ring.startColor = ring.endColor = Color.red;
        float step = speed * Time.deltaTime;
        foreach (RaycastHit2D hit in Physics2D.CircleCastAll(transform.position, radius, direction, step))
        {
            PlayerHealth victim = hit.collider.GetComponentInParent<PlayerHealth>();
            if (victim == null) continue;
            victim.TakeDamage(damage, victim.GetComponent<PlayerController>());
            Destroy(gameObject);
            return;
        }
        transform.position += (Vector3)(direction * step);
        if (Time.time >= expiresAt) Destroy(gameObject);
    }
}
