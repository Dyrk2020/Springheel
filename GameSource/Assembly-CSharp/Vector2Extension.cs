using System;
using UnityEngine;

public static class Vector2Extension
{
	public static Vector2 Rotate(this Vector2 v, float degrees)
	{
		float f = degrees * (MathF.PI / 180f);
		float num = Mathf.Sin(f);
		float num2 = Mathf.Cos(f);
		return new Vector2(num2 * v.x - num * v.y, num * v.x + num2 * v.y);
	}

	public static float Angle(this Vector2 v)
	{
		return Vector2.Angle(Vector2.right, v);
	}

	public static float SignedAngle(Vector2 v1, Vector2 v2)
	{
		return Vector2.Angle(v1, v2) * Mathf.Sign(v1.x * v2.y - v1.y * v2.x);
	}
}
