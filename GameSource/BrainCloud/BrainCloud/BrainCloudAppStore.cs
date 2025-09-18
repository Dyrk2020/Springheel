using System.Collections.Generic;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;

namespace BrainCloud;

public class BrainCloudAppStore
{
	private BrainCloudClient _client;

	public BrainCloudAppStore(BrainCloudClient client)
	{
		_client = client;
	}

	public void GetSalesInventory(string platform, string userCurrency, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		GetSalesInventoryByCategory(platform, userCurrency, null, success, failure, cbObject);
	}

	public void GetSalesInventoryByCategory(string storeId, string userCurrency, string category, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.AppStoreServiceStoreId.Value] = storeId;
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		if (Util.IsOptionalParameterValid(userCurrency))
		{
			dictionary2[OperationParam.AppStoreServiceUserCurrency.Value] = userCurrency;
		}
		dictionary[OperationParam.AppStoreServicePriceInfoCriteria.Value] = dictionary2;
		if (Util.IsOptionalParameterValid(category))
		{
			dictionary[OperationParam.AppStoreServiceCategory.Value] = category;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.AppStore, ServiceOperation.GetInventory, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetEligiblePromotions(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.AppStore, ServiceOperation.EligiblePromotions, null, callback);
		_client.SendRequest(serviceMessage);
	}

	public void VerifyPurchase(string storeId, string receiptJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.AppStoreServiceStoreId.Value] = storeId;
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(receiptJson);
		dictionary[OperationParam.AppStoreServiceReceiptData.Value] = value;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.AppStore, ServiceOperation.VerifyPurchase, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void StartPurchase(string storeId, string purchaseJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.AppStoreServiceStoreId.Value] = storeId;
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(purchaseJson);
		dictionary[OperationParam.AppStoreServicePurchaseData.Value] = value;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.AppStore, ServiceOperation.StartPurchase, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void FinalizePurchase(string storeId, string transactionId, string transactionJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.AppStoreServiceStoreId.Value] = storeId;
		dictionary[OperationParam.AppStoreServiceTransactionId.Value] = transactionId;
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(transactionJson);
		dictionary[OperationParam.AppStoreServiceTransactionData.Value] = value;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.AppStore, ServiceOperation.FinalizePurchase, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void RefreshPromotions(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> jsonData = new Dictionary<string, object>();
		ServerCallback callback = new ServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.AppStore, ServiceOperation.RefreshPromotions, jsonData, callback);
		_client.SendRequest(serviceMessage);
	}
}
