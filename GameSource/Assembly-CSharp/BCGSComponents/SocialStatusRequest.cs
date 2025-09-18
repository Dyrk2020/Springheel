using BCGSComponents.DataModels;

namespace BCGSComponents;

public class SocialStatusRequest : BCGSTypedRequest<SocialStatusRequest, SocialStatusResponse>
{
	public SocialStatusRequest()
		: base("SocialStatusRequest")
	{
	}

	public SocialStatusRequest(BCGSInstance instance)
		: base(instance, "SocialStatusRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new SocialStatusResponse(response);
	}
}
