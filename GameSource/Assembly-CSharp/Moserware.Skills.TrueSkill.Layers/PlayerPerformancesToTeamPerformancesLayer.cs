using System.Collections.Generic;
using System.Linq;
using Moserware.Numerics;
using Moserware.Skills.FactorGraphs;
using Moserware.Skills.TrueSkill.Factors;

namespace Moserware.Skills.TrueSkill.Layers;

internal class PlayerPerformancesToTeamPerformancesLayer<TPlayer> : TrueSkillFactorGraphLayer<TPlayer, KeyedVariable<TPlayer, GaussianDistribution>, GaussianWeightedSumFactor, Variable<GaussianDistribution>>
{
	public PlayerPerformancesToTeamPerformancesLayer(TrueSkillFactorGraph<TPlayer> parentGraph)
		: base(parentGraph)
	{
	}

	public override void BuildLayer()
	{
		foreach (IList<KeyedVariable<TPlayer, GaussianDistribution>> inputVariablesGroup in base.InputVariablesGroups)
		{
			Variable<GaussianDistribution> variable = CreateOutputVariable(inputVariablesGroup);
			AddLayerFactor(CreatePlayerToTeamSumFactor(inputVariablesGroup, variable));
			base.OutputVariablesGroups.Add(new Variable<GaussianDistribution>[1] { variable });
		}
	}

	public override Schedule<GaussianDistribution> CreatePriorSchedule()
	{
		return ScheduleSequence(base.LocalFactors.Select((GaussianWeightedSumFactor weightedSumFactor) => new ScheduleStep<GaussianDistribution>("Perf to Team Perf Step", weightedSumFactor, 0)), "all player perf to team perf schedule");
	}

	protected GaussianWeightedSumFactor CreatePlayerToTeamSumFactor(IList<KeyedVariable<TPlayer, GaussianDistribution>> teamMembers, Variable<GaussianDistribution> sumVariable)
	{
		double[] array = new double[teamMembers.Count];
		for (int i = 0; i != teamMembers.Count; i++)
		{
			array[i] = PartialPlay.GetPartialPlayPercentage(teamMembers[i]);
		}
		Variable<GaussianDistribution>[] variablesToSum = teamMembers.ToArray();
		return new GaussianWeightedSumFactor(sumVariable, variablesToSum, array);
	}

	public override Schedule<GaussianDistribution> CreatePosteriorSchedule()
	{
		return ScheduleSequence(base.LocalFactors.SelectMany((GaussianWeightedSumFactor currentFactor) => Enumerable.Range(1, currentFactor.NumberOfMessages - 1), delegate(GaussianWeightedSumFactor currentFactor, int currentIteration)
		{
			int num = currentIteration;
			return new ScheduleStep<GaussianDistribution>("team sum perf @" + num, currentFactor, currentIteration);
		}), "all of the team's sum iterations");
	}

	private Variable<GaussianDistribution> CreateOutputVariable(IList<KeyedVariable<TPlayer, GaussianDistribution>> team)
	{
		string text = string.Join(", ", team.Select((KeyedVariable<TPlayer, GaussianDistribution> teamMember) => teamMember.Key.ToString()).ToArray());
		return base.ParentFactorGraph.VariableFactory.CreateBasicVariable("Team[{0}]'s performance", text);
	}
}
