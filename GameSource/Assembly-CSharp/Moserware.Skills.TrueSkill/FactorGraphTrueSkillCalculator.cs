using System;
using System.Collections.Generic;
using System.Linq;
using Moserware.Numerics;
using Moserware.Skills.Numerics;

namespace Moserware.Skills.TrueSkill;

internal class FactorGraphTrueSkillCalculator : SkillCalculator
{
	public FactorGraphTrueSkillCalculator()
		: base(SupportedOptions.PartialPlay | SupportedOptions.PartialUpdate, Range<TeamsRange>.AtLeast(2), Range<PlayersRange>.AtLeast(1))
	{
	}

	public override IDictionary<TPlayer, Rating> CalculateNewRatings<TPlayer>(GameInfo gameInfo, IEnumerable<IDictionary<TPlayer, Rating>> teams, params int[] teamRanks)
	{
		Guard.ArgumentNotNull(gameInfo, "gameInfo");
		ValidateTeamCountAndPlayersCountPerTeam(teams);
		RankSorter.Sort(ref teams, ref teamRanks);
		TrueSkillFactorGraph<TPlayer> trueSkillFactorGraph = new TrueSkillFactorGraph<TPlayer>(gameInfo, teams, teamRanks);
		trueSkillFactorGraph.BuildGraph();
		trueSkillFactorGraph.RunSchedule();
		trueSkillFactorGraph.GetProbabilityOfRanking();
		return trueSkillFactorGraph.GetUpdatedRatings();
	}

	public override double CalculateMatchQuality<TPlayer>(GameInfo gameInfo, IEnumerable<IDictionary<TPlayer, Rating>> teams)
	{
		List<IDictionary<TPlayer, Rating>> teamAssignmentsList = teams.ToList();
		Matrix playerCovarianceMatrix = GetPlayerCovarianceMatrix(teamAssignmentsList);
		Vector playerMeansVector = GetPlayerMeansVector(teamAssignmentsList);
		Matrix transpose = playerMeansVector.Transpose;
		Matrix matrix = CreatePlayerTeamAssignmentMatrix(teamAssignmentsList, playerMeansVector.Rows);
		Matrix transpose2 = matrix.Transpose;
		double num = SkillCalculator.Square(gameInfo.Beta);
		Matrix matrix2 = transpose * matrix;
		Matrix matrix3 = num * transpose2 * matrix;
		Matrix matrix4 = transpose2 * playerCovarianceMatrix * matrix;
		Matrix matrix5 = matrix3 + matrix4;
		Matrix inverse = matrix5.Inverse;
		Matrix matrix6 = transpose2 * playerMeansVector;
		double determinant = (-0.5 * (matrix2 * inverse * matrix6)).Determinant;
		double determinant2 = matrix3.Determinant;
		double determinant3 = matrix5.Determinant;
		double d = determinant2 / determinant3;
		return Math.Exp(determinant) * Math.Sqrt(d);
	}

	private static Vector GetPlayerMeansVector<TPlayer>(IEnumerable<IDictionary<TPlayer, Rating>> teamAssignmentsList)
	{
		return new Vector(GetPlayerRatingValues(teamAssignmentsList, (Rating rating) => rating.Mean));
	}

	private static Matrix GetPlayerCovarianceMatrix<TPlayer>(IEnumerable<IDictionary<TPlayer, Rating>> teamAssignmentsList)
	{
		return new DiagonalMatrix(GetPlayerRatingValues(teamAssignmentsList, (Rating rating) => SkillCalculator.Square(rating.StandardDeviation)));
	}

	private static IList<double> GetPlayerRatingValues<TPlayer>(IEnumerable<IDictionary<TPlayer, Rating>> teamAssignmentsList, Func<Rating, double> playerRatingFunction)
	{
		List<double> list = new List<double>();
		foreach (IDictionary<TPlayer, Rating> teamAssignments in teamAssignmentsList)
		{
			foreach (Rating value in teamAssignments.Values)
			{
				list.Add(playerRatingFunction(value));
			}
		}
		return list;
	}

	private static Matrix CreatePlayerTeamAssignmentMatrix<TPlayer>(IList<IDictionary<TPlayer, Rating>> teamAssignmentsList, int totalPlayers)
	{
		List<IEnumerable<double>> list = new List<IEnumerable<double>>();
		int num = 0;
		for (int i = 0; i < teamAssignmentsList.Count - 1; i++)
		{
			IDictionary<TPlayer, Rating> dictionary = teamAssignmentsList[i];
			List<double> list2 = new List<double>(new double[num]);
			list.Add(list2);
			foreach (KeyValuePair<TPlayer, Rating> item in dictionary)
			{
				list2.Add(PartialPlay.GetPartialPlayPercentage(item.Key));
				num++;
			}
			foreach (KeyValuePair<TPlayer, Rating> item2 in teamAssignmentsList[i + 1])
			{
				list2.Add(-1.0 * PartialPlay.GetPartialPlayPercentage(item2.Key));
			}
		}
		return new Matrix(totalPlayers, teamAssignmentsList.Count - 1, list);
	}
}
