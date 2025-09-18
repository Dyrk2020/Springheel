using System.Collections.Generic;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;

namespace BrainCloud;

public class BrainCloudS3Handling
{
	private BrainCloudClient _client;

	public BrainCloudS3Handling(BrainCloudClient client)
	{
		_client = client;
	}

	public void GetUpdatedFiles(string category, string fileDetailsJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (Util.IsOptionalParameterValid(category))
		{
			dictionary[OperationParam.S3HandlingServiceFileCategory.Value] = category;
		}
		dictionary[OperationParam.S3HandlingServiceFileDetails.Value] = JsonReader.Deserialize<object[]>(fileDetailsJson);
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.S3Handling, ServiceOperation.GetUpdatedFiles, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetFileList(string category, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (Util.IsOptionalParameterValid(category))
		{
			dictionary[OperationParam.S3HandlingServiceFileCategory.Value] = category;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.S3Handling, ServiceOperation.GetFileList, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetCDNUrl(string fileId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.S3HandlingServiceFileId.Value] = fileId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.S3Handling, ServiceOperation.GetCdnUrl, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}
}
