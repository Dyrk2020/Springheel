using System.Collections.Generic;
using BrainCloud.Internal;

namespace BrainCloud;

public class BrainCloudGlobalApp
{
	private BrainCloudClient _client;

	public BrainCloudGlobalApp(BrainCloudClient client)
	{
		_client = client;
	}

	public void ReadProperties(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalApp, ServiceOperation.ReadProperties, null, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadSelectedProperties(string[] propertyNames, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GlobalAppPropertyNames.Value] = propertyNames;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalApp, ServiceOperation.ReadSelectedProperties, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadPropertiesInCategories(string[] categories, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GlobalAppCategories.Value] = categories;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalApp, ServiceOperation.ReadPropertiesInCategories, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}
}
