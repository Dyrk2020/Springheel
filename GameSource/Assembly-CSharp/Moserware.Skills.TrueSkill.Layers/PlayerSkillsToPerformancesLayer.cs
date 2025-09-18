using System.Collections.Generic;
using System.Linq;
using Moserware.Numerics;
using Moserware.Skills.FactorGraphs;
using Moserware.Skills.TrueSkill.Factors;

namespace Moserware.Skills.TrueSkill.Layers;

internal class PlayerSkillsToPerformancesLayer<TPlayer> : TrueSkillFactorGraphLayer<TPlayer, KeyedVariable<TPlayer, GaussianDistribution>, GaussianLikelihoodFactor, KeyedVariable<TPlayer, GaussianDistribution>>
{
	public PlayerSkillsToPerformancesLayer(TrueSkillFactorGraph<TPlayer> parentGraph)
		: base(parentGraph)
	{
	}

	public override void BuildLayer()
	{
		foreach (IList<KeyedVariable<TPlayer, GaussianDistribution>> inputVariablesGroup in base.InputVariablesGroups)
		{
			List<KeyedVariable<TPlayer, GaussianDistribution>> list = new List<KeyedVariable<TPlayer, GaussianDistribution>>();
			foreach (KeyedVariable<TPlayer, GaussianDistribution> item in inputVariablesGroup)
			{
				KeyedVariable<TPlayer, GaussianDistribution> keyedVariable = CreateOutputVariable(item.Key);
				AddLayerFactor(CreateLikelihood(item, keyedVariable));
				list.Add(keyedVariable);
			}
			base.OutputVariablesGroups.Add(list);
		}
	}

	private GaussianLikelihoodFactor CreateLikelihood(KeyedVariable<TPlayer, GaussianDistribution> playerSkill, KeyedVariable<TPlayer, GaussianDistribution> playerPerformance)
	{
		return new GaussianLikelihoodFactor(Square(base.ParentFactorGraph.GameInfo.Beta), playerPerformance, playerSkill);
	}

	private KeyedVariable<TPlayer, GaussianDistribution> CreateOutputVariable(TPlayer key)
	{
		return base.ParentFactorGraph.VariableFactory.CreateKeyedVariable(key, "{0}'s performance", key);
	}

	public override Schedule<GaussianDistribution> CreatePriorSchedule()
	{
		return ScheduleSequence(base.LocalFactors.Select((GaussianLikelihoodFactor likelihood) => new ScheduleStep<GaussianDistribution>("Skill to Perf step", likelihood, 0)), "All skill to performance sending");
	}

	public override Schedule<GaussianDistribution> CreatePosteriorSchedule()
	{
		return ScheduleSequence(base.LocalFactors.Select((GaussianLikelihoodFactor likelihood) => new ScheduleStep<GaussianDistribution>("name", likelihood, 1)), "All skill to performance sending");
	}
}
