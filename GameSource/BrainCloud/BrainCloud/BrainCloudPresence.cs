using System.Collections.Generic;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;

namespace BrainCloud;

public class BrainCloudPresence
{
	private BrainCloudClient _client;

	public BrainCloudPresence(BrainCloudClient client)
	{
		_client = client;
	}

	public void ForcePush(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> jsonData = new Dictionary<string, object>();
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Presence, ServiceOperation.ForcePush, jsonData, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetPresenceOfFriends(string platform, bool includeOffline, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PresenceServicePlatform.Value] = platform;
		dictionary[OperationParam.PresenceServiceIncludeOffline.Value] = includeOffline;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Presence, ServiceOperation.GetPresenceOfFriends, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetPresenceOfGroup(string groupId, bool includeOffline, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PresenceServiceGroupId.Value] = groupId;
		dictionary[OperationParam.PresenceServiceIncludeOffline.Value] = includeOffline;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Presence, ServiceOperation.GetPresenceOfGroup, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetPresenceOfUsers(List<string> profileIds, bool includeOffline, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PresenceServiceProfileIds.Value] = profileIds;
		dictionary[OperationParam.PresenceServiceIncludeOffline.Value] = includeOffline;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Presence, ServiceOperation.GetPresenceOfUsers, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void RegisterListenersForFriends(string platform, bool bidirectional, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PresenceServicePlatform.Value] = platform;
		dictionary[OperationParam.PresenceServiceBidirectional.Value] = bidirectional;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Presence, ServiceOperation.RegisterListenersForFriends, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void RegisterListenersForGroup(string groupId, bool bidirectional, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PresenceServiceGroupId.Value] = groupId;
		dictionary[OperationParam.PresenceServiceBidirectional.Value] = bidirectional;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Presence, ServiceOperation.RegisterListenersForGroup, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void RegisterListenersForProfiles(List<string> profileIds, bool bidirectional, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PresenceServiceProfileIds.Value] = profileIds;
		dictionary[OperationParam.PresenceServiceBidirectional.Value] = bidirectional;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Presence, ServiceOperation.RegisterListenersForProfiles, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void SetVisibility(bool visible, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PresenceServiceVisibile.Value] = visible;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Presence, ServiceOperation.SetVisibility, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void StopListening(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> jsonData = new Dictionary<string, object>();
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Presence, ServiceOperation.StopListening, jsonData, callback);
		_client.SendRequest(serviceMessage);
	}

	public void UpdateActivity(string jsonActivity, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonActivity);
		dictionary[OperationParam.PresenceServiceActivity.Value] = value;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Presence, ServiceOperation.UpdateActivity, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}
}
