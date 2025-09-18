namespace Moserware.Skills.FactorGraphs;

public class KeyedVariable<TKey, TValue> : Variable<TValue>
{
	public TKey Key { get; private set; }

	public KeyedVariable(TKey key, string name, TValue prior)
		: base(name, prior)
	{
		Key = key;
	}
}
