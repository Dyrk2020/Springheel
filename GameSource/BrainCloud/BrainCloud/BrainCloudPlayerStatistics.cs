using System.Collections.Generic;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;

namespace BrainCloud;

public class BrainCloudPlayerStatistics
{
	private BrainCloudClient _client;

	public BrainCloudPlayerStatistics(BrainCloudClient client)
	{
		_client = client;
	}

	public void ReadAllUserStats(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerStatistics, ServiceOperation.Read, null, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadUserStatsSubset(IList<string> playerStats, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PlayerStatisticsServiceStats.Value] = playerStats;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerStatistics, ServiceOperation.ReadSubset, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadUserStatsForCategory(string category, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GamificationServiceCategory.Value] = category;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerStatistics, ServiceOperation.ReadForCategory, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ResetAllUserStats(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerStatistics, ServiceOperation.Reset, null, callback);
		_client.SendRequest(serviceMessage);
	}

	public void IncrementUserStats(string jsonData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonData);
		dictionary[OperationParam.PlayerStatisticsServiceStats.Value] = value;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerStatistics, ServiceOperation.Update, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void IncrementUserStats(Dictionary<string, object> dictData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PlayerStatisticsServiceStats.Value] = dictData;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerStatistics, ServiceOperation.Update, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ProcessStatistics(string statisticsData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(statisticsData);
		dictionary[OperationParam.PlayerStatisticsServiceStats.Value] = value;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerStatistics, ServiceOperation.ProcessStatistics, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ProcessStatistics(Dictionary<string, object> statisticsData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PlayerStatisticsServiceStats.Value] = statisticsData;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerStatistics, ServiceOperation.ProcessStatistics, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetNextExperienceLevel(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerStatistics, ServiceOperation.ReadNextXpLevel, null, callback);
		_client.SendRequest(serviceMessage);
	}

	public void IncrementExperiencePoints(int xpValue, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PlayerStatisticsExperiencePoints.Value] = xpValue;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerStatistics, ServiceOperation.Update, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void SetExperiencePoints(int xpValue, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PlayerStatisticsExperiencePoints.Value] = xpValue;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerStatistics, ServiceOperation.SetXpPoints, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}
}
