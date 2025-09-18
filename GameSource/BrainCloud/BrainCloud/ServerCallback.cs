using BrainCloud.Internal;

namespace BrainCloud;

public class ServerCallback
{
	public object m_cbObject;

	public event SuccessCallback m_fnSuccessCallback;

	public event FailureCallback m_fnFailureCallback;

	public ServerCallback(SuccessCallback fnSuccessCallback, FailureCallback fnFailureCallback, object cbObject)
	{
		this.m_fnFailureCallback = fnFailureCallback;
		this.m_fnSuccessCallback = fnSuccessCallback;
		m_cbObject = cbObject;
	}

	public void OnSuccessCallback(string jsonResponse)
	{
		if (this.m_fnSuccessCallback != null)
		{
			this.m_fnSuccessCallback(jsonResponse, m_cbObject);
		}
	}

	public void OnErrorCallback(int statusCode, int reasonCode, string statusMessage)
	{
		if (this.m_fnFailureCallback != null)
		{
			this.m_fnFailureCallback(statusCode, reasonCode, statusMessage, m_cbObject);
		}
	}

	public void AddAuthCallbacks(ServerCallback in_callback)
	{
		if (in_callback.m_cbObject is WrapperAuthCallbackObject wrapperAuthCallbackObject)
		{
			m_fnSuccessCallback += wrapperAuthCallbackObject._successCallback;
			m_fnFailureCallback += wrapperAuthCallbackObject._failureCallback;
		}
	}

	public bool AreCallbacksNull()
	{
		if (this.m_fnSuccessCallback == null)
		{
			return this.m_fnFailureCallback == null;
		}
		return false;
	}
}
