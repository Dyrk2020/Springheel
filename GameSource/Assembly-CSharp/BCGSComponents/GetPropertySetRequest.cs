using BCGSComponents.DataModels;

namespace BCGSComponents;

public class GetPropertySetRequest : BCGSTypedRequest<GetPropertySetRequest, GetPropertySetResponse>
{
	public GetPropertySetRequest()
		: base("GetPropertySetRequest")
	{
	}

	public GetPropertySetRequest(BCGSInstance instance)
		: base(instance, "GetPropertySetRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new GetPropertySetResponse(response);
	}

	public GetPropertySetRequest SetPropertySetShortCode(string propertySetShortCode)
	{
		request.AddString("propertySetShortCode", propertySetShortCode);
		return this;
	}
}
