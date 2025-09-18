using System.Collections.Generic;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;

namespace BrainCloud;

public class BrainCloudItemCatalog
{
	private BrainCloudClient _client;

	public BrainCloudItemCatalog(BrainCloudClient client)
	{
		_client = client;
	}

	public void GetCatalogItemDefinition(string defId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ItemCatalogServiceDefId.Value] = defId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.ItemCatalog, ServiceOperation.GetCatalogItemDefinition, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetCatalogItemsPage(string context, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(context);
		dictionary[OperationParam.ItemCatalogServiceContext.Value] = value;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.ItemCatalog, ServiceOperation.GetCatalogItemsPage, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetCatalogItemsPageOffset(string context, int pageOffset, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ItemCatalogServiceContext.Value] = context;
		dictionary[OperationParam.ItemCatalogServicePageOffset.Value] = pageOffset;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.ItemCatalog, ServiceOperation.GetCatalogItemsPageOffset, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}
}
