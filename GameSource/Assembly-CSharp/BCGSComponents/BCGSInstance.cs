using System;
using System.Collections.Generic;
using BCGSComponents.DataModels;
using BrainCloud;
using UnityEngine;

namespace BCGSComponents;

public class BCGSInstance : MonoBehaviour
{
	private BrainCloudWrapper brainCloudClient;

	public BCGSController bCGSController;

	private BCGSRequestData authScriptData;

	private static BCGSInstance _instance;

	private bool usingBrainCloud;

	private BrainCloudWrapper _bc;

	private bool _paused;

	private string _sessionId;

	private bool _ready;

	private int _retryBase;

	private int _retryMax;

	private int _requestTimeout;

	private int _durableConcurrentRequests;

	private int _durableDrainInterval;

	private int _handshakeOffset;

	public bool IsWorking { get; set; }

	public Action<bool> BrainCloudAvailable { get; set; }

	public Action<string> BrainCloudAuthenticated { get; set; }

	public bool Available
	{
		get
		{
			if (_ready && brainCloudClient != null)
			{
				return brainCloudClient.Client.Initialized;
			}
			return false;
		}
	}

	public bool Authenticated
	{
		get
		{
			if (_ready && brainCloudClient != null)
			{
				return brainCloudClient.Client.Authenticated;
			}
			return false;
		}
	}

	private SuccessCallback BCSuccessCallback { get; set; }

	private FailureCallback BCFailureCallback { get; set; }

	private SuccessCallback BCAuthSuccessCallback { get; set; }

	public int HandshakeOffset
	{
		get
		{
			return _handshakeOffset;
		}
		set
		{
			if (value <= 0)
			{
				_handshakeOffset = BCGSDefaults.HandshakeOffset;
			}
			else
			{
				_handshakeOffset = value;
			}
		}
	}

	public int DurableDrainInterval
	{
		get
		{
			return _durableDrainInterval;
		}
		set
		{
			if (value <= 0)
			{
				_durableDrainInterval = BCGSDefaults.DurableDrainInterval;
			}
			else
			{
				_durableDrainInterval = value;
			}
		}
	}

	public int DurableConcurrentRequests
	{
		get
		{
			return _durableConcurrentRequests;
		}
		set
		{
			if (value <= 0)
			{
				_durableConcurrentRequests = BCGSDefaults.DurableConcurrentRequests;
			}
			else
			{
				_durableConcurrentRequests = value;
			}
		}
	}

	public int RequestTimeout
	{
		get
		{
			return _requestTimeout;
		}
		set
		{
			if (value <= 0)
			{
				_requestTimeout = BCGSDefaults.RequestTimeout;
			}
			else
			{
				_requestTimeout = value;
			}
		}
	}

	public int RetryMax
	{
		get
		{
			return _retryMax;
		}
		set
		{
			if (value < BCGSDefaults.RetryBase)
			{
				_retryMax = BCGSDefaults.RetryMax;
			}
			else
			{
				_retryMax = value;
			}
		}
	}

	public int RetryBase
	{
		get
		{
			return _retryBase;
		}
		set
		{
			if (value <= 0)
			{
				_retryBase = BCGSDefaults.RetryBase;
			}
			else
			{
				_retryBase = value;
			}
		}
	}

	public static BCGSInstance Instance()
	{
		if (_instance == null)
		{
			Debug.Log("BCGS: Initializing BCGS Instance...");
			GameObject gameObject = UnityEngine.Object.FindObjectOfType(typeof(BCGSInstance)) as GameObject;
			if (gameObject == null)
			{
				Debug.Log("BCGS: Creating BCGS Object...");
				gameObject = new GameObject("BCGSInstance");
				_instance = gameObject.AddComponent<BCGSInstance>();
			}
			else
			{
				_instance = gameObject.GetComponent<BCGSInstance>();
			}
		}
		return _instance;
	}

	private void Start()
	{
		Debug.Log("BCGS: Setting Up BCGS...");
		IsWorking = true;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		GameObject gameObject = new GameObject("BCGSController");
		bCGSController = gameObject.AddComponent<BCGSController>();
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		brainCloudClient = gameObject.AddComponent<BrainCloudWrapper>();
		brainCloudClient.WrapperName = "BrainCloudWrapper";
		brainCloudClient.Init();
		Debug.Log("BCGS: BCGS Ready...");
	}

