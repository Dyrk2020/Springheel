using System;

public class StatCountArray : StatBase
{
	public int[] values;

	public StatCountArray()
	{
		type = StatType.CountArray;
	}

	public StatCountArray(StatCountArray other)
		: this()
	{
		other.CopyBaseValuesTo(this);
		values = new int[other.values.Length];
		Array.Copy(other.values, values, other.values.Length);
	}

	public override void Reset()
	{
		base.Reset();
		for (int i = 0; i < values.Length; i++)
		{
			values[i] = 0;
		}
	}

	public void Increment(int index, int amount)
	{
		values[index] += amount;
		dirty = true;
	}

	public void OrValue(int index, int valueToOr)
	{
		values[index] |= valueToOr;
		dirty = true;
	}

	public void Set(int index, int value)
	{
		values[index] = value;
		dirty = true;
	}
}
