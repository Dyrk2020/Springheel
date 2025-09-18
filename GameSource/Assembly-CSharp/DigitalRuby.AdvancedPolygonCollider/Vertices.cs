using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DigitalRuby.AdvancedPolygonCollider;

public class Vertices : List<Vector2>
{
	public Vertices()
	{
	}

	public Vertices(int capacity)
		: base(capacity)
	{
	}

	public Vertices(IEnumerable<Vector2> vertices)
	{
		AddRange(vertices);
	}

	public int NextIndex(int index)
	{
		if (index + 1 <= base.Count - 1)
		{
			return index + 1;
		}
		return 0;
	}

	public Vector2 NextVertex(int index)
	{
		return base[NextIndex(index)];
	}

	public int PreviousIndex(int index)
	{
		if (index - 1 >= 0)
		{
			return index - 1;
		}
		return base.Count - 1;
	}

	public Vector2 PreviousVertex(int index)
	{
		return base[PreviousIndex(index)];
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < base.Count; i++)
		{
			stringBuilder.Append(base[i].ToString());
			if (i < base.Count - 1)
			{
				stringBuilder.Append(" ");
			}
		}
		return stringBuilder.ToString();
	}
}
