using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRuby.AdvancedPolygonCollider;

public sealed class TextureConverter
{
	private const int closePixelsLength = 8;

	private static int[,] closePixels = new int[8, 2]
	{
		{ -1, -1 },
		{ 0, -1 },
		{ 1, -1 },
		{ 1, 0 },
		{ 1, 1 },
		{ 0, 1 },
		{ -1, 1 },
		{ -1, 0 }
	};

	private const float hullTolerance = 0.9f;

	private byte[] solids;

	private int solidsLength;

	private int width;

	private int height;

	public List<Vertices> DetectVertices(Color[] colors, int width, int alphaTolerance)
	{
		this.width = width;
		height = colors.Length / width;
		solids = new byte[colors.Length];
		for (int i = 0; i < solids.Length; i++)
		{
			int num = alphaTolerance - (int)(colors[i].a * 255f);
			int num2 = (int)((num & 0x80000000u) >> 31) - 1;
			num = num * num2 * num2;
			solids[i] = (byte)num;
		}
		solidsLength = colors.Length;
		List<Vertices> list = DetectVertices();
		List<Vertices> list2 = new List<Vertices>();
		for (int j = 0; j < list.Count; j++)
		{
			list2.Add(list[j]);
		}
		return list2;
	}

	public List<Vertices> DetectVertices()
	{
		List<Vertices> list = new List<Vertices>();
		Vector2? lastHoleEntrance = null;
		Vector2? entrance = null;
		List<Vector2> list2 = new List<Vector2>();
		bool flag;
		do
		{
			Vertices vertices;
			if (list.Count == 0)
			{
				vertices = new Vertices(CreateSimplePolygon(Vector2.zero, Vector2.zero));
				if (vertices.Count > 2)
				{
					entrance = GetTopMostVertex(vertices);
				}
			}
			else
			{
				if (!entrance.HasValue)
				{
					break;
				}
				vertices = new Vertices(CreateSimplePolygon(entrance.Value, new Vector2(entrance.Value.x - 1f, entrance.Value.y)));
			}
			flag = false;
			if (vertices.Count > 2)
			{
				while (true)
				{
					lastHoleEntrance = SearchHoleEntrance(vertices, lastHoleEntrance);
					if (!lastHoleEntrance.HasValue || list2.Contains(lastHoleEntrance.Value))
					{
						break;
					}
					list2.Add(lastHoleEntrance.Value);
					Vertices vertices2 = CreateSimplePolygon(lastHoleEntrance.Value, new Vector2(lastHoleEntrance.Value.x + 1f, lastHoleEntrance.Value.y));
					if (vertices2 != null && vertices2.Count > 2)
					{
						vertices2.Add(vertices2[0]);
						if (SplitPolygonEdge(vertices, lastHoleEntrance.Value, out var _, out var vertex2Index))
						{
							vertices.InsertRange(vertex2Index, vertices2);
						}
						break;
					}
				}
				list.Add(vertices);
			}
			if (entrance.HasValue && SearchNextHullEntrance(list, entrance.Value, out entrance))
			{
				flag = true;
			}
		}
		while (flag);
		if (list == null || (list != null && list.Count == 0))
		{
			throw new Exception("Couldn't detect any vertices.");
		}
		return list;
	}

	private void ApplyTriangulationCompatibleWinding(ref List<Vertices> detectedPolygons)
	{
		for (int i = 0; i < detectedPolygons.Count; i++)
		{
			detectedPolygons[i].Reverse();
		}
	}

	public bool IsSolid(ref Vector2 v)
	{
		return IsSolid((int)v.x + (int)v.y * width);
	}

	public bool IsSolid(int x, int y)
	{
		return IsSolid(x + y * width);
	}

	public bool IsSolid(int index)
	{
		if (index >= 0 && index < solids.Length)
		{
			return solids[index] == 0;
		}
		return false;
	}

	public bool InBounds(ref Vector2 coord)
	{
		if (coord.x >= 0f && coord.x < (float)width && coord.y >= 0f)
		{
			return coord.y < (float)height;
		}
		return false;
	}

