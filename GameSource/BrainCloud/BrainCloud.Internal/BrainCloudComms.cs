using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using BrainCloud.JsonFx.Json;
using BrainCloud.MD5Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace BrainCloud.Internal;

internal sealed class BrainCloudComms
{
	private static int NO_PACKET_EXPECTED = -1;

	private BrainCloudClient _clientRef;

	private bool _initialized;

	private bool _enabled = true;

	private long _packetId;

	private long _expectedIncomingPacketId = NO_PACKET_EXPECTED;

	private List<ServerCall> _serviceCallsWaiting = new List<ServerCall>();

	private List<ServerCall> _serviceCallsInProgress = new List<ServerCall>();

	private List<ServerCall> _serviceCallsInTimeoutQueue = new List<ServerCall>();

	private RequestState _activeRequest;

	private DateTime _lastTimePacketSent;

	private TimeSpan _idleTimeout = TimeSpan.FromSeconds(300.0);

	private int _maxBundleMessages = 10;

	private int _killSwitchThreshold = 11;

	private int _identicalFailedAuthAttemptThreshold = 3;

	private int _failedAuthenticationAttempts;

	private Dictionary<string, object> blankResponseData = new Dictionary<string, object>();

	private Dictionary<string, object>[] _recentResponseJsonData = new Dictionary<string, object>[2]
	{
		new Dictionary<string, object>(),
		new Dictionary<string, object>()
	};

	private TimeSpan _authenticationTimeoutDuration = TimeSpan.FromSeconds(30.0);

	private DateTime _authenticationTimeoutStart;

	private long receivedPacketIdChecker;

	private EventCallback _eventCallback;

	private RewardCallback _rewardCallback;

	private FileUploadSuccessCallback _fileUploadSuccessCallback;

	private FileUploadFailedCallback _fileUploadFailedCallback;

	private FailureCallback _globalErrorCallback;

	private NetworkErrorCallback _networkErrorCallback;

	private List<FileUploader> _fileUploads = new List<FileUploader>();

	private int _cachedStatusCode;

	private int _cachedReasonCode;

	private string _cachedStatusMessage;

	private bool _killSwitchEngaged;

	private int _killSwitchErrorCount;

	private string _killSwitchService;

	private string _killSwitchOperation;

	private bool _authInProgress;

	private bool _isAuthenticated;

	private int _uploadLowTransferRateTimeout = 120;

	private int _uploadLowTransferRateThreshold = 50;

	private List<int> _packetTimeouts = new List<int> { 15, 20, 35, 50 };

	private readonly int[] _listAuthPacketTimeouts = new int[3] { 15, 30, 60 };

	private int _authPacketTimeoutSecs = 15;

	private bool _oldStyleStatusResponseInErrorCallback;

	private bool _cacheMessagesOnNetworkError;

	private bool _blockingQueue;

	public bool SupportsCompression { get; private set; }

	public int ClientSideCompressionThreshold { get; private set; } = 50000;

	public bool AuthenticateInProgress
	{
		get
		{
			return _authInProgress;
		}
		set
		{
			_authInProgress = value;
		}
	}

	public bool Authenticated => _isAuthenticated;

	public Dictionary<string, string> AppIdSecretMap { get; private set; }

	public string AppId { get; private set; }

	public string SecretKey
	{
		get
		{
			if (AppIdSecretMap.ContainsKey(AppId))
			{
				return AppIdSecretMap[AppId];
			}
			return "NO SECRET DEFINED FOR '" + AppId + "'";
		}
	}

	public string SessionID { get; private set; }

	public string ServerURL { get; private set; }

	public string UploadURL { get; private set; }

	public int UploadLowTransferRateTimeout
	{
		get
		{
			return _uploadLowTransferRateTimeout;
		}
		set
		{
			_uploadLowTransferRateTimeout = value;
		}
	}

	public int UploadLowTransferRateThreshold
	{
		get
		{
			return _uploadLowTransferRateThreshold;
		}
		set
		{
			_uploadLowTransferRateThreshold = value;
		}
	}

	public List<int> PacketTimeouts
	{
		get
		{
			return _packetTimeouts;
		}
		set
		{
			_packetTimeouts = value;
		}
	}

	public int AuthenticationPacketTimeoutSecs
	{
		get
		{
			return _authPacketTimeoutSecs;
		}
		set
		{
			_authPacketTimeoutSecs = value;
		}
	}

	public bool OldStyleStatusResponseInErrorCallback
	{
		get
		{
			return _oldStyleStatusResponseInErrorCallback;
		}
		set
		{
			_oldStyleStatusResponseInErrorCallback = value;
		}
	}

	public void EnableCompression(bool compress)
	{
		SupportsCompression = compress;
	}

	public long GetReceivedPacketId()
	{
		return receivedPacketIdChecker;
	}

	internal void setAuthenticated()
	{
		_isAuthenticated = true;
	}

	internal void setSessionId(string sessionId)
	{
		SessionID = sessionId;
	}

	public void SetPacketTimeoutsToDefault()
	{
		_packetTimeouts = new List<int> { 15, 20, 35, 50 };
	}

	public void EnableNetworkErrorMessageCaching(bool enabled)
	{
		_cacheMessagesOnNetworkError = enabled;
	}

	public BrainCloudComms(BrainCloudClient client)
	{
		AppIdSecretMap = new Dictionary<string, string>();
		_clientRef = client;
		ResetErrorCache();
	}

	public void Initialize(string serverURL, string appId, string secretKey)
	{
		ResetCommunication();
		_expectedIncomingPacketId = NO_PACKET_EXPECTED;
		ServerURL = serverURL;
		string text = "/dispatcherv2";
		string text2 = (ServerURL.EndsWith(text) ? ServerURL.Substring(0, ServerURL.Length - text.Length) : ServerURL);
		while (text2.Length > 0 && text2.EndsWith("/"))
		{
			text2 = text2.Substring(0, text2.Length - 1);
		}
		UploadURL = text2;
		UploadURL += "/uploader";
		AppIdSecretMap[appId] = secretKey;
		AppId = appId;
		_blockingQueue = false;
		_initialized = true;
	}

