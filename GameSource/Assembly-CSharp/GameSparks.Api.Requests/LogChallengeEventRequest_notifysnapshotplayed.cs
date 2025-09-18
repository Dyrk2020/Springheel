using GameSparks.Api.Responses;
using GameSparks.Core;

namespace GameSparks.Api.Requests;

public class LogChallengeEventRequest_notifysnapshotplayed : GSTypedRequest<LogChallengeEventRequest_notifysnapshotplayed, LogChallengeEventResponse>
{
	public LogChallengeEventRequest_notifysnapshotplayed()
		: base("LogChallengeEventRequest")
	{
		request.AddString("eventKey", "notifysnapshotplayed");
	}

	protected override GSTypedResponse BuildResponse(GSObject response)
	{
		return new LogChallengeEventResponse(response);
	}

	public LogChallengeEventRequest_notifysnapshotplayed SetChallengeInstanceId(string challengeInstanceId)
	{
		request.AddString("challengeInstanceId", challengeInstanceId);
		return this;
	}

	public LogChallengeEventRequest_notifysnapshotplayed Set_code(string value)
	{
		request.AddString("code", value);
		return this;
	}
}
