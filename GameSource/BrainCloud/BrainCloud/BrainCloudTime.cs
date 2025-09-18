using BrainCloud.Internal;

namespace BrainCloud;

public class BrainCloudTime
{
	private BrainCloudClient _client;

	public BrainCloudTime(BrainCloudClient inBrainCloudClient)
	{
		_client = inBrainCloudClient;
	}

	public void ReadServerTime(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Time, ServiceOperation.Read, null, callback);
		_client.SendRequest(serviceMessage);
	}
}