	public void InitializeWithApps(string serverURL, string defaultAppId, Dictionary<string, string> appIdSecretMap)
	{
		AppIdSecretMap.Clear();
		AppIdSecretMap = appIdSecretMap;
		Initialize(serverURL, defaultAppId, AppIdSecretMap[defaultAppId]);
	}

	public void RegisterEventCallback(EventCallback cb)
	{
		_eventCallback = cb;
	}

	public void DeregisterEventCallback()
	{
		_eventCallback = null;
	}

	public void RegisterRewardCallback(RewardCallback cb)
	{
		_rewardCallback = cb;
	}

	public void DeregisterRewardCallback()
	{
		_rewardCallback = null;
	}

	public void RegisterFileUploadCallbacks(FileUploadSuccessCallback success, FileUploadFailedCallback failure)
	{
		_fileUploadSuccessCallback = success;
		_fileUploadFailedCallback = failure;
	}

	public void DeregisterFileUploadCallbacks()
	{
		_fileUploadSuccessCallback = null;
		_fileUploadFailedCallback = null;
	}

	public void RegisterGlobalErrorCallback(FailureCallback callback)
	{
		_globalErrorCallback = callback;
	}

	public void DeregisterGlobalErrorCallback()
	{
		_globalErrorCallback = null;
	}

	public void RegisterNetworkErrorCallback(NetworkErrorCallback callback)
	{
		_networkErrorCallback = callback;
	}

	public void DeregisterNetworkErrorCallback()
	{
		_networkErrorCallback = null;
	}

	public void Update()
	{
		if (!_initialized || !_enabled || _blockingQueue)
		{
			return;
		}
		bool bypassTimeout = false;
		RequestState.eWebRequestStatus eWebRequestStatus = RequestState.eWebRequestStatus.STATUS_PENDING;
		if (_activeRequest != null)
		{
			eWebRequestStatus = GetWebRequestStatus(_activeRequest);
			switch (eWebRequestStatus)
			{
			case RequestState.eWebRequestStatus.STATUS_ERROR:
				bypassTimeout = _activeRequest.Retries >= GetMaxRetriesForPacket(_activeRequest);
				break;
			case RequestState.eWebRequestStatus.STATUS_DONE:
			{
				if (_activeRequest.WebRequest.responseCode == 200)
				{
					ResetIdleTimer();
					HandleResponseBundle(GetWebRequestResponse(_activeRequest));
					DisposeUploadHandler();
					_activeRequest = null;
					break;
				}
				if (_activeRequest.WebRequest.responseCode == 502 || _activeRequest.WebRequest.responseCode == 503 || _activeRequest.WebRequest.responseCode == 504)
				{
					_clientRef.Log("Packet in progress");
					RetryRequest(eWebRequestStatus, bypassTimeout);
					return;
				}
				string webRequestResponse = GetWebRequestResponse(_activeRequest);
				if (_serviceCallsInProgress.Count > 0)
				{
					_serviceCallsInProgress[0].GetCallback()?.OnErrorCallback(404, (int)_activeRequest.WebRequest.responseCode, webRequestResponse);
				}
				break;
			}
			}
		}
		RetryRequest(eWebRequestStatus, bypassTimeout);
		if (_isAuthenticated && !_blockingQueue && DateTime.Now.Subtract(_lastTimePacketSent) >= _idleTimeout)
		{
			SendHeartbeat();
		}
		if (tooManyAuthenticationAttempts())
		{
			if (_clientRef.LoggingEnabled)
			{
				_clientRef.Log("TIMER ON");
				_clientRef.Log(DateTime.Now.Subtract(_authenticationTimeoutStart).ToString());
			}
			if (DateTime.Now.Subtract(_authenticationTimeoutStart) >= _authenticationTimeoutDuration)
			{
				if (_clientRef.LoggingEnabled)
				{
					_clientRef.Log("TIMER FINISHED");
				}
				_killSwitchEngaged = false;
				ResetKillSwitch();
			}
		}
		RunFileUploadCallbacks();
	}

	private void RunFileUploadCallbacks()
	{
		for (int num = _fileUploads.Count - 1; num >= 0; num--)
		{
			_fileUploads[num].Update();
			if (_fileUploads[num].Status == FileUploader.FileUploaderStatus.CompleteSuccess)
			{
				if (_fileUploadSuccessCallback != null)
				{
					_fileUploadSuccessCallback(_fileUploads[num].UploadId, _fileUploads[num].Response);
				}
				if (_clientRef.LoggingEnabled)
				{
					_clientRef.Log("Upload success: " + _fileUploads[num].UploadId + " | " + _fileUploads[num].StatusCode + "\n" + _fileUploads[num].Response);
				}
				_fileUploads.RemoveAt(num);
			}
			else if (_fileUploads[num].Status == FileUploader.FileUploaderStatus.CompleteFailed)
			{
				if (_fileUploadFailedCallback != null)
				{
					_fileUploadFailedCallback(_fileUploads[num].UploadId, _fileUploads[num].StatusCode, _fileUploads[num].ReasonCode, _fileUploads[num].Response);
				}
				if (_clientRef.LoggingEnabled)
				{
					_clientRef.Log("Upload failed: " + _fileUploads[num].UploadId + " | " + _fileUploads[num].StatusCode + "\n" + _fileUploads[num].Response);
				}
				_fileUploads.RemoveAt(num);
			}
		}
	}

	public void CancelUpload(string uploadFileId)
	{
		GetFileUploader(uploadFileId)?.CancelUpload();
	}

	public double GetUploadProgress(string uploadFileId)
	{
		return GetFileUploader(uploadFileId)?.Progress ?? (-1.0);
	}

	public long GetUploadBytesTransferred(string uploadFileId)
	{
		return GetFileUploader(uploadFileId)?.BytesTransferred ?? (-1);
	}

	public long GetUploadTotalBytesToTransfer(string uploadFileId)
	{
		return GetFileUploader(uploadFileId)?.TotalBytesToTransfer ?? (-1);
	}

