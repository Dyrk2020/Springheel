using System;
using System.Collections.Generic;

namespace Moserware.Skills.FactorGraphs;

public class ScheduleSequence<TValue> : ScheduleSequence<TValue, Schedule<TValue>>
{
	public ScheduleSequence(string name, IEnumerable<Schedule<TValue>> schedules)
		: base(name, schedules)
	{
	}
}
public class ScheduleSequence<TValue, TSchedule> : Schedule<TValue> where TSchedule : Schedule<TValue>
{
	private readonly IEnumerable<TSchedule> _Schedules;

	public ScheduleSequence(string name, IEnumerable<TSchedule> schedules)
		: base(name)
	{
		_Schedules = schedules;
	}

	public override double Visit(int depth, int maxDepth)
	{
		double num = 0.0;
		foreach (TSchedule schedule in _Schedules)
		{
			num = Math.Max(schedule.Visit(depth + 1, maxDepth), num);
		}
		return num;
	}
}
