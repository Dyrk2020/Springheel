using BCGSComponents.DataModels;

namespace BCGSComponents;

public class SendTeamChatMessageRequest : BCGSTypedRequest<SendTeamChatMessageRequest, SendTeamChatMessageResponse>
{
	public SendTeamChatMessageRequest()
		: base("SendTeamChatMessageRequest")
	{
	}

	public SendTeamChatMessageRequest(BCGSInstance instance)
		: base(instance, "SendTeamChatMessageRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new SendTeamChatMessageResponse(response);
	}

	public SendTeamChatMessageRequest SetMessage(string message)
	{
		request.AddString("message", message);
		return this;
	}

	public SendTeamChatMessageRequest SetOwnerId(string ownerId)
	{
		request.AddString("ownerId", ownerId);
		return this;
	}

	public SendTeamChatMessageRequest SetTeamId(string teamId)
	{
		request.AddString("teamId", teamId);
		return this;
	}

	public SendTeamChatMessageRequest SetTeamType(string teamType)
	{
		request.AddString("teamType", teamType);
		return this;
	}
}
