using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ChangeUserDetailsRequest : BCGSTypedRequest<ChangeUserDetailsRequest, ChangeUserDetailsResponse>
{
	public ChangeUserDetailsRequest()
		: base("ChangeUserDetailsRequest")
	{
	}

	public ChangeUserDetailsRequest(BCGSInstance instance)
		: base(instance, "ChangeUserDetailsRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new ChangeUserDetailsResponse(response);
	}

	public ChangeUserDetailsRequest SetDisplayName(string displayName)
	{
		request.AddString("displayName", displayName);
		return this;
	}

	public ChangeUserDetailsRequest SetLanguage(string language)
	{
		request.AddString("language", language);
		return this;
	}

	public ChangeUserDetailsRequest SetNewPassword(string newPassword)
	{
		request.AddString("newPassword", newPassword);
		return this;
	}

	public ChangeUserDetailsRequest SetOldPassword(string oldPassword)
	{
		request.AddString("oldPassword", oldPassword);
		return this;
	}

	public ChangeUserDetailsRequest SetUserName(string userName)
	{
		request.AddString("userName", userName);
		return this;
	}
}
