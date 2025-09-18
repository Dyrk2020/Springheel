public class StatBase
{
	public enum StatType
	{
		Unknown,
		Count,
		CountArray,
		Float,
		FloatArray,
		Bool,
		BoolArray
	}

	public string name;

	public bool dirty;

	public StatType type;

	public virtual void Reset()
	{
		dirty = false;
	}

	public void CopyBaseValuesTo(StatBase other)
	{
		other.dirty = dirty;
		other.name = name;
		other.type = type;
	}

	public StatBase Clone()
	{
		return Clone<StatBase>();
	}

	public T Clone<T>() where T : StatBase
	{
		StatBase statBase = null;
		switch (type)
		{
		case StatType.Count:
			statBase = new StatCount((StatCount)this);
			break;
		case StatType.CountArray:
			statBase = new StatCountArray((StatCountArray)this);
			break;
		case StatType.Float:
			statBase = new StatFloat((StatFloat)this);
			break;
		case StatType.FloatArray:
			statBase = new StatFloatArray((StatFloatArray)this);
			break;
		case StatType.Bool:
			statBase = new StatBool((StatBool)this);
			break;
		case StatType.BoolArray:
			statBase = new StatBoolArray((StatBoolArray)this);
			break;
		}
		return (T)statBase;
	}
}
