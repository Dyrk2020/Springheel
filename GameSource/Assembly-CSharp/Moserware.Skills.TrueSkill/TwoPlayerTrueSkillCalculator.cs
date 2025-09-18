using System;
using System.Collections.Generic;
using System.Linq;
using Moserware.Skills.Numerics;

namespace Moserware.Skills.TrueSkill;

public class TwoPlayerTrueSkillCalculator : SkillCalculator
{
	public TwoPlayerTrueSkillCalculator()
		: base(SupportedOptions.None, Range<TeamsRange>.Exactly(2), Range<PlayersRange>.Exactly(1))
	{
	}

	public override IDictionary<TPlayer, Rating> CalculateNewRatings<TPlayer>(GameInfo gameInfo, IEnumerable<IDictionary<TPlayer, Rating>> teams, params int[] teamRanks)
	{
		Guard.ArgumentNotNull(gameInfo, "gameInfo");
		ValidateTeamCountAndPlayersCountPerTeam(teams);
		RankSorter.Sort(ref teams, ref teamRanks);
		List<IDictionary<TPlayer, Rating>> list = teams.ToList();
		IDictionary<TPlayer, Rating> dictionary = list[0];
		TPlayer key = dictionary.Keys.First();
		Rating rating = dictionary[key];
		IDictionary<TPlayer, Rating> dictionary2 = list[1];
		TPlayer key2 = dictionary2.Keys.First();
		Rating rating2 = dictionary2[key2];
		bool flag = teamRanks[0] == teamRanks[1];
		return new Dictionary<TPlayer, Rating>
		{
			[key] = CalculateNewRating(gameInfo, rating, rating2, (!flag) ? PairwiseComparison.Win : PairwiseComparison.Draw),
			[key2] = CalculateNewRating(gameInfo, rating2, rating, (!flag) ? PairwiseComparison.Lose : PairwiseComparison.Draw)
		};
	}

	private static Rating CalculateNewRating(GameInfo gameInfo, Rating selfRating, Rating opponentRating, PairwiseComparison comparison)
	{
		double drawMarginFromDrawProbability = DrawMargin.GetDrawMarginFromDrawProbability(gameInfo.DrawProbability, gameInfo.Beta);
		double num = Math.Sqrt(SkillCalculator.Square(selfRating.StandardDeviation) + SkillCalculator.Square(opponentRating.StandardDeviation) + 2.0 * SkillCalculator.Square(gameInfo.Beta));
		double mean = selfRating.Mean;
		double mean2 = opponentRating.Mean;
		switch (comparison)
		{
		case PairwiseComparison.Lose:
			mean = opponentRating.Mean;
			mean2 = selfRating.Mean;
			break;
		}
		double teamPerformanceDifference = mean - mean2;
		double num2;
		double num3;
		double num4;
		if (comparison != PairwiseComparison.Draw)
		{
			num2 = TruncatedGaussianCorrectionFunctions.VExceedsMargin(teamPerformanceDifference, drawMarginFromDrawProbability, num);
			num3 = TruncatedGaussianCorrectionFunctions.WExceedsMargin(teamPerformanceDifference, drawMarginFromDrawProbability, num);
			num4 = (double)comparison;
		}
		else
		{
			num2 = TruncatedGaussianCorrectionFunctions.VWithinMargin(teamPerformanceDifference, drawMarginFromDrawProbability, num);
			num3 = TruncatedGaussianCorrectionFunctions.WWithinMargin(teamPerformanceDifference, drawMarginFromDrawProbability, num);
			num4 = 1.0;
		}
		double num5 = (SkillCalculator.Square(selfRating.StandardDeviation) + SkillCalculator.Square(gameInfo.DynamicsFactor)) / num;
		double num6 = SkillCalculator.Square(selfRating.StandardDeviation) + SkillCalculator.Square(gameInfo.DynamicsFactor);
		double num7 = num6 / SkillCalculator.Square(num);
		double mean3 = selfRating.Mean + num4 * num5 * num2;
		double standardDeviation = Math.Sqrt(num6 * (1.0 - num3 * num7));
		return new Rating(mean3, standardDeviation);
	}

	public override double CalculateMatchQuality<TPlayer>(GameInfo gameInfo, IEnumerable<IDictionary<TPlayer, Rating>> teams)
	{
		Guard.ArgumentNotNull(gameInfo, "gameInfo");
		ValidateTeamCountAndPlayersCountPerTeam(teams);
		Rating rating = teams.First().Values.First();
		Rating rating2 = teams.Last().Values.First();
		double num = SkillCalculator.Square(gameInfo.Beta);
		double num2 = SkillCalculator.Square(rating.StandardDeviation);
		double num3 = SkillCalculator.Square(rating2.StandardDeviation);
		double num4 = Math.Sqrt(2.0 * num / (2.0 * num + num2 + num3));
		double num5 = Math.Exp(-1.0 * SkillCalculator.Square(rating.Mean - rating2.Mean) / (2.0 * (2.0 * num + num2 + num3)));
		return num4 * num5;
	}
}
