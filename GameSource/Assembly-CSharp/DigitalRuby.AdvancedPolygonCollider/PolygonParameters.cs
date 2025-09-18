using UnityEngine;

namespace DigitalRuby.AdvancedPolygonCollider;

public struct PolygonParameters
{
	public Texture2D Texture;

	public Rect Rect;

	public Vector2 Offset;

	public float XMultiplier;

	public float YMultiplier;

	public byte AlphaTolerance;

	public int DistanceThreshold;

	public bool Decompose;

	public bool UseCache;

	public override int GetHashCode()
	{
		int num = Texture.GetHashCode();
		if (num == 0)
		{
			num = 1;
		}
		return num * (int)((float)Rect.GetHashCode() * XMultiplier * YMultiplier * (float)(int)AlphaTolerance * (float)Mathf.Max(DistanceThreshold, 1) * (float)((!Decompose) ? 1 : 2));
	}

	public override bool Equals(object obj)
	{
		if (obj is PolygonParameters polygonParameters)
		{
			if (Texture == polygonParameters.Texture && Rect == polygonParameters.Rect && XMultiplier == polygonParameters.XMultiplier && YMultiplier == polygonParameters.YMultiplier && AlphaTolerance == polygonParameters.AlphaTolerance && DistanceThreshold == polygonParameters.DistanceThreshold)
			{
				return Decompose == polygonParameters.Decompose;
			}
			return false;
		}
		return false;
	}
}
