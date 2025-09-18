using BCGSComponents.DataModels;

namespace BCGSComponents;

public class GetMessageRequest : BCGSTypedRequest<GetMessageRequest, GetMessageResponse>
{
	public GetMessageRequest()
		: base("GetMessageRequest")
	{
	}

	public GetMessageRequest(BCGSInstance instance)
		: base(instance, "GetMessageRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new GetMessageResponse(response);
	}

	public GetMessageRequest SetMessageId(string messageId)
	{
		request.AddString("messageId", messageId);
		return this;
	}
}
