using BCGSComponents.DataModels;

namespace BCGSComponents;

public class GetUploadUrlRequest : BCGSTypedRequest<GetUploadUrlRequest, GetUploadUrlResponse>
{
	public GetUploadUrlRequest()
		: base("GetUploadUrlRequest")
	{
	}

	public GetUploadUrlRequest(BCGSInstance instance)
		: base(instance, "GetUploadUrlRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new GetUploadUrlResponse(response);
	}

	public GetUploadUrlRequest SetUploadData(BCGSRequestData uploadData)
	{
		request.AddObject("uploadData", uploadData);
		return this;
	}
}
