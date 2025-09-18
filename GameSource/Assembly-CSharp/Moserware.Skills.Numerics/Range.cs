using System;

namespace Moserware.Skills.Numerics;

public abstract class Range<T> where T : Range<T>, new()
{
	private static readonly T _Instance = new T();

	public int Min { get; private set; }

	public int Max { get; private set; }

	protected Range(int min, int max)
	{
		if (min > max)
		{
			throw new ArgumentOutOfRangeException();
		}
		Min = min;
		Max = max;
	}

	protected abstract T Create(int min, int max);

	public static T Inclusive(int min, int max)
	{
		return _Instance.Create(min, max);
	}

	public static T Exactly(int value)
	{
		return _Instance.Create(value, value);
	}

	public static T AtLeast(int minimumValue)
	{
		return _Instance.Create(minimumValue, int.MaxValue);
	}

	public bool IsInRange(int value)
	{
		if (Min <= value)
		{
			return value <= Max;
		}
		return false;
	}
}
