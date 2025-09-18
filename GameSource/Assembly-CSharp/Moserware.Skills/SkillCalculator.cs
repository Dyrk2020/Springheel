using System;
using System.Collections.Generic;

namespace Moserware.Skills;

public abstract class SkillCalculator
{
	[Flags]
	public enum SupportedOptions
	{
		None = 0,
		PartialPlay = 1,
		PartialUpdate = 2
	}

	private readonly SupportedOptions _SupportedOptions;

	private readonly PlayersRange _PlayersPerTeamAllowed;

	private readonly TeamsRange _TotalTeamsAllowed;

	protected SkillCalculator(SupportedOptions supportedOptions, TeamsRange totalTeamsAllowed, PlayersRange playerPerTeamAllowed)
	{
		_SupportedOptions = supportedOptions;
		_TotalTeamsAllowed = totalTeamsAllowed;
		_PlayersPerTeamAllowed = playerPerTeamAllowed;
	}

	public abstract IDictionary<TPlayer, Rating> CalculateNewRatings<TPlayer>(GameInfo gameInfo, IEnumerable<IDictionary<TPlayer, Rating>> teams, params int[] teamRanks);

	public abstract double CalculateMatchQuality<TPlayer>(GameInfo gameInfo, IEnumerable<IDictionary<TPlayer, Rating>> teams);

	public bool IsSupported(SupportedOptions option)
	{
		return (_SupportedOptions & option) == option;
	}

	protected static double Square(double value)
	{
		return value * value;
	}

	protected void ValidateTeamCountAndPlayersCountPerTeam<TPlayer>(IEnumerable<IDictionary<TPlayer, Rating>> teams)
	{
		ValidateTeamCountAndPlayersCountPerTeam(teams, _TotalTeamsAllowed, _PlayersPerTeamAllowed);
	}

	private static void ValidateTeamCountAndPlayersCountPerTeam<TPlayer>(IEnumerable<IDictionary<TPlayer, Rating>> teams, TeamsRange totalTeams, PlayersRange playersPerTeam)
	{
		Guard.ArgumentNotNull(teams, "teams");
		int num = 0;
		foreach (IDictionary<TPlayer, Rating> team in teams)
		{
			if (!playersPerTeam.IsInRange(team.Count))
			{
				throw new ArgumentException();
			}
			num++;
		}
		if (!totalTeams.IsInRange(num))
		{
			throw new ArgumentException();
		}
	}
}
