using System.Collections.Generic;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;

namespace BrainCloud;

public class BrainCloudDataStream
{
	private BrainCloudClient _client;

	public BrainCloudDataStream(BrainCloudClient client)
	{
		_client = client;
	}

	public void CustomPageEvent(string eventName, string jsonEventProperties, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.DataStreamEventName.Value] = eventName;
		if (Util.IsOptionalParameterValid(jsonEventProperties))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonEventProperties);
			dictionary[OperationParam.DataStreamEventProperties.Value] = value;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.DataStream, ServiceOperation.CustomPageEvent, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void CustomScreenEvent(string eventName, string jsonEventProperties, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.DataStreamEventName.Value] = eventName;
		if (Util.IsOptionalParameterValid(jsonEventProperties))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonEventProperties);
			dictionary[OperationParam.DataStreamEventProperties.Value] = value;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.DataStream, ServiceOperation.CustomScreenEvent, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void CustomTrackEvent(string eventName, string jsonEventProperties, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.DataStreamEventName.Value] = eventName;
		if (Util.IsOptionalParameterValid(jsonEventProperties))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonEventProperties);
			dictionary[OperationParam.DataStreamEventProperties.Value] = value;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.DataStream, ServiceOperation.CustomTrackEvent, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void SubmitCrashReport(string crashType, string errorMsg, string crashJson, string crashLog, string userName, string userEmail, string userNotes, bool userSubmitted, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.DataStreamCrashType.Value] = crashType;
		dictionary[OperationParam.DataStreamErrorMsg.Value] = errorMsg;
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(crashJson);
		dictionary[OperationParam.DataStreamCrashInfo.Value] = value;
		dictionary[OperationParam.DataStreamCrashLog.Value] = crashLog;
		dictionary[OperationParam.DataStreamUserName.Value] = userName;
		dictionary[OperationParam.DataStreamUserEmail.Value] = userEmail;
		dictionary[OperationParam.DataStreamUserNotes.Value] = userNotes;
		dictionary[OperationParam.DataStreamUserSubmitted.Value] = userSubmitted;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.DataStream, ServiceOperation.SubmitCrashReport, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}
}
