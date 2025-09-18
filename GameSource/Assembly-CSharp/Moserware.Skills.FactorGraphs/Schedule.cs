namespace Moserware.Skills.FactorGraphs;

public abstract class Schedule<T>
{
	private readonly string _Name;

	protected Schedule(string name)
	{
		_Name = name;
	}

	public abstract double Visit(int depth, int maxDepth);

	public double Visit()
	{
		return Visit(-1, 0);
	}

	public override string ToString()
	{
		return _Name;
	}
}
