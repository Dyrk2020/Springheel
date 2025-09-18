using BCGSComponents.DataModels;

namespace BCGSComponents;

public class GetPropertySetResponse : BCGSTypedResponse
{
	public BCGSData PropertySet => response.GetObject("propertySet");

	public GetPropertySetResponse(BCGSData data)
		: base(data)
	{
	}
}
