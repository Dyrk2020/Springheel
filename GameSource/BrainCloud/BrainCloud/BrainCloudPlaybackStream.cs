using System.Collections.Generic;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;

namespace BrainCloud;

public class BrainCloudPlaybackStream
{
	private BrainCloudClient _client;

	public BrainCloudPlaybackStream(BrainCloudClient client)
	{
		_client = client;
	}

	public void StartStream(string targetPlayerId, bool includeSharedData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PlaybackStreamServiceTargetPlayerId.Value] = targetPlayerId;
		dictionary[OperationParam.PlaybackStreamServiceIncludeSharedData.Value] = includeSharedData;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlaybackStream, ServiceOperation.StartStream, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadStream(string playbackStreamId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PlaybackStreamServicePlaybackStreamId.Value] = playbackStreamId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlaybackStream, ServiceOperation.ReadStream, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void EndStream(string playbackStreamId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PlaybackStreamServicePlaybackStreamId.Value] = playbackStreamId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlaybackStream, ServiceOperation.EndStream, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void DeleteStream(string playbackStreamId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PlaybackStreamServicePlaybackStreamId.Value] = playbackStreamId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlaybackStream, ServiceOperation.DeleteStream, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void AddEvent(string playbackStreamId, string eventData, string summary, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PlaybackStreamServicePlaybackStreamId.Value] = playbackStreamId;
		if (Util.IsOptionalParameterValid(eventData))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(eventData);
			dictionary[OperationParam.PlaybackStreamServiceEventData.Value] = value;
		}
		if (Util.IsOptionalParameterValid(summary))
		{
			Dictionary<string, object> value2 = JsonReader.Deserialize<Dictionary<string, object>>(summary);
			dictionary[OperationParam.PlaybackStreamServiceSummary.Value] = value2;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlaybackStream, ServiceOperation.AddEvent, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetRecentStreamsForInitiatingPlayer(string initiatingPlayerId, int maxNumStreams, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PlaybackStreamServiceInitiatingPlayerId.Value] = initiatingPlayerId;
		dictionary[OperationParam.PlaybackStreamServiceMaxNumberOfStreams.Value] = maxNumStreams;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlaybackStream, ServiceOperation.GetRecentStreamsForInitiatingPlayer, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetRecentStreamsForTargetPlayer(string targetPlayerId, int maxNumStreams, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PlaybackStreamServiceTargetPlayerId.Value] = targetPlayerId;
		dictionary[OperationParam.PlaybackStreamServiceMaxNumberOfStreams.Value] = maxNumStreams;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlaybackStream, ServiceOperation.GetRecentStreamsForTargetPlayer, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}
}
