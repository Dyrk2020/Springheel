using BCGSComponents.DataModels;

namespace BCGSComponents;

public class JoinPendingMatchRequest : BCGSTypedRequest<JoinPendingMatchRequest, JoinPendingMatchResponse>
{
	public JoinPendingMatchRequest()
		: base("JoinPendingMatchRequest")
	{
	}

	public JoinPendingMatchRequest(BCGSInstance instance)
		: base(instance, "JoinPendingMatchRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new JoinPendingMatchResponse(response);
	}

	public JoinPendingMatchRequest SetMatchGroup(string matchGroup)
	{
		request.AddString("matchGroup", matchGroup);
		return this;
	}

	public JoinPendingMatchRequest SetMatchShortCode(string matchShortCode)
	{
		request.AddString("matchShortCode", matchShortCode);
		return this;
	}

	public JoinPendingMatchRequest SetPendingMatchId(string pendingMatchId)
	{
		request.AddString("pendingMatchId", pendingMatchId);
		return this;
	}
}
