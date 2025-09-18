using Moserware.Numerics;
using Moserware.Skills.FactorGraphs;
using Moserware.Skills.TrueSkill.Factors;

namespace Moserware.Skills.TrueSkill.Layers;

internal class TeamPerformancesToTeamPerformanceDifferencesLayer<TPlayer> : TrueSkillFactorGraphLayer<TPlayer, Variable<GaussianDistribution>, GaussianWeightedSumFactor, Variable<GaussianDistribution>>
{
	public TeamPerformancesToTeamPerformanceDifferencesLayer(TrueSkillFactorGraph<TPlayer> parentGraph)
		: base(parentGraph)
	{
	}

	public override void BuildLayer()
	{
		for (int i = 0; i < base.InputVariablesGroups.Count - 1; i++)
		{
			Variable<GaussianDistribution> strongerTeam = base.InputVariablesGroups[i][0];
			Variable<GaussianDistribution> weakerTeam = base.InputVariablesGroups[i + 1][0];
			Variable<GaussianDistribution> variable = CreateOutputVariable();
			AddLayerFactor(CreateTeamPerformanceToDifferenceFactor(strongerTeam, weakerTeam, variable));
			base.OutputVariablesGroups.Add(new Variable<GaussianDistribution>[1] { variable });
		}
	}

	private GaussianWeightedSumFactor CreateTeamPerformanceToDifferenceFactor(Variable<GaussianDistribution> strongerTeam, Variable<GaussianDistribution> weakerTeam, Variable<GaussianDistribution> output)
	{
		return new GaussianWeightedSumFactor(output, new Variable<GaussianDistribution>[2] { strongerTeam, weakerTeam }, new double[2] { 1.0, -1.0 });
	}

	private Variable<GaussianDistribution> CreateOutputVariable()
	{
		return base.ParentFactorGraph.VariableFactory.CreateBasicVariable("Team performance difference");
	}
}
