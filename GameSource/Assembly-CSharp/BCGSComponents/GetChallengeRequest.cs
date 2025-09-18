using BCGSComponents.DataModels;

namespace BCGSComponents;

public class GetChallengeRequest : BCGSTypedRequest<GetChallengeRequest, GetChallengeResponse>
{
	public GetChallengeRequest()
		: base("GetChallengeRequest")
	{
	}

	public GetChallengeRequest(BCGSInstance instance)
		: base(instance, "GetChallengeRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new GetChallengeResponse(response);
	}

	public GetChallengeRequest SetChallengeInstanceId(string challengeInstanceId)
	{
		request.AddString("challengeInstanceId", challengeInstanceId);
		return this;
	}

	public GetChallengeRequest SetMessage(string message)
	{
		request.AddString("message", message);
		return this;
	}
}
