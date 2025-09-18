using BCGSComponents.DataModels;

namespace BCGSComponents;

public class PushRegistrationRequest : BCGSTypedRequest<PushRegistrationRequest, PushRegistrationResponse>
{
	public PushRegistrationRequest()
		: base("PushRegistrationRequest")
	{
	}

	public PushRegistrationRequest(BCGSInstance instance)
		: base(instance, "PushRegistrationRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new PushRegistrationResponse(response);
	}

	public PushRegistrationRequest SetDeviceOS(string deviceOS)
	{
		request.AddString("deviceOS", deviceOS);
		return this;
	}

	public PushRegistrationRequest SetPushId(string pushId)
	{
		request.AddString("pushId", pushId);
		return this;
	}
}
