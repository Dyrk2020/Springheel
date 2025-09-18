using System.Collections.Generic;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;

namespace BrainCloud;

public class BrainCloudEvent
{
	private BrainCloudClient _client;

	public BrainCloudEvent(BrainCloudClient client)
	{
		_client = client;
	}

	public void SendEvent(string toProfileId, string eventType, string jsonEventData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.EventServiceSendToId.Value] = toProfileId;
		dictionary[OperationParam.EventServiceSendEventType.Value] = eventType;
		if (Util.IsOptionalParameterValid(jsonEventData))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonEventData);
			dictionary[OperationParam.EventServiceSendEventData.Value] = value;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Event, ServiceOperation.Send, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void UpdateIncomingEventData(string evId, string jsonEventData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.EvId.Value] = evId;
		if (Util.IsOptionalParameterValid(jsonEventData))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonEventData);
			dictionary[OperationParam.EventServiceUpdateEventDataData.Value] = value;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Event, ServiceOperation.UpdateEventData, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void DeleteIncomingEvent(string evId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.EvId.Value] = evId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Event, ServiceOperation.DeleteIncoming, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void DeleteIncomingEvents(string[] in_eventIds, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.EventServiceEvIds.Value] = in_eventIds;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Event, ServiceOperation.DeleteIncomingEvents, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void DeleteIncomingEventsOlderThan(int in_dateMillis, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.EventServiceDateMillis.Value] = in_dateMillis;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Event, ServiceOperation.DeleteIncomingEventsOlderThan, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void DeleteIncomingEventsByTypeOlderThan(string in_eventId, int in_dateMillis, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.EventServiceDateMillis.Value] = in_dateMillis;
		dictionary[OperationParam.EventServiceEventType.Value] = in_eventId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Event, ServiceOperation.DeleteIncomingEventsByTypeOlderThan, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetEvents(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> jsonData = new Dictionary<string, object>();
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Event, ServiceOperation.GetEvents, jsonData, callback);
		_client.SendRequest(serviceMessage);
	}
}
