using GameSparks.Api.Responses;
using GameSparks.Core;

namespace GameSparks.Api.Requests;

public class LogEventRequest_notifysnapshotplayed : GSTypedRequest<LogEventRequest_notifysnapshotplayed, LogEventResponse>
{
	protected override GSTypedResponse BuildResponse(GSObject response)
	{
		return new LogEventResponse(response);
	}

	public LogEventRequest_notifysnapshotplayed()
		: base("LogEventRequest")
	{
		request.AddString("eventKey", "notifysnapshotplayed");
	}

	public LogEventRequest_notifysnapshotplayed Set_code(string value)
	{
		request.AddString("code", value);
		return this;
	}
}
