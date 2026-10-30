using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One live card per equipped weapon, plus dash and RMB bomb; reads actual combat timers.
public class AbilityCooldownHUD : MonoBehaviour
{
    [Range(24f, 64f)] public float iconSize = 36f;
    [Range(0f, 12f)] public float iconSpacing = 4f;
    private class Card
    {
        public int kind; // 0: primary, 1: dash, 2: bomb, 3: auxiliary
        public PlayerWeaponSystem weapon;
        public TextMeshProUGUI name, status;
        public Image fill;
    }
    private PlayerController player;
    private WeaponInventory inventory;
    private PlayerWeaponSystem primary;
    private GameObject canvasRoot;
    private RectTransform row;
    private readonly List<Card> cards = new List<Card>();
    private int auxiliaryCount = -1;

    private void Start()
    {
        player = GetComponent<PlayerController>();
        inventory = GetComponent<WeaponInventory>();
        // Replace the three legacy icons together, without changing their scene/prefab bindings.
        GameObject legacy = GameObject.Find("CooldownIndicators");
        if (legacy != null) legacy.SetActive(false);
        canvasRoot = new GameObject("AbilityCooldownHUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        canvasRoot.SetActive(gameObject.activeInHierarchy);
        Canvas canvas = canvasRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 4;
        CanvasScaler scaler = canvasRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = .5f;
        row = new GameObject("Abilities", typeof(RectTransform), typeof(HorizontalLayoutGroup)).GetComponent<RectTransform>();
        row.SetParent(canvasRoot.transform, false);
        row.anchorMin = row.anchorMax = new Vector2(.5f, 1f);
        row.pivot = new Vector2(.5f, 1f);
        row.anchoredPosition = new Vector2(0f, -10f);
        HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = iconSpacing;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;
        layout.childAlignment = TextAnchor.UpperLeft;
        Rebuild();
    }

    private void LateUpdate()
    {
        if (player == null || row == null) return;
        if (inventory == null) inventory = GetComponent<WeaponInventory>();
        if (primary != GetComponent<PlayerWeaponSystem>() || auxiliaryCount != (inventory != null ? inventory.AuxiliaryCount : 0)) Rebuild();
        foreach (Card card in cards)
        {
            float remaining, duration;
            string status = null;
            if (card.kind == 1)
            {
                remaining = player.DashCooldownRemaining;
                duration = player.DashCooldownDuration;
            }
            else if (card.kind == 2)
            {
                remaining = player.BombCooldownRemaining;
                duration = player.BombCooldownDuration;
            }
            else if (card.weapon != null && card.weapon.Definition != null)
            {
                PlayerWeaponSystem weapon = card.weapon;
                card.name.text = ShortName(weapon.Definition.kind);
                remaining = weapon.CooldownRemaining;
                duration = weapon.CooldownDuration;
                if (weapon.UsesSummonAttacks)
                    status = weapon.ActiveSummonCount == 0 ? "..." : weapon.ReadySummonCount > 0
                        ? weapon.ReadySummonCount + "/" + weapon.ActiveSummonCount : null;
                if (weapon.IsDrawing)
                {
                    card.status.text = Mathf.RoundToInt(weapon.DrawProgress * 100f) + "%";
                    SetProgress(card.fill, weapon.DrawProgress);
                    continue;
                }
            }
            else
            {
                remaining = player.PrimaryCooldownRemaining;
                duration = player.PrimaryCooldownDuration;
            }
            card.status.text = status ?? (remaining > .01f ? remaining.ToString("0.0") : "READY");
            SetProgress(card.fill, 1f - Mathf.Clamp01(remaining / Mathf.Max(.02f, duration)));
        }
    }

    private void Rebuild()
    {
        foreach (Transform child in row) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
        cards.Clear();
        primary = GetComponent<PlayerWeaponSystem>();
        AddCard(0, primary, "LMB | Primary", primary != null && primary.Definition != null ? primary.Definition.tint : Color.white);
        AddCard(1, null, "SPACE | Dash", new Color(.3f, .85f, 1f));
        AddCard(2, null, "RMB | Bomb", new Color(1f, .65f, .2f));
        auxiliaryCount = inventory != null ? inventory.AuxiliaryCount : 0;
        if (inventory != null)
            foreach (PlayerWeaponSystem weapon in inventory.AuxiliaryWeapons)
                if (weapon != null && weapon.Definition != null) AddCard(3, weapon, "AUTO | " + weapon.Definition.displayName, weapon.Definition.tint);
        row.sizeDelta = new Vector2(cards.Count * iconSize + Mathf.Max(0, cards.Count - 1) * iconSpacing, iconSize);
    }

    private void OnDestroy()
    {
        if (canvasRoot == null) return;
        if (Application.isPlaying) Destroy(canvasRoot);
        else DestroyImmediate(canvasRoot);
    }

    private void AddCard(int kind, PlayerWeaponSystem weapon, string name, Color color)
    {
        RectTransform panel = Panel(name, row, Color.Lerp(new Color(.055f,.075f,.1f,.95f), color, .25f));
        LayoutElement sizing = panel.gameObject.AddComponent<LayoutElement>();
        sizing.minWidth = sizing.preferredWidth = iconSize;
        sizing.minHeight = sizing.preferredHeight = iconSize;
        sizing.flexibleWidth = sizing.flexibleHeight = 0f;
        RectTransform cover = Panel("CooldownCover", panel, new Color(0f,0f,0f,.6f));
        Place(cover, Vector2.zero, Vector2.one);
        string shortName = kind == 1 ? "DASH" : kind == 2 ? "BOMB" : weapon != null && weapon.Definition != null ? ShortName(weapon.Definition.kind) : "LMB";
        TextMeshProUGUI title = Label(panel, shortName, new Vector2(.02f,.48f), new Vector2(.98f,.95f), 10f);
        TextMeshProUGUI status = Label(panel, "READY", new Vector2(.02f,.04f), new Vector2(.98f,.5f), 11f);
        status.color = Color.Lerp(color, Color.white, .35f);
        Image fill = cover.GetComponent<Image>();
        cards.Add(new Card { kind = kind, weapon = weapon, name = title, status = status, fill = fill });
    }

    private static void SetProgress(Image fill, float amount)
    {
        fill.rectTransform.anchorMax = new Vector2(1f, 1f - Mathf.Clamp01(amount));
    }

    private static string ShortName(WeaponKind kind)
    {
        switch (kind)
        {
            case WeaponKind.Pistol: return "GUN";
            case WeaponKind.Shotgun: return "SG";
            case WeaponKind.Bow: return "BOW";
            case WeaponKind.Melee: return "SPIN";
            case WeaponKind.Bomb: return "BLST";
            case WeaponKind.Turret: return "TUR";
            case WeaponKind.Drone: return "DRN";
            case WeaponKind.Creature: return "PET";
            case WeaponKind.FireOrb: return "FIRE";
            case WeaponKind.Lightning: return "ZAP";
            case WeaponKind.Axe: return "AXE";
            default: return "ATK";
        }
    }

    private static RectTransform Panel(string name, Transform parent, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        obj.GetComponent<Image>().color = color;
        obj.GetComponent<Image>().raycastTarget = false;
        return obj.GetComponent<RectTransform>();
    }
    private static void Place(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min; rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
    private static TextMeshProUGUI Label(Transform parent, string text, Vector2 min, Vector2 max, float size)
    {
        GameObject obj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        TextMeshProUGUI label = obj.GetComponent<TextMeshProUGUI>();
        Place(label.rectTransform, min, max);
        label.text = text;
        label.fontSize = size;
        label.enableAutoSizing = true;
        label.fontSizeMin = 8f;
        label.fontSizeMax = size;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        return label;
    }
}
