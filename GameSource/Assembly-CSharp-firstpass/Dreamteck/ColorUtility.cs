using UnityEngine;

namespace Dreamteck;

public static class ColorUtility
{
	public static Color MoveTowardsColor(Color from, Color to, float t)
	{
		Vector4 current = new Vector4(from.r, from.g, from.b, from.a);
		Vector4 target = new Vector4(to.r, to.g, to.b, to.a);
		Vector4 vector = Vector4.MoveTowards(current, target, t);
		return new Color(vector.x, vector.y, vector.z, vector.w);
	}
}
