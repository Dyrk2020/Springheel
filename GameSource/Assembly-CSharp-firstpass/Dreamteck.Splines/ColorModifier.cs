using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamteck.Splines;

[Serializable]
public class ColorModifier : SplineSampleModifier
{
	[Serializable]
	public class ColorKey : Key
	{
		public enum BlendMode
		{
			Lerp,
			Multiply,
			Add,
			Subtract
		}

		public Color color = Color.white;

		public BlendMode blendMode;

		public ColorKey(double f, double t, ColorModifier modifier)
			: base(f, t, modifier)
		{
		}

		public Color Blend(Color input, float percent)
		{
			return blendMode switch
			{
				BlendMode.Lerp => Color.Lerp(input, color, blend * percent), 
				BlendMode.Add => input + color * blend * percent, 
				BlendMode.Subtract => input - color * blend * percent, 
				BlendMode.Multiply => Color.Lerp(input, input * color, blend * percent), 
				_ => input, 
			};
		}
	}

	public List<ColorKey> keys = new List<ColorKey>();

	public ColorModifier()
	{
		keys = new List<ColorKey>();
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
		keys = new List<ColorKey>();
		for (int i = 0; i < input.Count; i++)
		{
			keys.Add((ColorKey)input[i]);
		}
		base.SetKeys(input);
	}

	public void AddKey(double f, double t)
	{
		keys.Add(new ColorKey(f, t, this));
	}

	public override void Apply(SplineSample result)
	{
		if (keys.Count != 0)
		{
			base.Apply(result);
			for (int i = 0; i < keys.Count; i++)
			{
				result.color = keys[i].Blend(result.color, keys[i].Evaluate(result.percent));
			}
		}
	}
}
