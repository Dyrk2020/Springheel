using BCGSComponents.DataModels;

namespace BCGSComponents;

public class GetPropertyResponse : BCGSTypedResponse
{
	public BCGSData Property => response.GetObject("property");

	public GetPropertyResponse(BCGSData data)
		: base(data)
	{
	}
}
