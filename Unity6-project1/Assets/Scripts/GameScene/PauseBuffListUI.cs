using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A scrolling acquisition list and independent scrolling details, inside the pause canvas.
public class PauseBuffListUI : MonoBehaviour
{
    private RectTransform panel;
    private RectTransform rows;
    private ScrollRect listScroll;
    private ScrollRect detailScroll;
    private TextMeshProUGUI heading;
    private TextMeshProUGUI empty;
    private TextMeshProUGUI detail;
    private PlayerBuffCollection collection;
    private PlayerBuffCollection.Entry hovered;

    public void Show(Transform parent, PlayerBuffCollection buffs)
    {
        if (panel == null) Build(parent);
        Unsubscribe();
        collection = buffs;
        if (collection != null) collection.Changed += Refresh;
        panel.gameObject.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        Unsubscribe();
        hovered = null;
        if (panel != null) panel.gameObject.SetActive(false);
    }

    private void Unsubscribe()
    {
        if (collection != null) collection.Changed -= Refresh;
        collection = null;
    }

    private void Refresh()
    {
        if (panel == null) return;
        foreach (Transform child in rows)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
        int count = collection != null ? collection.Entries.Count : 0;
        heading.text = "ACQUIRED BUFFS  (" + count + ")";
        empty.gameObject.SetActive(count == 0);
        if (collection != null)
            foreach (PlayerBuffCollection.Entry entry in collection.Entries) AddRow(entry);
        hovered = null;
        detail.text = "Hover a buff name to see its effects and acquired stacks.";
        Canvas.ForceUpdateCanvases();
        listScroll.verticalNormalizedPosition = 1f;
        detailScroll.verticalNormalizedPosition = 1f;
    }

    private void AddRow(PlayerBuffCollection.Entry entry)
    {
        RectTransform row = Panel("Buff - " + entry.Name, rows, new Color(.13f, .18f, .23f), Vector2.zero, Vector2.one);
        LayoutElement size = row.gameObject.AddComponent<LayoutElement>();
        size.minHeight = size.preferredHeight = 56f;
        size.flexibleHeight = 0f;
        Button button = row.gameObject.AddComponent<Button>();
        button.targetGraphic = row.GetComponent<Image>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1.35f, 1.35f, 1.35f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        TextMeshProUGUI name = Label(row, entry.Name, 23f);
        Color rarityColor = entry.DisplayColor;
        name.color = rarityColor;
        RectTransform accent = Panel("RarityAccent", row, rarityColor, Vector2.zero, new Vector2(.009f, 1f));
        accent.GetComponent<Image>().raycastTarget = false;
        Fit(name.rectTransform, new Vector2(.04f, .05f), new Vector2(.76f, .95f));
        name.enableAutoSizing = true;
        name.fontSizeMin = 15f;
        name.fontSizeMax = 23f;
        TextMeshProUGUI level = Label(row, entry.LevelLabel, 18f);
        Fit(level.rectTransform, new Vector2(.78f, .05f), new Vector2(.97f, .95f));
        level.alignment = TextAlignmentOptions.MidlineRight;
        level.enableAutoSizing = true;
        level.fontSizeMin = 12f;
        level.fontSizeMax = 18f;
        if (entry.HighestRarity == UpgradeRarity.Myth) UpgradeUIColors.StyleMythCard(row.gameObject);
        PauseBuffListItem hover = row.gameObject.AddComponent<PauseBuffListItem>();
        hover.Reveal = () =>
        {
            hovered = entry;
            detail.text = entry.Details();
            detail.enableVertexGradient = detail.overrideColorTags = false;
            if (entry.HighestRarity == UpgradeRarity.Myth) UpgradeUIColors.StyleMythText(detail);
            Canvas.ForceUpdateCanvases();
            detailScroll.verticalNormalizedPosition = 1f;
        };
        hover.Clear = () =>
        {
            if (hovered != entry) return;
            // Keep the last inspected description readable while scrolling it.
            hovered = null;
        };
    }

