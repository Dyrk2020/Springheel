using System.Collections.Generic;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class DismissMultipleMessagesRequest : BCGSTypedRequest<DismissMultipleMessagesRequest, DismissMultipleMessagesResponse>
{
	public DismissMultipleMessagesRequest()
		: base("DismissMultipleMessagesRequest")
	{
	}

	public DismissMultipleMessagesRequest(BCGSInstance instance)
		: base(instance, "DismissMultipleMessagesRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new DismissMultipleMessagesResponse(response);
	}

	public DismissMultipleMessagesRequest SetMessageIds(List<string> messageIds)
	{
		request.AddStringList("messageIds", messageIds);
		return this;
	}
}
