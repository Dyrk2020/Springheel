using BCGSComponents.DataModels;

namespace BCGSComponents;

public class BatchAdminResponse : BCGSTypedResponse
{
	public BCGSData Responses => response.GetObject("responses");

	public BatchAdminResponse(BCGSData data)
		: base(data)
	{
	}
}
