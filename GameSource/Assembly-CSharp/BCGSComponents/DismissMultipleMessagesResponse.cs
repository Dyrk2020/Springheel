using System.Collections.Generic;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class DismissMultipleMessagesResponse : BCGSTypedResponse
{
	public List<string> FailedDismissals => response.GetStringList("failedDismissals");

	public int? MessagesDismissed => response.GetInt("messagesDismissed");

	public DismissMultipleMessagesResponse(BCGSData data)
		: base(data)
	{
	}
}
