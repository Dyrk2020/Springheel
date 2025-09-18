using Moserware.Numerics;
using Moserware.Skills.FactorGraphs;
using Moserware.Skills.TrueSkill.Factors;

namespace Moserware.Skills.TrueSkill.Layers;

internal class TeamDifferencesComparisonLayer<TPlayer> : TrueSkillFactorGraphLayer<TPlayer, Variable<GaussianDistribution>, GaussianFactor, DefaultVariable<GaussianDistribution>>
{
	private readonly double _Epsilon;

	private readonly int[] _TeamRanks;

	public TeamDifferencesComparisonLayer(TrueSkillFactorGraph<TPlayer> parentGraph, int[] teamRanks)
		: base(parentGraph)
	{
		_TeamRanks = teamRanks;
		GameInfo gameInfo = base.ParentFactorGraph.GameInfo;
		_Epsilon = DrawMargin.GetDrawMarginFromDrawProbability(gameInfo.DrawProbability, gameInfo.Beta);
	}

	public override void BuildLayer()
	{
		for (int i = 0; i < base.InputVariablesGroups.Count; i++)
		{
			bool num = _TeamRanks[i] == _TeamRanks[i + 1];
			Variable<GaussianDistribution> variable = base.InputVariablesGroups[i][0];
			GaussianFactor factor = (num ? ((GaussianFactor)new GaussianWithinFactor(_Epsilon, variable)) : ((GaussianFactor)new GaussianGreaterThanFactor(_Epsilon, variable)));
			AddLayerFactor(factor);
		}
	}
}
