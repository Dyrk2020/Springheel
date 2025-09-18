using BCGSComponents.DataModels;

namespace BCGSComponents;

public class GetUploadedResponse : BCGSTypedResponse
{
	public long? Size => response.GetLong("size");

	public string Url => response.GetString("url");

	public GetUploadedResponse(BCGSData data)
		: base(data)
	{
	}
}
