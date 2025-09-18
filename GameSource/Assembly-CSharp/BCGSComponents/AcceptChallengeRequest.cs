using BCGSComponents.DataModels;

namespace BCGSComponents;

public class AcceptChallengeRequest : BCGSTypedRequest<AcceptChallengeRequest, AcceptChallengeResponse>
{
	public AcceptChallengeRequest()
		: base("AcceptChallengeRequest")
	{
	}

	public AcceptChallengeRequest(BCGSInstance instance)
		: base(instance, "AcceptChallengeRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new AcceptChallengeResponse(response);
	}

	public AcceptChallengeRequest SetChallengeInstanceId(string challengeInstanceId)
	{
		request.AddString("challengeInstanceId", challengeInstanceId);
		return this;
	}

	public AcceptChallengeRequest SetMessage(string message)
	{
		request.AddString("message", message);
		return this;
	}
}
