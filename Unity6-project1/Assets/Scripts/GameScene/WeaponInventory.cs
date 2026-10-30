using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

// Auxiliary weapon instances live under the player and never replace the primary.
[DisallowMultipleComponent]
public class WeaponInventory : MonoBehaviour
{
    [Min(1)] public int auxiliarySlots = 3;
    private readonly List<PlayerWeaponSystem> auxiliary = new List<PlayerWeaponSystem>();
    private GameObject hud;
    private TextMeshProUGUI loadoutText;
    public int AuxiliaryCount => auxiliary.Count;

    public PlayerWeaponSystem Find(WeaponKind kind)
    {
        PlayerWeaponSystem primary = GetComponent<PlayerWeaponSystem>();
        if (primary != null && primary.Definition != null && primary.Definition.kind == kind) return primary;
        foreach (PlayerWeaponSystem weapon in auxiliary)
            if (weapon != null && weapon.Definition != null && weapon.Definition.kind == kind) return weapon;
        return null;
    }

    public bool CanAcquire(WeaponDefinition definition)
    {
        return definition != null && definition.IsAuxiliary && auxiliary.Count < auxiliarySlots && Find(definition.kind) == null;
    }

    public bool Acquire(WeaponDefinition definition)
    {
        PlayerController owner = GetComponent<PlayerController>();
        if (owner == null || !CanAcquire(definition)) return false;
        GameObject holder = new GameObject("Auxiliary - " + definition.displayName);
        holder.transform.SetParent(transform, false);
        // Counter the character sprite scale so weapon ranges remain world-space values.
        holder.transform.localScale = new Vector3(1f / Mathf.Max(0.001f, transform.lossyScale.x),
            1f / Mathf.Max(0.001f, transform.lossyScale.y), 1f);
        PlayerWeaponSystem weapon = holder.AddComponent<PlayerWeaponSystem>();
        weapon.Configure(owner, definition);
        auxiliary.Add(weapon);
        RefreshHUD();
        return true;
    }

    private void Start()
    {
        hud = new GameObject("AuxiliaryLoadoutHUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        hud.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        hud.GetComponent<Canvas>().sortingOrder = 5;
        CanvasScaler scaler = hud.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = .5f;
        GameObject label = new GameObject("Loadout", typeof(RectTransform), typeof(TextMeshProUGUI));
        label.transform.SetParent(hud.transform, false);
        loadoutText = label.GetComponent<TextMeshProUGUI>();
        loadoutText.rectTransform.anchorMin = new Vector2(.02f, .02f);
        loadoutText.rectTransform.anchorMax = new Vector2(.98f, .08f);
        loadoutText.rectTransform.offsetMin = loadoutText.rectTransform.offsetMax = Vector2.zero;
        loadoutText.fontSize = 17f;
        loadoutText.enableAutoSizing = true;
        loadoutText.fontSizeMin = 10f;
        loadoutText.fontSizeMax = 17f;
        loadoutText.raycastTarget = false;
        loadoutText.color = Color.white;
        RefreshHUD();
    }

    private void RefreshHUD()
    {
        if (loadoutText == null) return;
        string names = "";
        foreach (PlayerWeaponSystem weapon in auxiliary)
            names += (names.Length > 0 ? "  |  " : "") + weapon.Definition.displayName;
        loadoutText.text = "AUXILIARY " + AuxiliaryCount + "/" + auxiliarySlots + "   " + names;
    }

    private void OnDestroy() { if (hud != null) Destroy(hud); }

    public static WeaponInventory Ensure(PlayerController player)
    {
        WeaponInventory inventory = player.GetComponent<WeaponInventory>();
        return inventory != null ? inventory : player.gameObject.AddComponent<WeaponInventory>();
    }
}
