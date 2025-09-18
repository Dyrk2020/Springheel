using BCGSComponents.DataModels;

namespace BCGSComponents;

public class DeclineChallengeRequest : BCGSTypedRequest<DeclineChallengeRequest, DeclineChallengeResponse>
{
	public DeclineChallengeRequest()
		: base("DeclineChallengeRequest")
	{
	}

	public DeclineChallengeRequest(BCGSInstance instance)
		: base(instance, "DeclineChallengeRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new DeclineChallengeResponse(response);
	}

	public DeclineChallengeRequest SetChallengeInstanceId(string challengeInstanceId)
	{
		request.AddString("challengeInstanceId", challengeInstanceId);
		return this;
	}

	public DeclineChallengeRequest SetMessage(string message)
	{
		request.AddString("message", message);
		return this;
	}
}
