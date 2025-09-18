using System;
using System.Collections.Generic;
using System.Linq;
using Moserware.Numerics;
using Moserware.Skills.FactorGraphs;
using Moserware.Skills.TrueSkill.Factors;

namespace Moserware.Skills.TrueSkill.Layers;

internal class IteratedTeamDifferencesInnerLayer<TPlayer> : TrueSkillFactorGraphLayer<TPlayer, Variable<GaussianDistribution>, GaussianWeightedSumFactor, Variable<GaussianDistribution>>
{
	private readonly TeamDifferencesComparisonLayer<TPlayer> _TeamDifferencesComparisonLayer;

	private readonly TeamPerformancesToTeamPerformanceDifferencesLayer<TPlayer> _TeamPerformancesToTeamPerformanceDifferencesLayer;

	public override IEnumerable<Factor<GaussianDistribution>> UntypedFactors => _TeamPerformancesToTeamPerformanceDifferencesLayer.UntypedFactors.Concat(_TeamDifferencesComparisonLayer.UntypedFactors);

	public IteratedTeamDifferencesInnerLayer(TrueSkillFactorGraph<TPlayer> parentGraph, TeamPerformancesToTeamPerformanceDifferencesLayer<TPlayer> teamPerformancesToPerformanceDifferences, TeamDifferencesComparisonLayer<TPlayer> teamDifferencesComparisonLayer)
		: base(parentGraph)
	{
		_TeamPerformancesToTeamPerformanceDifferencesLayer = teamPerformancesToPerformanceDifferences;
		_TeamDifferencesComparisonLayer = teamDifferencesComparisonLayer;
	}

	public override void BuildLayer()
	{
		_TeamPerformancesToTeamPerformanceDifferencesLayer.SetRawInputVariablesGroups(base.InputVariablesGroups);
		_TeamPerformancesToTeamPerformanceDifferencesLayer.BuildLayer();
		_TeamDifferencesComparisonLayer.SetRawInputVariablesGroups(_TeamPerformancesToTeamPerformanceDifferencesLayer.GetRawOutputVariablesGroups());
		_TeamDifferencesComparisonLayer.BuildLayer();
	}

	public override Schedule<GaussianDistribution> CreatePriorSchedule()
	{
		Schedule<GaussianDistribution> schedule = null;
		switch (base.InputVariablesGroups.Count)
		{
		case 0:
		case 1:
			throw new InvalidOperationException();
		case 2:
			schedule = CreateTwoTeamInnerPriorLoopSchedule();
			break;
		default:
			schedule = CreateMultipleTeamInnerPriorLoopSchedule();
			break;
		}
		int count = _TeamPerformancesToTeamPerformanceDifferencesLayer.LocalFactors.Count;
		return new ScheduleSequence<GaussianDistribution>("inner schedule", new Schedule<GaussianDistribution>[3]
		{
			schedule,
			new ScheduleStep<GaussianDistribution>("teamPerformanceToPerformanceDifferenceFactors[0] @ 1", _TeamPerformancesToTeamPerformanceDifferencesLayer.LocalFactors[0], 1),
			new ScheduleStep<GaussianDistribution>($"teamPerformanceToPerformanceDifferenceFactors[teamTeamDifferences = {count} - 1] @ 2", _TeamPerformancesToTeamPerformanceDifferencesLayer.LocalFactors[count - 1], 2)
		});
	}

	private Schedule<GaussianDistribution> CreateTwoTeamInnerPriorLoopSchedule()
	{
		return ScheduleSequence(new ScheduleStep<GaussianDistribution>[2]
		{
			new ScheduleStep<GaussianDistribution>("send team perf to perf differences", _TeamPerformancesToTeamPerformanceDifferencesLayer.LocalFactors[0], 0),
			new ScheduleStep<GaussianDistribution>("send to greater than or within factor", _TeamDifferencesComparisonLayer.LocalFactors[0], 0)
		}, "loop of just two teams inner sequence");
	}

	private Schedule<GaussianDistribution> CreateMultipleTeamInnerPriorLoopSchedule()
	{
		int count = _TeamPerformancesToTeamPerformanceDifferencesLayer.LocalFactors.Count;
		List<Schedule<GaussianDistribution>> list = new List<Schedule<GaussianDistribution>>();
		for (int i = 0; i < count - 1; i++)
		{
			Schedule<GaussianDistribution> item = ScheduleSequence(new Schedule<GaussianDistribution>[3]
			{
				new ScheduleStep<GaussianDistribution>($"team perf to perf diff {i}", _TeamPerformancesToTeamPerformanceDifferencesLayer.LocalFactors[i], 0),
				new ScheduleStep<GaussianDistribution>($"greater than or within result factor {i}", _TeamDifferencesComparisonLayer.LocalFactors[i], 0),
				new ScheduleStep<GaussianDistribution>($"team perf to perf diff factors [{i}], 2", _TeamPerformancesToTeamPerformanceDifferencesLayer.LocalFactors[i], 2)
			}, "current forward schedule piece {0}", i);
			list.Add(item);
		}
		ScheduleSequence<GaussianDistribution> scheduleSequence = new ScheduleSequence<GaussianDistribution>("forward schedule", list);
		List<Schedule<GaussianDistribution>> list2 = new List<Schedule<GaussianDistribution>>();
		for (int j = 0; j < count - 1; j++)
		{
			ScheduleSequence<GaussianDistribution> item2 = new ScheduleSequence<GaussianDistribution>("current backward schedule piece", new Schedule<GaussianDistribution>[3]
			{
				new ScheduleStep<GaussianDistribution>($"teamPerformanceToPerformanceDifferenceFactors[totalTeamDifferences - 1 - {j}] @ 0", _TeamPerformancesToTeamPerformanceDifferencesLayer.LocalFactors[count - 1 - j], 0),
				new ScheduleStep<GaussianDistribution>($"greaterThanOrWithinResultFactors[totalTeamDifferences - 1 - {j}] @ 0", _TeamDifferencesComparisonLayer.LocalFactors[count - 1 - j], 0),
				new ScheduleStep<GaussianDistribution>($"teamPerformanceToPerformanceDifferenceFactors[totalTeamDifferences - 1 - {j}] @ 1", _TeamPerformancesToTeamPerformanceDifferencesLayer.LocalFactors[count - 1 - j], 1)
			});
			list2.Add(item2);
		}
		ScheduleSequence<GaussianDistribution> scheduleSequence2 = new ScheduleSequence<GaussianDistribution>("backward schedule", list2);
		ScheduleSequence<GaussianDistribution> scheduleToLoop = new ScheduleSequence<GaussianDistribution>("forward Backward Schedule To Loop", new Schedule<GaussianDistribution>[2] { scheduleSequence, scheduleSequence2 });
		return new ScheduleLoop<GaussianDistribution>($"loop with max delta of {0.0001}", scheduleToLoop, 0.0001);
	}
}
