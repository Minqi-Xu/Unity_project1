using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Reuses the old top-left level label's position, preserving the health/timer layout.
[DisallowMultipleComponent]
public class ExperienceHUD : MonoBehaviour
{
    private PlayerController player;
    private TextMeshProUGUI legacyLabel;
    private bool legacyWasEnabled;
    private GameObject fallbackCanvas;
    private RectTransform root;
    private RectTransform fill;
    private TextMeshProUGUI levelLabel;
    private TextMeshProUGUI percentage;
    private int shownLevel = -1;
    private float shownExperience = -1f;
    private float shownThreshold = -1f;

    private void Start()
    {
        player = GetComponent<PlayerController>();
        if (player == null) return;
        legacyLabel = player.levelText;
        Transform parent;
        if (legacyLabel != null)
        {
            parent = legacyLabel.transform.parent;
            legacyWasEnabled = legacyLabel.enabled;
            legacyLabel.enabled = false;
        }
        else
        {
            fallbackCanvas = new GameObject("ExperienceCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            fallbackCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            fallbackCanvas.GetComponent<Canvas>().sortingOrder = 3;
            CanvasScaler scaler = fallbackCanvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = .5f;
            parent = fallbackCanvas.transform;
        }
        root = new GameObject("ExperienceBar", typeof(RectTransform)).GetComponent<RectTransform>();
        root.SetParent(parent, false);
        if (legacyLabel != null)
        {
            RectTransform source = legacyLabel.rectTransform;
            root.anchorMin = source.anchorMin;
            root.anchorMax = source.anchorMax;
            root.pivot = source.pivot;
            root.sizeDelta = new Vector2(370f, 50f);
            root.anchoredPosition = source.anchoredPosition + new Vector2((370f - source.rect.width) * source.pivot.x, 0f);
        }
        else
        {
            root.anchorMin = root.anchorMax = root.pivot = new Vector2(0f, 1f);
            root.anchoredPosition = new Vector2(18f, -12f);
            root.sizeDelta = new Vector2(250f, 44f);
        }
        levelLabel = Label("Level", root, 26f, new Vector2(0f, .52f), new Vector2(.48f, 1f));
        percentage = Label("Percentage", root, 23f, new Vector2(.48f, .52f), Vector2.one);
        percentage.alignment = TextAlignmentOptions.MidlineRight;
        RectTransform background = Panel("Track", root, new Color(.055f, .085f, .12f, .98f),
            new Vector2(0f, .04f), new Vector2(1f, .42f));
        Outline outline = background.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(.3f, .4f, .5f);
        outline.effectDistance = new Vector2(1f, -1f);
        RectTransform inset = new GameObject("Inset", typeof(RectTransform)).GetComponent<RectTransform>();
        inset.SetParent(background, false);
        inset.anchorMin = Vector2.zero;
        inset.anchorMax = Vector2.one;
        inset.offsetMin = Vector2.one * 2f;
        inset.offsetMax = Vector2.one * -2f;
        fill = Panel("Fill", inset, new Color(.2f, .75f, 1f), Vector2.zero, Vector2.one);
        Refresh();
    }

    private void LateUpdate()
    {
        if (player == null || fill == null) return;
        if (shownLevel != player.level || shownExperience != player.CurrentExperience || shownThreshold != player.experienceThreshold) Refresh();
    }

    private void Refresh()
    {
        float progress = player.ExperienceProgress;
        // Rect width works even without an assigned sprite; Image.fillAmount would not.
        fill.anchorMax = new Vector2(progress, 1f);
        levelLabel.text = "Level " + player.level;
        percentage.text = "EXP " + (progress * 100f).ToString("0.#") + "%";
        shownLevel = player.level;
        shownExperience = player.CurrentExperience;
        shownThreshold = player.experienceThreshold;
    }

    private static RectTransform Panel(string name, Transform parent, Color color, Vector2 min, Vector2 max)
    {
        RectTransform rect = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = min; rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        rect.GetComponent<Image>().color = color;
        rect.GetComponent<Image>().raycastTarget = false;
        return rect;
    }

    private static TextMeshProUGUI Label(string name, Transform parent, float size, Vector2 min, Vector2 max)
    {
        TextMeshProUGUI label = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        label.transform.SetParent(parent, false);
        label.rectTransform.anchorMin = min; label.rectTransform.anchorMax = max;
        label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
        label.fontSize = size;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.raycastTarget = false;
        label.richText = false;
        return label;
    }

    private void OnDisable() { if (root != null) root.gameObject.SetActive(false); }
    private void OnEnable() { if (root != null) root.gameObject.SetActive(true); }
    private void OnDestroy()
    {
        if (legacyLabel != null) legacyLabel.enabled = legacyWasEnabled;
        GameObject target = fallbackCanvas != null ? fallbackCanvas : root != null ? root.gameObject : null;
        if (target == null) return;
        if (Application.isPlaying) Destroy(target);
        else DestroyImmediate(target);
    }
}
