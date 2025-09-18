using System;
using BrainCloud.JsonFx.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace BrainCloud.Internal;

internal class FileUploader
{
	public enum FileUploaderStatus
	{
		None,
		Pending,
		Uploading,
		CompleteFailed,
		CompleteSuccess
	}

	private BrainCloudClient _client;

	private string _sessionId;

	private string _guidLocalPath;

	private string _serverUrl;

	private string _fileName;

	private string _peerCode;

	private long _timeoutThreshold = 50L;

	private int _timeout = 120;

	private const double TIME_INTERVAL = 0.25;

	private double _transferElapsedTime;

	private long _transferRatesTotal;

	private long _lastTransferTotal;

	private long _transferRatePerSecond;

	private DateTime _lastTime;

	private double _deltaTime;

	private double _elapsedTime;

	private double _timeUnderMinRate;

	private UnityWebRequest _request;

	public string UploadId { get; private set; }

	public double Progress { get; private set; }

	public long BytesTransferred => (long)((double)TotalBytesToTransfer * Progress);

	public long TotalBytesToTransfer { get; set; }

	public FileUploaderStatus Status { get; private set; }

	public string Response { get; private set; }

	public int StatusCode { get; private set; }

	public int ReasonCode { get; private set; }

	public string FileName
	{
		get
		{
			return _fileName;
		}
		set
		{
			_fileName = value;
		}
	}

	public FileUploader(string uploadId, string guidLocalPath, string serverUrl, string sessionId, int timeout, int timeoutThreshold, BrainCloudClient client, string peerCode = "")
	{
		_client = client;
		UploadId = uploadId;
		_guidLocalPath = guidLocalPath;
		_serverUrl = serverUrl;
		_sessionId = sessionId;
		_peerCode = peerCode;
		_timeout = timeout;
		_timeoutThreshold = timeoutThreshold;
		Status = FileUploaderStatus.Pending;
	}

	public void Start()
	{
		byte[] array = _client.FileService.FileStorage[_guidLocalPath];
		WWWForm wWWForm = new WWWForm();
		wWWForm.AddField("sessionId", _sessionId);
		if (_peerCode != "")
		{
			wWWForm.AddField("peerCode", _peerCode);
		}
		wWWForm.AddField("uploadId", UploadId);
		wWWForm.AddField("fileSize", array.Length);
		wWWForm.AddBinaryData("uploadFile", array, _fileName);
		_request = UnityWebRequest.Post(_serverUrl, wWWForm);
		_request.SendWebRequest();
		Status = FileUploaderStatus.Uploading;
		if (_client.LoggingEnabled)
		{
			_client.Log("Started upload of " + _fileName);
		}
		_lastTime = DateTime.Now;
	}

	public void CancelUpload()
	{
		_request.Abort();
		Status = FileUploaderStatus.CompleteFailed;
		StatusCode = 900;
		ReasonCode = 90100;
		Response = CreateErrorString(StatusCode, ReasonCode, "Upload of " + _fileName + " cancelled by user");
		if (_client.LoggingEnabled)
		{
			_client.Log("Upload of " + _fileName + " cancelled by user");
		}
	}

	public void Update()
	{
		UpdateDeltaTime();
		_elapsedTime += _deltaTime;
		UpdateTransferRate();
		if (Status == FileUploaderStatus.CompleteFailed || Status == FileUploaderStatus.CompleteSuccess)
		{
			CleanupRequest();
			return;
		}
		Progress = _request.uploadProgress;
		if (_request.isDone)
		{
			HandleResponse();
		}
	}

	private void HandleResponse()
	{
		_transferRatePerSecond = 0L;
		StatusCode = (int)_request.responseCode;
		if (StatusCode != 200)
		{
			Status = FileUploaderStatus.CompleteFailed;
			_client.FileService.FileStorage.Remove(_guidLocalPath);
			if (_request.error != null)
			{
				ReasonCode = 90102;
				Response = CreateErrorString(StatusCode, ReasonCode, _request.error);
			}
			else
			{
				Response = _request.downloadHandler.text;
			}
			JsonErrorMessage jsonErrorMessage = null;
			try
			{
				jsonErrorMessage = JsonReader.Deserialize<JsonErrorMessage>(Response);
			}
			catch (JsonDeserializationException ex)
			{
				if (_client.LoggingEnabled)
				{
					_client.Log(ex.Message);
				}
			}
			if (jsonErrorMessage != null)
			{
				ReasonCode = jsonErrorMessage.reason_code;
			}
			else
			{
				ReasonCode = 90102;
				Response = CreateErrorString(StatusCode, ReasonCode, Response);
			}
		}
		else
		{
			Status = FileUploaderStatus.CompleteSuccess;
			_client.FileService.FileStorage.Remove(_guidLocalPath);
			Response = _request.downloadHandler.text;
			if (_client.LoggingEnabled)
			{
				_client.Log("Uploaded " + _fileName + " in " + _elapsedTime.ToString("0.0##") + " seconds");
			}
		}
		CleanupRequest();
	}

	private void UpdateTransferRate()
	{
		_transferElapsedTime += _deltaTime;
		if (_transferElapsedTime > 0.25)
		{
			_transferRatePerSecond = (long)((double)_transferRatesTotal / _transferElapsedTime);
			_transferRatesTotal = 0L;
			_transferElapsedTime = 0.0;
		}
		else
		{
			_transferRatesTotal += BytesTransferred - _lastTransferTotal;
			_lastTransferTotal = BytesTransferred;
		}
	}

	private void CheckTimeout()
	{
		if (_transferRatePerSecond < _timeoutThreshold)
		{
			_timeUnderMinRate += _deltaTime;
		}
		else
		{
			_timeUnderMinRate = 0.0;
		}
		if (_timeUnderMinRate > (double)_timeout)
		{
			ThrowError(90101, "Upload of " + _fileName + " failed due to timeout.");
		}
	}

	private void UpdateDeltaTime()
	{
		_deltaTime = DateTime.Now.Subtract(_lastTime).TotalSeconds;
		_lastTime = DateTime.Now;
	}

	private void ThrowError(int reasonCode, string message)
	{
		Status = FileUploaderStatus.CompleteFailed;
		StatusCode = 900;
		ReasonCode = reasonCode;
		Response = CreateErrorString(StatusCode, ReasonCode, message);
	}

	private string CreateErrorString(int statusCode, int reasonCode, string message)
	{
		return new JsonErrorMessage(statusCode, reasonCode, message).GetJsonString();
	}

	private void CleanupRequest()
	{
		if (_request != null)
		{
			_request.Dispose();
			_request = null;
		}
	}
}
