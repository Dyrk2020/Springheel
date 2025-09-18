using System;

namespace Moserware.Skills.FactorGraphs;

public class DefaultVariable<TValue> : Variable<TValue>
{
	public override TValue Value
	{
		get
		{
			return default(TValue);
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public DefaultVariable()
		: base("Default", default(TValue))
	{
	}
}
