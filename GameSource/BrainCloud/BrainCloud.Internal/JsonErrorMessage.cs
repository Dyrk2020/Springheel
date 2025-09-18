using BrainCloud.JsonFx.Json;

namespace BrainCloud.Internal;

internal class JsonErrorMessage
{
	public int reason_code;

	public int status;

	public string status_message;

	public string severity = "ERROR";

	public JsonErrorMessage()
	{
	}

	public JsonErrorMessage(int status, int reasonCode, string statusMessage)
	{
		this.status = status;
		reason_code = reasonCode;
		status_message = statusMessage;
	}

	public string GetJsonString()
	{
		return JsonWriter.Serialize(this);
	}
}
