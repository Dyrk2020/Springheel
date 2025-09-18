using System;
using UnityEngine;

public class PS2DSnapAxis
{
	public int snapPoint;

	public float snapDist;

	public float snapSize;

	public Vector2 baseLocation;

	private Vector2 direction;

	public PS2DSnapAxis(Vector2 dir)
	{
		direction = dir;
	}

	public void Reset(float dist, float size)
	{
		snapPoint = -1;
		snapSize = size;
		snapDist = dist;
	}

	public void CheckPoint(int pointId, Vector2 pointDragging, Vector2 pointStatic)
	{
		Vector2 point = GetPoint(pointStatic, pointStatic + direction, pointDragging);
		float num = Vector2.Distance(pointDragging, point);
		if (num < snapDist * snapSize && num < snapDist)
		{
			snapPoint = pointId;
			snapDist = num;
			baseLocation = point;
		}
	}

	private Vector2 GetPoint(Vector2 b1, Vector2 b2, Vector2 t)
	{
		float num = Vector2.Distance(b1, t);
		float f = Vector2.Distance(b2, t);
		float num2 = Vector2.Distance(b1, b2);
		float f2 = Mathf.Acos((Mathf.Pow(num, 2f) + Mathf.Pow(num2, 2f) - Mathf.Pow(f, 2f)) / (2f * num * num2));
		if (float.IsNaN(f2))
		{
			return t;
		}
		float num3 = Mathf.Cos(f2) * num;
		return b1 + num3 * (b2 - b1).normalized;
	}

	public Vector2 GetIntersection(PS2DSnapAxis another)
	{
		return LineIntersectionPoint(baseLocation, baseLocation + direction, another.baseLocation, another.baseLocation + another.direction);
	}

	private Vector2 LineIntersectionPoint(Vector2 l1s, Vector2 l1e, Vector2 l2s, Vector2 l2e)
	{
		float num = l1e.y - l1s.y;
		float num2 = l1s.x - l1e.x;
		float num3 = num * l1s.x + num2 * l1s.y;
		float num4 = l2e.y - l2s.y;
		float num5 = l2s.x - l2e.x;
		float num6 = num4 * l2s.x + num5 * l2s.y;
		float num7 = num * num5 - num4 * num2;
		if (num7 == 0f)
		{
			throw new Exception("Lines are parallel");
		}
		return new Vector2((num5 * num3 - num2 * num6) / num7, (num * num6 - num4 * num3) / num7);
	}
}
