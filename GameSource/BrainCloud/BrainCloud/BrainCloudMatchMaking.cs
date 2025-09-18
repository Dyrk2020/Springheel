using System.Collections.Generic;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;

namespace BrainCloud;

public class BrainCloudMatchMaking
{
	private BrainCloudClient _client;

	public BrainCloudMatchMaking(BrainCloudClient client)
	{
		_client = client;
	}

	public void Read(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.MatchMaking, ServiceOperation.Read, null, callback);
		_client.SendRequest(serviceMessage);
	}

	public void SetPlayerRating(long playerRating, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.MatchMakingServicePlayerRating.Value] = playerRating;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.MatchMaking, ServiceOperation.SetPlayerRating, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ResetPlayerRating(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.MatchMaking, ServiceOperation.ResetPlayerRating, null, callback);
		_client.SendRequest(serviceMessage);
	}

	public void IncrementPlayerRating(long increment, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.MatchMakingServicePlayerRating.Value] = increment;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.MatchMaking, ServiceOperation.IncrementPlayerRating, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void DecrementPlayerRating(long decrement, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.MatchMakingServicePlayerRating.Value] = decrement;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.MatchMaking, ServiceOperation.DecrementPlayerRating, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void TurnShieldOn(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.MatchMaking, ServiceOperation.ShieldOn, null, callback);
		_client.SendRequest(serviceMessage);
	}

	public void TurnShieldOnFor(int minutes, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.MatchMakingServiceMinutes.Value] = minutes;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.MatchMaking, ServiceOperation.ShieldOnFor, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void TurnShieldOff(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.MatchMaking, ServiceOperation.ShieldOff, null, callback);
		_client.SendRequest(serviceMessage);
	}

	public void IncrementShieldOnFor(int minutes, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.MatchMakingServiceMinutes.Value] = minutes;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.MatchMaking, ServiceOperation.IncrementShieldOnFor, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetShieldExpiry(string playerId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (Util.IsOptionalParameterValid(playerId))
		{
			dictionary[OperationParam.MatchMakingServicePlayerId.Value] = playerId;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.MatchMaking, ServiceOperation.GetShieldExpiry, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void FindPlayers(long rangeDelta, long numMatches, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		FindPlayersWithAttributes(rangeDelta, numMatches, null, success, failure, cbObject);
	}

	public void FindPlayersWithAttributes(long rangeDelta, long numMatches, string jsonAttributes, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.MatchMakingServiceRangeDelta.Value] = rangeDelta;
		dictionary[OperationParam.MatchMakingServiceNumMatches.Value] = numMatches;
		if (Util.IsOptionalParameterValid(jsonAttributes))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonAttributes);
			dictionary[OperationParam.MatchMakingServiceAttributes.Value] = value;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.MatchMaking, ServiceOperation.FindPlayers, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void FindPlayersUsingFilter(long rangeDelta, long numMatches, string jsonExtraParms, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		FindPlayersWithAttributesUsingFilter(rangeDelta, numMatches, null, jsonExtraParms, success, failure, cbObject);
	}

	public void FindPlayersWithAttributesUsingFilter(long rangeDelta, long numMatches, string jsonAttributes, string jsonExtraParms, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.MatchMakingServiceRangeDelta.Value] = rangeDelta;
		dictionary[OperationParam.MatchMakingServiceNumMatches.Value] = numMatches;
		if (Util.IsOptionalParameterValid(jsonAttributes))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonAttributes);
			dictionary[OperationParam.MatchMakingServiceAttributes.Value] = value;
		}
		if (Util.IsOptionalParameterValid(jsonExtraParms))
		{
			Dictionary<string, object> value2 = JsonReader.Deserialize<Dictionary<string, object>>(jsonExtraParms);
			dictionary[OperationParam.MatchMakingServiceExtraParams.Value] = value2;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.MatchMaking, ServiceOperation.FindPlayersUsingFilter, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void EnableMatchMaking(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.MatchMaking, ServiceOperation.EnableMatchMaking, null, callback);
		_client.SendRequest(serviceMessage);
	}

	public void DisableMatchMaking(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.MatchMaking, ServiceOperation.DisableMatchMaking, null, callback);
		_client.SendRequest(serviceMessage);
	}
}
