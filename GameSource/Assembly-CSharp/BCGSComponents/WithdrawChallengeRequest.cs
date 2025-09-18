using BCGSComponents.DataModels;

namespace BCGSComponents;

public class WithdrawChallengeRequest : BCGSTypedRequest<WithdrawChallengeRequest, WithdrawChallengeResponse>
{
	public WithdrawChallengeRequest()
		: base("WithdrawChallengeRequest")
	{
	}

	public WithdrawChallengeRequest(BCGSInstance instance)
		: base(instance, "WithdrawChallengeRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new WithdrawChallengeResponse(response);
	}

	public WithdrawChallengeRequest SetChallengeInstanceId(string challengeInstanceId)
	{
		request.AddString("challengeInstanceId", challengeInstanceId);
		return this;
	}

	public WithdrawChallengeRequest SetMessage(string message)
	{
		request.AddString("message", message);
		return this;
	}
}
