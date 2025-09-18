using BCGSComponents.DataModels;

namespace BCGSComponents;

public class AuthenticationRequest : BCGSTypedRequest<AuthenticationRequest, AuthenticationResponse>
{
	public AuthenticationRequest()
		: base("AuthenticationRequest")
	{
	}

	public AuthenticationRequest(BCGSInstance instance)
		: base(instance, "AuthenticationRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new AuthenticationResponse(response);
	}

	public AuthenticationRequest SetPassword(string password)
	{
		request.AddString("password", password);
		return this;
	}

	public AuthenticationRequest SetUserName(string userName)
	{
		request.AddString("userName", userName);
		return this;
	}
}
