using System;
using System.Collections.Generic;
using BrainCloud.Internal;

namespace BrainCloud;

public class BrainCloudVirtualCurrency
{
	private BrainCloudClient _client;

	public BrainCloudVirtualCurrency(BrainCloudClient client)
	{
		_client = client;
	}

	public void GetCurrency(string currencyType, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.VirtualCurrencyServiceCurrencyId.Value] = currencyType;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.VirtualCurrency, ServiceOperation.GetPlayerVC, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetParentCurrency(string currencyType, string levelName, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.VirtualCurrencyServiceCurrencyId.Value] = currencyType;
		dictionary[OperationParam.AuthenticateServiceAuthenticateLevelName.Value] = levelName;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.VirtualCurrency, ServiceOperation.GetParentVC, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetPeerCurrency(string currencyType, string peerCode, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.VirtualCurrencyServiceCurrencyId.Value] = currencyType;
		dictionary[OperationParam.AuthenticateServiceAuthenticatePeerCode.Value] = peerCode;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.VirtualCurrency, ServiceOperation.GetPeerVC, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ResetCurrency(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> jsonData = new Dictionary<string, object>();
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.VirtualCurrency, ServiceOperation.ResetPlayerVC, jsonData, callback);
		_client.SendRequest(serviceMessage);
	}

	[Obsolete("For security reasons calling this API from the client is not recommended, and is rejected at the server by default. To over-ride, enable the 'Allow Currency Calls from Client' compatibility setting in the Design Portal.")]
	public void AwardCurrency(string currencyType, ulong amount, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.VirtualCurrencyServiceCurrencyId.Value] = currencyType;
		dictionary[OperationParam.VirtualCurrencyServiceCurrencyAmount.Value] = amount;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.VirtualCurrency, ServiceOperation.AwardVC, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	[Obsolete("For security reasons calling this API from the client is not recommended, and is rejected at the server by default. To over-ride, enable the 'Allow Currency Calls from Client' compatibility setting in the Design Portal.")]
	public void ConsumeCurrency(string currencyType, ulong amount, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.VirtualCurrencyServiceCurrencyId.Value] = currencyType;
		dictionary[OperationParam.VirtualCurrencyServiceCurrencyAmount.Value] = amount;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.VirtualCurrency, ServiceOperation.ConsumePlayerVC, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}
}
