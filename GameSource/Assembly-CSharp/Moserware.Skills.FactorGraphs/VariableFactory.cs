using System;

namespace Moserware.Skills.FactorGraphs;

public class VariableFactory<TValue>
{
	private readonly Func<TValue> _VariablePriorInitializer;

	public VariableFactory(Func<TValue> variablePriorInitializer)
	{
		_VariablePriorInitializer = variablePriorInitializer;
	}

	public Variable<TValue> CreateBasicVariable(string nameFormat, params object[] args)
	{
		return new Variable<TValue>(string.Format(nameFormat, args), _VariablePriorInitializer());
	}

	public KeyedVariable<TKey, TValue> CreateKeyedVariable<TKey>(TKey key, string nameFormat, params object[] args)
	{
		return new KeyedVariable<TKey, TValue>(key, string.Format(nameFormat, args), _VariablePriorInitializer());
	}
}
