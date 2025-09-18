using System.Collections.Generic;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class LogEventRequest : BCGSTypedRequest<LogEventRequest, LogEventResponse>
{
	public LogEventRequest()
		: base("LogEventRequest")
	{
	}

	public LogEventRequest(BCGSInstance instance)
		: base(instance, "LogEventRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new LogEventResponse(response);
	}

	public LogEventRequest SetEventAttribute(string key, long value)
	{
		request.AddNumber(key, value);
		return this;
	}

	public LogEventRequest SetEventAttribute(string key, int value)
	{
		request.AddNumber(key, value);
		return this;
	}

	public LogEventRequest SetEventAttribute(string key, string value)
	{
		request.AddString(key, value);
		return this;
	}

	public LogEventRequest SetEventAttribute(string key, bool value)
	{
		request.AddBoolean(key, value);
		return this;
	}

	public LogEventRequest SetEventAttribute(string key, BCGSRequestData value)
	{
		request.AddObject(key, value);
		return this;
	}

	public LogEventRequest SetEventAttribute(string key, List<BCGSData> value)
	{
		request.AddObjectList(key, value);
		return this;
	}

	public LogEventRequest SetEventAttribute(string key, List<string> value)
	{
		request.AddStringList(key, value);
		return this;
	}

	public LogEventRequest SetEventAttribute(string key, List<long> value)
	{
		request.AddNumberList(key, value);
		return this;
	}

	public LogEventRequest SetEventAttribute(string key, List<int> value)
	{
		request.AddNumberList(key, value);
		return this;
	}

	public new LogEventRequest SetScriptData(BCGSRequestData value)
	{
		base.SetScriptData(value);
		return this;
	}

	public LogEventRequest SetEventKey(string eventKey)
	{
		request.AddString("eventKey", eventKey);
		return this;
	}

	public LogEventRequest AddCustomData(string paramName, object value)
	{
		request.BaseData.Add(paramName, value);
		return this;
	}
}
