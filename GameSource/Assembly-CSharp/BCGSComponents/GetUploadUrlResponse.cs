using BCGSComponents.DataModels;

namespace BCGSComponents;

public class GetUploadUrlResponse : BCGSTypedResponse
{
	public string Url => response.GetString("url");

	public GetUploadUrlResponse(BCGSData data)
		: base(data)
	{
	}
}
