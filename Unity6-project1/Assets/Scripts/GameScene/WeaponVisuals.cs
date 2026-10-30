using UnityEngine;

// Lightweight placeholder art: existing sprites plus a brief melee range ring.
public static class WeaponVisuals
{
    private static Material lineMaterial;

    public static void Pulse(Vector2 position, float radius, Color color)
    {
        GameObject effect = new GameObject("AttackRing");
        effect.transform.position = position;
        LineRenderer line = effect.AddComponent<LineRenderer>();
        if (lineMaterial == null) lineMaterial = new Material(Shader.Find("Sprites/Default"));
        line.sharedMaterial = lineMaterial;
        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = 40;
        line.startWidth = line.endWidth = 0.07f;
        line.startColor = line.endColor = color;
        line.sortingOrder = 10;
        for (int i = 0; i < 40; i++)
        {
            float angle = i * Mathf.PI * 2f / 40f;
            line.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
        }
        Object.Destroy(effect, 0.15f);
    }

    public static void AddUnitSprite(GameObject unit, PlayerController owner, Color tint, WeaponKind kind)
    {
        SpriteRenderer source = owner.GetComponent<SpriteRenderer>();
        SpriteRenderer renderer = unit.AddComponent<SpriteRenderer>();
        if (source != null)
        {
            renderer.sprite = source.sprite;
            renderer.sharedMaterial = source.sharedMaterial;
            renderer.sortingOrder = source.sortingOrder + 1;
        }
        renderer.color = tint;
        unit.transform.localScale = owner.transform.localScale * 0.55f;
        if (kind == WeaponKind.Turret) unit.transform.rotation = Quaternion.Euler(0f, 0f, 45f);
    }
}
