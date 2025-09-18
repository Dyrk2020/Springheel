public class StatFloat : StatBase
{
	public float value;

	public StatFloat()
	{
		type = StatType.Float;
	}

	public StatFloat(StatFloat other)
		: this()
	{
		other.CopyBaseValuesTo(this);
		value = other.value;
	}

	public override void Reset()
	{
		base.Reset();
		value = 0f;
	}

	public void Increment(float amount)
	{
		value += amount;
		dirty = true;
	}

	public void Set(float value)
	{
		this.value = value;
		dirty = true;
	}
}
