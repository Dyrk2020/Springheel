using BCGSComponents.DataModels;

namespace BCGSComponents;

public class JoinChallengeRequest : BCGSTypedRequest<JoinChallengeRequest, JoinChallengeResponse>
{
	public JoinChallengeRequest()
		: base("JoinChallengeRequest")
	{
	}

	public JoinChallengeRequest(BCGSInstance instance)
		: base(instance, "JoinChallengeRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new JoinChallengeResponse(response);
	}

	public JoinChallengeRequest SetChallengeInstanceId(string challengeInstanceId)
	{
		request.AddString("challengeInstanceId", challengeInstanceId);
		return this;
	}

	public JoinChallengeRequest SetEligibility(BCGSRequestData eligibility)
	{
		request.AddObject("eligibility", eligibility);
		return this;
	}

	public JoinChallengeRequest SetMessage(string message)
	{
		request.AddString("message", message);
		return this;
	}
}
