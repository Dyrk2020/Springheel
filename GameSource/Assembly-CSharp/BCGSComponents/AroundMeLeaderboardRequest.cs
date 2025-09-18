using System.Collections.Generic;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class AroundMeLeaderboardRequest : BCGSTypedRequest<AroundMeLeaderboardRequest, AroundMeLeaderboardResponse>
{
	public AroundMeLeaderboardRequest()
		: base("AroundMeLeaderboardRequest")
	{
	}

	public AroundMeLeaderboardRequest(BCGSInstance instance)
		: base(instance, "AroundMeLeaderboardRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new AroundMeLeaderboardResponse(response);
	}

	public AroundMeLeaderboardRequest SetChallengeInstanceId(string challengeInstanceId)
	{
		request.AddString("challengeInstanceId", challengeInstanceId);
		return this;
	}

	public AroundMeLeaderboardRequest SetCustomIdFilter(BCGSRequestData customIdFilter)
	{
		request.AddObject("customIdFilter", customIdFilter);
		return this;
	}

	public AroundMeLeaderboardRequest SetDontErrorOnNotSocial(bool dontErrorOnNotSocial)
	{
		request.AddBoolean("dontErrorOnNotSocial", dontErrorOnNotSocial);
		return this;
	}

	public AroundMeLeaderboardRequest SetEntryCount(long entryCount)
	{
		request.AddNumber("entryCount", entryCount);
		return this;
	}

	public AroundMeLeaderboardRequest SetFriendIds(List<string> friendIds)
	{
		request.AddStringList("friendIds", friendIds);
		return this;
	}

	public AroundMeLeaderboardRequest SetIncludeFirst(long includeFirst)
	{
		request.AddNumber("includeFirst", includeFirst);
		return this;
	}

	public AroundMeLeaderboardRequest SetIncludeLast(long includeLast)
	{
		request.AddNumber("includeLast", includeLast);
		return this;
	}

	public AroundMeLeaderboardRequest SetInverseSocial(bool inverseSocial)
	{
		request.AddBoolean("inverseSocial", inverseSocial);
		return this;
	}

	public AroundMeLeaderboardRequest SetLeaderboardShortCode(string leaderboardShortCode)
	{
		request.AddString("leaderboardShortCode", leaderboardShortCode);
		return this;
	}

	public AroundMeLeaderboardRequest SetSocial(bool social)
	{
		request.AddBoolean("social", social);
		return this;
	}

	public AroundMeLeaderboardRequest SetTeamIds(List<string> teamIds)
	{
		request.AddStringList("teamIds", teamIds);
		return this;
	}

	public AroundMeLeaderboardRequest SetTeamTypes(List<string> teamTypes)
	{
		request.AddStringList("teamTypes", teamTypes);
		return this;
	}
}