	private FileUploader GetFileUploader(string uploadId)
	{
		for (int i = 0; i < _fileUploads.Count; i++)
		{
			if (_fileUploads[i].UploadId == uploadId)
			{
				return _fileUploads[i];
			}
		}
		if (_clientRef.LoggingEnabled)
		{
			_clientRef.Log("GetUploadProgress could not find upload ID " + uploadId);
		}
		return null;
	}

	private void TriggerCommsError(int status, int reasonCode, string statusMessage)
	{
		int num = 0;
		lock (_serviceCallsInProgress)
		{
			num = _serviceCallsInProgress.Count;
		}
		if (num <= 0)
		{
			num = 1;
		}
		JsonResponseErrorBundleV2 jsonResponseErrorBundleV = new JsonResponseErrorBundleV2();
		jsonResponseErrorBundleV.packetId = _expectedIncomingPacketId;
		jsonResponseErrorBundleV.responses = new JsonErrorMessage[num];
		for (int i = 0; i < num; i++)
		{
			jsonResponseErrorBundleV.responses[i] = new JsonErrorMessage(status, reasonCode, statusMessage);
		}
		string jsonData = JsonWriter.Serialize(jsonResponseErrorBundleV);
		HandleResponseBundle(jsonData);
	}

	public void ShutDown()
	{
		lock (_serviceCallsWaiting)
		{
			_serviceCallsWaiting.Clear();
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(null, null);
		ServerCall call = new ServerCall(ServiceName.PlayerState, ServiceOperation.Logout, null, callback);
		AddToQueue(call);
		DisposeUploadHandler();
		_activeRequest = null;
		Update();
		ResetCommunication();
	}

	public void RetryCachedMessages()
	{
		if (!_blockingQueue)
		{
			return;
		}
		if (_clientRef.LoggingEnabled)
		{
			_clientRef.Log("Retrying cached messages");
		}
		if (_activeRequest != null)
		{
			if (_clientRef.LoggingEnabled)
			{
				_clientRef.Log("ERROR - retrying cached messages but there is an active request!");
			}
			_activeRequest.CancelRequest();
			DisposeUploadHandler();
			_activeRequest = null;
		}
		_packetId--;
		_activeRequest = CreateAndSendNextRequestBundle();
		_blockingQueue = false;
	}

	public void FlushCachedMessages(bool sendApiErrorCallbacks)
	{
		if (!_blockingQueue)
		{
			return;
		}
		if (_clientRef.LoggingEnabled)
		{
			_clientRef.Log("Flushing cached messages");
		}
		if (_activeRequest != null)
		{
			_activeRequest.CancelRequest();
			DisposeUploadHandler();
			_activeRequest = null;
		}
		List<ServerCall> list = new List<ServerCall>();
		lock (_serviceCallsInTimeoutQueue)
		{
			int i = 0;
			for (int count = _serviceCallsInTimeoutQueue.Count; i < count; i++)
			{
				list.Add(_serviceCallsInTimeoutQueue[i]);
			}
			_serviceCallsInTimeoutQueue.Clear();
		}
		lock (_serviceCallsWaiting)
		{
			int j = 0;
			for (int count2 = _serviceCallsWaiting.Count; j < count2; j++)
			{
				list.Add(_serviceCallsWaiting[j]);
			}
			_serviceCallsWaiting.Clear();
		}
		lock (_serviceCallsInProgress)
		{
			_serviceCallsInProgress.Clear();
		}
		if (sendApiErrorCallbacks)
		{
			int k = 0;
			for (int count3 = list.Count; k < count3; k++)
			{
				ServerCall serverCall = list[k];
				if (serverCall.GetCallback() != null)
				{
					serverCall.GetCallback().OnErrorCallback(900, 90001, "Timeout trying to reach brainCloud server, please check the URL and/or certificates for server");
				}
			}
		}
		_blockingQueue = false;
	}

	internal void InsertEndOfMessageBundleMarker()
	{
		AddToQueue(new EndOfBundleMarker());
	}

	private void ResetIdleTimer()
	{
		_lastTimePacketSent = DateTime.Now;
	}

	private void ResetAuthenticationTimer()
	{
		_authenticationTimeoutStart = DateTime.Now;
	}

	private bool tooManyAuthenticationAttempts()
	{
		return _failedAuthenticationAttempts >= _identicalFailedAuthAttemptThreshold;
	}

	private void SaveProfileAndSessionIds(Dictionary<string, object> responseData, string data)
	{
		string jsonString = GetJsonString(responseData, OperationParam.ServiceMessageSessionId.Value, null);
		if (jsonString != null)
		{
			SessionID = jsonString;
			_isAuthenticated = true;
			_authInProgress = false;
		}
		string jsonString2 = GetJsonString(responseData, OperationParam.ProfileId.Value, null);
		if (jsonString2 != null)
		{
			_clientRef.AuthenticationService.ProfileId = jsonString2;
		}
	}

	private void HandleResponseBundle(string jsonData)
	{
		if (_clientRef.LoggingEnabled)
		{
			_clientRef.Log(string.Format("{0} - {1}\n{2}", "RESPONSE", DateTime.Now, jsonData));
		}
		JsonResponseBundleV2 jsonResponseBundleV = DeserializeJson(jsonData);
		if (jsonResponseBundleV == null)
		{
			_cachedReasonCode = 40408;
			_cachedStatusCode = 900;
			_cachedStatusMessage = "Received an invalid json format response, check your network settings.";
			_cacheMessagesOnNetworkError = true;
			lock (_serviceCallsWaiting)
			{
				if (_serviceCallsInProgress.Count > 0)
				{
					_serviceCallsInProgress[0].GetCallback().OnErrorCallback(_cachedStatusCode, _cachedReasonCode, _cachedStatusMessage);
					_serviceCallsInProgress.RemoveAt(0);
				}
			}
			_clientRef.Log(_cachedStatusMessage);
			return;
		}
		Dictionary<string, object>[] responses = jsonResponseBundleV.responses;
		Dictionary<string, object> dictionary = null;
		long num = (receivedPacketIdChecker = jsonResponseBundleV.packetId);
		if (num != NO_PACKET_EXPECTED && (_expectedIncomingPacketId == NO_PACKET_EXPECTED || _expectedIncomingPacketId != num))
		{
			if (_clientRef.LoggingEnabled)
			{
				_clientRef.Log("Dropping duplicate packet");
			}
			for (int i = 0; i < responses.Length; i++)
			{
				lock (_serviceCallsInProgress)
				{
					if (_serviceCallsInProgress.Count > 0)
					{
						_serviceCallsInProgress.RemoveAt(0);
					}
				}
			}
			return;
		}
		_expectedIncomingPacketId = NO_PACKET_EXPECTED;
		IList<Exception> list = new List<Exception>();
		string text = "";
		ServerCall serverCall = null;
		ServerCallback serverCallback = null;
		string text2 = "";
		string text3 = "";
		Dictionary<string, object> dictionary2 = null;
		for (int j = 0; j < responses.Length; j++)
		{
			dictionary = responses[j];
			int num2 = (int)dictionary["status"];
			text = "";
			dictionary2 = null;
			serverCall = null;
			serverCallback = null;
			text2 = "";
			text3 = "";
			lock (_serviceCallsWaiting)
			{
				if (_serviceCallsInProgress.Count > 0)
				{
					serverCall = _serviceCallsInProgress[0];
					_serviceCallsInProgress.RemoveAt(0);
				}
			}
			if (num2 == 200)
			{
				ResetKillSwitch();
				text2 = serverCall.GetService();
				if (dictionary[OperationParam.ServiceMessageData.Value] != null)
				{
					dictionary2 = (Dictionary<string, object>)dictionary[OperationParam.ServiceMessageData.Value];
					text = JsonWriter.Serialize(dictionary);
					if (text2 == ServiceName.Authenticate.Value || text2 == ServiceName.Identity.Value)
					{
						_authPacketTimeoutSecs = _listAuthPacketTimeouts[0];
						SaveProfileAndSessionIds(dictionary2, text);
					}
				}
				else
				{
					text = JsonWriter.Serialize(dictionary);
				}
				if (serverCall == null)
				{
					continue;
				}
				serverCallback = serverCall.GetCallback();
				text3 = serverCall.GetOperation();
				bool flag = false;
				try
				{
					flag = text3 == ServiceOperation.RunPeerScript.Value && dictionary.ContainsKey(OperationParam.ServiceMessageData.Value) && ((Dictionary<string, object>)dictionary[OperationParam.ServiceMessageData.Value]).ContainsKey("response") && ((Dictionary<string, object>)((Dictionary<string, object>)dictionary[OperationParam.ServiceMessageData.Value])["response"]).ContainsKey(OperationParam.ServiceMessageData.Value) && ((Dictionary<string, object>)((Dictionary<string, object>)((Dictionary<string, object>)dictionary[OperationParam.ServiceMessageData.Value])["response"])[OperationParam.ServiceMessageData.Value]).ContainsKey("fileDetails");
				}
				catch (Exception)
				{
				}
				if (text3 == ServiceOperation.FullReset.Value || text3 == ServiceOperation.Logout.Value)
				{
					_isAuthenticated = false;
					SessionID = "";
					_clientRef.AuthenticationService.ClearSavedProfileID();
					ResetErrorCache();
				}
				else if (text3 == ServiceOperation.Authenticate.Value)
				{
					ProcessAuthenticate(dictionary2);
				}
				else if (text3.Equals(ServiceOperation.SwitchToChildProfile.Value) || text3.Equals(ServiceOperation.SwitchToParentProfile.Value))
				{
					ProcessSwitchResponse(dictionary2);
				}
				else if (text3 == ServiceOperation.PrepareUserUpload.Value || flag)
				{
					string text4 = ((flag && serverCall.GetJsonData().Contains("peer")) ? ((string)serverCall.GetJsonData()["peer"]) : "");
					Dictionary<string, object> dictionary3 = ((text4 == "") ? ((Dictionary<string, object>)dictionary2["fileDetails"]) : ((Dictionary<string, object>)((Dictionary<string, object>)((Dictionary<string, object>)dictionary2["response"])[OperationParam.ServiceMessageData.Value])["fileDetails"]));
					if (dictionary3.ContainsKey("uploadId") && dictionary3.ContainsKey("localPath"))
					{
						string uploadId = (string)dictionary3["uploadId"];
						string text5 = (string)dictionary3["localPath"];
						string fileName = (string)dictionary3["cloudFilename"];
						FileUploader fileUploader = new FileUploader(uploadId, text5, UploadURL, SessionID, _uploadLowTransferRateTimeout, _uploadLowTransferRateThreshold, _clientRef, text4);
						fileUploader.FileName = fileName;
						if (_clientRef.FileService.FileStorage.ContainsKey(text5))
						{
							fileUploader.TotalBytesToTransfer = _clientRef.FileService.FileStorage[text5].Length;
						}
						_fileUploads.Add(fileUploader);
						fileUploader.Start();
					}
				}
				if (serverCallback != null)
				{
					try
					{
						serverCallback.OnSuccessCallback(text);
					}
					catch (Exception ex2)
					{
						if (_clientRef.LoggingEnabled)
						{
							_clientRef.Log(ex2.StackTrace);
						}
						list.Add(ex2);
					}
				}
				_failedAuthenticationAttempts = 0;
				if (_rewardCallback == null || dictionary2 == null)
				{
					continue;
				}
				try
				{
					Dictionary<string, object> dictionary4 = null;
					if (text3 == ServiceOperation.Authenticate.Value)
					{
						object value = null;
						if (dictionary2.TryGetValue("rewards", out value))
						{
							Dictionary<string, object> dictionary5 = (Dictionary<string, object>)value;
							if (dictionary5.TryGetValue("rewards", out value) && ((Dictionary<string, object>)value).Count > 0)
							{
								dictionary4 = dictionary5;
							}
						}
					}
					else if (text3 == ServiceOperation.Update.Value || text3 == ServiceOperation.Trigger.Value || text3 == ServiceOperation.TriggerMultiple.Value)
					{
						object value2 = null;
						if (dictionary2.TryGetValue("rewards", out value2) && ((Dictionary<string, object>)value2).Count > 0)
						{
							dictionary4 = dictionary2;
						}
					}
					if (dictionary4 != null)
					{
						Dictionary<string, object> dictionary6 = new Dictionary<string, object>();
						dictionary6["rewards"] = dictionary4;
						dictionary6["service"] = text2;
						dictionary6["operation"] = text3;
						string jsonResponse = JsonWriter.Serialize(new Dictionary<string, object> { ["apiRewards"] = new List<object> { dictionary6 } });
						_rewardCallback(jsonResponse);
					}
				}
				catch (Exception ex3)
				{
					if (_clientRef.LoggingEnabled)
					{
						_clientRef.Log(ex3.StackTrace);
					}
					list.Add(ex3);
				}
				continue;
			}
			object value3 = null;
			object value4 = null;
			int num3 = 0;
			string text6 = "";
			serverCallback = serverCall.GetCallback();
			text3 = serverCall.GetOperation();
			if (text3 == ServiceOperation.Authenticate.Value)
			{
				if (!tooManyAuthenticationAttempts())
				{
					_failedAuthenticationAttempts++;
					if (tooManyAuthenticationAttempts())
					{
						ResetAuthenticationTimer();
					}
				}
				_authInProgress = false;
			}
			if (dictionary.TryGetValue("reason_code", out value3))
			{
				num3 = (int)value3;
			}
			if (_oldStyleStatusResponseInErrorCallback)
			{
				if (dictionary.TryGetValue("status_message", out value4))
				{
					text6 = (string)value4;
				}
			}
			else
			{
				text6 = JsonWriter.Serialize(dictionary);
			}
			if (num3 == 40303 || num3 == 40304 || num3 == 40356)
			{
				_isAuthenticated = false;
				SessionID = "";
				if (_clientRef.LoggingEnabled)
				{
					_clientRef.Log("Received session expired or not found, need to re-authenticate");
				}
				_cachedStatusCode = num2;
				_cachedReasonCode = num3;
				object value5 = null;
				if (dictionary.TryGetValue("status_message", out value5))
				{
					_cachedStatusMessage = value5 as string;
				}
			}
			if (text3 == ServiceOperation.Logout.Value && num3 == 90001)
			{
				_isAuthenticated = false;
				SessionID = "";
				if (_clientRef.LoggingEnabled)
				{
					_clientRef.Log("Could not communicate with the server on logout due to network timeout");
				}
			}
			if (serverCallback != null)
			{
				try
				{
					serverCallback.OnErrorCallback(num2, num3, text6);
				}
				catch (Exception ex4)
				{
					if (_clientRef.LoggingEnabled)
					{
						_clientRef.Log(ex4.StackTrace);
					}
					list.Add(ex4);
				}
			}
			if (_globalErrorCallback != null)
			{
				object obj = null;
				if (serverCallback != null)
				{
					obj = serverCallback.m_cbObject;
					if (obj != null && obj is WrapperAuthCallbackObject)
					{
						obj = ((WrapperAuthCallbackObject)obj)._cbObject;
					}
				}
				_globalErrorCallback(num2, num3, text6, obj);
			}
			UpdateKillSwitch(serverCall.Service, serverCall.Operation, num2);
		}
		if (jsonResponseBundleV.events != null && _eventCallback != null)
		{
			string jsonResponse2 = JsonWriter.Serialize(new Dictionary<string, Dictionary<string, object>[]> { ["events"] = jsonResponseBundleV.events });
			try
			{
				_eventCallback(jsonResponse2);
			}
			catch (Exception ex5)
			{
				if (_clientRef.LoggingEnabled)
				{
					_clientRef.Log(ex5.StackTrace);
				}
				list.Add(ex5);
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		DisposeUploadHandler();
		_activeRequest = null;
		throw new Exception("User callback handlers threw " + list.Count + " exception(s). See the Unity log for callstacks or inner exception for first exception thrown.", list[0]);
	}

	private void UpdateKillSwitch(string service, string operation, int statusCode)
	{
		if (statusCode == 900)
		{
			return;
		}
		if (_killSwitchService == null)
		{
			_killSwitchService = service;
			_killSwitchOperation = operation;
			_killSwitchErrorCount++;
		}
		else if (service == _killSwitchService && operation == _killSwitchOperation)
		{
			_killSwitchErrorCount++;
		}
		if (!_killSwitchEngaged && _killSwitchErrorCount >= _killSwitchThreshold)
		{
			_killSwitchEngaged = true;
			if (_clientRef.LoggingEnabled)
			{
				_clientRef.Log("Client disabled due to repeated errors from a single API call: " + service + " | " + operation);
			}
		}
		if (!(operation == ServiceOperation.Authenticate.Value))
		{
			return;
		}
		if (_clientRef.LoggingEnabled)
		{
			_clientRef.Log("Failed Authentication Call");
		}
		string text = _failedAuthenticationAttempts.ToString();
		if (_clientRef.LoggingEnabled)
		{
			_clientRef.Log("Current number of failed authentications: " + text);
		}
		if (tooManyAuthenticationAttempts())
		{
			if (_clientRef.LoggingEnabled)
			{
				_clientRef.Log("Too many repeat authentication failures");
			}
			_killSwitchEngaged = true;
			ResetAuthenticationTimer();
		}
	}

	private void ResetKillSwitch()
	{
		_killSwitchErrorCount = 0;
		_killSwitchService = null;
		_killSwitchOperation = null;
		_failedAuthenticationAttempts = 0;
		_recentResponseJsonData[0] = blankResponseData;
		_recentResponseJsonData[1] = blankResponseData;
	}

	private RequestState CreateAndSendNextRequestBundle()
	{
		RequestState requestState = null;
		lock (_serviceCallsWaiting)
		{
			if (_blockingQueue)
			{
				_serviceCallsInProgress.InsertRange(0, _serviceCallsInTimeoutQueue);
				_serviceCallsInTimeoutQueue.Clear();
			}
			else if (_serviceCallsWaiting.Count > 0)
			{
				ServerCall serverCall = null;
				int num = _serviceCallsWaiting.Count;
				for (int i = 0; i < _serviceCallsWaiting.Count; i++)
				{
					serverCall = _serviceCallsWaiting[i];
					if (serverCall.GetType() == typeof(EndOfBundleMarker))
					{
						if (i != 0)
						{
							num = i;
							_serviceCallsWaiting.RemoveAt(i);
							break;
						}
						_serviceCallsWaiting.RemoveAt(0);
						i--;
						num--;
					}
					else if (serverCall.GetOperation() == ServiceOperation.Authenticate.Value)
					{
						if (i != 0)
						{
							_serviceCallsWaiting.RemoveAt(i);
							_serviceCallsWaiting.Insert(0, serverCall);
						}
						num = 1;
						break;
					}
				}
				if (num > _maxBundleMessages)
				{
					num = _maxBundleMessages;
				}
				if (num <= 0)
				{
					return null;
				}
				if (_serviceCallsInProgress.Count > 0)
				{
					if (_clientRef.LoggingEnabled)
					{
						_clientRef.Log("ERROR - in progress queue is not empty but we're ready for the next message!");
					}
					_serviceCallsInProgress.Clear();
				}
				_serviceCallsInProgress = _serviceCallsWaiting.GetRange(0, num);
				_serviceCallsWaiting.RemoveRange(0, num);
			}
			if (_serviceCallsInProgress.Count > 0)
			{
				requestState = new RequestState();
				List<object> list = new List<object>();
				bool flag = false;
				string text = "";
				for (int j = 0; j < _serviceCallsInProgress.Count; j++)
				{
					ServerCall serverCall2 = _serviceCallsInProgress[j];
					text = serverCall2.GetOperation();
					if (serverCall2.GetService().Equals(ServiceName.HeartBeat) && text.Equals(ServiceOperation.Read) && (serverCall2.GetCallback() == null || serverCall2.GetCallback().AreCallbacksNull()) && _serviceCallsInProgress.Count > 1)
					{
						_serviceCallsInProgress.RemoveAt(j);
						j--;
						continue;
					}
					Dictionary<string, object> dictionary = new Dictionary<string, object>();
					dictionary[OperationParam.ServiceMessageService.Value] = serverCall2.Service;
					dictionary[OperationParam.ServiceMessageOperation.Value] = serverCall2.Operation;
					dictionary[OperationParam.ServiceMessageData.Value] = serverCall2.GetJsonData();
					list.Add(dictionary);
					if (text.Equals(ServiceOperation.Authenticate.Value))
					{
						requestState.PacketNoRetry = true;
					}
					if (text.Equals(ServiceOperation.Authenticate.Value) || text.Equals(ServiceOperation.ResetEmailPassword.Value) || text.Equals(ServiceOperation.ResetEmailPasswordAdvanced.Value) || text.Equals(ServiceOperation.ResetUniversalIdPassword.Value) || text.Equals(ServiceOperation.ResetUniversalIdPasswordAdvanced.Value))
					{
						flag = true;
					}
					if (text.Equals(ServiceOperation.FullReset.Value) || text.Equals(ServiceOperation.Logout.Value))
					{
						requestState.PacketRequiresLongTimeout = true;
					}
				}
				requestState.PacketId = _packetId;
				_expectedIncomingPacketId = _packetId;
				requestState.MessageList = list;
				_packetId++;
				if (!_killSwitchEngaged && !tooManyAuthenticationAttempts())
				{
					if (_isAuthenticated || flag)
					{
						if (_clientRef.LoggingEnabled)
						{
							_clientRef.Log("SENDING REQUEST");
						}
						InternalSendMessage(requestState);
					}
					else
					{
						FakeErrorResponse(requestState, _cachedStatusCode, _cachedReasonCode, _cachedStatusMessage);
						requestState = null;
					}
				}
				else if (tooManyAuthenticationAttempts())
				{
					FakeErrorResponse(requestState, 900, 90201, "Client has been disabled due to identical repeat Authentication calls that are throwing errors. Authenticating with the same credentials is disabled for 30 seconds");
					requestState = null;
				}
				else
				{
					FakeErrorResponse(requestState, 900, 90200, "Client has been disabled due to repeated errors from a single API call");
					requestState = null;
				}
			}
		}
		return requestState;
	}

	private void FakeErrorResponse(RequestState requestState, int statusCode, int reasonCode, string statusMessage)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ServiceMessagePacketId.Value] = requestState.PacketId;
		dictionary[OperationParam.ServiceMessageSessionId.Value] = SessionID;
		if (AppId != null && AppId.Length > 0)
		{
			dictionary[OperationParam.ServiceMessageGameId.Value] = AppId;
		}
		dictionary[OperationParam.ServiceMessageMessages.Value] = requestState.MessageList;
		string arg = JsonWriter.Serialize(dictionary);
		if (_clientRef.LoggingEnabled)
		{
			_clientRef.Log(string.Format("{0} - {1}\n{2}", "REQUEST" + ((requestState.Retries > 0) ? (" Retry(" + requestState.Retries + ")") : ""), DateTime.Now, arg));
		}
		ResetIdleTimer();
		TriggerCommsError(statusCode, reasonCode, statusMessage);
		DisposeUploadHandler();
		_activeRequest = null;
	}

	private void InternalSendMessage(RequestState requestState)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ServiceMessagePacketId.Value] = requestState.PacketId;
		dictionary[OperationParam.ServiceMessageSessionId.Value] = SessionID;
		if (AppId != null && AppId.Length > 0)
		{
			dictionary[OperationParam.ServiceMessageGameId.Value] = AppId;
		}
		dictionary[OperationParam.ServiceMessageMessages.Value] = requestState.MessageList;
		string text = JsonWriter.Serialize(dictionary);
		string text2 = CalculateMD5Hash(text + SecretKey);
		byte[] array = Encoding.UTF8.GetBytes(text);
		requestState.Signature = text2;
		int num;
		if (SupportsCompression && ClientSideCompressionThreshold >= 0)
		{
			num = ((array.Length >= ClientSideCompressionThreshold) ? 1 : 0);
			if (num != 0)
			{
				array = Compress(array);
			}
		}
		else
		{
			num = 0;
		}
		requestState.ByteArray = array;
		UnityWebRequest unityWebRequest = UnityWebRequest.Post(formFields: new Dictionary<string, string>(), uri: ServerURL);
		unityWebRequest.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
		unityWebRequest.SetRequestHeader("X-SIG", text2);
		if (AppId != null && AppId.Length > 0)
		{
			unityWebRequest.SetRequestHeader("X-APPID", AppId);
		}
		if (num != 0)
		{
			unityWebRequest.SetRequestHeader("Content-Encoding", "gzip");
		}
		unityWebRequest.uploadHandler = new UploadHandlerRaw(array);
		unityWebRequest.SendWebRequest();
		requestState.WebRequest = unityWebRequest;
		requestState.RequestString = text;
		requestState.TimeSent = DateTime.Now;
		ResetIdleTimer();
		if (_clientRef.LoggingEnabled)
		{
			_clientRef.Log(string.Format("{0} - {1}\n{2}", "REQUEST" + ((requestState.Retries > 0) ? (" Retry(" + requestState.Retries + ")") : ""), DateTime.Now, text));
		}
	}

	private byte[] Compress(byte[] raw)
	{
		MemoryStream memoryStream = new MemoryStream();
		using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
		{
			gZipStream.Write(raw, 0, raw.Length);
		}
		return memoryStream.ToArray();
	}

	private byte[] Decompress(byte[] compressedBytes)
	{
		using MemoryStream stream = new MemoryStream(compressedBytes);
		using GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress);
		using MemoryStream memoryStream = new MemoryStream();
		gZipStream.CopyTo(memoryStream);
		memoryStream.Read(compressedBytes, 0, compressedBytes.Length);
		return memoryStream.ToArray();
	}

	private bool ResendMessage(RequestState requestState)
	{
		if (_activeRequest.Retries >= GetMaxRetriesForPacket(requestState))
		{
			return false;
		}
		RequestState activeRequest = _activeRequest;
		int retries = activeRequest.Retries + 1;
		activeRequest.Retries = retries;
		InternalSendMessage(requestState);
		return true;
	}

	private RequestState.eWebRequestStatus GetWebRequestStatus(RequestState requestState)
	{
		RequestState.eWebRequestStatus result = RequestState.eWebRequestStatus.STATUS_PENDING;
		if (_activeRequest.LoseThisPacket)
		{
			return result;
		}
		if (!string.IsNullOrEmpty(_activeRequest.WebRequest.error))
		{
			result = RequestState.eWebRequestStatus.STATUS_ERROR;
		}
		else if (_activeRequest.WebRequest.downloadHandler.isDone)
		{
			result = RequestState.eWebRequestStatus.STATUS_DONE;
		}
		else if (_activeRequest.WebRequest.isDone)
		{
			result = RequestState.eWebRequestStatus.STATUS_DONE;
		}
		return result;
	}

	private string GetWebRequestResponse(RequestState requestState)
	{
		string text = "";
		if (_activeRequest.WebRequest.result == UnityWebRequest.Result.ConnectionError)
		{
			Debug.LogWarning("Failed to communicate with the server. For example, the request couldn't connect or it could not establish a secure channel");
		}
		else if (_activeRequest.WebRequest.result == UnityWebRequest.Result.ProtocolError)
		{
			Debug.LogWarning("The server returned an error response. The request succeeded in communicating with the server, but received an error as defined by the connection protocol.");
		}
		else if (_activeRequest.WebRequest.result == UnityWebRequest.Result.DataProcessingError)
		{
			Debug.LogWarning("Error processing data. The request succeeded in communicating with the server, but encountered an error when processing the received data. For example, the data was corrupted or not in the correct format.");
		}
		if (!string.IsNullOrEmpty(_activeRequest.WebRequest.error))
		{
			text = _activeRequest.WebRequest.error;
		}
		if (_activeRequest.WebRequest.GetRequestHeader("Content-Encoding") != "gzip")
		{
			text = _activeRequest.WebRequest.downloadHandler.text;
		}
		else
		{
			byte[] array = Decompress(_activeRequest.WebRequest.downloadHandler.data);
			text = Encoding.UTF8.GetString(array, 0, array.Length);
		}
		if (text.Contains("Security violation 47") || text.StartsWith("<"))
		{
			Debug.LogWarning("Please re-select app in brainCloud settings, something went wrong");
		}
		return text;
	}

	private int GetMaxRetriesForPacket(RequestState requestState)
	{
		if (requestState.PacketNoRetry)
		{
			return 0;
		}
		return _packetTimeouts.Count;
	}

	private TimeSpan GetPacketTimeout(RequestState requestState)
	{
		if (requestState.PacketNoRetry)
		{
			if (DateTime.Now.Subtract(_activeRequest.TimeSent) > TimeSpan.FromSeconds(_authPacketTimeoutSecs))
			{
				for (int i = 0; i < _listAuthPacketTimeouts.Length; i++)
				{
					if (_listAuthPacketTimeouts[i] == _authPacketTimeoutSecs && i + 1 < _listAuthPacketTimeouts.Length)
					{
						_authPacketTimeoutSecs = _listAuthPacketTimeouts[i + 1];
						break;
					}
				}
			}
			return TimeSpan.FromSeconds(_authPacketTimeoutSecs);
		}
		int retries = requestState.Retries;
		_ = requestState.PacketRequiresLongTimeout;
		if (retries >= _packetTimeouts.Count)
		{
			int num = 10;
			if (_packetTimeouts.Count > 0)
			{
				num = _packetTimeouts[_packetTimeouts.Count - 1];
			}
			return TimeSpan.FromSeconds(num);
		}
		return TimeSpan.FromSeconds(_packetTimeouts[retries]);
	}

	private void SendHeartbeat()
	{
		ServerCall call = new ServerCall(ServiceName.HeartBeat, ServiceOperation.Read, null, null);
		AddToQueue(call);
	}

	internal void AddToQueue(ServerCall call)
	{
		lock (_serviceCallsWaiting)
		{
			_serviceCallsWaiting.Add(call);
		}
	}

	public void EnableComms(bool value)
	{
		_enabled = value;
	}

	private JsonResponseBundleV2 DeserializeJson(string jsonData)
	{
		if (string.IsNullOrWhiteSpace(jsonData))
		{
			return null;
		}
		if (string.IsNullOrEmpty(jsonData))
		{
			if (_clientRef.LoggingEnabled)
			{
				_clientRef.Log("ERROR - Incoming packet data was null or empty! This is probably a network issue.");
			}
			return null;
		}
		jsonData = jsonData.Trim();
		if ((jsonData.StartsWith("{") && jsonData.EndsWith("}")) || (jsonData.StartsWith("[") && jsonData.EndsWith("]")))
		{
			try
			{
				return JsonReader.Deserialize<JsonResponseBundleV2>(jsonData);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return null;
			}
		}
		return null;
	}

	internal void ResetCommunication()
	{
		lock (_serviceCallsWaiting)
		{
			_isAuthenticated = false;
			_blockingQueue = false;
			_serviceCallsWaiting.Clear();
			_serviceCallsInProgress.Clear();
			_serviceCallsInTimeoutQueue.Clear();
			DisposeUploadHandler();
			_activeRequest = null;
			_clientRef.AuthenticationService.ProfileId = "";
			SessionID = "";
			_packetId = 0L;
		}
	}

	private string CalculateMD5Hash(string input)
	{
		MD5 mD = MD5.Create();
		byte[] bytes = Encoding.UTF8.GetBytes(input);
		byte[] array = mD.ComputeHash(bytes);
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i].ToString("X2"));
		}
		return stringBuilder.ToString();
	}

	private void ProcessAuthenticate(Dictionary<string, object> jsonData)
	{
		if (jsonData.ContainsKey("compressIfLarger"))
		{
			ClientSideCompressionThreshold = (int)jsonData["compressIfLarger"];
		}
		long num = (long)((double)GetJsonLong(jsonData, OperationParam.AuthenticateServicePlayerSessionExpiry.Value, 300L) * 0.85);
		_idleTimeout = TimeSpan.FromSeconds(num);
		object value = null;
		jsonData.TryGetValue("maxBundleMsgs", out value);
		if (value != null)
		{
			_maxBundleMessages = (int)value;
		}
		object value2 = null;
		jsonData.TryGetValue("maxKillCount", out value2);
		if (value2 != null)
		{
			_killSwitchThreshold = (int)value2;
		}
		ResetErrorCache();
		_isAuthenticated = true;
	}

	private void ProcessSwitchResponse(Dictionary<string, object> jsonData)
	{
		if (jsonData.ContainsKey("switchToAppId"))
		{
			string appId = (string)jsonData["switchToAppId"];
			AppId = appId;
		}
	}

	private static string GetJsonString(Dictionary<string, object> jsonData, string key, string defaultReturn)
	{
		object value = null;
		jsonData.TryGetValue(key, out value);
		if (value == null)
		{
			return defaultReturn;
		}
		return value as string;
	}

	private static long GetJsonLong(Dictionary<string, object> jsonData, string key, long defaultReturn)
	{
		object value = null;
		if (jsonData.TryGetValue(key, out value))
		{
			if (value is long)
			{
				return (long)value;
			}
			if (value is int)
			{
				return (int)value;
			}
		}
		return defaultReturn;
	}

	private void RetryRequest(RequestState.eWebRequestStatus status, bool bypassTimeout)
	{
		if (_activeRequest != null)
		{
			if (!bypassTimeout && !(DateTime.Now.Subtract(_activeRequest.TimeSent) >= GetPacketTimeout(_activeRequest)))
			{
				return;
			}
			if (_clientRef.LoggingEnabled)
			{
				string text = "";
				if (status == RequestState.eWebRequestStatus.STATUS_ERROR)
				{
					text = GetWebRequestResponse(_activeRequest);
					if (!string.IsNullOrEmpty(text))
					{
						_clientRef.Log("Timeout with network error: " + text);
					}
					else
					{
						_clientRef.Log("Timeout with network error: Please check the URL and/or certificates for server");
					}
				}
				else
				{
					_clientRef.Log("Timeout no reply from server");
				}
			}
			if (ResendMessage(_activeRequest))
			{
				return;
			}
			DisposeUploadHandler();
			_activeRequest = null;
			if (_cacheMessagesOnNetworkError && _networkErrorCallback != null)
			{
				if (_clientRef.LoggingEnabled)
				{
					_clientRef.Log("Caching messages");
				}
				_blockingQueue = true;
				lock (_serviceCallsInTimeoutQueue)
				{
					_serviceCallsInTimeoutQueue.InsertRange(0, _serviceCallsInProgress);
					_serviceCallsInProgress.Clear();
				}
				_networkErrorCallback();
			}
			else
			{
				TriggerCommsError(900, 90001, "Timeout trying to reach brainCloud server");
			}
		}
		else
		{
			_activeRequest = CreateAndSendNextRequestBundle();
		}
	}

	private void ResetErrorCache()
	{
		_cachedStatusCode = 403;
		_cachedReasonCode = 40304;
		_cachedStatusMessage = "No session";
	}

	private void DisposeUploadHandler()
	{
		if (_activeRequest != null && _activeRequest.WebRequest != null && _activeRequest.WebRequest.uploadHandler != null)
		{
			_activeRequest.WebRequest.Dispose();
		}
	}

	public void AddCallbackToAuthenticateRequest(ServerCallback in_callback)
	{
		bool flag = false;
		for (int i = 0; i < _serviceCallsInProgress.Count; i++)
		{
			if (flag)
			{
				break;
			}
			if (_serviceCallsInProgress[i].Operation == ServiceOperation.Authenticate.Value)
			{
				flag = true;
				_serviceCallsInProgress[i].GetCallback().AddAuthCallbacks(in_callback);
			}
		}
	}

	public bool IsAuthenticateRequestInProgress()
	{
		bool flag = false;
		for (int i = 0; i < _serviceCallsInProgress.Count; i++)
		{
			if (flag)
			{
				break;
			}
			if (_serviceCallsInProgress[i].Operation == ServiceOperation.Authenticate.Value)
			{
				flag = true;
			}
		}
		return flag;
	}
}
