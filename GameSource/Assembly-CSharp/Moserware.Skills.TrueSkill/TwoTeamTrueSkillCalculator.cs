using System;
using System.Collections.Generic;
using System.Linq;
using Moserware.Skills.Numerics;

namespace Moserware.Skills.TrueSkill;

public class TwoTeamTrueSkillCalculator : SkillCalculator
{
	public TwoTeamTrueSkillCalculator()
		: base(SupportedOptions.None, Range<TeamsRange>.Exactly(2), Range<PlayersRange>.AtLeast(1))
	{
	}

	public override IDictionary<TPlayer, Rating> CalculateNewRatings<TPlayer>(GameInfo gameInfo, IEnumerable<IDictionary<TPlayer, Rating>> teams, params int[] teamRanks)
	{
		Guard.ArgumentNotNull(gameInfo, "gameInfo");
		ValidateTeamCountAndPlayersCountPerTeam(teams);
		RankSorter.Sort(ref teams, ref teamRanks);
		IDictionary<TPlayer, Rating> dictionary = teams.First();
		IDictionary<TPlayer, Rating> dictionary2 = teams.Last();
		bool flag = teamRanks[0] == teamRanks[1];
		Dictionary<TPlayer, Rating> dictionary3 = new Dictionary<TPlayer, Rating>();
		UpdatePlayerRatings(gameInfo, dictionary3, dictionary, dictionary2, (!flag) ? PairwiseComparison.Win : PairwiseComparison.Draw);
		UpdatePlayerRatings(gameInfo, dictionary3, dictionary2, dictionary, (!flag) ? PairwiseComparison.Lose : PairwiseComparison.Draw);
		return dictionary3;
	}

	private static void UpdatePlayerRatings<TPlayer>(GameInfo gameInfo, IDictionary<TPlayer, Rating> newPlayerRatings, IDictionary<TPlayer, Rating> selfTeam, IDictionary<TPlayer, Rating> otherTeam, PairwiseComparison selfToOtherTeamComparison)
	{
		double drawMarginFromDrawProbability = DrawMargin.GetDrawMarginFromDrawProbability(gameInfo.DrawProbability, gameInfo.Beta);
		double num = SkillCalculator.Square(gameInfo.Beta);
		double num2 = SkillCalculator.Square(gameInfo.DynamicsFactor);
		int num3 = selfTeam.Count() + otherTeam.Count();
		double num4 = selfTeam.Values.Sum((Rating r) => r.Mean);
		double num5 = otherTeam.Values.Sum((Rating r) => r.Mean);
		double num6 = Math.Sqrt(selfTeam.Values.Sum((Rating r) => SkillCalculator.Square(r.StandardDeviation)) + otherTeam.Values.Sum((Rating r) => SkillCalculator.Square(r.StandardDeviation)) + (double)num3 * num);
		double num7 = num4;
		double num8 = num5;
		switch (selfToOtherTeamComparison)
		{
		case PairwiseComparison.Lose:
			num7 = num5;
			num8 = num4;
			break;
		}
		double teamPerformanceDifference = num7 - num8;
		double num9;
		double num10;
		double num11;
		if (selfToOtherTeamComparison != PairwiseComparison.Draw)
		{
			num9 = TruncatedGaussianCorrectionFunctions.VExceedsMargin(teamPerformanceDifference, drawMarginFromDrawProbability, num6);
			num10 = TruncatedGaussianCorrectionFunctions.WExceedsMargin(teamPerformanceDifference, drawMarginFromDrawProbability, num6);
			num11 = (double)selfToOtherTeamComparison;
		}
		else
		{
			num9 = TruncatedGaussianCorrectionFunctions.VWithinMargin(teamPerformanceDifference, drawMarginFromDrawProbability, num6);
			num10 = TruncatedGaussianCorrectionFunctions.WWithinMargin(teamPerformanceDifference, drawMarginFromDrawProbability, num6);
			num11 = 1.0;
		}
		foreach (KeyValuePair<TPlayer, Rating> item in selfTeam)
		{
			Rating value = item.Value;
			double num12 = (SkillCalculator.Square(value.StandardDeviation) + num2) / num6;
			double num13 = (SkillCalculator.Square(value.StandardDeviation) + num2) / SkillCalculator.Square(num6);
			double num14 = num11 * num12 * num9;
			double mean = value.Mean + num14;
			double standardDeviation = Math.Sqrt((SkillCalculator.Square(value.StandardDeviation) + num2) * (1.0 - num10 * num13));
			newPlayerRatings[item.Key] = new Rating(mean, standardDeviation);
		}
	}

	public override double CalculateMatchQuality<TPlayer>(GameInfo gameInfo, IEnumerable<IDictionary<TPlayer, Rating>> teams)
	{
		Guard.ArgumentNotNull(gameInfo, "gameInfo");
		ValidateTeamCountAndPlayersCountPerTeam(teams);
		ICollection<Rating> values = teams.First().Values;
		int num = values.Count();
		ICollection<Rating> values2 = teams.Last().Values;
		int num2 = values2.Count();
		int num3 = num + num2;
		double num4 = SkillCalculator.Square(gameInfo.Beta);
		double num5 = values.Sum((Rating r) => r.Mean);
		double num6 = values.Sum((Rating r) => SkillCalculator.Square(r.StandardDeviation));
		double num7 = values2.Sum((Rating r) => r.Mean);
		double num8 = values2.Sum((Rating r) => SkillCalculator.Square(r.StandardDeviation));
		double num9 = Math.Sqrt((double)num3 * num4 / ((double)num3 * num4 + num6 + num8));
		return Math.Exp(-1.0 * SkillCalculator.Square(num5 - num7) / (2.0 * ((double)num3 * num4 + num6 + num8))) * num9;
	}
}
