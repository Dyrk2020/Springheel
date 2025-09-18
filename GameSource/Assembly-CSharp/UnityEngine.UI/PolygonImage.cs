using System.Collections.Generic;

namespace UnityEngine.UI;

[RequireComponent(typeof(Image))]
[AddComponentMenu("UI/Effects/PolygonImage", 16)]
public class PolygonImage : BaseMeshEffect
{
	protected PolygonImage()
	{
	}

	public override void ModifyMesh(VertexHelper vh)
	{
		Image component = GetComponent<Image>();
		if (component.type != Image.Type.Simple)
		{
			return;
		}
		Sprite overrideSprite = component.overrideSprite;
		if (!(overrideSprite == null) && overrideSprite.triangles.Length != 6 && vh.currentVertCount == 4)
		{
			UIVertex vertex = default(UIVertex);
			vh.PopulateUIVertex(ref vertex, 0);
			Vector2 vector = vertex.position;
			vh.PopulateUIVertex(ref vertex, 2);
			Vector2 vector2 = vertex.position;
			int num = overrideSprite.vertices.Length;
			List<UIVertex> list = new List<UIVertex>(num);
			Vector2 vector3 = overrideSprite.bounds.center;
			Vector2 vector4 = new Vector2(1f / overrideSprite.bounds.size.x, 1f / overrideSprite.bounds.size.y);
			for (int i = 0; i < num; i++)
			{
				vertex = default(UIVertex);
				float t = (overrideSprite.vertices[i].x - vector3.x) * vector4.x + 0.5f;
				float t2 = (overrideSprite.vertices[i].y - vector3.y) * vector4.y + 0.5f;
				vertex.position = new Vector2(Mathf.Lerp(vector.x, vector2.x, t), Mathf.Lerp(vector.y, vector2.y, t2));
				vertex.color = component.color;
				vertex.uv0 = overrideSprite.uv[i];
				list.Add(vertex);
			}
			num = overrideSprite.triangles.Length;
			List<int> list2 = new List<int>(num);
			for (int j = 0; j < num; j++)
			{
				list2.Add(overrideSprite.triangles[j]);
			}
			vh.Clear();
			vh.AddUIVertexStream(list, list2);
		}
	}
}
