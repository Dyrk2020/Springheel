public class StatBool : StatBase
{
	public bool value;

	public StatBool()
	{
		type = StatType.Bool;
	}

	public StatBool(StatBool other)
		: this()
	{
		other.CopyBaseValuesTo(this);
		value = other.value;
	}

	public override void Reset()
	{
		base.Reset();
		value = false;
	}

	public void Set(bool value)
	{
		this.value = value;
		dirty = true;
	}
}
