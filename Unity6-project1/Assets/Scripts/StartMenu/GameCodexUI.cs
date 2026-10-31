using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Start-menu library only. Browsing does not change selected character, upgrades or game time.
public class GameCodexUI : MonoBehaviour
{
    private GameCodexCatalog catalog;
    private GameObject canvasRoot;
    private RectTransform navigation, details;
    private ScrollRect detailScroll;
    private TMP_InputField search;
    private TextMeshProUGUI heading;
    private Button entryButton;
    private TMP_FontAsset fontAsset;
    private int tab;
    private int characterIndex;
    private WeaponKind selectedWeapon;
    private string selectedGroup = "Attack";
    private UpgradeRarity? selectedRarity;
    private int lastWidth, lastHeight;

    private void Start() => InstallMenuButton();

    public void InstallMenuButton()
    {
        if (entryButton != null) return;
        GameObject settings = GameObject.Find("Setting");
        GameObject quit = GameObject.Find("Quit Game");
        Button template = settings != null ? settings.GetComponent<Button>() : null;
        if (template == null) { Debug.LogError("Codex needs the StartMenu Setting button as a style template."); return; }
        EnsureFont();
        entryButton = Instantiate(template, template.transform.parent);
        entryButton.name = "Codex";
        entryButton.onClick = new Button.ButtonClickedEvent();
        entryButton.onClick.AddListener(Open);
        RectTransform entryRect = entryButton.GetComponent<RectTransform>();
        if (quit != null)
        {
            RectTransform quitRect = quit.GetComponent<RectTransform>();
            entryRect.anchoredPosition = quitRect.anchoredPosition;
            entryButton.transform.SetSiblingIndex(quit.transform.GetSiblingIndex());
            quitRect.anchoredPosition -= new Vector2(0f, 70f);
        }
        else entryRect.anchoredPosition -= new Vector2(0f, 70f);
        TextMeshProUGUI label = entryButton.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
        {
            ApplyFont(label);
            label.text = "Index";
            label.enableAutoSizing = true;
            label.fontSizeMin = 12f;
            label.fontSizeMax = 26f;
        }
    }

    public void Open()
    {
        if (catalog == null) catalog = new GameCodexCatalog();
        if (canvasRoot == null) Build();
        canvasRoot.SetActive(true);
        SetTab(0);
    }

