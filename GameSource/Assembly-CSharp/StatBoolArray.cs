using System;

public class StatBoolArray : StatBase
{
	public bool[] values;

	public StatBoolArray()
	{
		type = StatType.BoolArray;
	}

	public StatBoolArray(StatBoolArray other)
		: this()
	{
		other.CopyBaseValuesTo(this);
		values = new bool[other.values.Length];
		Array.Copy(other.values, values, other.values.Length);
	}

	public override void Reset()
	{
		base.Reset();
		for (int i = 0; i < values.Length; i++)
		{
			values[i] = false;
		}
	}

	public void Set(int index, bool value)
	{
		values[index] = value;
		dirty = true;
	}
}
