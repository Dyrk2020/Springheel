using BCGSComponents.DataModels;

namespace BCGSComponents;

public class DismissMessageRequest : BCGSTypedRequest<DismissMessageRequest, DismissMessageResponse>
{
	public DismissMessageRequest()
		: base("DismissMessageRequest")
	{
	}

	public DismissMessageRequest(BCGSInstance instance)
		: base(instance, "DismissMessageRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new DismissMessageResponse(response);
	}

	public DismissMessageRequest SetMessageId(string messageId)
	{
		request.AddString("messageId", messageId);
		return this;
	}
}
