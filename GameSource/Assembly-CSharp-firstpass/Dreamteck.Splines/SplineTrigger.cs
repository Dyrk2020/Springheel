using System;
using UnityEngine;
using UnityEngine.Events;

namespace Dreamteck.Splines;

[Serializable]
public class SplineTrigger
{
	public enum Type
	{
		Double,
		Forward,
		Backward
	}

	public string name = "Trigger";

	[SerializeField]
	public Type type;

	public bool workOnce;

	private bool worked;

	[Range(0f, 1f)]
	public double position = 0.5;

	[SerializeField]
	public bool enabled = true;

	[SerializeField]
	public Color color = Color.white;

	[HideInInspector]
	[SerializeField]
	public UnityEvent onCross = new UnityEvent();

	public event Action<SplineUser> onUserCross;

	public SplineTrigger(Type t)
	{
		type = t;
		enabled = true;
		onCross = new UnityEvent();
	}

	public void AddListener(UnityAction action)
	{
		onCross.AddListener(action);
	}

	public void Reset()
	{
		worked = false;
	}

	public bool Check(double previousPercent, double currentPercent)
	{
		if (!enabled)
		{
			return false;
		}
		if (workOnce && worked)
		{
			return false;
		}
		bool flag = false;
		switch (type)
		{
		case Type.Double:
			flag = (previousPercent <= position && currentPercent >= position) || (currentPercent <= position && previousPercent >= position);
			break;
		case Type.Forward:
			flag = previousPercent <= position && currentPercent >= position;
			break;
		case Type.Backward:
			flag = currentPercent <= position && previousPercent >= position;
			break;
		}
		if (flag)
		{
			worked = true;
		}
		return flag;
	}

	public void Invoke(SplineUser user = null)
	{
		onCross.Invoke();
		if ((bool)user && this.onUserCross != null)
		{
			this.onUserCross(user);
		}
	}
}
