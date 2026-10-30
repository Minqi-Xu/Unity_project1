using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Creates a responsive roster and a detail card without duplicating prefab logic.
public class CharacterSelectionUI : MonoBehaviour
{
    private TextMeshProUGUI details;
    private TextMeshProUGUI heading;
    private Image portrait;
    private Button[] buttons;

    public void Build(CharacterDefinition[] roster, Action<int> select, Action confirm)
    {
        GameObject root = new GameObject("CharacterRoster", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.transform.SetParent(transform, false);
        root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        root.GetComponent<Canvas>().sortingOrder = 100;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;
        RectTransform background = Panel(root.transform, "Background", new Color(0.04f, 0.06f, 0.09f), Vector2.zero, Vector2.one);
        Text(background, "CHOOSE YOUR SURVIVOR", new Vector2(0.04f, 0.88f), new Vector2(0.96f, 0.97f), 32f);
        RectTransform list = Panel(background, "Roster", new Color(0.07f, 0.1f, 0.14f), new Vector2(0.04f, 0.16f), new Vector2(0.49f, 0.85f));
        buttons = new Button[roster.Length];
        int rows = Mathf.CeilToInt(roster.Length / 2f);
        for (int i = 0; i < roster.Length; i++)
        {
            int index = i;
            float x = i % 2 * 0.5f;
            float top = 1f - i / 2 / (float)rows;
            buttons[i] = Button(list, roster[i].displayName + "\n<size=65%>" + roster[i].startingWeapon.displayName + "</size>",
                new Vector2(x + 0.015f, top - 1f / rows + 0.015f), new Vector2(x + 0.485f, top - 0.015f), () => select(index));
        }
        RectTransform card = Panel(background, "Details", new Color(0.09f, 0.13f, 0.18f), new Vector2(0.52f, 0.16f), new Vector2(0.96f, 0.85f));
        heading = Text(card, "", new Vector2(0.06f, 0.8f), new Vector2(0.73f, 0.96f), 30f);
        RectTransform image = Panel(card, "Portrait", Color.white, new Vector2(0.77f, 0.79f), new Vector2(0.95f, 0.96f));
        portrait = image.GetComponent<Image>();
        portrait.preserveAspect = true;
        details = Text(card, "", new Vector2(0.06f, 0.04f), new Vector2(0.94f, 0.77f), 21f);
        details.alignment = TextAlignmentOptions.TopLeft;
        Button(background, "BACK", new Vector2(0.04f, 0.04f), new Vector2(0.22f, 0.12f), () => SceneManager.LoadScene("StartMenu"));
        Button(background, "START RUN", new Vector2(0.68f, 0.04f), new Vector2(0.96f, 0.12f), confirm);
        if (EventSystem.current == null) new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    public void Show(CharacterDefinition character, int index)
    {
        heading.text = character.displayName;
        WeaponDefinition weapon = character.startingWeapon;
        string controls = weapon.Automatic ? "Primary: automatic" : "LMB: attack / deploy";
        details.text = $"HP  {character.maxHealth:0}     SPEED  {character.moveSpeed:0.0}\n" +
            $"DIFFICULTY  {character.difficulty}\n\nWEAPON  {weapon.displayName}\n{weapon.description}\n\n" +
            $"PASSIVE\n{character.passive}\n\nLIMITATION\n{character.limitation}\n\n" +
            $"{controls}\nRMB: bomb   |   Space: dash";
        SpriteRenderer source = character.playerPrefab != null ? character.playerPrefab.GetComponent<SpriteRenderer>() : null;
        portrait.sprite = source != null ? source.sprite : null;
        portrait.color = character.tint;
        for (int i = 0; i < buttons.Length; i++)
            buttons[i].GetComponent<Image>().color = i == index ? new Color(0.2f, 0.4f, 0.5f) : new Color(0.12f, 0.18f, 0.24f);
    }

    private static RectTransform Panel(Transform parent, string name, Color color, Vector2 min, Vector2 max)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = min; rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        obj.GetComponent<Image>().color = color;
        return rect;
    }

    private static TextMeshProUGUI Text(Transform parent, string value, Vector2 min, Vector2 max, float size)
    {
        GameObject obj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        TextMeshProUGUI label = obj.GetComponent<TextMeshProUGUI>();
        label.rectTransform.anchorMin = min; label.rectTransform.anchorMax = max;
        label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
        label.text = value; label.fontSize = size; label.color = Color.white;
        label.enableAutoSizing = true; label.fontSizeMin = 12f; label.fontSizeMax = size;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        return label;
    }

    private static Button Button(Transform parent, string label, Vector2 min, Vector2 max, Action action)
    {
        RectTransform rect = Panel(parent, label, new Color(0.12f, 0.18f, 0.24f), min, max);
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = rect.GetComponent<Image>();
        button.onClick.AddListener(() => action());
        Text(rect, label, new Vector2(0.03f, 0.05f), new Vector2(0.97f, 0.95f), 22f);
        return button;
    }
}
