namespace Moserware.Skills.FactorGraphs;

public class ScheduleStep<T> : Schedule<T>
{
	private readonly Factor<T> _Factor;

	private readonly int _Index;

	public ScheduleStep(string name, Factor<T> factor, int index)
		: base(name)
	{
		_Factor = factor;
		_Index = index;
	}

	public override double Visit(int depth, int maxDepth)
	{
		return _Factor.UpdateMessage(_Index);
	}
}
