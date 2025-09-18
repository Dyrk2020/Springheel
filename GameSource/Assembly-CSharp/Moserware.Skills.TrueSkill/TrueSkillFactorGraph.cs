using System;
using System.Collections.Generic;
using System.Linq;
using Moserware.Numerics;
using Moserware.Skills.FactorGraphs;
using Moserware.Skills.TrueSkill.Layers;

namespace Moserware.Skills.TrueSkill;

public class TrueSkillFactorGraph<TPlayer> : FactorGraph<TrueSkillFactorGraph<TPlayer>, GaussianDistribution, Variable<GaussianDistribution>>
{
	private readonly List<FactorGraphLayerBase<GaussianDistribution>> _Layers;

	private readonly PlayerPriorValuesToSkillsLayer<TPlayer> _PriorLayer;

	public GameInfo GameInfo { get; private set; }

	public TrueSkillFactorGraph(GameInfo gameInfo, IEnumerable<IDictionary<TPlayer, Rating>> teams, int[] teamRanks)
	{
		_PriorLayer = new PlayerPriorValuesToSkillsLayer<TPlayer>(this, teams);
		GameInfo = gameInfo;
		base.VariableFactory = new VariableFactory<GaussianDistribution>(() => GaussianDistribution.FromPrecisionMean(0.0, 0.0));
		_Layers = new List<FactorGraphLayerBase<GaussianDistribution>>
		{
			_PriorLayer,
			new PlayerSkillsToPerformancesLayer<TPlayer>(this),
			new PlayerPerformancesToTeamPerformancesLayer<TPlayer>(this),
			new IteratedTeamDifferencesInnerLayer<TPlayer>(this, new TeamPerformancesToTeamPerformanceDifferencesLayer<TPlayer>(this), new TeamDifferencesComparisonLayer<TPlayer>(this, teamRanks))
		};
	}

	public void BuildGraph()
	{
		object obj = null;
		foreach (FactorGraphLayerBase<GaussianDistribution> layer in _Layers)
		{
			if (obj != null)
			{
				layer.SetRawInputVariablesGroups(obj);
			}
			layer.BuildLayer();
			obj = layer.GetRawOutputVariablesGroups();
		}
	}

	public void RunSchedule()
	{
		CreateFullSchedule().Visit();
	}

	public double GetProbabilityOfRanking()
	{
		FactorList<GaussianDistribution> factorList = new FactorList<GaussianDistribution>();
		foreach (FactorGraphLayerBase<GaussianDistribution> layer in _Layers)
		{
			foreach (Factor<GaussianDistribution> untypedFactor in layer.UntypedFactors)
			{
				factorList.AddFactor(untypedFactor);
			}
		}
		return Math.Exp(factorList.LogNormalization);
	}

	private Schedule<GaussianDistribution> CreateFullSchedule()
	{
		List<Schedule<GaussianDistribution>> list = new List<Schedule<GaussianDistribution>>();
		foreach (FactorGraphLayerBase<GaussianDistribution> layer in _Layers)
		{
			Schedule<GaussianDistribution> schedule = layer.CreatePriorSchedule();
			if (schedule != null)
			{
				list.Add(schedule);
			}
		}
		foreach (FactorGraphLayerBase<GaussianDistribution> item in Enumerable.Reverse(_Layers))
		{
			Schedule<GaussianDistribution> schedule2 = item.CreatePosteriorSchedule();
			if (schedule2 != null)
			{
				list.Add(schedule2);
			}
		}
		return new ScheduleSequence<GaussianDistribution>("Full schedule", list);
	}

	public IDictionary<TPlayer, Rating> GetUpdatedRatings()
	{
		Dictionary<TPlayer, Rating> dictionary = new Dictionary<TPlayer, Rating>();
		foreach (IList<KeyedVariable<TPlayer, GaussianDistribution>> outputVariablesGroup in _PriorLayer.OutputVariablesGroups)
		{
			foreach (KeyedVariable<TPlayer, GaussianDistribution> item in outputVariablesGroup)
			{
				dictionary[item.Key] = new Rating(item.Value.Mean, item.Value.StandardDeviation);
			}
		}
		return dictionary;
	}
}
