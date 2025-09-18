using BCGSComponents.DataModels;

namespace BCGSComponents;

public class GetUploadedRequest : BCGSTypedRequest<GetUploadedRequest, GetUploadedResponse>
{
	public GetUploadedRequest()
		: base("GetUploadedRequest")
	{
	}

	public GetUploadedRequest(BCGSInstance instance)
		: base(instance, "GetUploadedRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new GetUploadedResponse(response);
	}

	public GetUploadedRequest SetUploadId(string uploadId)
	{
		request.AddString("uploadId", uploadId);
		return this;
	}
}
