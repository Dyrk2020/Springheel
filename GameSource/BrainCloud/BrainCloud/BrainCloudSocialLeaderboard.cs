using System;
using System.Collections.Generic;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;

namespace BrainCloud;

public class BrainCloudSocialLeaderboard
{
	public enum SocialLeaderboardType
	{
		HIGH_VALUE,
		CUMULATIVE,
		LAST_VALUE,
		LOW_VALUE
	}

	public enum RotationType
	{
		NEVER,
		DAILY,
		WEEKLY,
		MONTHLY,
		YEARLY
	}

	public enum FetchType
	{
		HIGHEST_RANKED
	}

	public enum SortOrder
	{
		HIGH_TO_LOW,
		LOW_TO_HIGH
	}

	private BrainCloudClient _client;

	public BrainCloudSocialLeaderboard(BrainCloudClient client)
	{
		_client = client;
	}

	public void GetSocialLeaderboard(string leaderboardId, bool replaceName, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardId.Value] = leaderboardId;
		dictionary[OperationParam.SocialLeaderboardServiceReplaceName.Value] = replaceName;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Leaderboard, ServiceOperation.GetSocialLeaderboard, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetSocialLeaderboardByVersion(string leaderboardId, bool replaceName, int versionId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardId.Value] = leaderboardId;
		dictionary[OperationParam.SocialLeaderboardServiceReplaceName.Value] = replaceName;
		dictionary[OperationParam.SocialLeaderboardServiceVersionId.Value] = versionId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Leaderboard, ServiceOperation.GetSocialLeaderboardByVersion, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetMultiSocialLeaderboard(IList<string> leaderboardIds, int leaderboardResultCount, bool replaceName, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardIds.Value] = leaderboardIds;
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardResultCount.Value] = leaderboardResultCount;
		dictionary[OperationParam.SocialLeaderboardServiceReplaceName.Value] = replaceName;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Leaderboard, ServiceOperation.GetMultiSocialLeaderboard, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetGlobalLeaderboardPage(string leaderboardId, SortOrder sort, int startIndex, int endIndex, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardId.Value] = leaderboardId;
		dictionary[OperationParam.SocialLeaderboardServiceSort.Value] = sort.ToString();
		dictionary[OperationParam.SocialLeaderboardServiceStartIndex.Value] = startIndex;
		dictionary[OperationParam.SocialLeaderboardServiceEndIndex.Value] = endIndex;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Leaderboard, ServiceOperation.GetGlobalLeaderboardPage, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetGlobalLeaderboardPageByVersion(string leaderboardId, SortOrder sort, int startIndex, int endIndex, int versionId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardId.Value] = leaderboardId;
		dictionary[OperationParam.SocialLeaderboardServiceSort.Value] = sort.ToString();
		dictionary[OperationParam.SocialLeaderboardServiceStartIndex.Value] = startIndex;
		dictionary[OperationParam.SocialLeaderboardServiceEndIndex.Value] = endIndex;
		dictionary[OperationParam.SocialLeaderboardServiceVersionId.Value] = versionId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Leaderboard, ServiceOperation.GetGlobalLeaderboardPage, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetGlobalLeaderboardView(string leaderboardId, SortOrder sort, int beforeCount, int afterCount, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		GetGlobalLeaderboardViewByVersion(leaderboardId, sort, beforeCount, afterCount, -1, success, failure, cbObject);
	}

	public void GetGlobalLeaderboardViewByVersion(string leaderboardId, SortOrder sort, int beforeCount, int afterCount, int versionId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardId.Value] = leaderboardId;
		dictionary[OperationParam.SocialLeaderboardServiceSort.Value] = sort.ToString();
		dictionary[OperationParam.SocialLeaderboardServiceBeforeCount.Value] = beforeCount;
		dictionary[OperationParam.SocialLeaderboardServiceAfterCount.Value] = afterCount;
		if (versionId != -1)
		{
			dictionary[OperationParam.SocialLeaderboardServiceVersionId.Value] = versionId;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Leaderboard, ServiceOperation.GetGlobalLeaderboardView, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetGlobalLeaderboardVersions(string leaderboardId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardId.Value] = leaderboardId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Leaderboard, ServiceOperation.GetGlobalLeaderboardVersions, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetGroupSocialLeaderboard(string leaderboardId, string groupId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardId.Value] = leaderboardId;
		dictionary[OperationParam.SocialLeaderboardServiceGroupId.Value] = groupId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Leaderboard, ServiceOperation.GetGroupSocialLeaderboard, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetGroupSocialLeaderboardByVersion(string leaderboardId, string groupId, int versionId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardId.Value] = leaderboardId;
		dictionary[OperationParam.SocialLeaderboardServiceGroupId.Value] = groupId;
		dictionary[OperationParam.SocialLeaderboardServiceVersionId.Value] = versionId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Leaderboard, ServiceOperation.GetGroupSocialLeaderboardByVersion, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void PostScoreToLeaderboard(string leaderboardId, long score, string jsonData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardId.Value] = leaderboardId;
		dictionary[OperationParam.SocialLeaderboardServiceScore.Value] = score;
		if (Util.IsOptionalParameterValid(jsonData))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonData);
			dictionary[OperationParam.SocialLeaderboardServiceData.Value] = value;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Leaderboard, ServiceOperation.PostScore, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void RemovePlayerScore(string leaderboardId, int versionId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardId.Value] = leaderboardId;
		dictionary[OperationParam.SocialLeaderboardServiceVersionId.Value] = versionId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Leaderboard, ServiceOperation.RemovePlayerScore, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void PostScoreToDynamicLeaderboardUTC(string leaderboardId, long score, string jsonData, SocialLeaderboardType leaderboardType, RotationType rotationType, ulong? rotationResetUTC, int retainedCount, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardId.Value] = leaderboardId;
		dictionary[OperationParam.SocialLeaderboardServiceScore.Value] = score;
		if (Util.IsOptionalParameterValid(jsonData))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonData);
			dictionary[OperationParam.SocialLeaderboardServiceData.Value] = value;
		}
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardType.Value] = leaderboardType.ToString();
		dictionary[OperationParam.SocialLeaderboardServiceRotationType.Value] = rotationType.ToString();
		if (rotationResetUTC.HasValue)
		{
			dictionary[OperationParam.SocialLeaderboardServiceRotationResetTime.Value] = rotationResetUTC.Value;
		}
		dictionary[OperationParam.SocialLeaderboardServiceRetainedCount.Value] = retainedCount;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Leaderboard, ServiceOperation.PostScoreDynamic, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void PostScoreToDynamicGroupLeaderboardUTC(string leaderboardId, string groupId, long score, string jsonData, SocialLeaderboardType leaderboardType, RotationType rotationType, ulong? rotationResetUTC, int retainedCount, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardId.Value] = leaderboardId;
		dictionary[OperationParam.SocialLeaderboardServiceGroupId.Value] = groupId;
		dictionary[OperationParam.SocialLeaderboardServiceScore.Value] = score;
		if (Util.IsOptionalParameterValid(jsonData))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonData);
			dictionary[OperationParam.SocialLeaderboardServiceData.Value] = value;
		}
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardType.Value] = leaderboardType.ToString();
		dictionary[OperationParam.SocialLeaderboardServiceRotationType.Value] = rotationType.ToString();
		if (rotationResetUTC.HasValue)
		{
			dictionary[OperationParam.SocialLeaderboardServiceRotationResetTime.Value] = rotationResetUTC;
		}
		dictionary[OperationParam.SocialLeaderboardServiceRetainedCount.Value] = retainedCount;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Leaderboard, ServiceOperation.PostScoreToDynamicGroupLeaderboard, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void PostScoreToDynamicLeaderboardDaysUTC(string leaderboardId, long score, string jsonData, SocialLeaderboardType leaderboardType, ulong? rotationResetUTC, int retainedCount, int numDaysToRotate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardId.Value] = leaderboardId;
		dictionary[OperationParam.SocialLeaderboardServiceScore.Value] = score;
		if (Util.IsOptionalParameterValid(jsonData))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonData);
			dictionary[OperationParam.SocialLeaderboardServiceData.Value] = value;
		}
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardType.Value] = leaderboardType.ToString();
		dictionary[OperationParam.SocialLeaderboardServiceRotationType.Value] = "DAYS";
		if (rotationResetUTC.HasValue)
		{
			dictionary[OperationParam.SocialLeaderboardServiceRotationResetTime.Value] = rotationResetUTC;
		}
		dictionary[OperationParam.SocialLeaderboardServiceRetainedCount.Value] = retainedCount;
		dictionary[OperationParam.NumDaysToRotate.Value] = numDaysToRotate;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Leaderboard, ServiceOperation.PostScoreDynamic, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void PostScoreToDynamicGroupLeaderboardDaysUTC(string leaderboardId, string groupId, long score, string jsonData, SocialLeaderboardType leaderboardType, ulong? rotationResetUTC, int retainedCount, int numDaysToRotate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardId.Value] = leaderboardId;
		dictionary[OperationParam.SocialLeaderboardServiceScore.Value] = score;
		dictionary[OperationParam.PresenceServiceGroupId.Value] = groupId;
		if (Util.IsOptionalParameterValid(jsonData))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonData);
			dictionary[OperationParam.SocialLeaderboardServiceData.Value] = value;
		}
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardType.Value] = leaderboardType.ToString();
		dictionary[OperationParam.SocialLeaderboardServiceRotationType.Value] = "DAYS";
		if (rotationResetUTC.HasValue)
		{
			dictionary[OperationParam.SocialLeaderboardServiceRotationResetTime.Value] = rotationResetUTC;
		}
		dictionary[OperationParam.SocialLeaderboardServiceRetainedCount.Value] = retainedCount;
		dictionary[OperationParam.NumDaysToRotate.Value] = numDaysToRotate;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Leaderboard, ServiceOperation.PostScoreDynamic, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetPlayersSocialLeaderboard(string leaderboardId, IList<string> profileIds, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardId.Value] = leaderboardId;
		dictionary[OperationParam.SocialLeaderboardServiceProfileIds.Value] = profileIds;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Leaderboard, ServiceOperation.GetPlayersSocialLeaderboard, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetPlayersSocialLeaderboardByVersion(string leaderboardId, IList<string> profileIds, int versionId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardId.Value] = leaderboardId;
		dictionary[OperationParam.SocialLeaderboardServiceProfileIds.Value] = profileIds;
		dictionary[OperationParam.SocialLeaderboardServiceVersionId.Value] = versionId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Leaderboard, ServiceOperation.GetPlayersSocialLeaderboardByVersion, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	[Obsolete("This has been deprecated, use ListAllLeaderboards instead - removal after Match 1 2022")]
	public void ListLeaderboards(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Leaderboard, ServiceOperation.ListAllLeaderboards, null, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ListAllLeaderboards(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Leaderboard, ServiceOperation.ListAllLeaderboards, null, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetGlobalLeaderboardEntryCount(string leaderboardId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		GetGlobalLeaderboardEntryCountByVersion(leaderboardId, -1, success, failure, cbObject);
	}

	public void GetGlobalLeaderboardEntryCountByVersion(string leaderboardId, int versionId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardId.Value] = leaderboardId;
		if (versionId > -1)
		{
			dictionary[OperationParam.SocialLeaderboardServiceVersionId.Value] = versionId;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Leaderboard, ServiceOperation.GetGlobalLeaderboardEntryCount, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetPlayerScore(string leaderboardId, int versionId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardId.Value] = leaderboardId;
		dictionary[OperationParam.SocialLeaderboardServiceVersionId.Value] = versionId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Leaderboard, ServiceOperation.GetPlayerScore, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetPlayerScores(string leaderboardId, int versionId, int maxResults, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardId.Value] = leaderboardId;
		dictionary[OperationParam.SocialLeaderboardServiceMaxResults.Value] = maxResults;
		dictionary[OperationParam.SocialLeaderboardServiceVersionId.Value] = versionId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Leaderboard, ServiceOperation.GetPlayerScores, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetPlayerScoresFromLeaderboards(IList<string> leaderboardIds, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardIds.Value] = leaderboardIds;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Leaderboard, ServiceOperation.GetPlayerScoresFromLeaderboards, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void PostScoreToGroupLeaderboard(string leaderboardId, string groupId, int score, string jsonData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardId.Value] = leaderboardId;
		dictionary[OperationParam.SocialLeaderboardServiceGroupId.Value] = groupId;
		dictionary[OperationParam.SocialLeaderboardServiceScore.Value] = score;
		if (Util.IsOptionalParameterValid(jsonData))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonData);
			dictionary[OperationParam.SocialLeaderboardServiceData.Value] = value;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Leaderboard, ServiceOperation.PostScoreToGroupLeaderboard, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void RemoveGroupScore(string leaderboardId, string groupId, int versionId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardId.Value] = leaderboardId;
		dictionary[OperationParam.SocialLeaderboardServiceGroupId.Value] = groupId;
		dictionary[OperationParam.SocialLeaderboardServiceVersionId.Value] = versionId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Leaderboard, ServiceOperation.RemoveGroupScore, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetGroupLeaderboardView(string leaderboardId, string groupId, SortOrder sort, int beforeCount, int afterCount, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardId.Value] = leaderboardId;
		dictionary[OperationParam.SocialLeaderboardServiceGroupId.Value] = groupId;
		dictionary[OperationParam.SocialLeaderboardServiceSort.Value] = sort.ToString();
		dictionary[OperationParam.SocialLeaderboardServiceBeforeCount.Value] = beforeCount;
		dictionary[OperationParam.SocialLeaderboardServiceAfterCount.Value] = afterCount;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Leaderboard, ServiceOperation.GetGroupLeaderboardView, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetGroupLeaderboardViewByVersion(string leaderboardId, string groupId, int versionId, SortOrder sort, int beforeCount, int afterCount, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.SocialLeaderboardServiceLeaderboardId.Value] = leaderboardId;
		dictionary[OperationParam.SocialLeaderboardServiceGroupId.Value] = groupId;
		dictionary[OperationParam.SocialLeaderboardServiceSort.Value] = sort.ToString();
		dictionary[OperationParam.SocialLeaderboardServiceBeforeCount.Value] = beforeCount;
		dictionary[OperationParam.SocialLeaderboardServiceAfterCount.Value] = afterCount;
		dictionary[OperationParam.SocialLeaderboardServiceVersionId.Value] = versionId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Leaderboard, ServiceOperation.GetGroupLeaderboardView, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}
}
