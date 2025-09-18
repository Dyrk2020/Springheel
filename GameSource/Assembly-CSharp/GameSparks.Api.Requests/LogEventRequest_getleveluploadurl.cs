using GameSparks.Api.Responses;
using GameSparks.Core;

namespace GameSparks.Api.Requests;

public class LogEventRequest_getleveluploadurl : GSTypedRequest<LogEventRequest_getleveluploadurl, LogEventResponse>
{
	protected override GSTypedResponse BuildResponse(GSObject response)
	{
		return new LogEventResponse(response);
	}

	public LogEventRequest_getleveluploadurl()
		: base("LogEventRequest")
	{
		request.AddString("eventKey", "getleveluploadurl");
	}

	public LogEventRequest_getleveluploadurl Set_code(string value)
	{
		request.AddString("code", value);
		return this;
	}

	public LogEventRequest_getleveluploadurl Set_incrementGetCount(long value)
	{
		request.AddNumber("incrementGetCount", value);
		return this;
	}
}