	private Vector2? SearchHoleEntrance(Vertices polygon, Vector2? lastHoleEntrance)
	{
		if (polygon == null)
		{
			throw new ArgumentNullException("'polygon' can't be null.");
		}
		if (polygon.Count < 3)
		{
			throw new ArgumentException("'polygon.MainPolygon.Count' can't be less then 3.");
		}
		int num = 0;
		int num2 = ((!lastHoleEntrance.HasValue) ? ((int)GetTopMostCoord(polygon)) : ((int)lastHoleEntrance.Value.y));
		int num3 = (int)GetBottomMostCoord(polygon);
		if (num2 >= 0 && num2 < height && num3 > 0 && num3 < height)
		{
			for (int i = num2; i <= num3; i++)
			{
				List<float> list = SearchCrossingEdges(polygon, i);
				if (list.Count > 1 && list.Count % 2 == 0)
				{
					for (int j = 0; j < list.Count; j += 2)
					{
						bool flag = false;
						bool flag2 = false;
						for (int k = (int)list[j]; k <= (int)list[j + 1]; k++)
						{
							if (IsSolid(k, i))
							{
								if (!flag2)
								{
									flag = true;
									num = k;
								}
								if (flag && flag2)
								{
									Vector2? result = new Vector2(num, i);
									if (DistanceToHullAcceptable(polygon, result.Value, higherDetail: true))
									{
										return result;
									}
									result = null;
									break;
								}
							}
							else if (flag)
							{
								flag2 = true;
							}
						}
					}
				}
				else
				{
					_ = list.Count % 2;
				}
			}
		}
		return null;
	}

	private bool DistanceToHullAcceptableHoles(Vertices polygon, Vector2 point, bool higherDetail)
	{
		if (polygon == null)
		{
			throw new ArgumentNullException("polygon", "'polygon' can't be null.");
		}
		if (polygon.Count < 3)
		{
			throw new ArgumentException("'polygon.MainPolygon.Count' can't be less then 3.");
		}
		if (DistanceToHullAcceptable(polygon, point, higherDetail))
		{
			return true;
		}
		return false;
	}

	private bool DistanceToHullAcceptable(Vertices polygon, Vector2 point, bool higherDetail)
	{
		if (polygon == null)
		{
			throw new ArgumentNullException("polygon", "'polygon' can't be null.");
		}
		if (polygon.Count < 3)
		{
			throw new ArgumentException("'polygon.Count' can't be less then 3.");
		}
		Vector2 end = polygon[polygon.Count - 1];
		if (higherDetail)
		{
			for (int i = 0; i < polygon.Count; i++)
			{
				Vector2 start = polygon[i];
				if (LineTools.DistanceBetweenPointAndLineSegment(ref point, ref start, ref end) <= 0.9f || Vector2.Distance(point, start) <= 0.9f)
				{
					return false;
				}
				end = polygon[i];
			}
			return true;
		}
		for (int j = 0; j < polygon.Count; j++)
		{
			Vector2 start = polygon[j];
			if (LineTools.DistanceBetweenPointAndLineSegment(ref point, ref start, ref end) <= 0.9f)
			{
				return false;
			}
			end = polygon[j];
		}
		return true;
	}

	private bool InPolygon(Vertices polygon, Vector2 point)
	{
		if (DistanceToHullAcceptableHoles(polygon, point, higherDetail: true))
		{
			List<float> list = SearchCrossingEdgesHoles(polygon, (int)point.y);
			if (list.Count > 0 && list.Count % 2 == 0)
			{
				for (int i = 0; i < list.Count; i += 2)
				{
					if (list[i] <= point.x && list[i + 1] >= point.x)
					{
						return true;
					}
				}
			}
			return false;
		}
		return true;
	}

	private Vector2? GetTopMostVertex(Vertices vertices)
	{
		float num = float.MaxValue;
		Vector2? result = null;
		for (int i = 0; i < vertices.Count; i++)
		{
			if (num > vertices[i].y)
			{
				num = vertices[i].y;
				result = vertices[i];
			}
		}
		return result;
	}

	private float GetTopMostCoord(Vertices vertices)
	{
		float num = float.MaxValue;
		for (int i = 0; i < vertices.Count; i++)
		{
			if (num > vertices[i].y)
			{
				num = vertices[i].y;
			}
		}
		return num;
	}

	private float GetBottomMostCoord(Vertices vertices)
	{
		float num = float.MinValue;
		for (int i = 0; i < vertices.Count; i++)
		{
			if (num < vertices[i].y)
			{
				num = vertices[i].y;
			}
		}
		return num;
	}

	private List<float> SearchCrossingEdgesHoles(Vertices polygon, int y)
	{
		if (polygon == null)
		{
			throw new ArgumentNullException("polygon", "'polygon' can't be null.");
		}
		if (polygon.Count < 3)
		{
			throw new ArgumentException("'polygon.MainPolygon.Count' can't be less then 3.");
		}
		List<float> list = SearchCrossingEdges(polygon, y);
		list.Sort();
		return list;
	}

