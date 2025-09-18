using System;
using System.Collections.Generic;

namespace Dreamteck.Splines;

[Serializable]
public class FollowerSpeedModifier : SplineSampleModifier
{
	[Serializable]
	public class SpeedKey : Key
	{
		public float speed;

		public SpeedKey(double f, double t, FollowerSpeedModifier modifier)
			: base(f, t, modifier)
		{
		}
	}

	public List<SpeedKey> keys = new List<SpeedKey>();

	public FollowerSpeedModifier()
	{
		keys = new List<SpeedKey>();
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
		keys = new List<SpeedKey>();
		for (int i = 0; i < input.Count; i++)
		{
			input[i].modifier = this;
			keys.Add((SpeedKey)input[i]);
		}
	}

	public void AddKey(double f, double t)
	{
		keys.Add(new SpeedKey(f, t, this));
	}

	public override void Apply(SplineSample result)
	{
	}

	public float GetSpeed(SplineSample sample)
	{
		float num = 0f;
		for (int i = 0; i < keys.Count; i++)
		{
			float num2 = keys[i].Evaluate(sample.percent);
			num += keys[i].speed * num2;
		}
		return num;
	}
}
