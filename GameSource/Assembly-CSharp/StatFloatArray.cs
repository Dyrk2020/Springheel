using System;

public class StatFloatArray : StatBase
{
	public float[] values;

	public StatFloatArray()
	{
		type = StatType.FloatArray;
	}

	public StatFloatArray(StatFloatArray other)
		: this()
	{
		other.CopyBaseValuesTo(this);
		values = new float[other.values.Length];
		Array.Copy(other.values, values, other.values.Length);
	}

	public override void Reset()
	{
		base.Reset();
		for (int i = 0; i < values.Length; i++)
		{
			values[i] = 0f;
		}
	}

	public void Increment(int index, float amount)
	{
		values[index] += amount;
		dirty = true;
	}

	public void Set(int index, float value)
	{
		values[index] = value;
		dirty = true;
	}
}
