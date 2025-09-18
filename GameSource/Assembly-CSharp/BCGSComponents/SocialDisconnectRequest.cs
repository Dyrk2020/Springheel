using BCGSComponents.DataModels;

namespace BCGSComponents;

public class SocialDisconnectRequest : BCGSTypedRequest<SocialDisconnectRequest, SocialDisconnectResponse>
{
	public SocialDisconnectRequest()
		: base("SocialDisconnectRequest")
	{
	}

	public SocialDisconnectRequest(BCGSInstance instance)
		: base(instance, "SocialDisconnectRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new SocialDisconnectResponse(response);
	}

	public SocialDisconnectRequest SetSystemId(string systemId)
	{
		request.AddString("systemId", systemId);
		return this;
	}
}
