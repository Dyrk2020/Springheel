using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamteck.Splines;

[Serializable]
public class RotationModifier : SplineSampleModifier
{
	[Serializable]
	public class RotationKey : Key
	{
		public bool useLookTarget;

		public Transform target;

		public Vector3 rotation = Vector3.zero;

		public RotationKey(Vector3 rotation, double f, double t, RotationModifier modifier)
			: base(f, t, modifier)
		{
			this.rotation = rotation;
		}
	}

	public List<RotationKey> keys = new List<RotationKey>();

	public RotationModifier()
	{
		keys = new List<RotationKey>();
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
		keys = new List<RotationKey>();
		for (int i = 0; i < input.Count; i++)
		{
			keys.Add((RotationKey)input[i]);
		}
		base.SetKeys(input);
	}

	public void AddKey(Vector3 rotation, double f, double t)
	{
		keys.Add(new RotationKey(rotation, f, t, this));
	}

	public override void Apply(SplineSample result)
	{
		if (keys.Count == 0)
		{
			return;
		}
		base.Apply(result);
		Quaternion quaternion = Quaternion.identity;
		Quaternion quaternion2 = result.rotation;
		for (int i = 0; i < keys.Count; i++)
		{
			if (keys[i].useLookTarget && keys[i].target != null)
			{
				Quaternion b = Quaternion.LookRotation(keys[i].target.position - result.position);
				quaternion2 = Quaternion.Slerp(quaternion2, b, keys[i].Evaluate(result.percent));
			}
			else
			{
				Quaternion quaternion3 = Quaternion.Euler(keys[i].rotation.x, keys[i].rotation.y, keys[i].rotation.z);
				quaternion = Quaternion.Slerp(quaternion, quaternion * quaternion3, keys[i].Evaluate(result.percent));
			}
		}
		Quaternion quaternion4 = quaternion2 * quaternion;
		Vector3 vector = Quaternion.Inverse(result.rotation) * result.up;
		result.forward = quaternion4 * Vector3.forward;
		result.up = quaternion4 * vector;
	}
}
