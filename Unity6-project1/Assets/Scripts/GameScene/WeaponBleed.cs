using System.Collections.Generic;
using UnityEngine;

// Tick damage never invokes hit procs; up to three independent bleed stacks.
public class WeaponBleed : MonoBehaviour
{
    private class Stack { public PlayerController owner; public float damage; public float bonus; public float nextTick; public int ticks; }
    private readonly List<Stack> stacks = new List<Stack>();
    public void Apply(PlayerController owner, float damage, float weaponDamageBonus = 0f)
    {
        if (stacks.Count >= 3) stacks.RemoveAt(0);
        stacks.Add(new Stack { owner = owner, damage = damage, bonus = weaponDamageBonus, nextTick = Time.time + 1f, ticks = 3 });
    }
    private void Update()
    {
        if (Time.timeScale <= 0f) return;
        Enemy enemy = GetComponent<Enemy>();
        if (enemy == null || !enemy.IsAlive) return;
        for (int i = stacks.Count - 1; i >= 0; i--)
        {
            Stack stack = stacks[i];
            if (Time.time < stack.nextTick) continue;
            if (stack.owner != null && stack.owner.Stats != null)
                enemy.TakeDamage(stack.owner.Stats.CalculateDamage(stack.damage, stack.owner.damageMultiplier, false, stack.bonus), stack.owner.Stats);
            stack.nextTick += 1f;
            if (--stack.ticks <= 0) stacks.RemoveAt(i);
        }
    }
}
