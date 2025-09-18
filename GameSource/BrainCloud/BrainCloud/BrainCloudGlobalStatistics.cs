using System.Collections.Generic;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;

namespace BrainCloud;

public class BrainCloudGlobalStatistics
{
	private BrainCloudClient _client;

	public BrainCloudGlobalStatistics(BrainCloudClient client)
	{
		_client = client;
	}

	public void ReadAllGlobalStats(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalStatistics, ServiceOperation.Read, null, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadGlobalStatsSubset(IList<string> globalStats, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PlayerStatisticsServiceStats.Value] = globalStats;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalStatistics, ServiceOperation.ReadSubset, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadGlobalStatsForCategory(string category, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GamificationServiceCategory.Value] = category;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalStatistics, ServiceOperation.ReadForCategory, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void IncrementGlobalStats(string jsonData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonData);
		dictionary[OperationParam.PlayerStatisticsServiceStats.Value] = value;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalStatistics, ServiceOperation.UpdateIncrement, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ProcessStatistics(string statisticsData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(statisticsData);
		dictionary[OperationParam.PlayerStatisticsServiceStats.Value] = value;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalStatistics, ServiceOperation.ProcessStatistics, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ProcessStatistics(Dictionary<string, object> statisticsData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PlayerStatisticsServiceStats.Value] = statisticsData;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalStatistics, ServiceOperation.ProcessStatistics, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}
}
