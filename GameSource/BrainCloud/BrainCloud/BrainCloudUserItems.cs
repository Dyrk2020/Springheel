using System.Collections.Generic;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;

namespace BrainCloud;

public class BrainCloudUserItems
{
	private BrainCloudClient _client;

	public BrainCloudUserItems(BrainCloudClient client)
	{
		_client = client;
	}

	public void AwardUserItem(string defId, int quantity, bool includeDef, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.UserItemsServiceDefId.Value] = defId;
		dictionary[OperationParam.UserItemsServiceQuantity.Value] = quantity;
		dictionary[OperationParam.UserItemsServiceIncludeDef.Value] = includeDef;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.UserItems, ServiceOperation.AwardUserItem, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void DropUserItem(string itemId, int quantity, bool includeDef, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.UserItemsServiceItemId.Value] = itemId;
		dictionary[OperationParam.UserItemsServiceQuantity.Value] = quantity;
		dictionary[OperationParam.UserItemsServiceIncludeDef.Value] = includeDef;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.UserItems, ServiceOperation.DropUserItem, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetUserItemsPage(string context, bool includeDef, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(context);
		dictionary[OperationParam.UserItemsServiceContext.Value] = value;
		dictionary[OperationParam.UserItemsServiceIncludeDef.Value] = includeDef;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.UserItems, ServiceOperation.GetUserItemsPage, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetUserItemsPageOffset(string context, int pageOffset, bool includeDef, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.UserItemsServiceContext.Value] = context;
		dictionary[OperationParam.UserItemsServicePageOffset.Value] = pageOffset;
		dictionary[OperationParam.UserItemsServiceIncludeDef.Value] = includeDef;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.UserItems, ServiceOperation.GetUserItemsPageOffset, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetUserItem(string itemId, bool includeDef, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.UserItemsServiceItemId.Value] = itemId;
		dictionary[OperationParam.UserItemsServiceIncludeDef.Value] = includeDef;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.UserItems, ServiceOperation.GetUserItem, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GiveUserItemTo(string profileId, string itemId, int version, int quantity, bool immediate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.UserItemsServiceProfileId.Value] = profileId;
		dictionary[OperationParam.UserItemsServiceItemId.Value] = itemId;
		dictionary[OperationParam.UserItemsServiceVersion.Value] = version;
		dictionary[OperationParam.UserItemsServiceQuantity.Value] = quantity;
		dictionary[OperationParam.UserItemsServiceImmediate.Value] = immediate;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.UserItems, ServiceOperation.GiveUserItemTo, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void PurchaseUserItem(string defId, int quantity, string shopId, bool includeDef, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.UserItemsServiceDefId.Value] = defId;
		dictionary[OperationParam.UserItemsServiceQuantity.Value] = quantity;
		dictionary[OperationParam.UserItemsServiceShopId.Value] = shopId;
		dictionary[OperationParam.UserItemsServiceIncludeDef.Value] = includeDef;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.UserItems, ServiceOperation.PurchaseUserItem, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReceiveUserItemFrom(string profileId, string itemId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.UserItemsServiceProfileId.Value] = profileId;
		dictionary[OperationParam.UserItemsServiceItemId.Value] = itemId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.UserItems, ServiceOperation.ReceiveUserItemFrom, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void SellUserItem(string itemId, int version, int quantity, string shopId, bool includeDef, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.UserItemsServiceItemId.Value] = itemId;
		dictionary[OperationParam.UserItemsServiceVersion.Value] = version;
		dictionary[OperationParam.UserItemsServiceQuantity.Value] = quantity;
		dictionary[OperationParam.UserItemsServiceShopId.Value] = shopId;
		dictionary[OperationParam.UserItemsServiceIncludeDef.Value] = includeDef;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.UserItems, ServiceOperation.SellUserItem, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void UpdateUserItemData(string itemId, int version, string newItemData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.UserItemsServiceItemId.Value] = itemId;
		dictionary[OperationParam.UserItemsServiceVersion.Value] = version;
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(newItemData);
		dictionary[OperationParam.UserItemsServiceNewItemData.Value] = value;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.UserItems, ServiceOperation.UpdateUserItemData, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void UseUserItem(string itemId, int version, string newItemData, bool includeDef, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.UserItemsServiceItemId.Value] = itemId;
		dictionary[OperationParam.UserItemsServiceVersion.Value] = version;
		dictionary[OperationParam.UserItemsServiceIncludeDef.Value] = includeDef;
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(newItemData);
		dictionary[OperationParam.UserItemsServiceNewItemData.Value] = value;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.UserItems, ServiceOperation.UseUserItem, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void PublishUserItemToBlockchain(string itemId, int version, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.UserItemsServiceItemId.Value] = itemId;
		dictionary[OperationParam.UserItemsServiceVersion.Value] = version;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.UserItems, ServiceOperation.PublishUserItemToBlockchain, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void RefreshBlockchainUserItems(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> jsonData = new Dictionary<string, object>();
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.UserItems, ServiceOperation.RefreshBlockchainUserItems, jsonData, callback);
		_client.SendRequest(serviceMessage);
	}

	public void RemoveUserItemFromBlockchain(string itemId, int version, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.UserItemsServiceItemId.Value] = itemId;
		dictionary[OperationParam.UserItemsServiceVersion.Value] = version;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.UserItems, ServiceOperation.RemoveUserItemFromBlockchain, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}
}
