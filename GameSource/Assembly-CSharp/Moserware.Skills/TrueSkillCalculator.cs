using System.Collections.Generic;
using Moserware.Skills.TrueSkill;

namespace Moserware.Skills;

public static class TrueSkillCalculator
{
	private static readonly SkillCalculator _Calculator = new FactorGraphTrueSkillCalculator();

	public static IDictionary<TPlayer, Rating> CalculateNewRatings<TPlayer>(GameInfo gameInfo, IEnumerable<IDictionary<TPlayer, Rating>> teams, params int[] teamRanks)
	{
		return _Calculator.CalculateNewRatings(gameInfo, teams, teamRanks);
	}

	public static double CalculateMatchQuality<TPlayer>(GameInfo gameInfo, IEnumerable<IDictionary<TPlayer, Rating>> teams)
	{
		return _Calculator.CalculateMatchQuality(gameInfo, teams);
	}
}
