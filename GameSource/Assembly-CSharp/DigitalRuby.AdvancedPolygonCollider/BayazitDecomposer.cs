using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRuby.AdvancedPolygonCollider;

public static class BayazitDecomposer
{
	private static Vector2 At(int i, List<Vector2> vertices)
	{
		int count = vertices.Count;
		if (count != 1)
		{
			return vertices[(i < 0) ? (count - -i % count) : (i % count)];
		}
		return vertices[0];
	}

	private static List<Vector2> Copy(int i, int j, List<Vector2> vertices)
	{
		List<Vector2> list = new List<Vector2>();
		while (j < i)
		{
			j += vertices.Count;
		}
		while (i <= j)
		{
			list.Add(At(i, vertices));
			i++;
		}
		return list;
	}

	public static List<List<Vector2>> ConvexPartition(List<Vector2> vertices)
	{
		ForceCounterClockWise(vertices);
		List<List<Vector2>> list = new List<List<Vector2>>();
		Vector2 vector = default(Vector2);
		Vector2 vector2 = default(Vector2);
		int num = 0;
		int i = 0;
		for (int j = 0; j < vertices.Count; j++)
		{
			if (!Reflex(j, vertices))
			{
				continue;
			}
			float num3;
			float num2 = (num3 = float.MaxValue);
			for (int k = 0; k < vertices.Count; k++)
			{
				Vector2 vector3;
				if (Left(At(j - 1, vertices), At(j, vertices), At(k, vertices)) && RightOn(At(j - 1, vertices), At(j, vertices), At(k - 1, vertices)))
				{
					vector3 = LineIntersect(At(j - 1, vertices), At(j, vertices), At(k, vertices), At(k - 1, vertices));
					if (Right(At(j + 1, vertices), At(j, vertices), vector3))
					{
						float num4 = SquareDist(At(j, vertices), vector3);
						if (num4 < num2)
						{
							num2 = num4;
							vector = vector3;
							num = k;
						}
					}
				}
				if (!Left(At(j + 1, vertices), At(j, vertices), At(k + 1, vertices)) || !RightOn(At(j + 1, vertices), At(j, vertices), At(k, vertices)))
				{
					continue;
				}
				vector3 = LineIntersect(At(j + 1, vertices), At(j, vertices), At(k, vertices), At(k + 1, vertices));
				if (Left(At(j - 1, vertices), At(j, vertices), vector3))
				{
					float num4 = SquareDist(At(j, vertices), vector3);
					if (num4 < num3)
					{
						num3 = num4;
						i = k;
						vector2 = vector3;
					}
				}
			}
			List<Vector2> list2;
			List<Vector2> list3;
			if (num == (i + 1) % vertices.Count)
			{
				Vector2 item = (vector + vector2) / 2f;
				list2 = Copy(j, i, vertices);
				list2.Add(item);
				list3 = Copy(num, j, vertices);
				list3.Add(item);
			}
			else
			{
				double num5 = 0.0;
				double num6 = num;
				for (; i < num; i += vertices.Count)
				{
				}
				for (int l = num; l <= i; l++)
				{
					if (CanSee(j, l, vertices))
					{
						double num7 = 1f / (SquareDist(At(j, vertices), At(l, vertices)) + 1f);
						num7 = ((!Reflex(l, vertices)) ? (num7 + 1.0) : ((!RightOn(At(l - 1, vertices), At(l, vertices), At(j, vertices)) || !LeftOn(At(l + 1, vertices), At(l, vertices), At(j, vertices))) ? (num7 + 2.0) : (num7 + 3.0)));
						if (num7 > num5)
						{
							num6 = l;
							num5 = num7;
						}
					}
				}
				list2 = Copy(j, (int)num6, vertices);
				list3 = Copy((int)num6, j, vertices);
			}
			list.AddRange(ConvexPartition(list2));
			list.AddRange(ConvexPartition(list3));
			return list;
		}
		list.Add(vertices);
		for (int m = 0; m < list.Count; m++)
		{
		}
		for (int num8 = list.Count - 1; num8 >= 0; num8--)
		{
			if (list[num8].Count == 0)
			{
				list.RemoveAt(num8);
			}
		}
		return list;
	}

