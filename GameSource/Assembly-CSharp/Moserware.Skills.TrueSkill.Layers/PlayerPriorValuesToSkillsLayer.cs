using System.Collections.Generic;
using System.Linq;
using Moserware.Numerics;
using Moserware.Skills.FactorGraphs;
using Moserware.Skills.TrueSkill.Factors;

namespace Moserware.Skills.TrueSkill.Layers;

internal class PlayerPriorValuesToSkillsLayer<TPlayer> : TrueSkillFactorGraphLayer<TPlayer, DefaultVariable<GaussianDistribution>, GaussianPriorFactor, KeyedVariable<TPlayer, GaussianDistribution>>
{
	private readonly IEnumerable<IDictionary<TPlayer, Rating>> _Teams;

	public PlayerPriorValuesToSkillsLayer(TrueSkillFactorGraph<TPlayer> parentGraph, IEnumerable<IDictionary<TPlayer, Rating>> teams)
		: base(parentGraph)
	{
		_Teams = teams;
	}

	public override void BuildLayer()
	{
		foreach (IDictionary<TPlayer, Rating> team in _Teams)
		{
			List<KeyedVariable<TPlayer, GaussianDistribution>> list = new List<KeyedVariable<TPlayer, GaussianDistribution>>();
			foreach (KeyValuePair<TPlayer, Rating> item in team)
			{
				KeyedVariable<TPlayer, GaussianDistribution> keyedVariable = CreateSkillOutputVariable(item.Key);
				AddLayerFactor(CreatePriorFactor(item.Key, item.Value, keyedVariable));
				list.Add(keyedVariable);
			}
			base.OutputVariablesGroups.Add(list);
		}
	}

	public override Schedule<GaussianDistribution> CreatePriorSchedule()
	{
		return ScheduleSequence(base.LocalFactors.Select((GaussianPriorFactor prior) => new ScheduleStep<GaussianDistribution>("Prior to Skill Step", prior, 0)), "All priors");
	}

	private GaussianPriorFactor CreatePriorFactor(TPlayer player, Rating priorRating, Variable<GaussianDistribution> skillsVariable)
	{
		return new GaussianPriorFactor(priorRating.Mean, Square(priorRating.StandardDeviation) + Square(base.ParentFactorGraph.GameInfo.DynamicsFactor), skillsVariable);
	}

	private KeyedVariable<TPlayer, GaussianDistribution> CreateSkillOutputVariable(TPlayer key)
	{
		return base.ParentFactorGraph.VariableFactory.CreateKeyedVariable(key, "{0}'s skill", key);
	}
}
