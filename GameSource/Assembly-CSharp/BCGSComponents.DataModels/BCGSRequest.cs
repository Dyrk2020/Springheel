using System;
using System.Collections.Generic;
using BrainCloud;
using UnityEngine;

namespace BCGSComponents.DataModels;

public class BCGSRequest : BCGSObject
{
	private BCGSObject _response;

	internal Action<BCGSObject> _callback;

	internal Action<BCGSObject> _errorCallback;

	internal Action<BCGSObject> _completer;

	internal bool Durable;

	internal string requestId;

	private static int _requestCounter;

	private BCGSInstance snInstance;

	internal long RequestExpiresAt { get; set; }

	internal long WaitForResponseTicks { get; set; }

	internal int MaxResponseTimeInMillis { get; set; }

	internal int DurableAttempts { get; set; }

	internal SuccessCallback success { get; set; }

	internal FailureCallback failure { get; set; }

	public BCGSRequest(IDictionary<string, object> data)
		: base(data)
	{
		requestId = DateTime.Now.Ticks + "_" + _requestCounter++;
	}

	public BCGSRequest(string requestType)
	{
		AddString("scriptName", requestType);
		requestId = DateTime.Now.Ticks + "_" + _requestCounter++;
	}

	public BCGSRequest(BCGSInstance instance, string requestType)
	{
		snInstance = instance;
		AddString("scriptName", requestType);
		requestId = DateTime.Now.Ticks + "_" + _requestCounter++;
	}

	public new BCGSRequest AddBoolean(string paramName, bool value)
	{
		base.AddBoolean(paramName, value);
		return this;
	}

	public new BCGSRequest AddDate(string paramName, DateTime date)
	{
		base.AddDate(paramName, date);
		return this;
	}

	public new BCGSRequest AddNumber(string paramName, long value)
	{
		base.AddNumber(paramName, value);
		return this;
	}

	public new BCGSRequest AddObject(string paramName, BCGSData child)
	{
		base.AddObject(paramName, child);
		return this;
	}

	public new BCGSRequest AddObjectList(string paramName, List<BCGSData> child)
	{
		base.AddObjectList(paramName, child);
		return this;
	}

	public new BCGSRequest AddString(string paramName, string value)
	{
		base.AddString(paramName, value);
		return this;
	}

	public new BCGSRequest AddStringList(string paramName, List<string> child)
	{
		base.AddStringList(paramName, child);
		return this;
	}

	public bool[] HasBCCallbacks()
	{
		return new bool[2]
		{
			success != null,
			failure != null
		};
	}

	public void SetCallback(Action<BCGSObject> callback)
	{
		success = delegate(string response, object cbObject)
		{
			if (snInstance != null)
			{
				snInstance.SuccessCallback(this, response, cbObject);
			}
			else
			{
				BCGSDefaults.Instance.SuccessCallback(this, response, cbObject);
			}
			callback(new BCGSObject((Dictionary<string, object>)BCGSJson.From(response)));
		};
		failure = delegate(int status, int code, string error, object cbObject)
		{
			if (snInstance != null)
			{
				snInstance.ErrorCallback(this, status, code, error, cbObject);
			}
			else
			{
				BCGSDefaults.Instance.ErrorCallback(this, status, code, error, cbObject);
			}
		};
	}

	public void SetCallbacks(Action<BCGSObject> successCallback, Action<BCGSObject> errorCallback)
	{
		_callback = successCallback;
		_errorCallback = errorCallback;
		success = delegate(string response, object cbObject)
		{
			if (snInstance != null)
			{
				snInstance.SuccessCallback(this, response, cbObject);
			}
			else
			{
				BCGSDefaults.Instance.SuccessCallback(this, response, cbObject);
			}
			successCallback(new BCGSObject((Dictionary<string, object>)BCGSJson.From(response)));
		};
		failure = delegate(int status, int code, string error, object cbObject)
		{
			if (snInstance != null)
			{
				snInstance.ErrorCallback(this, status, code, error, cbObject);
			}
			else
			{
				BCGSDefaults.Instance.ErrorCallback(this, status, code, error, cbObject);
			}
			errorCallback(new BCGSObject((Dictionary<string, object>)BCGSJson.From(error)));
		};
	}

	internal void Complete(BCGSInstance snInstance, BCGSObject response)
	{
		_response = response;
		if (_completer != null)
		{
			_completer(response);
		}
		if ((_errorCallback == null || !response.ContainsKey("error")) && _callback != null)
		{
			_callback(response);
		}
	}

	internal int GetResponseTimeout()
	{
		if (MaxResponseTimeInMillis == 0)
		{
			MaxResponseTimeInMillis = ((snInstance != null) ? snInstance.RequestTimeout : BCGSDefaults.RequestTimeout);
		}
		return MaxResponseTimeInMillis;
	}

	internal void SendVia(Action<BCGSRequest, Action<BCGSObject>> sender, Action<BCGSObject> callback)
	{
		_callback = callback;
		BCGSRequest arg = DeepCopy();
		sender?.Invoke(arg, delegate(BCGSObject rpcResponse)
		{
			if (rpcResponse.ContainsKey("error"))
			{
				Debug.LogWarning("Nakama BACK - Error <<<<<<< response: " + rpcResponse.JSON);
			}
			else if (Application.isEditor)
			{
				Debug.Log("Nakama BACK <<<<<<< response: " + rpcResponse.JSON);
			}
			callback(rpcResponse);
		});
	}

	internal void Send(Action<BCGSObject> callback)
	{
		_callback = callback;
		BCGSRequest requestData = DeepCopy();
		BCGSInstance.Instance().SendScriptRequest(requestData, delegate(BCGSObject rpcResponse)
		{
			if (rpcResponse.ContainsKey("error"))
			{
				Debug.LogWarning("Nakama BACK - Error <<<<<<< response: " + rpcResponse.JSON);
			}
			else if (Application.isEditor)
			{
				Debug.Log("Nakama BACK <<<<<<< response: " + rpcResponse.JSON);
			}
			callback(rpcResponse);
		});
	}

	internal void Send(Action<BCGSObject> successCallback, Action<BCGSObject> errorCallback)
	{
		_callback = successCallback;
		_errorCallback = errorCallback;
		success = delegate(string response, object cbObject)
		{
			if (snInstance != null)
			{
				snInstance.SuccessCallback(DeepCopy(), response, cbObject);
			}
			else
			{
				BCGSDefaults.Instance.SuccessCallback(DeepCopy(), response, cbObject);
			}
			successCallback(new BCGSObject((Dictionary<string, object>)BCGSJson.From(response)));
		};
		failure = delegate(int status, int code, string error, object cbObject)
		{
			if (snInstance != null)
			{
				snInstance.ErrorCallback(this, status, code, error, cbObject);
			}
			else
			{
				BCGSDefaults.Instance.ErrorCallback(this, status, code, error, cbObject);
			}
			errorCallback(new BCGSObject((Dictionary<string, object>)BCGSJson.From(error)));
		};
		DeepCopy();
		throw new NotImplementedException("BCGSRequest: Send <successCallback, errorCallback>");
	}

	private BCGSRequest DeepCopy()
	{
		return new BCGSRequest(base.BaseData)
		{
			Durable = Durable,
			_callback = _callback,
			_completer = _completer,
			_errorCallback = _errorCallback,
			_response = _response,
			MaxResponseTimeInMillis = MaxResponseTimeInMillis,
			WaitForResponseTicks = WaitForResponseTicks,
			RequestExpiresAt = RequestExpiresAt,
			DurableAttempts = DurableAttempts,
			success = success,
			failure = failure
		};
	}
}
