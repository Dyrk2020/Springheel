using BCGSComponents.DataModels;

namespace BCGSComponents;

public class UpdateMessageRequest : BCGSTypedRequest<UpdateMessageRequest, UpdateMessageResponse>
{
	public UpdateMessageRequest()
		: base("UpdateMessageRequest")
	{
	}

	public UpdateMessageRequest(BCGSInstance instance)
		: base(instance, "UpdateMessageRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new UpdateMessageResponse(response);
	}

	public UpdateMessageRequest SetMessageId(string messageId)
	{
		request.AddString("messageId", messageId);
		return this;
	}

	public UpdateMessageRequest SetStatus(string status)
	{
		request.AddString("status", status);
		return this;
	}
}
