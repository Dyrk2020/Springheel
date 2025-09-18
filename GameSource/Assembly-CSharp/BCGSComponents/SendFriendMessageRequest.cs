using System.Collections.Generic;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class SendFriendMessageRequest : BCGSTypedRequest<SendFriendMessageRequest, SendFriendMessageResponse>
{
	public SendFriendMessageRequest()
		: base("SendFriendMessageRequest")
	{
	}

	public SendFriendMessageRequest(BCGSInstance instance)
		: base(instance, "SendFriendMessageRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new SendFriendMessageResponse(response);
	}

	public SendFriendMessageRequest SetFriendIds(List<string> friendIds)
	{
		request.AddStringList("friendIds", friendIds);
		return this;
	}

	public SendFriendMessageRequest SetMessage(string message)
	{
		request.AddString("message", message);
		return this;
	}
}
