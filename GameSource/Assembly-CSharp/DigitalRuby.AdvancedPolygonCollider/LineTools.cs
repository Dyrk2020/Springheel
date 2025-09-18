using System;
using UnityEngine;

namespace DigitalRuby.AdvancedPolygonCollider;

public static class LineTools
{
	public static float DistanceBetweenPointAndLineSegment(ref Vector2 point, ref Vector2 start, ref Vector2 end)
	{
		if (start == end)
		{
			return Vector2.Distance(point, start);
		}
		Vector2 vector = end - start;
		float num = Vector2.Dot(point - start, vector);
		if (num <= 0f)
		{
			return Vector2.Distance(point, start);
		}
		float num2 = Vector2.Dot(vector, vector);
		if (num2 <= num)
		{
			return Vector2.Distance(point, end);
		}
		float num3 = num / num2;
		Vector2 b = start + vector * num3;
		return Vector2.Distance(point, b);
	}

	public static bool LineIntersect2(ref Vector2 a0, ref Vector2 a1, ref Vector2 b0, ref Vector2 b1, out Vector2 intersectionPoint)
	{
		intersectionPoint = Vector2.zero;
		if (a0 == b0 || a0 == b1 || a1 == b0 || a1 == b1)
		{
			return false;
		}
		float x = a0.x;
		float y = a0.y;
		float x2 = a1.x;
		float y2 = a1.y;
		float x3 = b0.x;
		float y3 = b0.y;
		float x4 = b1.x;
		float y4 = b1.y;
		if (Math.Max(x, x2) < Math.Min(x3, x4) || Math.Max(x3, x4) < Math.Min(x, x2))
		{
			return false;
		}
		if (Math.Max(y, y2) < Math.Min(y3, y4) || Math.Max(y3, y4) < Math.Min(y, y2))
		{
			return false;
		}
		float num = (x4 - x3) * (y - y3) - (y4 - y3) * (x - x3);
		float num2 = (x2 - x) * (y - y3) - (y2 - y) * (x - x3);
		float num3 = (y4 - y3) * (x2 - x) - (x4 - x3) * (y2 - y);
		if (Math.Abs(num3) < Mathf.Epsilon)
		{
			return false;
		}
		num /= num3;
		num2 /= num3;
		if (0f < num && num < 1f && 0f < num2 && num2 < 1f)
		{
			intersectionPoint.x = x + num * (x2 - x);
			intersectionPoint.y = y + num * (y2 - y);
			return true;
		}
		return false;
	}

	public static Vector2 LineIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
	{
		Vector2 zero = Vector2.zero;
		float num = p2.y - p1.y;
		float num2 = p1.x - p2.x;
		float num3 = num * p1.x + num2 * p1.y;
		float num4 = q2.y - q1.y;
		float num5 = q1.x - q2.x;
		float num6 = num4 * q1.x + num5 * q1.y;
		float num7 = num * num5 - num4 * num2;
		if (Mathf.Abs(num7) > Mathf.Epsilon)
		{
			zero.x = (num5 * num3 - num2 * num6) / num7;
			zero.y = (num * num6 - num4 * num3) / num7;
		}
		return zero;
	}

	public static bool LineIntersect(ref Vector2 point1, ref Vector2 point2, ref Vector2 point3, ref Vector2 point4, bool firstIsSegment, bool secondIsSegment, out Vector2 point)
	{
		point = default(Vector2);
		float num = point4.y - point3.y;
		float num2 = point2.x - point1.x;
		float num3 = point4.x - point3.x;
		float num4 = point2.y - point1.y;
		float num5 = num * num2 - num3 * num4;
		if (Mathf.Abs(num5) > Mathf.Epsilon)
		{
			float num6 = point1.y - point3.y;
			float num7 = point1.x - point3.x;
			float num8 = 1f / num5;
			float num9 = num3 * num6 - num * num7;
			num9 *= num8;
			if (!firstIsSegment || (num9 >= 0f && num9 <= 1f))
			{
				float num10 = num2 * num6 - num4 * num7;
				num10 *= num8;
				if ((!secondIsSegment || (num10 >= 0f && num10 <= 1f)) && (num9 != 0f || num10 != 0f))
				{
					point.x = point1.x + num9 * num2;
					point.y = point1.y + num9 * num4;
					return true;
				}
			}
		}
		return false;
	}

	public static bool LineIntersect(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4, bool firstIsSegment, bool secondIsSegment, out Vector2 intersectionPoint)
	{
		return LineIntersect(ref point1, ref point2, ref point3, ref point4, firstIsSegment, secondIsSegment, out intersectionPoint);
	}

	public static bool LineIntersect(ref Vector2 point1, ref Vector2 point2, ref Vector2 point3, ref Vector2 point4, out Vector2 intersectionPoint)
	{
		return LineIntersect(ref point1, ref point2, ref point3, ref point4, firstIsSegment: true, secondIsSegment: true, out intersectionPoint);
	}

	public static bool LineIntersect(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4, out Vector2 intersectionPoint)
	{
		return LineIntersect(ref point1, ref point2, ref point3, ref point4, firstIsSegment: true, secondIsSegment: true, out intersectionPoint);
	}

	public static Vertices LineSegmentVerticesIntersect(ref Vector2 point1, ref Vector2 point2, Vertices vertices)
	{
		Vertices vertices2 = new Vertices();
		for (int i = 0; i < vertices.Count; i++)
		{
			if (LineIntersect(vertices[i], vertices[vertices.NextIndex(i)], point1, point2, firstIsSegment: true, secondIsSegment: true, out var intersectionPoint))
			{
				vertices2.Add(intersectionPoint);
			}
		}
		return vertices2;
	}
}
