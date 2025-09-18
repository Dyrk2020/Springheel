using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ListMessageSummaryResponse : BCGSTypedResponse
{
	public BCGSEnumerable<BCGSData> MessageList => new BCGSEnumerable<BCGSData>(response.GetObjectList("messageList"), (BCGSData data) => new BCGSData(data));

	public ListMessageSummaryResponse(BCGSData data)
		: base(data)
	{
	}
}