	private List<float> SearchCrossingEdges(Vertices polygon, int y)
	{
		List<float> list = new List<float>();
		if (polygon.Count > 2)
		{
			Vector2 vector = polygon[polygon.Count - 1];
			for (int i = 0; i < polygon.Count; i++)
			{
				Vector2 vector2 = polygon[i];
				if (((vector2.y >= (float)y && vector.y <= (float)y) || (vector2.y <= (float)y && vector.y >= (float)y)) && vector2.y != vector.y)
				{
					bool flag = true;
					Vector2 vector3 = vector - vector2;
					if (vector2.y == (float)y)
					{
						Vector2 vector4 = polygon[(i + 1) % polygon.Count];
						Vector2 vector5 = vector2 - vector4;
						flag = ((!(vector3.y > 0f)) ? (vector5.y >= 0f) : (vector5.y <= 0f));
					}
					if (flag)
					{
						list.Add(((float)y - vector2.y) / vector3.y * vector3.x + vector2.x);
					}
				}
				vector = vector2;
			}
		}
		list.Sort();
		return list;
	}

	private bool SplitPolygonEdge(Vertices polygon, Vector2 coordInsideThePolygon, out int vertex1Index, out int vertex2Index)
	{
		int num = 0;
		int index = 0;
		bool flag = false;
		float num2 = float.MaxValue;
		bool flag2 = false;
		Vector2 point = Vector2.zero;
		List<float> list = SearchCrossingEdges(polygon, (int)coordInsideThePolygon.y);
		vertex1Index = 0;
		vertex2Index = 0;
		point.y = coordInsideThePolygon.y;
		if (list != null && list.Count > 1 && list.Count % 2 == 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] < coordInsideThePolygon.x)
				{
					float num3 = coordInsideThePolygon.x - list[i];
					if (num3 < num2)
					{
						num2 = num3;
						point.x = list[i];
						flag2 = true;
					}
				}
			}
			if (flag2)
			{
				num2 = float.MaxValue;
				int num4 = polygon.Count - 1;
				for (int j = 0; j < polygon.Count; j++)
				{
					Vector2 start = polygon[j];
					Vector2 end = polygon[num4];
					float num3 = LineTools.DistanceBetweenPointAndLineSegment(ref point, ref start, ref end);
					if (num3 < num2)
					{
						num2 = num3;
						num = j;
						index = num4;
						flag = true;
					}
					num4 = j;
				}
				if (flag)
				{
					Vector2 vector = polygon[index] - polygon[num];
					vector.Normalize();
					float num3 = Vector2.Distance(polygon[num], point);
					vertex1Index = num;
					vertex2Index = num + 1;
					polygon.Insert(num, num3 * vector + polygon[vertex1Index]);
					polygon.Insert(num, num3 * vector + polygon[vertex2Index]);
					return true;
				}
			}
		}
		return false;
	}

	private Vertices CreateSimplePolygon(Vector2 entrance, Vector2 last)
	{
		bool flag = false;
		bool flag2 = false;
		Vertices vertices = new Vertices(32);
		Vertices vertices2 = new Vertices(32);
		Vertices vertices3 = new Vertices(32);
		Vector2 current = Vector2.zero;
		if (entrance == Vector2.zero || !InBounds(ref entrance))
		{
			flag = SearchHullEntrance(out entrance);
			if (flag)
			{
				current = new Vector2(entrance.x - 1f, entrance.y);
			}
		}
		else if (IsSolid(ref entrance))
		{
			Vector2 foundPixel;
			if (IsNearPixel(ref entrance, ref last))
			{
				current = last;
				flag = true;
			}
			else if (SearchNearPixels(searchingForSolidPixel: false, ref entrance, out foundPixel))
			{
				current = foundPixel;
				flag = true;
			}
			else
			{
				flag = false;
			}
		}
		if (flag)
		{
			vertices.Add(entrance);
			vertices2.Add(entrance);
			Vector2 next = entrance;
			while (true)
			{
				if (SearchForOutstandingVertex(vertices2, out var outstanding))
				{
					if (flag2)
					{
						if (vertices3.Contains(outstanding))
						{
							vertices.Add(outstanding);
						}
						break;
					}
					vertices.Add(outstanding);
					vertices2.RemoveRange(0, vertices2.IndexOf(outstanding));
				}
				last = current;
				current = next;
				if (!GetNextHullPoint(ref last, ref current, out next))
				{
					break;
				}
				vertices2.Add(next);
				if (next == entrance && !flag2)
				{
					flag2 = true;
					vertices3.AddRange(vertices2);
					if (vertices3.Contains(entrance))
					{
						vertices3.Remove(entrance);
					}
				}
			}
		}
		return vertices;
	}

	private bool SearchNearPixels(bool searchingForSolidPixel, ref Vector2 current, out Vector2 foundPixel)
	{
		for (int i = 0; i < 8; i++)
		{
			int num = (int)current.x + closePixels[i, 0];
			int num2 = (int)current.y + closePixels[i, 1];
			if (!searchingForSolidPixel ^ IsSolid(num, num2))
			{
				foundPixel = new Vector2(num, num2);
				return true;
			}
		}
		foundPixel = Vector2.zero;
		return false;
	}

	private bool IsNearPixel(ref Vector2 current, ref Vector2 near)
	{
		for (int i = 0; i < 8; i++)
		{
			int num = (int)current.x + closePixels[i, 0];
			int num2 = (int)current.y + closePixels[i, 1];
			if (num >= 0 && num <= width && num2 >= 0 && num2 <= height && num == (int)near.x && num2 == (int)near.y)
			{
				return true;
			}
		}
		return false;
	}

	private bool SearchHullEntrance(out Vector2 entrance)
	{
		for (int i = 0; i <= height; i++)
		{
			for (int j = 0; j <= width; j++)
			{
				if (IsSolid(j, i))
				{
					entrance = new Vector2(j, i);
					return true;
				}
			}
		}
		entrance = Vector2.zero;
		return false;
	}

	private bool SearchNextHullEntrance(List<Vertices> detectedPolygons, Vector2 start, out Vector2? entrance)
	{
		bool flag = false;
		bool flag2 = false;
		for (int i = (int)start.x + (int)start.y * width; i <= solidsLength; i++)
		{
			if (IsSolid(i))
			{
				if (!flag)
				{
					continue;
				}
				int num = i % width;
				entrance = new Vector2(num, (float)(i - num) / (float)width);
				flag2 = false;
				for (int j = 0; j < detectedPolygons.Count; j++)
				{
					if (InPolygon(detectedPolygons[j], entrance.Value))
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					return true;
				}
				flag = false;
			}
			else
			{
				flag = true;
			}
		}
		entrance = null;
		return false;
	}

	private bool GetNextHullPoint(ref Vector2 last, ref Vector2 current, out Vector2 next)
	{
		int indexOfFirstPixelToCheck = GetIndexOfFirstPixelToCheck(ref last, ref current);
		for (int i = 0; i < 8; i++)
		{
			int num = (indexOfFirstPixelToCheck + i) % 8;
			int num2 = (int)current.x + closePixels[num, 0];
			int num3 = (int)current.y + closePixels[num, 1];
			if (num2 >= 0 && num2 < width && num3 >= 0 && num3 <= height && IsSolid(num2, num3))
			{
				next = new Vector2(num2, num3);
				return true;
			}
		}
		next = Vector2.zero;
		return false;
	}

	private bool SearchForOutstandingVertex(Vertices hullArea, out Vector2 outstanding)
	{
		Vector2 vector = Vector2.zero;
		bool result = false;
		if (hullArea.Count > 2)
		{
			int num = hullArea.Count - 1;
			Vector2 start = hullArea[0];
			Vector2 end = hullArea[num];
			for (int i = 1; i < num; i++)
			{
				Vector2 point = hullArea[i];
				if (LineTools.DistanceBetweenPointAndLineSegment(ref point, ref start, ref end) >= 0.9f)
				{
					vector = hullArea[i];
					result = true;
					break;
				}
			}
		}
		outstanding = vector;
		return result;
	}

	private int GetIndexOfFirstPixelToCheck(ref Vector2 last, ref Vector2 current)
	{
		switch ((int)(current.x - last.x))
		{
		case 1:
			switch ((int)(current.y - last.y))
			{
			case 1:
				return 1;
			case 0:
				return 0;
			case -1:
				return 7;
			}
			break;
		case 0:
			switch ((int)(current.y - last.y))
			{
			case 1:
				return 2;
			case -1:
				return 6;
			}
			break;
		case -1:
			switch ((int)(current.y - last.y))
			{
			case 1:
				return 3;
			case 0:
				return 4;
			case -1:
				return 5;
			}
			break;
		}
		return 0;
	}
}
