using UnityEngine;
using UnityEngine.UI;

// Dark rainbow bands keep bright multicolour text readable without extra textures or materials.
[RequireComponent(typeof(Image))]
public class MythCardBackground : BaseMeshEffect
{
    public override void ModifyMesh(VertexHelper mesh)
    {
        if (!IsActive()) return;
        Rect rect = graphic.rectTransform.rect;
        mesh.Clear();
        const int bands = 24;
        for (int i = 0; i < bands; i++)
        {
            float left = (float)i / bands, right = (float)(i + 1) / bands;
            Color a = Color.HSVToRGB(left * .85f, .65f, .24f);
            Color b = Color.HSVToRGB(right * .85f, .65f, .24f);
            a.a = b.a = graphic.color.a;
            int start = mesh.currentVertCount;
            mesh.AddVert(new Vector3(Mathf.Lerp(rect.xMin, rect.xMax, left), rect.yMin), a, Vector2.zero);
            mesh.AddVert(new Vector3(Mathf.Lerp(rect.xMin, rect.xMax, left), rect.yMax), a, Vector2.zero);
            mesh.AddVert(new Vector3(Mathf.Lerp(rect.xMin, rect.xMax, right), rect.yMax), b, Vector2.zero);
            mesh.AddVert(new Vector3(Mathf.Lerp(rect.xMin, rect.xMax, right), rect.yMin), b, Vector2.zero);
            mesh.AddTriangle(start, start + 1, start + 2);
            mesh.AddTriangle(start + 2, start + 3, start);
        }
    }
}
