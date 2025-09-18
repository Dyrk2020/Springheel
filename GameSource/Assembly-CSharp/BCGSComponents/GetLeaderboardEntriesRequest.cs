using System.Collections.Generic;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class GetLeaderboardEntriesRequest : BCGSTypedRequest<GetLeaderboardEntriesRequest, GetLeaderboardEntriesResponse>
{
	public GetLeaderboardEntriesRequest()
		: base("GetLeaderboardEntriesRequest")
	{
	}

	public GetLeaderboardEntriesRequest(BCGSInstance instance)
		: base(instance, "GetLeaderboardEntriesRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new GetLeaderboardEntriesResponse(response);
	}

	public GetLeaderboardEntriesRequest SetChallenges(List<string> challenges)
	{
		request.AddStringList("challenges", challenges);
		return this;
	}

	public GetLeaderboardEntriesRequest SetInverseSocial(bool inverseSocial)
	{
		request.AddBoolean("inverseSocial", inverseSocial);
		return this;
	}

	public GetLeaderboardEntriesRequest SetLeaderboards(List<string> leaderboards)
	{
		request.AddStringList("leaderboards", leaderboards);
		return this;
	}

	public GetLeaderboardEntriesRequest SetPlayer(string player)
	{
		request.AddString("player", player);
		return this;
	}

	public GetLeaderboardEntriesRequest SetSocial(bool social)
	{
		request.AddBoolean("social", social);
		return this;
	}

	public GetLeaderboardEntriesRequest SetTeamTypes(List<string> teamTypes)
	{
		request.AddStringList("teamTypes", teamTypes);
		return this;
	}
}
