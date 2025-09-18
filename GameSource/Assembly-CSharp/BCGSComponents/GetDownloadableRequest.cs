using BCGSComponents.DataModels;

namespace BCGSComponents;

public class GetDownloadableRequest : BCGSTypedRequest<GetDownloadableRequest, GetDownloadableResponse>
{
	public GetDownloadableRequest()
		: base("GetDownloadableRequest")
	{
	}

	public GetDownloadableRequest(BCGSInstance instance)
		: base(instance, "GetDownloadableRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new GetDownloadableResponse(response);
	}

	public GetDownloadableRequest SetShortCode(string shortCode)
	{
		request.AddString("shortCode", shortCode);
		return this;
	}
}
