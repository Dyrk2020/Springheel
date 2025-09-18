using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamteck.Splines;

[Serializable]
public class MeshScaleModifier : SplineSampleModifier
{
	[Serializable]
	public class ScaleKey : Key
	{
		public Vector2 scale = Vector2.one;

		public ScaleKey(double f, double t, MeshScaleModifier modifier)
			: base(f, t, modifier)
		{
		}
	}

	public List<ScaleKey> keys = new List<ScaleKey>();

	public MeshScaleModifier()
	{
		keys = new List<ScaleKey>();
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
		keys = new List<ScaleKey>();
		for (int i = 0; i < input.Count; i++)
		{
			input[i].modifier = this;
			keys.Add((ScaleKey)input[i]);
		}
	}

	public void AddKey(double f, double t)
	{
		keys.Add(new ScaleKey(f, t, this));
	}

	public override void Apply(SplineSample result)
	{
		if (keys.Count != 0)
		{
			for (int i = 0; i < keys.Count; i++)
			{
				result.size += keys[i].Evaluate(result.percent) * keys[i].scale.magnitude;
			}
		}
	}

	public Vector2 GetScale(SplineSample sample)
	{
		Vector2 one = Vector2.one;
		for (int i = 0; i < keys.Count; i++)
		{
			float t = keys[i].Evaluate(sample.percent);
			Vector2 vector = Vector2.Lerp(Vector2.one, keys[i].scale, t);
			one.x *= vector.x;
			one.y *= vector.y;
		}
		return one;
	}
}
