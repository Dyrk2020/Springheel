using BCGSComponents.DataModels;

namespace BCGSComponents;

public class DeviceAuthenticationRequest : BCGSTypedRequest<DeviceAuthenticationRequest, AuthenticationResponse>
{
	public DeviceAuthenticationRequest()
		: base("DeviceAuthenticationRequest")
	{
	}

	public DeviceAuthenticationRequest(BCGSInstance instance)
		: base(instance, "DeviceAuthenticationRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new AuthenticationResponse(response);
	}

	public DeviceAuthenticationRequest SetDisplayName(string displayName)
	{
		request.AddString("displayName", displayName);
		return this;
	}

	public DeviceAuthenticationRequest SetSegments(BCGSRequestData segments)
	{
		request.AddObject("segments", segments);
		return this;
	}
}