    public void Close()
    {
        if (canvasRoot != null) canvasRoot.SetActive(false);
        if (entryButton != null && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(entryButton.gameObject);
    }

    private void Update()
    {
        if (canvasRoot == null || !canvasRoot.activeSelf) return;
        if (Input.GetKeyDown(KeyCode.Escape)) Close();
        if (lastWidth != Screen.width || lastHeight != Screen.height)
        {
            lastWidth = Screen.width; lastHeight = Screen.height;
            Canvas.ForceUpdateCanvases();
            RenderDetails();
        }
    }

    private void EnsureFont()
    {
        if (fontAsset == null)
            fontAsset = TMP_Settings.defaultFontAsset ?? Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
    }

    private void ApplyFont(TextMeshProUGUI label)
    {
        if (fontAsset == null) return;
        label.font = fontAsset;
        label.fontSharedMaterial = fontAsset.material;
    }

    private void Build()
    {
        EnsureFont();
        canvasRoot = new GameObject("CodexCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasRoot.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        canvasRoot.GetComponent<Canvas>().sortingOrder = 200;
        CanvasScaler scaler = canvasRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = .5f;
        RectTransform background = Panel("CodexBackground", canvasRoot.transform, new Color(.035f,.05f,.075f), Vector2.zero, Vector2.one);
        Text(background, "Index", new Vector2(.03f,.915f), new Vector2(.79f,.985f), 27f);
        ButtonAt(background, "BACK", new Vector2(.83f,.92f), new Vector2(.97f,.98f), Close);
        string[] tabs = { "Characters", "Auxiliary Weapons", "Buffs" };
        for (int i = 0; i < tabs.Length; i++)
        {
            int index = i;
            ButtonAt(background, tabs[i], new Vector2(.03f + i * .17f,.835f), new Vector2(.19f + i * .17f,.9f), () => SetTab(index));
        }
        RectTransform inputRoot = Panel("Search", background, new Color(.1f,.14f,.19f), new Vector2(.57f,.835f), new Vector2(.97f,.9f));
        search = inputRoot.gameObject.AddComponent<TMP_InputField>();
        RectTransform inputViewport = Panel("Viewport", inputRoot, Color.clear, new Vector2(.03f,.05f), new Vector2(.97f,.95f));
        inputViewport.gameObject.AddComponent<RectMask2D>();
        TextMeshProUGUI inputText = Text(inputViewport, "", Vector2.zero, Vector2.one, 19f);
        TextMeshProUGUI placeholder = Text(inputViewport, "Search names and effects...", Vector2.zero, Vector2.one, 18f);
        placeholder.color = new Color(.55f,.64f,.73f);
        search.textViewport = inputViewport;
        search.textComponent = inputText;
        search.placeholder = placeholder;
        search.onValueChanged.AddListener(_ => RenderDetails());
        navigation = Scroll(background, "Navigation", new Vector2(.03f,.07f), new Vector2(.285f,.81f), out _);
        details = Scroll(background, "Details", new Vector2(.305f,.07f), new Vector2(.97f,.75f), out detailScroll);
        heading = Text(background, "", new Vector2(.315f,.758f), new Vector2(.97f,.812f), 21f);
        TextMeshProUGUI footer = Text(background,
            $"{catalog.Characters.Length} characters | {catalog.AuxiliaryWeapons.Length} auxiliary weapons | {catalog.Buffs.Length} buff variants | Esc: back | Scroll to browse",
            new Vector2(.03f,.015f), new Vector2(.97f,.055f), 14f);
        footer.color = new Color(.55f,.65f,.75f);
        if (EventSystem.current == null) new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    private void SetTab(int index)
    {
        tab = index;
        selectedRarity = null;
        search.SetTextWithoutNotify("");
        Clear(navigation);
        if (tab == 0)
        {
            for (int i = 0; i < catalog.Characters.Length; i++)
            {
                int selected = i;
                CharacterDefinition character = catalog.Characters[i];
                Nav(character.displayName + (character.availableAtStart ? "" : "  [Not selectable]"), false, () => { characterIndex = selected; RenderDetails(); });
            }
        }
        else if (tab == 1)
        {
            if (catalog.AuxiliaryWeapons.Length > 0) selectedWeapon = catalog.AuxiliaryWeapons[0].kind;
            foreach (WeaponDefinition weapon in catalog.AuxiliaryWeapons)
            {
                WeaponKind kind = weapon.kind;
                Nav(GameCodexCatalog.WeaponName(kind), false, () => { selectedWeapon = kind; selectedRarity = null; RenderDetails(); });
                foreach (var group in catalog.WeaponBuffs(kind).GroupBy(x => x.rarity))
                {
                    UpgradeRarity rarity = group.Key;
                    Nav("    " + GameCodexCatalog.Rarity(rarity) + "  (" + group.Count() + ")", true,
                        () => { selectedWeapon = kind; selectedRarity = rarity; RenderDetails(); });
                }
            }
        }
        else
        {
            Nav("All Buffs  (" + catalog.Buffs.Length + ")", false, () => { selectedGroup = null; selectedRarity = null; RenderDetails(); });
            foreach (string category in GameCodexCatalog.BuffGroups)
            {
                string type = category;
                Nav(type + "  (" + catalog.GroupBuffs(type).Count() + ")", false, () => { selectedGroup = type; selectedRarity = null; RenderDetails(); });
                foreach (var group in catalog.GroupBuffs(type).GroupBy(x => x.rarity))
                {
                    UpgradeRarity rarity = group.Key;
                    Nav("    " + GameCodexCatalog.Rarity(rarity) + "  (" + group.Count() + ")", true,
                        () => { selectedGroup = type; selectedRarity = rarity; RenderDetails(); });
                }
            }
        }
        Canvas.ForceUpdateCanvases();
        RenderDetails();
    }

    private void RenderDetails()
    {
        if (catalog == null || details == null) return;
        Clear(details);
        string query = search.text.Trim();
        int matches = 0;
        if (tab == 0)
        {
            heading.text = "Characters | Traits / Starting Weapon / Stats";
            for (int i = 0; i < catalog.Characters.Length; i++)
            {
                CharacterDefinition character = catalog.Characters[i];
                string description = GameCodexCatalog.DescribeCharacter(character);
                if (query.Length == 0 ? i != characterIndex : !Matches(character.displayName + description, query)) continue;
                Info(character.displayName, description);
                AddBuffs(catalog.WeaponBuffs(character.startingWeapon != null ? character.startingWeapon.kind : WeaponKind.Pistol), "Starting Weapon Buffs", "");
                matches++;
            }
        }
        else if (tab == 1)
        {
            WeaponDefinition weapon = catalog.AuxiliaryWeapons.FirstOrDefault(x => x.kind == selectedWeapon);
            heading.text = "Auxiliary Weapons > Rarity";
            if (weapon != null)
            {
                Info(GameCodexCatalog.WeaponName(weapon), GameCodexCatalog.DescribeWeapon(weapon));
                matches = AddBuffs(catalog.WeaponBuffs(selectedWeapon), "Weapon Buffs", query);
                if (matches == 0) Info("No matching weapon buffs", "Try another rarity or search term. Base weapon stats are shown above.");
            }
        }
        else
        {
            heading.text = "Buff Type > Rarity" + (selectedGroup != null ? " | " + selectedGroup : " | All");
            foreach (string type in GameCodexCatalog.BuffGroups.Where(x => selectedGroup == null || x == selectedGroup))
                matches += AddBuffs(catalog.GroupBuffs(type), type, query);
        }
        if (matches == 0 && tab != 1) Info("No matching entries", "Clear the search or choose another category.");
        Canvas.ForceUpdateCanvases();
        detailScroll.verticalNormalizedPosition = 1f;
    }

    private int AddBuffs(IEnumerable<UpgradeDefinition> source, string title, string query)
    {
        int count = 0;
        foreach (var rarityGroup in source.Where(x => (!selectedRarity.HasValue || x.rarity == selectedRarity.Value) &&
            Matches(x.DisplayName + GameCodexCatalog.DescribeBuff(x), query)).GroupBy(x => x.rarity).OrderBy(x => x.Key))
        {
            Info(title + " / " + GameCodexCatalog.Rarity(rarityGroup.Key), "Effects per acquisition. All rarity variants share the same family level cap.", UpgradeUIColors.ForRarity(rarityGroup.Key));
            foreach (UpgradeDefinition buff in rarityGroup)
            {
                Info(buff.DisplayName + (buff.weaponSpecific ? " | " + GameCodexCatalog.WeaponName(buff.targetWeapon) : ""),
                    GameCodexCatalog.Rarity(buff.rarity) + " | Max level: " + (buff.maxLevel > 0 ? buff.maxLevel.ToString() : "Unlimited") + "\n" + GameCodexCatalog.DescribeBuff(buff),
                    rarity: buff.rarity);
                count++;
            }
        }
        return count;
    }
    private static bool Matches(string text, string query) => query.Length == 0 || text.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;

    public static Color BuffBackgroundColor(UpgradeRarity rarity) =>
        Color.Lerp(new Color(.045f, .055f, .075f), UpgradeUIColors.ForRarity(rarity), .3f);

    private void Info(string title, string body, Color? accent = null, UpgradeRarity? rarity = null)
    {
        RectTransform panel = Panel("Entry", details, rarity.HasValue ? BuffBackgroundColor(rarity.Value) : new Color(.075f,.105f,.15f), Vector2.zero, Vector2.one);
        if (rarity.HasValue)
        {
            Outline border = panel.gameObject.AddComponent<Outline>();
            border.effectColor = UpgradeUIColors.ForRarity(rarity.Value);
            border.effectDistance = new Vector2(1f, -1f);
        }
        TextMeshProUGUI label = Text(panel, title + "\n" + body, Vector2.zero, Vector2.one, 18f);
        label.rectTransform.offsetMin = new Vector2(14f, 12f);
        label.rectTransform.offsetMax = new Vector2(-14f, -12f);
        label.color = accent ?? new Color(.87f,.91f,.96f);
        if (rarity == UpgradeRarity.Myth) UpgradeUIColors.StyleMythCard(panel.gameObject);
        float width = details.rect.width > 80f ? details.rect.width - 40f : 760f;
        LayoutElement layout = panel.gameObject.AddComponent<LayoutElement>();
        layout.preferredHeight = Mathf.Max(70f, label.GetPreferredValues(label.text, width, Mathf.Infinity).y + 28f);
    }
    private void Nav(string text, bool nested, Action action)
    {
        RectTransform rect = Panel("Category", navigation, nested ? new Color(.07f,.1f,.14f) : new Color(.13f,.2f,.27f), Vector2.zero, Vector2.one);
        rect.gameObject.AddComponent<LayoutElement>().preferredHeight = nested ? 36f : 45f;
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = rect.GetComponent<Image>();
        button.onClick.AddListener(() => action());
        TextMeshProUGUI label = Text(rect, text, new Vector2(.05f,.08f), new Vector2(.95f,.92f), nested ? 15f : 18f);
        label.enableAutoSizing = true; label.fontSizeMin = 11f; label.fontSizeMax = nested ? 15f : 18f;
    }
    private RectTransform Scroll(Transform parent, string name, Vector2 min, Vector2 max, out ScrollRect scroll)
    {
        RectTransform root = Panel(name, parent, new Color(.045f,.065f,.095f), min, max);
        scroll = root.gameObject.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 26f;
        RectTransform viewport = Panel("Viewport", root, Color.clear, Vector2.zero, Vector2.one);
        viewport.gameObject.AddComponent<RectMask2D>();
        RectTransform content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter)).GetComponent<RectTransform>();
        content.SetParent(viewport, false);
        content.anchorMin = new Vector2(0f,1f); content.anchorMax = Vector2.one; content.pivot = new Vector2(.5f,1f);
        content.sizeDelta = Vector2.zero;
        VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(6,6,6,6); layout.spacing = 7f;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
        content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.viewport = viewport; scroll.content = content;
        return content;
    }
    private void ButtonAt(Transform parent, string text, Vector2 min, Vector2 max, Action action)
    {
        RectTransform rect = Panel(text, parent, new Color(.12f,.22f,.29f), min, max);
        Button button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = rect.GetComponent<Image>();
        button.onClick.AddListener(() => action());
        TextMeshProUGUI label = Text(rect, text, new Vector2(.04f,.1f), new Vector2(.96f,.9f), 20f);
        label.alignment = TextAlignmentOptions.Center;
    }
    private static RectTransform Panel(string name, Transform parent, Color color, Vector2 min, Vector2 max)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image)); obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero; obj.GetComponent<Image>().color = color;
        return rect;
    }
    private TextMeshProUGUI Text(Transform parent, string value, Vector2 min, Vector2 max, float size)
    {
        GameObject obj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI)); obj.transform.SetParent(parent, false);
        TextMeshProUGUI label = obj.GetComponent<TextMeshProUGUI>();
        label.rectTransform.anchorMin = min; label.rectTransform.anchorMax = max;
        label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
        ApplyFont(label);
        label.text = value; label.fontSize = size; label.color = Color.white;
        label.richText = false; label.raycastTarget = false;
        label.alignment = TextAlignmentOptions.TopLeft; label.textWrappingMode = TextWrappingModes.Normal;
        return label;
    }
    private static void Clear(Transform content)
    {
        foreach (Transform child in content) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
    }
    private void OnDestroy()
    {
        catalog?.Dispose();
        if (canvasRoot != null) Destroy(canvasRoot);
    }
}
