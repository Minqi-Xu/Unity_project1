using UnityEngine;

// Deliberately NOT an Enemy: enemy-only homing searches can never select a seal.
public class ZoneSeal : MonoBehaviour
{
    [Min(1f)] public float requiredDamage = 150f;
    [Min(.1f)] public float runeStayTime = 1.5f;
    public float DamageTaken { get; private set; }
    public int RunesActivated { get; private set; }
    public bool IsBroken { get; private set; }
    public Vector3 NextRunePosition => area.transform.position +
        Quaternion.Euler(0f, 0f, 120f * RunesActivated) * Vector3.right * (area.radius + 2f);
    private ZoneRewardArea area;
    private PlayerController player;
    private float runeTime;
    private LineRenderer rune;

    public void Initialize(ZoneRewardArea owner)
    {
        area = owner;
        // Bullet prefabs use kinematic bodies; a frozen dynamic body reliably receives their triggers.
        Rigidbody2D body = gameObject.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.constraints = RigidbodyConstraints2D.FreezeAll;
        CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true; collider.radius = .75f;
        LineRenderer crystal = WeaponVisuals.Stroke("Seal crystal", transform, new Color(1f,.3f,.7f), .15f, 5);
        crystal.SetPositions(new[] { Vector3.up, Vector3.right * .65f, Vector3.down, Vector3.left * .65f, Vector3.up });
        rune = WeaponVisuals.Stroke("Seal Rune", area.transform, Color.yellow, .12f, 33);
        MoveRune();
    }

    private void MoveRune()
    {
        rune.transform.position = NextRunePosition;
        for (int i = 0; i < rune.positionCount; i++)
        {
            float angle = i / 32f * Mathf.PI * 2f;
            rune.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * .8f);
        }
    }

    private void Update()
    {
        if (IsBroken || area == null || Time.timeScale <= 0f || Time.deltaTime <= 0f) return;
        if (player == null) player = FindAnyObjectByType<PlayerController>();
        if (player == null) return;
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health == null || health.currentHealth <= 0f) return;
        if (Vector2.Distance(player.transform.position, NextRunePosition) > .9f) { runeTime = 0f; return; }
        runeTime += Time.deltaTime;
        if (runeTime < runeStayTime) return;
        runeTime = 0f;
        RunesActivated++;
        if (RunesActivated >= 3) BreakSeal();
        else MoveRune();
    }

    public void TakeDamage(float amount)
    {
        if (IsBroken || Time.timeScale <= 0f || !ExperienceMath.IsPositiveFinite(amount)) return;
        DamageTaken += amount;
        if (DamageTaken >= requiredDamage) BreakSeal();
    }

    private void BreakSeal()
    {
        if (IsBroken) return;
        IsBroken = true;
        area.Unlock();
        if (rune != null) Destroy(rune.gameObject);
        gameObject.SetActive(false);
    }

    public static void DamageInRadius(Vector2 center, float radius, float damage)
    {
        foreach (ZoneSeal seal in FindObjectsByType<ZoneSeal>())
            if (Vector2.Distance(center, seal.transform.position) <= radius + .75f) seal.TakeDamage(damage);
    }
}
