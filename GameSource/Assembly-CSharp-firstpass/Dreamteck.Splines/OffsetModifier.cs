using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamteck.Splines;

[Serializable]
public class OffsetModifier : SplineSampleModifier
{
	[Serializable]
	public class OffsetKey : Key
	{
		public Vector2 offset = Vector2.zero;

		public OffsetKey(Vector2 o, double f, double t, OffsetModifier modifier)
			: base(f, t, modifier)
		{
			offset = o;
		}
	}

	public List<OffsetKey> keys = new List<OffsetKey>();

	public OffsetModifier()
	{
		keys = new List<OffsetKey>();
	}

	public override List<Key> GetKeys()
	{
		List<Key> list = new List<Key>();
		for (int i = 0; i < keys.Count; i++)
		{
			list.Add(keys[i]);
		}
		return list;
	}

	public override void SetKeys(List<Key> input)
	{
		keys = new List<OffsetKey>();
		for (int i = 0; i < input.Count; i++)
		{
			keys.Add((OffsetKey)input[i]);
		}
		base.SetKeys(input);
	}

	public void AddKey(Vector2 offset, double f, double t)
	{
		keys.Add(new OffsetKey(offset, f, t, this));
	}

	public override void Apply(SplineSample result)
	{
		if (keys.Count != 0)
		{
			base.Apply(result);
			Vector2 vector = Evaluate(result.percent);
			result.position += result.right * vector.x + result.up * vector.y;
		}
	}

	private Vector2 Evaluate(double time)
	{
		if (keys.Count == 0)
		{
			return Vector2.zero;
		}
		Vector2 zero = Vector2.zero;
		for (int i = 0; i < keys.Count; i++)
		{
			zero += keys[i].offset * keys[i].Evaluate(time);
		}
		return zero * blend;
	}
}
