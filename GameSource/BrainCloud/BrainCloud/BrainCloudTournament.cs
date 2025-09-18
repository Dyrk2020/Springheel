using System.Collections.Generic;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;

namespace BrainCloud;

public class BrainCloudTournament
{
	private BrainCloudClient _client;

	public BrainCloudTournament(BrainCloudClient client)
	{
		_client = client;
	}

	public void ClaimTournamentReward(string leaderboardId, int versionId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.LeaderboardId.Value] = leaderboardId;
		dictionary[OperationParam.VersionId.Value] = versionId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Tournament, ServiceOperation.ClaimTournamentReward, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetDivisionInfo(string divSetId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.DivSetId.Value] = divSetId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Tournament, ServiceOperation.GetDivisionInfo, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetMyDivisions(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Tournament, ServiceOperation.GetMyDivisions, null, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetTournamentStatus(string leaderboardId, int versionId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.LeaderboardId.Value] = leaderboardId;
		dictionary[OperationParam.VersionId.Value] = versionId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Tournament, ServiceOperation.GetTournamentStatus, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void JoinDivision(string divSetId, string tournamentCode, long initialScore, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.DivSetId.Value] = divSetId;
		dictionary[OperationParam.TournamentCode.Value] = tournamentCode;
		dictionary[OperationParam.InitialScore.Value] = initialScore;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Tournament, ServiceOperation.JoinDivision, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void JoinTournament(string leaderboardId, string tournamentCode, long initialScore, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.LeaderboardId.Value] = leaderboardId;
		dictionary[OperationParam.TournamentCode.Value] = tournamentCode;
		dictionary[OperationParam.InitialScore.Value] = initialScore;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Tournament, ServiceOperation.JoinTournament, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void LeaveDivisionInstance(string leaderboardId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.LeaderboardId.Value] = leaderboardId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Tournament, ServiceOperation.LeaveDivisionInstance, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void LeaveTournament(string leaderboardId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.LeaderboardId.Value] = leaderboardId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Tournament, ServiceOperation.LeaveTournament, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void PostTournamentScoreUTC(string leaderboardId, long score, string jsonData, ulong roundStartTimeUTC, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.LeaderboardId.Value] = leaderboardId;
		dictionary[OperationParam.Score.Value] = score;
		dictionary[OperationParam.RoundStartedEpoch.Value] = roundStartTimeUTC;
		if (Util.IsOptionalParameterValid(jsonData))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonData);
			dictionary[OperationParam.Data.Value] = value;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Tournament, ServiceOperation.PostTournamentScore, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void PostTournamentScoreWithResultsUTC(string leaderboardId, long score, string jsonData, ulong roundStartTimeUTC, BrainCloudSocialLeaderboard.SortOrder sort, int beforeCount, int afterCount, long initialScore, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardId.Value] = leaderboardId;
		dictionary[OperationParam.Score.Value] = score;
		dictionary[OperationParam.RoundStartedEpoch.Value] = roundStartTimeUTC;
		dictionary[OperationParam.InitialScore.Value] = initialScore;
		if (Util.IsOptionalParameterValid(jsonData))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonData);
			dictionary[OperationParam.Data.Value] = value;
		}
		dictionary[OperationParam.SocialLeaderboardServiceSort.Value] = sort.ToString();
		dictionary[OperationParam.SocialLeaderboardServiceBeforeCount.Value] = beforeCount;
		dictionary[OperationParam.SocialLeaderboardServiceAfterCount.Value] = afterCount;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		_client.SendRequest(new ServerCall(ServiceName.Tournament, ServiceOperation.PostTournamentScoreWithResults, dictionary, callback));
	}

	public void ViewCurrentReward(string leaderboardId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.LeaderboardId.Value] = leaderboardId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Tournament, ServiceOperation.ViewCurrentReward, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ViewReward(string leaderboardId, int versionId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.LeaderboardId.Value] = leaderboardId;
		dictionary[OperationParam.VersionId.Value] = versionId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Tournament, ServiceOperation.ViewReward, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}
}
