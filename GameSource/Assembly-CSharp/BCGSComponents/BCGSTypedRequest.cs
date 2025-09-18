using System;
using System.Collections.Generic;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public abstract class BCGSTypedRequest<IN, OUT> where IN : BCGSTypedRequest<IN, OUT> where OUT : BCGSTypedResponse
{
	protected BCGSRequest request;

	public string JSONString => request.ToString();

	public IDictionary<string, object> JSONData => request.BaseData;

	public BCGSTypedRequest(BCGSInstance instance, string type)
	{
		request = new BCGSRequest(instance, type);
	}

	protected BCGSTypedRequest(string type)
	{
		request = new BCGSRequest(type);
	}

	public void SendVia(Action<BCGSRequest, Action<BCGSObject>> sender, Action<OUT> callback)
	{
		request.SendVia(sender, delegate(BCGSObject response)
		{
			if (callback != null)
			{
				callback((OUT)BuildResponse(response));
			}
		});
	}

	public void Send(Action<OUT> callback)
	{
		request.Send(delegate(BCGSObject response)
		{
			if (callback != null)
			{
				callback((OUT)BuildResponse(response));
			}
		});
	}

	public void Send(Action<OUT> callback, int timeoutMillis)
	{
		if (request.MaxResponseTimeInMillis == 0)
		{
			request.MaxResponseTimeInMillis = timeoutMillis;
		}
		request.Send(delegate(BCGSObject response)
		{
			if (callback != null)
			{
				callback((OUT)BuildResponse(response));
			}
		});
	}

	public void Send(Action<OUT> successCallback, Action<OUT> errorCallback)
	{
		request.Send(delegate(BCGSObject response)
		{
			if (successCallback != null)
			{
				successCallback((OUT)BuildResponse(response));
			}
		}, delegate(BCGSObject response)
		{
			if (errorCallback != null)
			{
				errorCallback((OUT)BuildResponse(response));
			}
		});
	}

	public void Send(Action<OUT> successCallback, Action<OUT> errorCallback, int timeoutMillis)
	{
		if (request.MaxResponseTimeInMillis == 0)
		{
			request.MaxResponseTimeInMillis = timeoutMillis;
		}
		request.Send(delegate(BCGSObject response)
		{
			if (successCallback != null)
			{
				successCallback((OUT)BuildResponse(response));
			}
		}, delegate(BCGSObject response)
		{
			if (errorCallback != null)
			{
				errorCallback((OUT)BuildResponse(response));
			}
		});
	}

	public IN SetDurable(bool durable)
	{
		request.Durable = durable;
		return (IN)this;
	}

	public IN SetMaxResponseTimeInMillis(int maxResponseTime)
	{
		request.MaxResponseTimeInMillis = maxResponseTime;
		return (IN)this;
	}

	public IN SetScriptData(BCGSRequestData data)
	{
		request.AddObject("scriptData", data);
		return (IN)this;
	}

	protected abstract BCGSTypedResponse BuildResponse(BCGSObject response);

	public override string ToString()
	{
		return BCGSJson.To(request.BaseData);
	}
}
