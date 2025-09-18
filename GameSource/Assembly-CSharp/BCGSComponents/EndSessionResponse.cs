using BCGSComponents.DataModels;

namespace BCGSComponents;

public class EndSessionResponse : BCGSTypedResponse
{
	public long? SessionDuration => response.GetLong("sessionDuration");

	public EndSessionResponse(BCGSData data)
		: base(data)
	{
	}
}
