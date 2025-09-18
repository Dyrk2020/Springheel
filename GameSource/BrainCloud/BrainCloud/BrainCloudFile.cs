using System;
using System.Collections.Generic;
using System.IO;
using BrainCloud.Internal;

namespace BrainCloud;

public class BrainCloudFile
{
	private BrainCloudClient _client;

	public Dictionary<string, byte[]> FileStorage = new Dictionary<string, byte[]>();

	public BrainCloudFile(BrainCloudClient client)
	{
		_client = client;
	}

	[Obsolete("This has been deprecated use UploadFileFromMemory instead - removal after June 22nd 2022")]
	public bool UploadFile(string cloudPath, string cloudFilename, bool shareable, bool replaceIfExists, string localPath, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Stream stream = new FileStream(localPath, FileMode.Open);
		if (stream.Length == 0L)
		{
			_client.Log("File at " + localPath + " does not exist");
			return false;
		}
		byte[] array = new byte[(int)stream.Length];
		stream.Seek(0L, SeekOrigin.Begin);
		stream.Read(array, 0, (int)stream.Length);
		stream.Close();
		return UploadFileFromMemory(cloudPath, cloudFilename, shareable, replaceIfExists, array, success, failure, cbObject);
	}

	public bool UploadFileFromMemory(string cloudPath, string cloudFilename, bool shareable, bool replaceIfExists, byte[] fileData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		if (fileData.Length == 0)
		{
			_client.Log("File data is empty");
			return false;
		}
		string text = Guid.NewGuid().ToString();
		_client.FileService.FileStorage.Add(text, fileData);
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.UploadLocalPath.Value] = text;
		dictionary[OperationParam.UploadCloudFilename.Value] = cloudFilename;
		dictionary[OperationParam.UploadCloudPath.Value] = cloudPath;
		dictionary[OperationParam.UploadShareable.Value] = shareable;
		dictionary[OperationParam.UploadReplaceIfExists.Value] = replaceIfExists;
		dictionary[OperationParam.UploadFileSize.Value] = fileData.Length;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.File, ServiceOperation.PrepareUserUpload, dictionary, callback);
		_client.SendRequest(serviceMessage);
		return true;
	}

	public void CancelUpload(string uploadId)
	{
		_client.Comms.CancelUpload(uploadId);
	}

	public double GetUploadProgress(string uploadId)
	{
		return _client.Comms.GetUploadProgress(uploadId);
	}

	public long GetUploadBytesTransferred(string uploadId)
	{
		return _client.Comms.GetUploadBytesTransferred(uploadId);
	}

	public long GetUploadTotalBytesToTransfer(string uploadId)
	{
		return _client.Comms.GetUploadTotalBytesToTransfer(uploadId);
	}

	public void ListUserFiles(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ListUserFiles(null, null, success, failure, cbObject);
	}

	public void ListUserFiles(string cloudPath, bool? recurse, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (Util.IsOptionalParameterValid(cloudPath))
		{
			dictionary[OperationParam.UploadPath.Value] = cloudPath;
		}
		if (recurse.HasValue)
		{
			dictionary[OperationParam.UploadRecurse.Value] = recurse.Value;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.File, ServiceOperation.ListUserFiles, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void DeleteUserFile(string cloudPath, string cloudFileName, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.UploadCloudPath.Value] = cloudPath;
		dictionary[OperationParam.UploadCloudFilename.Value] = cloudFileName;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.File, ServiceOperation.DeleteUserFile, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void DeleteUserFiles(string cloudPath, bool recurse, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.UploadCloudPath.Value] = cloudPath;
		dictionary[OperationParam.UploadRecurse.Value] = recurse;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.File, ServiceOperation.DeleteUserFiles, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetCDNUrl(string cloudPath, string cloudFilename, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.UploadCloudPath.Value] = cloudPath;
		dictionary[OperationParam.UploadCloudFilename.Value] = cloudFilename;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.File, ServiceOperation.GetCdnUrl, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}
}
