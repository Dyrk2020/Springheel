using System;
using System.Collections.Generic;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;

namespace BrainCloud;

public class BrainCloudScript
{
	private BrainCloudClient _client;

	public BrainCloudScript(BrainCloudClient client)
	{
		_client = client;
	}

	public void RunScript(string scriptName, string jsonScriptData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ScriptServiceRunScriptName.Value] = scriptName;
		if (Util.IsOptionalParameterValid(jsonScriptData))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonScriptData);
			dictionary[OperationParam.ScriptServiceRunScriptData.Value] = value;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Script, ServiceOperation.Run, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ScheduleRunScriptMillisUTC(string scriptName, string jsonScriptData, ulong roundStartTimeUTC, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ScriptServiceRunScriptName.Value] = scriptName;
		if (Util.IsOptionalParameterValid(jsonScriptData))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonScriptData);
			dictionary[OperationParam.ScriptServiceRunScriptData.Value] = value;
		}
		dictionary[OperationParam.ScriptServiceStartDateUTC.Value] = roundStartTimeUTC;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Script, ServiceOperation.ScheduleCloudScript, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ScheduleRunScriptMinutes(string scriptName, string jsonScriptData, long minutesFromNow, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ScriptServiceRunScriptName.Value] = scriptName;
		if (Util.IsOptionalParameterValid(jsonScriptData))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonScriptData);
			dictionary[OperationParam.ScriptServiceRunScriptData.Value] = value;
		}
		dictionary[OperationParam.ScriptServiceStartMinutesFromNow.Value] = minutesFromNow;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Script, ServiceOperation.ScheduleCloudScript, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void RunParentScript(string scriptName, string jsonScriptData, string parentLevel, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ScriptServiceRunScriptName.Value] = scriptName;
		if (Util.IsOptionalParameterValid(jsonScriptData))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonScriptData);
			dictionary[OperationParam.ScriptServiceRunScriptData.Value] = value;
		}
		dictionary[OperationParam.ScriptServiceParentLevel.Value] = parentLevel;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Script, ServiceOperation.RunParentScript, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void CancelScheduledScript(string jobId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ScriptServiceJobId.Value] = jobId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Script, ServiceOperation.CancelScheduledScript, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetScheduledCloudScripts(DateTime startDateUTC, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ScriptServiceStartDateUTC.Value] = startDateUTC;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Script, ServiceOperation.GetScheduledCloudScripts, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetRunningOrQueuedCloudScripts(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Script, ServiceOperation.GetRunningOrQueuedCloudScripts, null, callback);
		_client.SendRequest(serviceMessage);
	}

	public void RunPeerScript(string scriptName, string jsonScriptData, string peer, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ScriptServiceRunScriptName.Value] = scriptName;
		if (Util.IsOptionalParameterValid(jsonScriptData))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonScriptData);
			dictionary[OperationParam.ScriptServiceRunScriptData.Value] = value;
		}
		dictionary[OperationParam.Peer.Value] = peer;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Script, ServiceOperation.RunPeerScript, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void RunPeerScriptAsync(string scriptName, string jsonScriptData, string peer, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ScriptServiceRunScriptName.Value] = scriptName;
		if (Util.IsOptionalParameterValid(jsonScriptData))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonScriptData);
			dictionary[OperationParam.ScriptServiceRunScriptData.Value] = value;
		}
		dictionary[OperationParam.Peer.Value] = peer;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Script, ServiceOperation.RunPeerScriptAsync, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}
}
