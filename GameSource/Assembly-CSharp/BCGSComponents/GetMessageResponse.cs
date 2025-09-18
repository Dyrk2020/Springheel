using BCGSComponents.DataModels;

namespace BCGSComponents;

public class GetMessageResponse : BCGSTypedResponse
{
	public BCGSData Message => response.GetObject("message");

	public string Status => response.GetString("status");

	public GetMessageResponse(BCGSData data)
		: base(data)
	{
	}
}
