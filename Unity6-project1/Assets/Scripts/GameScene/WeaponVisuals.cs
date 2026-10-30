using UnityEngine;

// Lightweight placeholder art: existing sprites plus a brief melee range ring.
public static class WeaponVisuals
{
    private static Material lineMaterial;

    public static LineRenderer Stroke(string name, Transform parent, Color color, float width, int points)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        LineRenderer line = obj.AddComponent<LineRenderer>();
        if (lineMaterial == null) lineMaterial = new Material(Shader.Find("Sprites/Default"));
        line.sharedMaterial = lineMaterial;
        line.useWorldSpace = false;
        line.positionCount = points;
        line.startWidth = line.endWidth = width;
        line.startColor = line.endColor = color;
        line.sortingOrder = 15;
        return line;
    }

    public static void Lightning(Vector2 position, Color color)
    {
        GameObject effect = new GameObject("LightningStrike");
        LineRenderer line = effect.AddComponent<LineRenderer>();
        if (lineMaterial == null) lineMaterial = new Material(Shader.Find("Sprites/Default"));
        line.sharedMaterial = lineMaterial;
        line.positionCount = 4;
        line.SetPosition(0, position + Vector2.up * 4f);
        line.SetPosition(1, position + new Vector2(.3f, 2.5f));
        line.SetPosition(2, position + new Vector2(-.3f, 1.5f));
        line.SetPosition(3, position);
        line.startWidth = line.endWidth = .1f;
        line.startColor = line.endColor = color;
        line.sortingOrder = 20;
        Object.Destroy(effect, .2f);
    }

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
