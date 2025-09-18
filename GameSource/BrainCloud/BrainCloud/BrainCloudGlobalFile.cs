using System.Collections.Generic;
using BrainCloud.Internal;

namespace BrainCloud;

public class BrainCloudGlobalFile
{
	private BrainCloudClient _client;

	public BrainCloudGlobalFile(BrainCloudClient client)
	{
		_client = client;
	}

	public void GetFileInfo(string fileId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GlobalFileServiceFileId.Value] = fileId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalFile, ServiceOperation.GetFileInfo, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetFileInfoSimple(string folderPath, string filename, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GlobalFileServiceFolderPath.Value] = folderPath;
		dictionary[OperationParam.GlobalFileServiceFileName.Value] = filename;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalFile, ServiceOperation.GetFileInfoSimple, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetGlobalCDNUrl(string fileId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GlobalFileServiceFileId.Value] = fileId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalFile, ServiceOperation.GetGlobalCDNUrl, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetGlobalFileList(string folderPath, bool recurse, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GlobalFileServiceFolderPath.Value] = folderPath;
		dictionary[OperationParam.GlobalFileServiceRecurse.Value] = recurse;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalFile, ServiceOperation.GetGlobalFileList, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}
}