	private void SetAvailability(bool avail)
	{
		if (_ready != avail)
		{
			_ready = avail;
			if (BrainCloudAvailable != null)
			{
				IsWorking = false;
				BrainCloudAvailable(avail);
			}
		}
	}

	public void DeviceAuthenticationRequest(BCGSRequestData scriptData, Action<AuthenticationResponse> onAuth = null)
	{
		Debug.Log("BCGSInstance: Sending DeviceAuth Request... ");
		authScriptData = scriptData;
		brainCloudClient.Client.InitializeIdentity(null, SystemInfo.deviceUniqueIdentifier);
		brainCloudClient.AuthenticateAnonymous(delegate(string response, object cbObject)
		{
			AuthenticationSuccess(response, onAuth);
		}, delegate(int status, int code, string error, object cbObject)
		{
			Debug.Log($"Auth failed - status: {status.GetType()} - {status}, code: {code.GetType()} - {code}");
			if (status == 202 && code == 40206)
			{
				Debug.Log("player deleted, authing again..");
				brainCloudClient.ResetStoredAnonymousId();
				DeviceAuthenticationRequest(scriptData, onAuth);
			}
			else
			{
				AuthenticationFailed(status, code, error);
			}
		});
	}

	public void AuthenticationRequest(string userName, string password, Action<AuthenticationResponse> onAuth = null)
	{
		Debug.Log("BCGSInstance: Sending Auth Request: userName:" + userName + ", password:" + password);
		brainCloudClient.AuthenticateUniversal(userName, password, forceCreate: false, delegate(string response, object cbObject)
		{
			AuthenticationSuccess(response, onAuth);
		}, delegate(int status, int code, string error, object cbObject)
		{
			AuthenticationFailed(status, code, error);
		});
	}

	public void FacebookConnectRequest(string userId, string token, Action<AuthenticationResponse> onAuth = null)
	{
		Debug.Log("BCGSInstance: Sending Facebook Auth Request: Id: " + userId + ", Token: " + token + " ");
		brainCloudClient.AuthenticateFacebook(userId, token, forceCreate: false, delegate(string response, object cbObject)
		{
			AuthenticationSuccess(response, onAuth);
		}, delegate(int status, int code, string error, object cbObject)
		{
			AuthenticationFailed(status, code, error);
		});
	}

	private void AuthenticationSuccess(string response, Action<AuthenticationResponse> onAuth = null)
	{
		Debug.Log("BCGSInstance: Auth Response: " + response);
		BCGSObject bCGSObject = BCGSObject.FromJson(response);
		BCGSData bCGSData = bCGSObject.GetObject("data");
		foreach (KeyValuePair<string, object> baseDatum in bCGSData.BaseData)
		{
			bCGSData.BaseData[baseDatum.Key]?.GetType().ToString();
			switch (Type.GetTypeCode(bCGSData.BaseData[baseDatum.Key]?.GetType()))
			{
			case TypeCode.String:
				bCGSObject.AddString(baseDatum.Key, baseDatum.Value as string);
				break;
			case TypeCode.Boolean:
				bCGSObject.AddBoolean(baseDatum.Key, (bool)baseDatum.Value);
				break;
			case TypeCode.Int32:
				bCGSObject.AddNumber(baseDatum.Key, (int)baseDatum.Value);
				break;
			}
		}
		if (bCGSData.ContainsKey("scriptData"))
		{
			bCGSObject.AddObject("scriptData", bCGSData.GetBCGSData("scriptData"));
		}
		Debug.Log($"responseObj: {bCGSObject}");
		LogEventRequest logEventRequest = new LogEventRequest();
		logEventRequest.SetEventKey("ootb/post_auth");
		logEventRequest.SetEventAttribute("deviceId", SystemInfo.deviceUniqueIdentifier);
		logEventRequest.SetEventAttribute("deviceOS", SystemInfo.operatingSystemFamily.ToString().ToUpper());
		logEventRequest.SetEventAttribute("deviceType", SystemInfo.deviceType.ToString());
		logEventRequest.SetEventAttribute("operatingSystem", SystemInfo.operatingSystemFamily.ToString());
		logEventRequest.SetEventAttribute("languageCode", (string)bCGSObject.BaseData["languageCode"]);
		logEventRequest.SetEventAttribute("countryCode", bCGSObject.ContainsKey("countryCode") ? ((string)bCGSObject.BaseData["countryCode"]) : "");
		logEventRequest.SetEventAttribute("newPlayer", Convert.ToBoolean((string)bCGSObject.BaseData["newUser"]));
		logEventRequest.SetScriptData(authScriptData);
		Instance().bCGSController.SendRequestToQueue(logEventRequest, delegate(LogEventResponse resp)
		{
			Debug.Log("Post_Auth: ResponseJSON: " + resp.JSONString);
			AuthenticationResponse authenticationResponse = new AuthenticationResponse(resp.BaseData.GetBCGSData("data").GetBCGSData("response"));
			BrainCloudAuthenticated(authenticationResponse.UserId);
			onAuth(authenticationResponse);
		});
	}

