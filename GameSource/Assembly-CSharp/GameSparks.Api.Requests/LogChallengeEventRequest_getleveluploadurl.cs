using GameSparks.Api.Responses;
using GameSparks.Core;

namespace GameSparks.Api.Requests;

public class LogChallengeEventRequest_getleveluploadurl : GSTypedRequest<LogChallengeEventRequest_getleveluploadurl, LogChallengeEventResponse>
{
	public LogChallengeEventRequest_getleveluploadurl()
		: base("LogChallengeEventRequest")
	{
		request.AddString("eventKey", "getleveluploadurl");
	}

	protected override GSTypedResponse BuildResponse(GSObject response)
	{
		return new LogChallengeEventResponse(response);
	}

	public LogChallengeEventRequest_getleveluploadurl SetChallengeInstanceId(string challengeInstanceId)
	{
		request.AddString("challengeInstanceId", challengeInstanceId);
		return this;
	}

	public LogChallengeEventRequest_getleveluploadurl Set_code(string value)
	{
		request.AddString("code", value);
		return this;
	}

	public LogChallengeEventRequest_getleveluploadurl Set_incrementGetCount(long value)
	{
		request.AddNumber("incrementGetCount", value);
		return this;
	}
}
