using BCGSComponents.DataModels;

namespace BCGSComponents;

public class EndSessionRequest : BCGSTypedRequest<EndSessionRequest, EndSessionResponse>
{
	public EndSessionRequest()
		: base("EndSessionRequest")
	{
	}

	public EndSessionRequest(BCGSInstance instance)
		: base(instance, "EndSessionRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new EndSessionResponse(response);
	}
}
