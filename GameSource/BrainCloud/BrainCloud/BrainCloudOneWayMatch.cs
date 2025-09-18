using System.Collections.Generic;
using BrainCloud.Internal;

namespace BrainCloud;

public class BrainCloudOneWayMatch
{
	private BrainCloudClient _client;

	public BrainCloudOneWayMatch(BrainCloudClient client)
	{
		_client = client;
	}

	public void StartMatch(string otherPlayerId, long rangeDelta, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.OfflineMatchServicePlayerId.Value] = otherPlayerId;
		dictionary[OperationParam.OfflineMatchServiceRangeDelta.Value] = rangeDelta;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.OneWayMatch, ServiceOperation.StartMatch, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void CancelMatch(string playbackStreamId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.OfflineMatchServicePlaybackStreamId.Value] = playbackStreamId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.OneWayMatch, ServiceOperation.CancelMatch, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void CompleteMatch(string playbackStreamId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.OfflineMatchServicePlaybackStreamId.Value] = playbackStreamId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.OneWayMatch, ServiceOperation.CompleteMatch, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}
}
