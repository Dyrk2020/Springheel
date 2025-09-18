using System.Collections.Generic;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class LogChallengeEventRequest : BCGSTypedRequest<LogChallengeEventRequest, LogChallengeEventResponse>
{
	public LogChallengeEventRequest()
		: base("LogChallengeEventRequest")
	{
	}

	public LogChallengeEventRequest(BCGSInstance instance)
		: base(instance, "LogChallengeEventRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new LogChallengeEventResponse(response);
	}

	public LogChallengeEventRequest SetEventAttribute(string key, long value)
	{
		request.AddNumber(key, value);
		return this;
	}

	public LogChallengeEventRequest SetEventAttribute(string key, int value)
	{
		request.AddNumber(key, value);
		return this;
	}

	public LogChallengeEventRequest SetEventAttribute(string key, string value)
	{
		request.AddString(key, value);
		return this;
	}

	public LogChallengeEventRequest SetEventAttribute(string key, BCGSRequestData value)
	{
		request.AddObject(key, value);
		return this;
	}

	public LogChallengeEventRequest SetEventAttribute(string key, List<BCGSData> value)
	{
		request.AddObjectList(key, value);
		return this;
	}

	public LogChallengeEventRequest SetEventAttribute(string key, List<string> value)
	{
		request.AddStringList(key, value);
		return this;
	}

	public LogChallengeEventRequest SetEventAttribute(string key, List<long> value)
	{
		request.AddNumberList(key, value);
		return this;
	}

	public LogChallengeEventRequest SetEventAttribute(string key, List<int> value)
	{
		request.AddNumberList(key, value);
		return this;
	}

	public LogChallengeEventRequest SetChallengeInstanceId(string challengeInstanceId)
	{
		request.AddString("challengeInstanceId", challengeInstanceId);
		return this;
	}

	public LogChallengeEventRequest SetEventKey(string eventKey)
	{
		request.AddString("eventKey", eventKey);
		return this;
	}
}
