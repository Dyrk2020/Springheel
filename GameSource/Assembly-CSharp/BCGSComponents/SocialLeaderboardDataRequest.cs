using System.Collections.Generic;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class SocialLeaderboardDataRequest : BCGSTypedRequest<SocialLeaderboardDataRequest, LeaderboardDataResponse>
{
	public SocialLeaderboardDataRequest()
		: base("SocialLeaderboardDataRequest")
	{
	}

	public SocialLeaderboardDataRequest(BCGSInstance instance)
		: base(instance, "SocialLeaderboardDataRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new LeaderboardDataResponse(response);
	}

	public SocialLeaderboardDataRequest SetChallengeInstanceId(string challengeInstanceId)
	{
		request.AddString("challengeInstanceId", challengeInstanceId);
		return this;
	}

	public SocialLeaderboardDataRequest SetDontErrorOnNotSocial(bool dontErrorOnNotSocial)
	{
		request.AddBoolean("dontErrorOnNotSocial", dontErrorOnNotSocial);
		return this;
	}

	public SocialLeaderboardDataRequest SetEntryCount(long entryCount)
	{
		request.AddNumber("entryCount", entryCount);
		return this;
	}

	public SocialLeaderboardDataRequest SetFriendIds(List<string> friendIds)
	{
		request.AddStringList("friendIds", friendIds);
		return this;
	}

	public SocialLeaderboardDataRequest SetIncludeFirst(long includeFirst)
	{
		request.AddNumber("includeFirst", includeFirst);
		return this;
	}

	public SocialLeaderboardDataRequest SetIncludeLast(long includeLast)
	{
		request.AddNumber("includeLast", includeLast);
		return this;
	}

	public SocialLeaderboardDataRequest SetInverseSocial(bool inverseSocial)
	{
		request.AddBoolean("inverseSocial", inverseSocial);
		return this;
	}

	public SocialLeaderboardDataRequest SetLeaderboardShortCode(string leaderboardShortCode)
	{
		request.AddString("leaderboardShortCode", leaderboardShortCode);
		return this;
	}

	public SocialLeaderboardDataRequest SetOffset(long offset)
	{
		request.AddNumber("offset", offset);
		return this;
	}

	public SocialLeaderboardDataRequest SetSocial(bool social)
	{
		request.AddBoolean("social", social);
		return this;
	}

	public SocialLeaderboardDataRequest SetTeamIds(List<string> teamIds)
	{
		request.AddStringList("teamIds", teamIds);
		return this;
	}

	public SocialLeaderboardDataRequest SetTeamTypes(List<string> teamTypes)
	{
		request.AddStringList("teamTypes", teamTypes);
		return this;
	}
}
