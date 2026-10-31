using System.Collections.Generic;
using UnityEngine;

// Scene-local schedule, announcements and navigation. No prefab wiring is required.
public class ZoneRewardDirector : MonoBehaviour
{
    public static ZoneRewardDirector Instance { get; private set; }
    public ZoneRewardDefinition[] definitions;
    [Min(0f)] public float firstWaveTime = 180f;
    [Min(0f)] public float sealedWaveTime = 720f;
    [Tooltip("Fixed walking speed used for spawn distance, independent of character buffs and dash.")]
    [Min(.1f)] public float referenceMoveSpeed = 4.5f;
    [Min(1f)] public float minTravelTime = 10f;
    [Min(1f)] public float maxTravelTime = 25f;
    [Min(1f)] public float revealDistance = 10f;
    [Range(.1f, 1f)] public float areaRadiusMultiplier = .6f;
    [Min(1f)] public float announcementDuration = 3.5f;
    [Range(0f, 1f)] public float announcementVerticalPosition = .38f;
    public readonly List<ZoneRewardArea> Areas = new List<ZoneRewardArea>();
    private struct Notice { public string text; public Color color; public ZoneRewardArea discovery; }
    private readonly Queue<Notice> notices = new Queue<Notice>();
    private readonly HashSet<ZoneRewardArea> pendingDiscoveries = new HashSet<ZoneRewardArea>();
    private Notice notice;
    private float noticeRemaining;
    private float elapsed;
    private bool firstSpawned, secondSpawned, ownsDefinitions;
    private PlayerController player;
    private GUIStyle titleStyle, labelStyle;
    private Font font;
    private Texture2D arrowTexture;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        if (definitions == null || definitions.Length == 0)
        {
            definitions = Resources.LoadAll<ZoneRewardDefinition>("Zones");
            if (definitions.Length == 0) { definitions = ZoneRewardDefinition.CreateDefaults(); ownsDefinitions = true; }
        }
    }

    private void Update()
    {
        if (Time.timeScale <= 0f || Time.deltaTime <= 0f) return;
        if (player == null) player = FindAnyObjectByType<PlayerController>();
        if (player == null) return;
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health == null || health.currentHealth <= 0f) return;
        elapsed += Time.deltaTime;
        if (!firstSpawned && elapsed >= firstWaveTime) { firstSpawned = true; SpawnWave(false); }
        if (!secondSpawned && elapsed >= sealedWaveTime) { secondSpawned = true; SpawnWave(true); }
        foreach (ZoneRewardArea area in Areas)
        {
            if (area == null || !area.isActiveAndEnabled || area.IsCompleted || area.IsRevealed || pendingDiscoveries.Contains(area)) continue;
            if (Vector2.Distance(player.transform.position, area.transform.position) > revealDistance) continue;
            pendingDiscoveries.Add(area);
            notices.Enqueue(new Notice { text = "Discovered: " + area.displayName + "\n" + area.discoveryAnnouncement +
                (area.IsLocked ? "\nSealed: activate 3 runes or break the seal." : ""), color = area.color, discovery = area });
        }
        noticeRemaining -= Time.deltaTime;
        while (noticeRemaining <= 0f && notices.Count > 0)
        {
            Notice next = notices.Dequeue();
            if (next.discovery != null)
            {
                ZoneRewardArea area = next.discovery;
                pendingDiscoveries.Remove(area);
                // Reveal only when the concrete discovery notice actually starts displaying nearby.
                if (!area.isActiveAndEnabled || area.IsCompleted || area.IsRevealed ||
                    Vector2.Distance(player.transform.position, area.transform.position) > revealDistance) continue;
                area.Reveal();
            }
            notice = next; noticeRemaining = announcementDuration;
        }
    }

    public void SpawnWave(bool sealedWave)
    {
        if (player == null) player = FindAnyObjectByType<PlayerController>();
        if (player == null || definitions == null) return;
        Camera camera = Camera.main;
        float rotation = Random.Range(0f, 360f);
        bool spawned = false;
        for (int i = 0; i < definitions.Length; i++)
        {
            ZoneRewardDefinition definition = definitions[i];
            if (definition == null) continue;
            float angle = (rotation + i * 360f / definitions.Length) * Mathf.Deg2Rad;
            float travelDistance = GetTargetTravelDistance(camera);
            float zoneRadius = Mathf.Max(.5f, definition.radius * Mathf.Clamp(areaRadiusMultiplier, .1f, 1f));
            // Aim for reaching the boundary at the requested time, rather than the center.
            float zoneDistance = travelDistance + zoneRadius;
            GameObject obj = new GameObject((sealedWave ? "Sealed " : "") + definition.displayName);
            obj.transform.SetParent(transform);
            obj.transform.position = player.transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * zoneDistance;
            ZoneRewardArea area = obj.AddComponent<ZoneRewardArea>();
            area.Configure(definition, sealedWave, areaRadiusMultiplier); Areas.Add(area);
            spawned = true;
        }
        if (spawned) Announce("Altars have appeared in the distance...", Color.white);
    }

    private float GetTargetTravelDistance(Camera camera)
    {
        CameraScaler scaler = camera != null ? camera.GetComponent<CameraScaler>() : null;
        float worldScale = scaler != null && scaler.baseOrthographicSize > 0f
            ? camera.orthographicSize / scaler.baseOrthographicSize : 1f;
        float min = Mathf.Max(1f, Mathf.Min(minTravelTime, maxTravelTime));
        float max = Mathf.Max(min, Mathf.Max(minTravelTime, maxTravelTime));
        return ZoneTravelDistance.Estimate(Random.Range(min, max), referenceMoveSpeed, worldScale);
    }

    public void Announce(string message, Color color) => notices.Enqueue(new Notice { text = message, color = color });

    private void EnsureStyles()
    {
        if (labelStyle != null) return;
        font = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "Noto Sans CJK SC", "Arial Unicode MS", "Arial" }, 24);
        labelStyle = new GUIStyle(GUI.skin.label) { font = font, alignment = TextAnchor.MiddleCenter, wordWrap = true, fontSize = 16 };
        titleStyle = new GUIStyle(labelStyle) { fontSize = 34, fontStyle = FontStyle.Bold };
        arrowTexture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        arrowTexture.name = "Zone direction arrow";
        var pixels = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
            for (int x = 0; x < 32; x++)
                pixels[y * 32 + x] = x >= 4 && x <= 28 && Mathf.Abs(y - 15.5f) <= (28f - x) * .5f ? Color.white : Color.clear;
        arrowTexture.SetPixels(pixels);
        arrowTexture.Apply();
    }

    private void OnGUI()
    {
        Camera camera = Camera.main;
        if (camera == null || player == null || Time.timeScale <= 0f) return;
        EnsureStyles();
        Matrix4x4 previous = GUI.matrix;
        Color previousColor = GUI.color;
        // Reference coordinates keep labels and edge padding readable at different resolutions.
        float width = 1280f, height = 1280f * Screen.height / Screen.width;
        GUI.matrix = Matrix4x4.Scale(Vector3.one * (Screen.width / width));
        float scale = Screen.width / width;
        Rect pixelRect = camera.pixelRect;
        Rect cameraRect = new Rect(pixelRect.x / scale, (Screen.height - pixelRect.yMax) / scale,
            pixelRect.width / scale, pixelRect.height / scale);
        Rect safe = Rect.MinMaxRect(cameraRect.xMin + 28f, cameraRect.yMin + Mathf.Min(70f, cameraRect.height * .15f),
            cameraRect.xMax - 28f, cameraRect.yMax - Mathf.Min(65f, cameraRect.height * .15f));
        Vector3 playerScreen = camera.WorldToScreenPoint(player.transform.position);
        Vector2 playerPoint = new Vector2(playerScreen.x / scale, (Screen.height - playerScreen.y) / scale);
        var used = new List<Rect>();
        var usedArrows = new List<Vector2>();
        foreach (ZoneRewardArea area in Areas)
        {
            if (area == null || !area.isActiveAndEnabled || area.IsCompleted) continue;
            Vector3 viewport = camera.WorldToViewportPoint(area.transform.position);
            Vector3 targetScreen = camera.WorldToScreenPoint(area.transform.position);
            Vector2 targetPoint = new Vector2(targetScreen.x / scale, (Screen.height - targetScreen.y) / scale);
            float distance = Vector2.Distance(player.transform.position, area.transform.position);
            string state = area.IsLocked ? "Sealed" : area.IsInside ? "Charging" : area.Elapsed > 0f ? "Progress Saved" : "Enter to Activate";
            string text = area.NavigationName + (area.IsRevealed && area.IsLocked ? " [Sealed]" : "") + " · " + Mathf.CeilToInt(distance) + "m";
            bool onScreen = viewport.z > 0f && viewport.x > .08f && viewport.x < .92f && viewport.y > .12f && viewport.y < .88f;
            GUI.color = area.IsRevealed ? area.color : Color.white;
            if (onScreen)
            {
                Rect label = new Rect(targetPoint.x - 140f, targetPoint.y - 74f, 280f, 68f);
                GUI.Box(label, GUIContent.none);
                GUI.Label(label, text + "\n" + (area.IsRevealed ? state + "  " + area.Elapsed.ToString("0.0") + " / " + area.requiredStayTime.ToString("0") + "s" : "Move closer to identify"), labelStyle);
                if (area.IsRevealed && area.IsLocked && area.Seal != null)
                {
                    ZoneSeal seal = area.Seal;
                    DrawWorldLabel(camera, seal.transform.position, "Break Seal: " + Mathf.Max(0f, seal.requiredDamage - seal.DamageTaken).ToString("0") + " HP\nOr activate runes: " + seal.RunesActivated + "/3", width, height);
                    DrawWorldLabel(camera, seal.NextRunePosition, "Stay on rune for " + seal.runeStayTime.ToString("0.0") + "s", width, height);
                }
            }
            else
            {
                if (viewport.z < 0f) targetPoint = playerPoint * 2f - targetPoint;
                DrawEdgeIndicator(playerPoint, targetPoint, safe, cameraRect, text, used, usedArrows);
            }
        }
        if (noticeRemaining > 0f)
        {
            float fade = Mathf.Clamp01(noticeRemaining / .6f) * Mathf.Clamp01((announcementDuration - noticeRemaining) / .3f);
            Rect box = new Rect(100f, height * announcementVerticalPosition - 60f, width - 200f, 120f);
            GUI.color = new Color(.04f, .06f, .10f, .55f * fade);
            GUI.DrawTexture(box, Texture2D.whiteTexture);
            GUI.color = new Color(notice.color.r, notice.color.g, notice.color.b, .85f * fade);
            GUI.Label(box, notice.text, titleStyle);
        }
        GUI.color = previousColor; GUI.matrix = previous;
    }

    private void DrawEdgeIndicator(Vector2 origin, Vector2 target, Rect safe, Rect cameraRect, string text,
        List<Rect> usedLabels, List<Vector2> usedArrows)
    {
        ZoneNavigationGeometry.Point hit = ZoneNavigationGeometry.EdgePoint(origin.x, origin.y, target.x, target.y,
            safe.xMin, safe.yMin, safe.xMax, safe.yMax);
        Vector2 edge = new Vector2(hit.x, hit.y);
        bool verticalEdge = Mathf.Abs(edge.x - safe.xMin) < .1f || Mathf.Abs(edge.x - safe.xMax) < .1f;
        Vector2 initial = edge;
        // Spread markers along their own edge; never wrap a bottom marker to the top.
        for (int attempt = 1; attempt <= 20 && usedArrows.Exists(p => Vector2.Distance(p, edge) < 36f); attempt++)
        {
            float offset = ((attempt + 1) / 2) * 40f * (attempt % 2 == 1 ? 1f : -1f);
            edge = verticalEdge ? new Vector2(initial.x, Mathf.Clamp(initial.y + offset, safe.yMin, safe.yMax))
                : new Vector2(Mathf.Clamp(initial.x + offset, safe.xMin, safe.xMax), initial.y);
        }
        usedArrows.Add(edge);
        Vector2 direction = target - edge;
        if (direction.sqrMagnitude < .001f) direction = edge - origin;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Matrix4x4 scaled = GUI.matrix;
        // Compose rotation in reference coordinates before screen scaling; the pivot stays fixed.
        GUI.matrix = scaled * Matrix4x4.Translate(edge) * Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, angle)) * Matrix4x4.Translate(-edge);
        GUI.DrawTexture(new Rect(edge.x - 16f, edge.y - 16f, 32f, 32f), arrowTexture);
        GUI.matrix = scaled;

        Rect label = new Rect(edge.x - 105f, edge.y + 22f, 210f, 42f);
        if (verticalEdge) label = new Rect(edge.x < safe.center.x ? edge.x + 22f : edge.x - 232f, edge.y - 21f, 210f, 42f);
        else if (edge.y > safe.center.y) label.y = edge.y - 64f;
        label.x = Mathf.Clamp(label.x, cameraRect.xMin + 6f, Mathf.Max(cameraRect.xMin + 6f, cameraRect.xMax - label.width - 6f));
        float originalY = label.y;
        for (int attempt = 0; attempt < 12 && usedLabels.Exists(r => r.Overlaps(label)); attempt++)
        {
            float offset = ((attempt / 2) + 1) * 44f * (attempt % 2 == 0 ? 1f : -1f);
            label.y = Mathf.Clamp(originalY + offset, cameraRect.yMin + 6f, Mathf.Max(cameraRect.yMin + 6f, cameraRect.yMax - label.height - 6f));
        }
        usedLabels.Add(label);
        GUI.Box(label, GUIContent.none);
        GUI.Label(label, text, labelStyle);
    }

    private void DrawWorldLabel(Camera camera, Vector3 point, string text, float width, float height)
    {
        Vector3 viewport = camera.WorldToViewportPoint(point);
        if (viewport.z <= 0f || viewport.x < 0f || viewport.x > 1f || viewport.y < 0f || viewport.y > 1f) return;
        Vector3 screen = camera.WorldToScreenPoint(point);
        float scale = Screen.width / width;
        GUI.Label(new Rect(screen.x / scale - 135f, (Screen.height - screen.y) / scale + 15f, 270f, 52f), text, labelStyle);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (ownsDefinitions && definitions != null)
            foreach (var definition in definitions) if (definition != null) Destroy(definition);
        if (font != null) Destroy(font);
        if (arrowTexture != null) Destroy(arrowTexture);
    }
}
