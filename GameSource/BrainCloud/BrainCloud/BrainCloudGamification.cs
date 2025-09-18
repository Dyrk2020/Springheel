using System.Collections.Generic;
using BrainCloud.Internal;

namespace BrainCloud;

public class BrainCloudGamification
{
	private BrainCloudClient _client;

	public BrainCloudGamification(BrainCloudClient client)
	{
		_client = client;
	}

	public void ReadAllGamification(bool includeMetaData = false, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GamificationServiceIncludeMetaData.Value] = includeMetaData;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Gamification, ServiceOperation.Read, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadMilestones(bool includeMetaData = false, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GamificationServiceIncludeMetaData.Value] = includeMetaData;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Gamification, ServiceOperation.ReadMilestones, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadAchievements(bool includeMetaData = false, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GamificationServiceIncludeMetaData.Value] = includeMetaData;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Gamification, ServiceOperation.ReadAchievements, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadXpLevelsMetaData(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Gamification, ServiceOperation.ReadXpLevels, null, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadAchievedAchievements(bool includeMetaData = false, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GamificationServiceIncludeMetaData.Value] = includeMetaData;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Gamification, ServiceOperation.ReadAchievedAchievements, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadCompletedMilestones(bool includeMetaData = false, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GamificationServiceIncludeMetaData.Value] = includeMetaData;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Gamification, ServiceOperation.ReadCompletedMilestones, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadInProgressMilestones(bool includeMetaData = false, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GamificationServiceIncludeMetaData.Value] = includeMetaData;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Gamification, ServiceOperation.ReadInProgressMilestones, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadMilestonesByCategory(string category, bool includeMetaData = false, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GamificationServiceCategory.Value] = category;
		dictionary[OperationParam.GamificationServiceIncludeMetaData.Value] = includeMetaData;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Gamification, ServiceOperation.ReadMilestonesByCategory, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void AwardAchievements(IList<string> achievementIds, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GamificationServiceAchievementsName.Value] = achievementIds;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure);
		ServerCall serviceMessage = new ServerCall(ServiceName.Gamification, ServiceOperation.AwardAchievements, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadQuests(bool includeMetaData = false, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GamificationServiceIncludeMetaData.Value] = includeMetaData;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Gamification, ServiceOperation.ReadQuests, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadCompletedQuests(bool includeMetaData = false, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GamificationServiceIncludeMetaData.Value] = includeMetaData;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Gamification, ServiceOperation.ReadCompletedQuests, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadInProgressQuests(bool includeMetaData = false, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GamificationServiceIncludeMetaData.Value] = includeMetaData;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Gamification, ServiceOperation.ReadInProgressQuests, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadNotStartedQuests(bool includeMetaData = false, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GamificationServiceIncludeMetaData.Value] = includeMetaData;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Gamification, ServiceOperation.ReadNotStartedQuests, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadQuestsWithStatus(bool includeMetaData = false, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GamificationServiceIncludeMetaData.Value] = includeMetaData;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Gamification, ServiceOperation.ReadQuestsWithStatus, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadQuestsWithBasicPercentage(bool includeMetaData = false, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GamificationServiceIncludeMetaData.Value] = includeMetaData;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Gamification, ServiceOperation.ReadQuestsWithBasicPercentage, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadQuestsWithComplexPercentage(bool includeMetaData = false, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GamificationServiceIncludeMetaData.Value] = includeMetaData;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Gamification, ServiceOperation.ReadQuestsWithComplexPercentage, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadQuestsByCategory(string category, bool includeMetaData = false, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GamificationServiceCategory.Value] = category;
		dictionary[OperationParam.GamificationServiceIncludeMetaData.Value] = includeMetaData;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Gamification, ServiceOperation.ReadQuestsByCategory, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}
}
