using BCGSComponents.DataModels;

namespace BCGSComponents;

public class MatchDetailsRequest : BCGSTypedRequest<MatchDetailsRequest, MatchDetailsResponse>
{
	public MatchDetailsRequest()
		: base("MatchDetailsRequest")
	{
	}

	public MatchDetailsRequest(BCGSInstance instance)
		: base(instance, "MatchDetailsRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new MatchDetailsResponse(response);
	}

	public MatchDetailsRequest SetMatchId(string matchId)
	{
		request.AddString("matchId", matchId);
		return this;
	}

	public MatchDetailsRequest SetRealtimeEnabled(bool realtimeEnabled)
	{
		request.AddBoolean("realtimeEnabled", realtimeEnabled);
		return this;
	}
}
