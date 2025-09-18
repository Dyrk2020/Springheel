using System;
using UnityEngine;

namespace Dreamteck.Splines;

[Serializable]
public class TriggerGroup
{
	public bool enabled = true;

	public string name = "";

	public Color color = Color.white;

	public SplineTrigger[] triggers = new SplineTrigger[0];

	public void Check(double start, double end, SplineUser user = null)
	{
		for (int i = 0; i < triggers.Length; i++)
		{
			if (triggers[i] != null && triggers[i].Check(start, end))
			{
				triggers[i].Invoke(user);
			}
		}
	}

	public void Reset()
	{
		for (int i = 0; i < triggers.Length; i++)
		{
			triggers[i].Reset();
		}
	}
}
