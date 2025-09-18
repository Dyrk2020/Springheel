using System.Collections.Generic;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class LeaderboardDataRequest : BCGSTypedRequest<LeaderboardDataRequest, LeaderboardDataResponse>
{
	public LeaderboardDataRequest()
		: base("LeaderboardDataRequest")
	{
	}

	public LeaderboardDataRequest(BCGSInstance instance)
		: base(instance, "LeaderboardDataRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new LeaderboardDataResponse(response);
	}

	public LeaderboardDataRequest SetChallengeInstanceId(string challengeInstanceId)
	{
		request.AddString("challengeInstanceId", challengeInstanceId);
		return this;
	}

	public LeaderboardDataRequest SetDontErrorOnNotSocial(bool dontErrorOnNotSocial)
	{
		request.AddBoolean("dontErrorOnNotSocial", dontErrorOnNotSocial);
		return this;
	}

	public LeaderboardDataRequest SetEntryCount(long entryCount)
	{
		request.AddNumber("entryCount", entryCount);
		return this;
	}

	public LeaderboardDataRequest SetFriendIds(List<string> friendIds)
	{
		request.AddStringList("friendIds", friendIds);
		return this;
	}

	public LeaderboardDataRequest SetIncludeFirst(long includeFirst)
	{
		request.AddNumber("includeFirst", includeFirst);
		return this;
	}

	public LeaderboardDataRequest SetIncludeLast(long includeLast)
	{
		request.AddNumber("includeLast", includeLast);
		return this;
	}

	public LeaderboardDataRequest SetInverseSocial(bool inverseSocial)
	{
		request.AddBoolean("inverseSocial", inverseSocial);
		return this;
	}

	public LeaderboardDataRequest SetLeaderboardShortCode(string leaderboardShortCode)
	{
		request.AddString("leaderboardShortCode", leaderboardShortCode);
		return this;
	}

	public LeaderboardDataRequest SetOffset(long offset)
	{
		request.AddNumber("offset", offset);
		return this;
	}

	public LeaderboardDataRequest SetSocial(bool social)
	{
		request.AddBoolean("social", social);
		return this;
	}

	public LeaderboardDataRequest SetTeamIds(List<string> teamIds)
	{
		request.AddStringList("teamIds", teamIds);
		return this;
	}

	public LeaderboardDataRequest SetTeamTypes(List<string> teamTypes)
	{
		request.AddStringList("teamTypes", teamTypes);
		return this;
	}
}
