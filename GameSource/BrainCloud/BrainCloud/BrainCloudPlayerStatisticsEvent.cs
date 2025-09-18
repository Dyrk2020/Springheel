using System;
using System.Collections.Generic;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;

namespace BrainCloud;

public class BrainCloudPlayerStatisticsEvent
{
	private BrainCloudClient _client;

	public BrainCloudPlayerStatisticsEvent(BrainCloudClient client)
	{
		_client = client;
	}

	[Obsolete("This has been deprecated use TriggerUserStatsEvent instead - removal after September 1 2021")]
	public void TriggerStatsEvent(string eventName, int eventMultiplier, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PlayerStatisticEventServiceEventName.Value] = eventName;
		dictionary[OperationParam.PlayerStatisticEventServiceEventMultiplier.Value] = eventMultiplier;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerStatisticsEvent, ServiceOperation.Trigger, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void TriggerUserStatsEvent(string eventName, int eventMultiplier, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PlayerStatisticEventServiceEventName.Value] = eventName;
		dictionary[OperationParam.PlayerStatisticEventServiceEventMultiplier.Value] = eventMultiplier;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerStatisticsEvent, ServiceOperation.Trigger, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	[Obsolete("This has been deprecated use TriggerUserStatsEvents instead - removal after September 1 2021")]
	public void TriggerStatsEvents(string jsonData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		object[] value = JsonReader.Deserialize<object[]>(jsonData);
		dictionary[OperationParam.PlayerStatisticEventServiceEvents.Value] = value;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerStatisticsEvent, ServiceOperation.TriggerMultiple, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void TriggerUserStatsEvents(string jsonData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		object[] value = JsonReader.Deserialize<object[]>(jsonData);
		dictionary[OperationParam.PlayerStatisticEventServiceEvents.Value] = value;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerStatisticsEvent, ServiceOperation.TriggerMultiple, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}
}