    private void Build(Transform parent)
    {
        panel = Panel("AcquiredBuffs", parent, new Color(.055f, .08f, .11f, .98f),
            // Reserve the top HUD band so SurvivalTimeText remains unobstructed.
            new Vector2(.64f, .08f), new Vector2(.98f, .84f));
        heading = Label(panel, "ACQUIRED BUFFS", 28f);
        Fit(heading.rectTransform, new Vector2(.05f, .91f), new Vector2(.95f, .98f));
        heading.fontStyle = FontStyles.Bold;
        heading.enableAutoSizing = true;
        heading.fontSizeMin = 16f;
        heading.fontSizeMax = 28f;

        listScroll = Scroll(panel, "Names", new Vector2(.04f, .46f), new Vector2(.96f, .89f), out rows);
        VerticalLayoutGroup layout = rows.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 6f;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        empty = Label(listScroll.viewport, "No buffs acquired yet.\nLevel up to obtain your first buff.", 22f);
        Fit(empty.rectTransform, new Vector2(.05f, .45f), new Vector2(.95f, .9f));
        empty.alignment = TextAlignmentOptions.TopLeft;

        TextMeshProUGUI caption = Label(panel, "BUFF DETAILS", 20f);
        Fit(caption.rectTransform, new Vector2(.05f, .39f), new Vector2(.95f, .44f));
        caption.color = new Color(.5f, .8f, .95f);
        detailScroll = Scroll(panel, "Details", new Vector2(.04f, .04f), new Vector2(.96f, .38f), out RectTransform description);
        detail = Label(description, "", 21f);
        detail.alignment = TextAlignmentOptions.TopLeft;
        Fit(detail.rectTransform, new Vector2(.03f, 1f), new Vector2(.97f, 1f));
        detail.rectTransform.pivot = new Vector2(.5f, 1f);
        VerticalLayoutGroup descriptionLayout = description.gameObject.AddComponent<VerticalLayoutGroup>();
        descriptionLayout.padding = new RectOffset(12, 12, 10, 10);
        descriptionLayout.childControlWidth = descriptionLayout.childControlHeight = true;
        descriptionLayout.childForceExpandWidth = true;
        descriptionLayout.childForceExpandHeight = false;
    }

    private static ScrollRect Scroll(Transform parent, string name, Vector2 min, Vector2 max, out RectTransform content)
    {
        RectTransform root = Panel(name, parent, new Color(.075f, .1f, .135f), min, max);
        ScrollRect scroll = root.gameObject.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 32f;
        RectTransform viewport = Panel("Viewport", root, Color.clear, Vector2.zero, new Vector2(.95f, 1f));
        viewport.gameObject.AddComponent<RectMask2D>();
        content = new GameObject("Content", typeof(RectTransform), typeof(ContentSizeFitter)).GetComponent<RectTransform>();
        content.SetParent(viewport, false);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = Vector2.one;
        content.pivot = new Vector2(.5f, 1f);
        content.offsetMin = content.offsetMax = Vector2.zero;
        content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.viewport = viewport;
        scroll.content = content;
        RectTransform track = Panel("Scrollbar", root, new Color(.1f, .14f, .18f), new Vector2(.96f, 0f), Vector2.one);
        RectTransform handle = Panel("Handle", track, new Color(.35f, .48f, .58f), Vector2.zero, Vector2.one);
        Scrollbar scrollbar = track.gameObject.AddComponent<Scrollbar>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        scrollbar.handleRect = handle;
        scrollbar.targetGraphic = handle.GetComponent<Image>();
        scroll.verticalScrollbar = scrollbar;
        scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
        return scroll;
    }

    private static RectTransform Panel(string name, Transform parent, Color color, Vector2 min, Vector2 max)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        obj.GetComponent<Image>().color = color;
        RectTransform rect = obj.GetComponent<RectTransform>();
        Fit(rect, min, max);
        return rect;
    }

    private static TextMeshProUGUI Label(Transform parent, string text, float size)
    {
        GameObject obj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        TextMeshProUGUI label = obj.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.raycastTarget = false;
        label.richText = false;
        return label;
    }

    private static void Fit(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min; rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private void OnDisable() => Hide();
    private void OnDestroy()
    {
        Unsubscribe();
        if (panel != null) Destroy(panel.gameObject);
    }
}
