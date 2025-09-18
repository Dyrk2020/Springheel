using System;
using System.Collections.Generic;
using System.Linq;

namespace Moserware.Skills.FactorGraphs;

public abstract class FactorGraphLayer<TParentGraph, TValue, TBaseVariable, TInputVariable, TFactor, TOutputVariable> : FactorGraphLayerBase<TValue> where TParentGraph : FactorGraph<TParentGraph, TValue, TBaseVariable> where TBaseVariable : Variable<TValue> where TInputVariable : TBaseVariable where TFactor : Factor<TValue> where TOutputVariable : TBaseVariable
{
	private readonly List<TFactor> _LocalFactors = new List<TFactor>();

	private readonly List<IList<TOutputVariable>> _OutputVariablesGroups = new List<IList<TOutputVariable>>();

	private IList<IList<TInputVariable>> _InputVariablesGroups = new List<IList<TInputVariable>>();

	protected IList<IList<TInputVariable>> InputVariablesGroups => _InputVariablesGroups;

	public TParentGraph ParentFactorGraph { get; private set; }

	public IList<IList<TOutputVariable>> OutputVariablesGroups => _OutputVariablesGroups;

	public IList<TFactor> LocalFactors => _LocalFactors;

	public override IEnumerable<Factor<TValue>> UntypedFactors => _LocalFactors.Cast<Factor<TValue>>();

	protected FactorGraphLayer(TParentGraph parentGraph)
	{
		ParentFactorGraph = parentGraph;
	}

	public override void SetRawInputVariablesGroups(object value)
	{
		if (!(value is IList<IList<TInputVariable>> inputVariablesGroups))
		{
			throw new ArgumentException();
		}
		_InputVariablesGroups = inputVariablesGroups;
	}

	public override object GetRawOutputVariablesGroups()
	{
		return _OutputVariablesGroups;
	}

	protected Schedule<TValue> ScheduleSequence<TSchedule>(IEnumerable<TSchedule> itemsToSequence, string nameFormat, params object[] args) where TSchedule : Schedule<TValue>
	{
		return new ScheduleSequence<TValue, TSchedule>(string.Format(nameFormat, args), itemsToSequence);
	}

	protected void AddLayerFactor(TFactor factor)
	{
		_LocalFactors.Add(factor);
	}

	protected double Square(double x)
	{
		return x * x;
	}
}
