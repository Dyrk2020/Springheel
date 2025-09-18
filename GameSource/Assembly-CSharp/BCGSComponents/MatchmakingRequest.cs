using BCGSComponents.DataModels;

namespace BCGSComponents;

public class MatchmakingRequest : BCGSTypedRequest<MatchmakingRequest, MatchmakingResponse>
{
	public MatchmakingRequest()
		: base("MatchmakingRequest")
	{
	}

	public MatchmakingRequest(BCGSInstance instance)
		: base(instance, "MatchmakingRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new MatchmakingResponse(response);
	}

	public MatchmakingRequest SetAction(string action)
	{
		request.AddString("action", action);
		return this;
	}

	public MatchmakingRequest SetCustomQuery(BCGSRequestData customQuery)
	{
		request.AddObject("customQuery", customQuery);
		return this;
	}

	public MatchmakingRequest SetMatchData(BCGSRequestData matchData)
	{
		request.AddObject("matchData", matchData);
		return this;
	}

	public MatchmakingRequest SetMatchGroup(string matchGroup)
	{
		request.AddString("matchGroup", matchGroup);
		return this;
	}

	public MatchmakingRequest SetMatchShortCode(string matchShortCode)
	{
		request.AddString("matchShortCode", matchShortCode);
		return this;
	}

	public MatchmakingRequest SetParticipantData(BCGSRequestData participantData)
	{
		request.AddObject("participantData", participantData);
		return this;
	}

	public MatchmakingRequest SetSkill(long skill)
	{
		request.AddNumber("skill", skill);
		return this;
	}
}
