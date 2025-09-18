using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ChatOnChallengeRequest : BCGSTypedRequest<ChatOnChallengeRequest, ChatOnChallengeResponse>
{
	public ChatOnChallengeRequest()
		: base("ChatOnChallengeRequest")
	{
	}

	public ChatOnChallengeRequest(BCGSInstance instance)
		: base(instance, "ChatOnChallengeRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new ChatOnChallengeResponse(response);
	}

	public ChatOnChallengeRequest SetChallengeInstanceId(string challengeInstanceId)
	{
		request.AddString("challengeInstanceId", challengeInstanceId);
		return this;
	}

	public ChatOnChallengeRequest SetMessage(string message)
	{
		request.AddString("message", message);
		return this;
	}
}
