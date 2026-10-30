using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class AbilityUiSmokeTests
{
    [MenuItem("Tools/UI/Run Ability and Countdown Smoke Tests")]
    public static void Run()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Run outside Play Mode.");
        GameObject root = new GameObject("AbilityUiChecks");
        root.SetActive(false);
        GameObject legacy = GameObject.Find("CooldownIndicators");
        GameObject testCanvas = null;
        float previousScale = Time.timeScale;
        try
        {
            PlayerController player = root.AddComponent<PlayerController>();
            PlayerStats stats = root.AddComponent<PlayerStats>();
            typeof(PlayerController).GetProperty("Stats").SetValue(player, stats);
            root.AddComponent<PlayerHealth>().currentHealth = 100f;
            PlayerWeaponSystem primary = root.AddComponent<PlayerWeaponSystem>();
            primary.Configure(player, Resources.Load<WeaponDefinition>("Weapons/Axe"));
            Set(primary, "nextAttack", Time.time + .7f);
            Set(primary, "activeCooldownDuration", .8f);
            Near(primary.CooldownRemaining, .7f, "Weapon exposes actual remaining cooldown");
            Near(primary.CooldownDuration, .8f, "Weapon exposes actual attack interval");
            Set(player, "dashCooldownTimer", 2f);
            Near(player.DashCooldownRemaining, 2f, "Dash reads its real countdown");
            Set(player, "lastBombTime", Time.time);
            Near(player.BombCooldownRemaining, player.BombCooldownDuration, "Bomb reports its real current cooldown");

            WeaponInventory inventory = WeaponInventory.Ensure(player);
            foreach (string asset in new[] { "Drone", "Creature", "Lightning" })
                Check(inventory.Acquire(Resources.Load<WeaponDefinition>("Weapons/" + asset)), "Acquire HUD auxiliary " + asset);
            PlayerWeaponSystem lightning = inventory.Find(WeaponKind.Lightning);
            Set(lightning, "lightningSearchRetryUntil", Time.time + .15f);
            Near(lightning.CooldownRemaining, 0f, "Lightning searching does not consume its attack cooldown");
            PlayerWeaponSystem drone = inventory.Find(WeaponKind.Drone);
            var summons = (List<SummonedUnit>)Get(drone, "summons");
            for (int i = 0; i < 2; i++)
            {
                GameObject unitRoot = new GameObject("CooldownUnit");
                unitRoot.transform.SetParent(root.transform, false);
                SummonedUnit unit = unitRoot.AddComponent<SummonedUnit>();
                unit.Initialize(player, drone, i);
                Set(unit, "nextAttack", Time.time + (i == 0 ? .6f : .2f));
                summons.Add(unit);
            }
            Near(drone.CooldownRemaining, .2f, "Summon cooldown uses next ready permanent unit");
            Set(summons[0], "nextAttack", Time.time - 1f);
            Check(drone.ActiveSummonCount == 2 && drone.ReadySummonCount == 1, "Summon readiness reflects individual attack clocks");

            AbilityCooldownHUD hud = root.AddComponent<AbilityCooldownHUD>();
            Call(hud, "Start");
            testCanvas = (GameObject)Get(hud, "canvasRoot");
            Call(hud, "LateUpdate");
            var cards = (IList)Get(hud, "cards");
            Check(cards.Count == 6, "Primary, dash, bomb and three auxiliary indicators");
            TextMeshProUGUI[] labels = testCanvas.GetComponentsInChildren<TextMeshProUGUI>(true);
            Check(labels.Any(x => x.text == "AXE"), "Compact HUD identifies the class's actual primary weapon");
            Check(labels.Any(x => x.text == "1/2"), "Compact HUD displays ready summon count");
            Check(labels.Any(x => x.text == "READY"), "Ready abilities remain ready while lightning searches");

            PauseMenu pause = root.AddComponent<PauseMenu>();
            pause.pauseMenuUI = new GameObject("PauseTestPanel");
            pause.pauseMenuUI.transform.SetParent(root.transform, false);
            pause.pauseText = Text(pause.pauseMenuUI.transform, "Paused");
            pause.countdownText = Text(pause.pauseMenuUI.transform, "Countdown");
            List<GameObject> buttons = new List<GameObject>();
            foreach (string name in new[] { "Continue", "Restart", "Settings", "Exit" })
            {
                GameObject button = new GameObject(name, typeof(RectTransform), typeof(Button));
                button.transform.SetParent(pause.pauseMenuUI.transform, false);
                buttons.Add(button);
            }
            GameObject alreadyHidden = new GameObject("HiddenButton", typeof(RectTransform), typeof(Button));
            alreadyHidden.transform.SetParent(pause.pauseMenuUI.transform, false);
            alreadyHidden.SetActive(false);
            IEnumerator countdown = (IEnumerator)Call(pause, "CountdownAndResume");
            Check(countdown.MoveNext(), "Countdown starts");
            Check(buttons.All(x => !x.activeSelf), "All four menu buttons hidden during countdown");
            Check(pause.countdownText.gameObject.activeSelf && !pause.pauseText.gameObject.activeSelf, "Only countdown label remains");
            Call(pause, "RestoreCountdownButtons");
            Check(buttons.All(x => x.activeSelf) && !alreadyHidden.activeSelf, "Next pause restores only previously visible buttons");
            Debug.Log("[AbilityUiSmokeTests] PASS: weapon/dash/bomb clocks, summon readiness, dynamic six-card HUD, countdown button visibility and restoration.");
        }
        finally
        {
            if (testCanvas != null) UnityEngine.Object.DestroyImmediate(testCanvas);
            UnityEngine.Object.DestroyImmediate(root);
            if (legacy != null) legacy.SetActive(true);
            Time.timeScale = previousScale;
        }
    }

    private static TextMeshProUGUI Text(Transform parent, string name)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        return obj.GetComponent<TextMeshProUGUI>();
    }
    private static object Get(object target, string name) => target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(target);
    private static void Set(object target, string name, object value) => target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
    private static object Call(object target, string name) => target.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(target, null);
    private static void Check(bool condition, string label) { if (!condition) throw new InvalidOperationException(label); }
    private static void Near(float actual, float expected, string label) => Check(Mathf.Abs(actual - expected) < .001f, label);
}
