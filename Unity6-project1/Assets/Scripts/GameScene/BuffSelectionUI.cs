using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Runtime modal window for choosing one level-up reward.
public class BuffSelectionUI : MonoBehaviour
{
    // UI references and time state owned by this window.
    private GameObject canvasRoot;
    private RectTransform content;
    private TextMeshProUGUI title;
    private float previousTimeScale;
    private bool visible;
    private bool acceptingSelection;
    private int openedFrame;

    // Builds the window lazily and pauses gameplay until a choice is made.
    public void Show(IReadOnlyList<UpgradeDefinition> choices, int level, Action<UpgradeDefinition> onSelected)
    {
        if (canvasRoot == null)
        {
            BuildWindow();
        }

        if (!visible)
        {
            previousTimeScale = Time.timeScale;
            visible = true;
        }

        Time.timeScale = 0f;
        canvasRoot.SetActive(true);
        title.text = "Level " + (level - 1) + " -> Level " + level;
        foreach (Transform child in content)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        openedFrame = Time.frameCount;
        acceptingSelection = true;
        Button firstButton = null;
        foreach (UpgradeDefinition upgrade in choices)
        {
            Button button = CreateOption(upgrade);
            if (firstButton == null) firstButton = button;
            button.onClick.AddListener(() =>
            {
                // Ignore the input frame that opened the window and repeated submissions.
                if (!acceptingSelection || Time.frameCount <= openedFrame) return;
                acceptingSelection = false;
                onSelected(upgrade);
            });
        }

        if (EventSystem.current != null && firstButton != null)
        {
            EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
        }
    }

    // Closes the window and restores the time scale that was active before it opened.
    public void Hide()
    {
        if (!visible) return;
        visible = false;
        acceptingSelection = false;
        canvasRoot.SetActive(false);
        Time.timeScale = previousTimeScale;
    }

    // Creates a screen-space modal with equal-width options visible simultaneously.
    private void BuildWindow()
    {
        canvasRoot = new GameObject("BuffSelectionCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasRoot.transform.SetParent(transform, false);
        Canvas canvas = canvasRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        CanvasScaler scaler = canvasRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform overlay = Panel("Dimmer", canvasRoot.transform, new Color(0f, 0f, 0f, 0.78f));
        Stretch(overlay, 0f, 0f, 0f, 0f);
        RectTransform window = Panel("SelectionWindow", overlay, new Color(0.09f, 0.1f, 0.12f));
        window.anchorMin = new Vector2(0.04f, 0.06f);
        window.anchorMax = new Vector2(0.96f, 0.94f);
        window.offsetMin = Vector2.zero;
        window.offsetMax = Vector2.zero;
        Outline outline = window.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.38f, 0.43f, 0.48f);
        outline.effectDistance = new Vector2(1f, -1f);

        title = Label("Title", window, "", 26f, Color.white);
        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = Vector2.one;
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.offsetMin = new Vector2(28f, -76f);
        titleRect.offsetMax = new Vector2(-28f, -20f);
        title.fontStyle = FontStyles.Bold;
        title.enableAutoSizing = true;
        title.fontSizeMin = 14f;
        title.fontSizeMax = 26f;

        content = new GameObject("Options", typeof(RectTransform)).GetComponent<RectTransform>();
        content.SetParent(window, false);
        Stretch(content, 24f, 24f, 24f, 86f);
        HorizontalLayoutGroup layout = content.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 16f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        if (EventSystem.current == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }

    // Creates an equal-width card with separate text regions that cannot resize its layout.
    private Button CreateOption(UpgradeDefinition upgrade)
    {
        Color rarityColor = RarityColor(upgrade.rarity);
        RectTransform row = Panel(upgrade.DisplayName, content, new Color(0.15f, 0.17f, 0.2f));
        LayoutElement size = row.gameObject.AddComponent<LayoutElement>();
        size.minWidth = 0f;
        size.preferredWidth = 0f;
        size.flexibleWidth = 1f;
        Button button = row.gameObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1.25f, 1.25f, 1.25f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        TextMeshProUGUI rarity = Label("Rarity", row, upgrade.rarity.ToString().ToUpperInvariant() + " | " + upgrade.category, 14f, rarityColor);
        FitCardText(rarity, 0.85f, 0.95f, 14f);
        TextMeshProUGUI nameLabel = Label("Name", row, upgrade.DisplayName, 23f, Color.white);
        nameLabel.fontStyle = FontStyles.Bold;
        FitCardText(nameLabel, 0.62f, 0.82f, 23f);
        TextMeshProUGUI detail = Label("Description", row, upgrade.description, 17f, new Color(0.83f, 0.86f, 0.89f));
        FitCardText(detail, 0.05f, 0.58f, 17f);
        return button;
    }

    // Wraps and auto-sizes the full text within a stable, proportional region of its card.
    private static void FitCardText(TextMeshProUGUI label, float bottom, float top, float maxSize)
    {
        RectTransform rect = label.rectTransform;
        rect.anchorMin = new Vector2(0.06f, bottom);
        rect.anchorMax = new Vector2(0.94f, top);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        label.alignment = TextAlignmentOptions.TopLeft;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.enableAutoSizing = true;
        label.fontSizeMin = 6f;
        label.fontSizeMax = maxSize;
        label.characterSpacing = 0f;
    }

    // Creates a UI panel without requiring scene or prefab references.
    private static RectTransform Panel(string name, Transform parent, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        obj.GetComponent<Image>().color = color;
        return obj.GetComponent<RectTransform>();
    }

    // Creates text that lets its parent button receive pointer events.
    private static TextMeshProUGUI Label(string name, Transform parent, string text, float size, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        TextMeshProUGUI label = obj.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.color = color;
        label.raycastTarget = false;
        label.richText = false;
        return label;
    }

    // Fits a rectangle inside its parent with explicit edge padding.
    private static void Stretch(RectTransform rect, float left, float right, float bottom, float top)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    // Rarity uses both a written label and color so meaning does not depend on color alone.
    private static Color RarityColor(UpgradeRarity rarity)
    {
        switch (rarity)
        {
            case UpgradeRarity.Rare: return new Color(0.3f, 0.75f, 1f);
            case UpgradeRarity.Epic: return new Color(0.8f, 0.52f, 1f);
            case UpgradeRarity.Legendary: return new Color(1f, 0.76f, 0.25f);
            default: return new Color(0.65f, 0.87f, 0.7f);
        }
    }
}