	private void AuthenticationFailed(int status, int code, string error)
	{
		Debug.LogWarning($"BCGSInstance: Auth Response Failed: code:{code}, status:{status}, error:{error}");
	}

	public void SendScriptRequest(BCGSRequest requestData, Action<BCGSObject> onResponse)
	{
		Debug.Log(string.Format("BCGSRequest basedata: {0}", requestData.BaseData["scriptData"]));
		BCGSData bCGSData = requestData.GetObject("scriptData");
		requestData.AddJSONStringAsObject("scriptData", bCGSData.JSON);
		Debug.Log("BCGSRequest basedata: " + requestData.JSON);
		Debug.Log("BCGSInstance: SendScriptRequest: OUT >>>>>> " + requestData.JSON);
		if (requestData.GetString("scriptName") == "LogEventRequest")
		{
			Debug.Log("BCGSInstance: Processing LogEvent Request...");
			brainCloudClient.ScriptService.RunScript(requestData.GetString("eventKey"), requestData.JSON, delegate(string response, object cbObject)
			{
				BCGSObject obj = BCGSObject.FromJson(response);
				onResponse(obj);
			}, delegate(int status, int code, string error, object cbObject)
			{
				Debug.LogWarning($"BCGSInstance: SendScriptRequest Failed: code:{code}, status:{status}, error:{error}");
			});
		}
		else
		{
			Debug.Log("BCGSInstance: Processing OOTB Request...");
			brainCloudClient.ScriptService.RunScript(requestData.Type, requestData.JSON, delegate(string response, object cbObject)
			{
				BCGSObject obj = BCGSObject.FromJson(response);
				onResponse(obj);
			}, delegate(int status, int code, string error, object cbObject)
			{
				Debug.LogWarning($"BCGSInstance: SendScriptRequest Failed: code:{code}, status:{status}, error:{error}");
			});
		}
	}

	private void Update()
	{
		if (brainCloudClient.Client.Initialized && !Available)
		{
			SetAvailability(avail: true);
		}
	}

	private void Stop(bool terminate)
	{
		_bc.Client.ShutDown();
		SetAvailability(avail: false);
	}

	private void CancelRequest(BCGSRequest request)
	{
		BCGSObject bCGSObject = new BCGSObject("ClientError");
		bCGSObject.AddObject("error", new BCGSRequestData().AddString("error", "timeout"));
		bCGSObject.AddString("requestId", request.requestId);
		if (usingBrainCloud)
		{
			try
			{
				request.Complete(this, bCGSObject);
			}
			catch (Exception ex)
			{
				Debug.Log(ex.ToString());
			}
		}
	}

	internal void SuccessCallback(BCGSRequest request, string returned, object cbObject)
	{
		new BCGSObject((Dictionary<string, object>)BCGSJson.From(returned));
		_ = request.requestId;
		throw new NotImplementedException("BCGSInstance: SuccessCallback: NOT IMPLEMENTED");
	}

	internal void ErrorCallback(BCGSRequest request, int status, int code, string returned, object cbObject)
	{
		new BCGSObject((Dictionary<string, object>)BCGSJson.From(returned));
		_ = request.requestId;
		throw new NotImplementedException("BCGSInstance: ErrorCallback: NOT IMPLEMENTED");
	}
}
