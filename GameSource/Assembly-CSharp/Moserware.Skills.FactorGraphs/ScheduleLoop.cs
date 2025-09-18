namespace Moserware.Skills.FactorGraphs;

public class ScheduleLoop<T> : Schedule<T>
{
	private readonly double _MaxDelta;

	private readonly Schedule<T> _ScheduleToLoop;

	public ScheduleLoop(string name, Schedule<T> scheduleToLoop, double maxDelta)
		: base(name)
	{
		_ScheduleToLoop = scheduleToLoop;
		_MaxDelta = maxDelta;
	}

	public override double Visit(int depth, int maxDepth)
	{
		int num = 1;
		double num2 = _ScheduleToLoop.Visit(depth + 1, maxDepth);
		while (num2 > _MaxDelta)
		{
			num2 = _ScheduleToLoop.Visit(depth + 1, maxDepth);
			num++;
		}
		return num2;
	}
}
