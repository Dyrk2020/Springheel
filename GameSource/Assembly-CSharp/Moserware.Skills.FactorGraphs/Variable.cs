namespace Moserware.Skills.FactorGraphs;

public class Variable<TValue>
{
	private readonly string _Name;

	private readonly TValue _Prior;

	public virtual TValue Value { get; set; }

	public Variable(string name, TValue prior)
	{
		_Name = "Variable[" + name + "]";
		_Prior = prior;
		ResetToPrior();
	}

	public void ResetToPrior()
	{
		Value = _Prior;
	}

	public override string ToString()
	{
		return _Name;
	}
}
