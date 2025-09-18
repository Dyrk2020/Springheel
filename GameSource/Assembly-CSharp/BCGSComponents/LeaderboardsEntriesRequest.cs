using System.Collections.Generic;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class LeaderboardsEntriesRequest : BCGSTypedRequest<LeaderboardsEntriesRequest, LeaderboardsEntriesResponse>
{
	public LeaderboardsEntriesRequest()
		: base("LeaderboardsEntriesRequest")
	{
	}

	public LeaderboardsEntriesRequest(BCGSInstance instance)
		: base(instance, "LeaderboardsEntriesRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new LeaderboardsEntriesResponse(response);
	}

	public LeaderboardsEntriesRequest SetChallenges(List<string> challenges)
	{
		request.AddStringList("challenges", challenges);
		return this;
	}

	public LeaderboardsEntriesRequest SetInverseSocial(bool inverseSocial)
	{
		request.AddBoolean("inverseSocial", inverseSocial);
		return this;
	}

	public LeaderboardsEntriesRequest SetLeaderboards(List<string> leaderboards)
	{
		request.AddStringList("leaderboards", leaderboards);
		return this;
	}

	public LeaderboardsEntriesRequest SetPlayer(string player)
	{
		request.AddString("player", player);
		return this;
	}

	public LeaderboardsEntriesRequest SetSocial(bool social)
	{
		request.AddBoolean("social", social);
		return this;
	}

	public LeaderboardsEntriesRequest SetTeamTypes(List<string> teamTypes)
	{
		request.AddStringList("teamTypes", teamTypes);
		return this;
	}
}
