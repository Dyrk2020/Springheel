using System.Collections.Generic;

namespace Moserware.Skills.FactorGraphs;

public abstract class FactorGraphLayerBase<TValue>
{
	public abstract IEnumerable<Factor<TValue>> UntypedFactors { get; }

	public abstract void BuildLayer();

	public virtual Schedule<TValue> CreatePriorSchedule()
	{
		return null;
	}

	public virtual Schedule<TValue> CreatePosteriorSchedule()
	{
		return null;
	}

	public abstract void SetRawInputVariablesGroups(object value);

	public abstract object GetRawOutputVariablesGroups();
}
