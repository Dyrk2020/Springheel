using BCGSComponents.DataModels;

namespace BCGSComponents;

public class AccountDetailsRequest : BCGSTypedRequest<AccountDetailsRequest, AccountDetailsResponse>
{
	public AccountDetailsRequest()
		: base("AccountDetailsRequest")
	{
	}

	public AccountDetailsRequest(BCGSInstance instance)
		: base(instance, "AccountDetailsRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new AccountDetailsResponse(response);
	}
}