	private static bool CanSee(int i, int j, List<Vector2> vertices)
	{
		if (Reflex(i, vertices))
		{
			if (LeftOn(At(i, vertices), At(i - 1, vertices), At(j, vertices)) && RightOn(At(i, vertices), At(i + 1, vertices), At(j, vertices)))
			{
				return false;
			}
		}
		else if (RightOn(At(i, vertices), At(i + 1, vertices), At(j, vertices)) || LeftOn(At(i, vertices), At(i - 1, vertices), At(j, vertices)))
		{
			return false;
		}
		if (Reflex(j, vertices))
		{
			if (LeftOn(At(j, vertices), At(j - 1, vertices), At(i, vertices)) && RightOn(At(j, vertices), At(j + 1, vertices), At(i, vertices)))
			{
				return false;
			}
		}
		else if (RightOn(At(j, vertices), At(j + 1, vertices), At(i, vertices)) || LeftOn(At(j, vertices), At(j - 1, vertices), At(i, vertices)))
		{
			return false;
		}
		for (int k = 0; k < vertices.Count; k++)
		{
			if ((k + 1) % vertices.Count != i && k != i && (k + 1) % vertices.Count != j && k != j && LineIntersect2(At(i, vertices), At(j, vertices), At(k, vertices), At(k + 1, vertices), out var _))
			{
				return false;
			}
		}
		return true;
	}

	private static bool Reflex(int i, List<Vector2> vertices)
	{
		return Right(i, vertices);
	}

	private static bool Right(int i, List<Vector2> vertices)
	{
		return Right(At(i - 1, vertices), At(i, vertices), At(i + 1, vertices));
	}

	private static bool Left(Vector2 a, Vector2 b, Vector2 c)
	{
		return Area(ref a, ref b, ref c) > 0f;
	}

	private static bool LeftOn(Vector2 a, Vector2 b, Vector2 c)
	{
		return Area(ref a, ref b, ref c) >= 0f;
	}

	private static bool Right(Vector2 a, Vector2 b, Vector2 c)
	{
		return Area(ref a, ref b, ref c) < 0f;
	}

	private static bool RightOn(Vector2 a, Vector2 b, Vector2 c)
	{
		return Area(ref a, ref b, ref c) <= 0f;
	}

	private static float SquareDist(Vector2 a, Vector2 b)
	{
		float num = b.x - a.x;
		float num2 = b.y - a.y;
		return num * num + num2 * num2;
	}

	private static void ForceCounterClockWise(List<Vector2> vertices)
	{
		if (!IsCounterClockWise(vertices))
		{
			vertices.Reverse();
		}
	}

	private static bool IsCounterClockWise(List<Vector2> vertices)
	{
		if (vertices.Count < 3)
		{
			return true;
		}
		return GetSignedArea(vertices) > 0f;
	}

	private static float GetSignedArea(List<Vector2> vertices)
	{
		float num = 0f;
		for (int i = 0; i < vertices.Count; i++)
		{
			int index = (i + 1) % vertices.Count;
			num += vertices[i].x * vertices[index].y;
			num -= vertices[i].y * vertices[index].x;
		}
		return num / 2f;
	}

	private static Vector2 LineIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
	{
		Vector2 zero = Vector2.zero;
		float num = p2.y - p1.y;
		float num2 = p1.x - p2.x;
		float num3 = num * p1.x + num2 * p1.y;
		float num4 = q2.y - q1.y;
		float num5 = q1.x - q2.x;
		float num6 = num4 * q1.x + num5 * q1.y;
		float num7 = num * num5 - num4 * num2;
		if (!FloatEquals(num7, 0f))
		{
			zero.x = (num5 * num3 - num2 * num6) / num7;
			zero.y = (num * num6 - num4 * num3) / num7;
		}
		return zero;
	}

	private static bool LineIntersect2(Vector2 a0, Vector2 a1, Vector2 b0, Vector2 b1, out Vector2 intersectionPoint)
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

	private static bool FloatEquals(float value1, float value2)
	{
		return Math.Abs(value1 - value2) <= Mathf.Epsilon;
	}

	private static float Area(ref Vector2 a, ref Vector2 b, ref Vector2 c)
	{
		return a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y);
	}
}
