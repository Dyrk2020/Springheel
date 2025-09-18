using System.Collections.Generic;
using BrainCloud.Common;
using BrainCloud.Internal;

namespace BrainCloud;

public class BrainCloudFriend
{
	public enum FriendPlatform
	{
		All,
		brainCloud,
		Facebook
	}

	private BrainCloudClient _client;

	public BrainCloudFriend(BrainCloudClient client)
	{
		_client = client;
	}

	public void GetProfileInfoForCredential(string externalId, AuthenticationType authenticationType, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.FriendServiceExternalId.Value] = externalId;
		dictionary[OperationParam.FriendServiceAuthenticationType.Value] = authenticationType.ToString();
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Friend, ServiceOperation.GetProfileInfoForCredential, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetProfileInfoForExternalAuthId(string externalId, string externalAuthType, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.FriendServiceExternalId.Value] = externalId;
		dictionary[OperationParam.ExternalAuthType.Value] = externalAuthType;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Friend, ServiceOperation.GetProfileInfoForExternalAuthId, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetExternalIdForProfileId(string profileId, string authenticationType, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.FriendServiceProfileId.Value] = profileId;
		dictionary[OperationParam.FriendServiceAuthenticationType.Value] = authenticationType;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Friend, ServiceOperation.GetExternalIdForProfileId, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadFriendEntity(string entityId, string friendId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.FriendServiceEntityId.Value] = entityId;
		dictionary[OperationParam.FriendServiceFriendId.Value] = friendId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Friend, ServiceOperation.ReadFriendEntity, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadFriendsEntities(string entityType, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.FriendServiceEntityType.Value] = entityType;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Friend, ServiceOperation.ReadFriendsEntities, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadFriendUserState(string friendId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.FriendServiceReadPlayerStateFriendId.Value] = friendId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Friend, ServiceOperation.ReadFriendPlayerState, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetSummaryDataForProfileId(string profileId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.FriendServiceProfileId.Value] = profileId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Friend, ServiceOperation.GetSummaryDataForProfileId, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void FindUsersByExactName(string searchText, int maxResults, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.FriendServiceSearchText.Value] = searchText;
		dictionary[OperationParam.FriendServiceMaxResults.Value] = maxResults;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Friend, ServiceOperation.FindUsersByExactName, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void FindUserByExactUniversalId(string searchText, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.FriendServiceSearchText.Value] = searchText;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Friend, ServiceOperation.FindUserByExactUniversalId, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void FindUsersBySubstrName(string searchText, int maxResults, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.FriendServiceSearchText.Value] = searchText;
		dictionary[OperationParam.FriendServiceMaxResults.Value] = maxResults;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Friend, ServiceOperation.FindUsersBySubstrName, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ListFriends(FriendPlatform friendPlatform, bool includeSummaryData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.FriendServiceFriendPlatform.Value] = friendPlatform.ToString();
		dictionary[OperationParam.FriendServiceIncludeSummaryData.Value] = includeSummaryData;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Friend, ServiceOperation.ListFriends, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetMySocialInfo(FriendPlatform friendPlatform, bool includeSummaryData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.FriendServiceFriendPlatform.Value] = friendPlatform.ToString();
		dictionary[OperationParam.FriendServiceIncludeSummaryData.Value] = includeSummaryData;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Friend, ServiceOperation.ListFriends, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void AddFriends(IList<string> profileIds, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.FriendServiceProfileIds.Value] = profileIds;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Friend, ServiceOperation.AddFriends, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void AddFriendsFromPlatform(FriendPlatform friendPlatform, string mode, IList<string> externalIds, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.FriendServiceFriendPlatform.Value] = friendPlatform.ToString();
		dictionary[OperationParam.FriendServiceMode.Value] = mode;
		dictionary[OperationParam.FriendServiceExternalIds.Value] = externalIds;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Friend, ServiceOperation.AddFriendsFromPlatform, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void RemoveFriends(IList<string> profileIds, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.FriendServiceProfileIds.Value] = profileIds;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Friend, ServiceOperation.RemoveFriends, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetUsersOnlineStatus(IList<string> profileIds, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.FriendServiceProfileIds.Value] = profileIds;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Friend, ServiceOperation.GetUsersOnlineStatus, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void FindUsersByNameStartingWith(string searchText, int maxResults, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.FriendServiceSearchText.Value] = searchText;
		dictionary[OperationParam.FriendServiceMaxResults.Value] = maxResults;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Friend, ServiceOperation.FindUsersByNameStartingWith, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void FindUsersByUniversalIdStartingWith(string searchText, int maxResults, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.FriendServiceSearchText.Value] = searchText;
		dictionary[OperationParam.FriendServiceMaxResults.Value] = maxResults;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Friend, ServiceOperation.FindUsersByUniversalIdStartingWith, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}
}
