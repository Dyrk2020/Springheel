public class StatCount : StatBase
{
	public int count;

	public StatCount()
	{
		type = StatType.Count;
	}

	public StatCount(StatCount other)
		: this()
	{
		other.CopyBaseValuesTo(this);
		count = other.count;
	}

	public override void Reset()
	{
		base.Reset();
		count = 0;
	}

	public void Increment(int amount)
	{
		count += amount;
		dirty = true;
	}

	public void Set(int value)
	{
		count = value;
		dirty = true;
	}
}
