using BCGSComponents.DataModels;

namespace BCGSComponents;

public class RegistrationRequest : BCGSTypedRequest<RegistrationRequest, RegistrationResponse>
{
	public RegistrationRequest()
		: base("RegistrationRequest")
	{
	}

	public RegistrationRequest(BCGSInstance instance)
		: base(instance, "RegistrationRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new RegistrationResponse(response);
	}

	public RegistrationRequest SetDisplayName(string displayName)
	{
		request.AddString("displayName", displayName);
		return this;
	}

	public RegistrationRequest SetPassword(string password)
	{
		request.AddString("password", password);
		return this;
	}

	public RegistrationRequest SetSegments(BCGSRequestData segments)
	{
		request.AddObject("segments", segments);
		return this;
	}

	public RegistrationRequest SetUserName(string userName)
	{
		request.AddString("userName", userName);
		return this;
	}
}
