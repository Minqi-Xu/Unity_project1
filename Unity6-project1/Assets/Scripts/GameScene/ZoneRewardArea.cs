using UnityEngine;

[DisallowMultipleComponent]
public class ZoneRewardArea : MonoBehaviour
{
    [Min(.1f)] public float requiredStayTime = 10f;
    [Min(0f)] public float exitGraceTime = .5f;
    public ZoneRewardType rewardType;
    public bool oneShot = true;
    public bool spawnEnemiesWhileInside;
    [Min(.5f)] public float radius = 2.5f;
    [Min(1)] public int enemyWaveSize = 8;
    public string displayName = "Reward Area";
    [TextArea] public string discoveryAnnouncement;
    public Color color = Color.cyan;
    public bool IsLocked { get; private set; }
    public bool IsCompleted { get; private set; }
    public bool IsInside { get; private set; }
    public bool IsRevealed { get; private set; }
    public string NavigationName => IsRevealed ? displayName : "Unknown Altar";
    public float Progress => Mathf.Clamp01(stay.Elapsed / Mathf.Max(.1f, requiredStayTime));
    public float Elapsed => stay.Elapsed;
    public ZoneSeal Seal { get; private set; }
    private readonly ZoneStayProgress stay = new ZoneStayProgress();
    private PlayerController player;
    private bool waveSpawned;
    private bool waitingForExit;
    private bool granting;
    private LineRenderer boundary;
    private LineRenderer progressRing;

    public void Configure(ZoneRewardDefinition definition, bool locked, float radiusMultiplier = 1f)
    {
        requiredStayTime = definition.requiredStayTime; exitGraceTime = definition.exitGraceTime;
        rewardType = definition.rewardType; oneShot = definition.oneShot;
        spawnEnemiesWhileInside = definition.spawnEnemiesWhileInside; enemyWaveSize = definition.enemyWaveSize;
        radius = Mathf.Max(.5f, definition.radius * Mathf.Clamp(radiusMultiplier, .1f, 1f));
        displayName = definition.displayName; color = definition.color;
        discoveryAnnouncement = definition.announcement;
        IsRevealed = false;
        IsLocked = locked;
        if (locked)
        {
            GameObject obj = new GameObject("Outer Seal");
            obj.transform.SetParent(transform, false);
            obj.transform.localPosition = Vector3.up * (radius + 1.5f);
            Seal = obj.AddComponent<ZoneSeal>();
            Seal.Initialize(this);
        }
    }

    private void Start()
    {
        boundary = WeaponVisuals.Stroke("Zone boundary", transform, color, .10f, 65);
        progressRing = WeaponVisuals.Stroke("Stay progress", transform, Color.white, .16f, 65);
        SetArc(boundary, 1f, radius);
        // A permanent central sigil distinguishes map events from temporary attack rings.
        LineRenderer sigil = WeaponVisuals.Stroke("Zone sigil", transform, color, .12f, 5);
        sigil.SetPositions(new[] { Vector3.up * .7f, Vector3.right * .5f, Vector3.down * .7f, Vector3.left * .5f, Vector3.up * .7f });
    }

    private static void SetArc(LineRenderer line, float fraction, float size)
    {
        for (int i = 0; i < line.positionCount; i++)
        {
            float angle = i / (float)(line.positionCount - 1) * Mathf.PI * 2f * fraction;
            line.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * size);
        }
    }

    private void Update()
    {
        if (player == null) player = FindAnyObjectByType<PlayerController>();
        if (player == null || Time.timeScale <= 0f || Time.deltaTime <= 0f || IsCompleted) return;
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health == null || health.currentHealth <= 0f) return;
        IsInside = Vector2.Distance(player.transform.position, transform.position) <= radius;
        if (IsLocked) return;
        if (waitingForExit)
        {
            if (!IsInside) { waitingForExit = false; stay.Reset(); waveSpawned = false; }
            return;
        }
        if (IsInside && spawnEnemiesWhileInside && !waveSpawned)
        {
            EnemySpawner spawner = FindAnyObjectByType<EnemySpawner>();
            // Do not allow an unguarded chest when its wave could not be created.
            if (spawner == null || !spawner.SpawnZoneWave(transform.position, radius + 2f, enemyWaveSize)) return;
            waveSpawned = true;
        }
        bool ready = stay.Tick(IsInside, Time.deltaTime, requiredStayTime, exitGraceTime);
        if (progressRing != null) SetArc(progressRing, Progress, radius - .18f);
        if (!ready || granting || RewardManager.Instance == null) return;
        granting = true;
        bool granted = RewardManager.Instance.GrantZoneReward(player, rewardType);
        granting = false;
        if (!granted) return;
        if (ZoneRewardDirector.Instance != null)
            ZoneRewardDirector.Instance.Announce(NavigationName + " - Reward Claimed", IsRevealed ? color : Color.white);
        if (oneShot) { IsCompleted = true; gameObject.SetActive(false); }
        else waitingForExit = true;
    }

    public void Unlock()
    {
        if (!IsLocked) return;
        IsLocked = false;
        if (ZoneRewardDirector.Instance != null)
            ZoneRewardDirector.Instance.Announce(NavigationName + " - Seal Broken", IsRevealed ? color : Color.white);
    }

    public void Reveal() => IsRevealed = true;
}
