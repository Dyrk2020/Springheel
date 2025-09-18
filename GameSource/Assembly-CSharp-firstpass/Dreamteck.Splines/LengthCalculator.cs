using System;
using UnityEngine;
using UnityEngine.Events;

namespace Dreamteck.Splines;

[AddComponentMenu("Dreamteck/Splines/Users/Length Calculator")]
public class LengthCalculator : SplineUser
{
	[Serializable]
	public class LengthEvent
	{
		public enum Type
		{
			Growing,
			Shrinking,
			Both
		}

		public bool enabled = true;

		public float targetLength;

		public UnityEvent onChange = new UnityEvent();

		public Type type = Type.Both;

		public LengthEvent()
		{
		}

		public LengthEvent(Type t)
		{
			type = t;
		}

		public void Check(float fromLength, float toLength)
		{
			if (enabled)
			{
				bool flag = false;
				switch (type)
				{
				case Type.Growing:
					flag = toLength >= targetLength && fromLength < targetLength;
					break;
				case Type.Shrinking:
					flag = toLength <= targetLength && fromLength > targetLength;
					break;
				case Type.Both:
					flag = (toLength >= targetLength && fromLength < targetLength) || (toLength <= targetLength && fromLength > targetLength);
					break;
				}
				if (flag)
				{
					onChange.Invoke();
				}
			}
		}
	}

	[HideInInspector]
	public LengthEvent[] lengthEvents = new LengthEvent[0];

	[HideInInspector]
	public float idealLength = 1f;

	private float _length;

	private float lastLength;

	public float length => _length;

	protected override void Awake()
	{
		base.Awake();
		_length = CalculateLength();
		lastLength = _length;
		for (int i = 0; i < lengthEvents.Length; i++)
		{
			if (lengthEvents[i].targetLength == _length)
			{
				lengthEvents[i].onChange.Invoke();
			}
		}
	}

	protected override void Build()
	{
		base.Build();
		_length = CalculateLength();
		if (lastLength != _length)
		{
			for (int i = 0; i < lengthEvents.Length; i++)
			{
				lengthEvents[i].Check(lastLength, _length);
			}
			lastLength = _length;
		}
	}

	private void AddEvent(LengthEvent lengthEvent)
	{
		LengthEvent[] array = new LengthEvent[lengthEvents.Length + 1];
		lengthEvents.CopyTo(array, 0);
		array[^1] = lengthEvent;
		lengthEvents = array;
	}
}
