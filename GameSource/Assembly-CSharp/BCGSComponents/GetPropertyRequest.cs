using BCGSComponents.DataModels;

namespace BCGSComponents;

public class GetPropertyRequest : BCGSTypedRequest<GetPropertyRequest, GetPropertyResponse>
{
	public GetPropertyRequest()
		: base("GetPropertyRequest")
	{
	}

	public GetPropertyRequest(BCGSInstance instance)
		: base(instance, "GetPropertyRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new GetPropertyResponse(response);
	}

	public GetPropertyRequest SetPropertyShortCode(string propertyShortCode)
	{
		request.AddString("propertyShortCode", propertyShortCode);
		return this;
	}
}
