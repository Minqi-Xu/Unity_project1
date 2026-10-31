using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerWeaponSystem
{
    private bool axeBusy;
    public float AxeArc => Mathf.Min(170f, Definition.meleeArc + Get(WeaponBuffEffect.AxeArc));

    private bool CanSwing => owner != null && owner.GetComponent<PlayerHealth>() != null
        && owner.GetComponent<PlayerHealth>().currentHealth > 0f;

    private IEnumerator AxeAttack(Vector2 aim)
    {
        axeBusy = true;
        float speedScale = Cooldown / Mathf.Max(.02f, Definition.cooldown);
        float windup = Definition.windup * Mathf.Max(.4f, 1f - Get(WeaponBuffEffect.AxeWindup)) * speedScale;
        float active = Definition.swingDuration * speedScale;
        float recovery = Definition.recovery * speedScale;
        float windupEnds = Time.time + windup;
        float swingEnds = windupEnds + active;
        float recoveryEnds = swingEnds + recovery;
        axeRecoveryEnds = recoveryEnds;
        float reach = Radius;
        float sideReach = reach * Definition.meleeSideScale;
        float arc = AxeArc;
        GameObject visualObject = new GameObject("HeavyAxeSwing");
        AxeSwingVisual visual = visualObject.AddComponent<AxeSwingVisual>();
        visual.Initialize(owner.transform, aim, reach, sideReach, arc, windup, active, recovery, Definition.tint);
        try
        {
            while (Time.time < windupEnds)
            {
                if (!CanSwing) yield break;
                yield return null;
            }
            if (!CanSwing) yield break;
            HashSet<Enemy> struck = new HashSet<Enemy>();
            WeaponAttackContext context = new WeaponAttackContext();
            while (Time.time < swingEnds)
            {
                if (!CanSwing) yield break;
                if (Time.timeScale <= 0f) { yield return null; continue; }
                DamageAxeSweep(aim, reach, sideReach, arc, (Time.time - windupEnds) / Mathf.Max(.001f, active), struck, context);
                yield return null;
            }
            while (Time.timeScale <= 0f) yield return null;
            if (CanSwing) DamageAxeSweep(aim, reach, sideReach, arc, 1f, struck, context);
            while (Time.time < recoveryEnds) yield return null;
        }
        finally
        {
            axeBusy = false;
            if (visualObject != null) Destroy(visualObject);
        }
    }

    // One enemy can have multiple colliders but takes at most one hit per swing.
    public void DamageAxeSweep(Vector2 aim, float reach, float sideReach, float arc, float progress,
        HashSet<Enemy> struck, WeaponAttackContext context)
    {
        if (owner == null || Time.timeScale <= 0f) return;
        Vector2 origin = owner.transform.position;
        foreach (ZoneSeal seal in FindObjectsByType<ZoneSeal>())
        {
            if (context.seals.Contains(seal) || !MeleeSweepShape.Contains((Vector2)seal.transform.position - origin,
                aim, reach, sideReach, arc, progress)) continue;
            context.seals.Add(seal);
            seal.TakeDamage(owner.Stats.CalculateDamage(Definition.damage * CharacterDamageMultiplier,
                owner.damageMultiplier, IsPrimaryWeapon, AdditiveDamageBonus));
        }
        foreach (Collider2D collider in Physics2D.OverlapCircleAll(origin, reach))
        {
            Enemy enemy = collider.GetComponentInParent<Enemy>();
            if (enemy == null || !enemy.IsAlive || struck.Contains(enemy)) continue;
            if (!MeleeSweepShape.Contains((Vector2)enemy.transform.position - origin, aim, reach, sideReach, arc, progress)) continue;
            struck.Add(enemy);
            Hit(enemy, Definition.damage, origin, 0, context);
        }
    }
}
