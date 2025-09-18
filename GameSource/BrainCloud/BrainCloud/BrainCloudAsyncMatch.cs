using System.Collections.Generic;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;

namespace BrainCloud;

public class BrainCloudAsyncMatch
{
	private BrainCloudClient _client;

	public BrainCloudAsyncMatch(BrainCloudClient client)
	{
		_client = client;
	}

	public void CreateMatch(string jsonOpponentIds, string pushNotificationMessage, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		CreateMatchInternal(jsonOpponentIds, null, pushNotificationMessage, null, null, null, success, failure, cbObject);
	}

	public void CreateMatchWithInitialTurn(string jsonOpponentIds, string jsonMatchState, string pushNotificationMessage, string nextPlayer, string jsonSummary, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		CreateMatchInternal(jsonOpponentIds, (jsonMatchState == null) ? "{}" : jsonMatchState, pushNotificationMessage, null, nextPlayer, jsonSummary, success, failure, cbObject);
	}

	public void SubmitTurn(string ownerId, string matchId, ulong version, string jsonMatchState, string pushNotificationMessage, string nextPlayer, string jsonSummary, string jsonStatistics, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["ownerId"] = ownerId;
		dictionary["matchId"] = matchId;
		dictionary["version"] = version;
		dictionary["matchState"] = JsonReader.Deserialize<Dictionary<string, object>>(jsonMatchState);
		if (Util.IsOptionalParameterValid(nextPlayer))
		{
			Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
			dictionary2["currentPlayer"] = nextPlayer;
			dictionary["status"] = dictionary2;
		}
		if (Util.IsOptionalParameterValid(jsonSummary))
		{
			dictionary["summary"] = JsonReader.Deserialize<Dictionary<string, object>>(jsonSummary);
		}
		if (Util.IsOptionalParameterValid(jsonStatistics))
		{
			dictionary["statistics"] = JsonReader.Deserialize<Dictionary<string, object>>(jsonStatistics);
		}
		if (Util.IsOptionalParameterValid(pushNotificationMessage))
		{
			dictionary["pushContent"] = pushNotificationMessage;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.AsyncMatch, ServiceOperation.SubmitTurn, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void UpdateMatchSummaryData(string ownerId, string matchId, ulong version, string jsonSummary, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["ownerId"] = ownerId;
		dictionary["matchId"] = matchId;
		dictionary["version"] = version;
		if (Util.IsOptionalParameterValid(jsonSummary))
		{
			dictionary["summary"] = JsonReader.Deserialize<Dictionary<string, object>>(jsonSummary);
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.AsyncMatch, ServiceOperation.UpdateMatchSummary, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void CompleteMatch(string ownerId, string matchId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["ownerId"] = ownerId;
		dictionary["matchId"] = matchId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.AsyncMatch, ServiceOperation.Complete, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadMatch(string ownerId, string matchId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["ownerId"] = ownerId;
		dictionary["matchId"] = matchId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.AsyncMatch, ServiceOperation.ReadMatch, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadMatchHistory(string ownerId, string matchId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["ownerId"] = ownerId;
		dictionary["matchId"] = matchId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.AsyncMatch, ServiceOperation.ReadMatchHistory, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void FindMatches(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.AsyncMatch, ServiceOperation.FindMatches, null, callback);
		_client.SendRequest(serviceMessage);
	}

	public void FindCompleteMatches(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.AsyncMatch, ServiceOperation.FindMatchesCompleted, null, callback);
		_client.SendRequest(serviceMessage);
	}

	public void AbandonMatch(string ownerId, string matchId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["ownerId"] = ownerId;
		dictionary["matchId"] = matchId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.AsyncMatch, ServiceOperation.Abandon, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void DeleteMatch(string ownerId, string matchId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["ownerId"] = ownerId;
		dictionary["matchId"] = matchId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.AsyncMatch, ServiceOperation.DeleteMatch, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void CompleteMatchWithSummaryData(string ownerId, string matchId, string pushContent, string summary, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["ownerId"] = ownerId;
		dictionary["matchId"] = matchId;
		if (pushContent != null)
		{
			dictionary["pushContent"] = pushContent;
		}
		dictionary["summary"] = JsonReader.Deserialize<Dictionary<string, object>>(summary);
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.AsyncMatch, ServiceOperation.CompleteMatchWithSummaryData, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void AbandonMatchWithSummaryData(string ownerId, string matchId, string pushContent, string summary, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["ownerId"] = ownerId;
		dictionary["matchId"] = matchId;
		if (pushContent != null)
		{
			dictionary["pushContent"] = pushContent;
		}
		dictionary["summary"] = JsonReader.Deserialize<Dictionary<string, object>>(summary);
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.AsyncMatch, ServiceOperation.AbandonMatchWithSummaryData, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	private void CreateMatchInternal(string jsonOpponentIds, string jsonMatchState, string pushNotificationMessage, string matchId, string nextPlayer, string jsonSummary, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["players"] = JsonReader.Deserialize<object[]>(jsonOpponentIds);
		if (Util.IsOptionalParameterValid(jsonMatchState))
		{
			dictionary["matchState"] = JsonReader.Deserialize<Dictionary<string, object>>(jsonMatchState);
		}
		if (Util.IsOptionalParameterValid(matchId))
		{
			dictionary["matchId"] = matchId;
		}
		if (Util.IsOptionalParameterValid(nextPlayer))
		{
			Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
			dictionary2["currentPlayer"] = nextPlayer;
			dictionary["status"] = dictionary2;
		}
		if (Util.IsOptionalParameterValid(jsonSummary))
		{
			dictionary["summary"] = JsonReader.Deserialize<Dictionary<string, object>>(jsonSummary);
		}
		if (Util.IsOptionalParameterValid(pushNotificationMessage))
		{
			dictionary["pushContent"] = pushNotificationMessage;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.AsyncMatch, ServiceOperation.Create, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}
}
