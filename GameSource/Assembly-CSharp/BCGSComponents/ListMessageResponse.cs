using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ListMessageResponse : BCGSTypedResponse
{
	public BCGSEnumerable<BCGSData> MessageList => new BCGSEnumerable<BCGSData>(response.GetObjectList("messageList"), (BCGSData data) => new BCGSData(data));

	public ListMessageResponse(BCGSData data)
		: base(data)
	{
	}
}
