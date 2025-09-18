using BrainCloud.Internal;

namespace BrainCloud;

public class BrainCloudRTT
{
	private RTTComms m_commsLayer;

	private BrainCloudClient m_clientRef;

	internal BrainCloudRTT(RTTComms in_comms, BrainCloudClient in_client)
	{
		m_commsLayer = in_comms;
		m_clientRef = in_client;
	}

	public void EnableRTT(RTTConnectionType in_connectionType = RTTConnectionType.WEBSOCKET, SuccessCallback in_success = null, FailureCallback in_failure = null, object cb_object = null)
	{
		m_commsLayer.EnableRTT(in_connectionType, in_success, in_failure, cb_object);
	}

	public void DisableRTT()
	{
		m_commsLayer.DisableRTT();
	}

	public bool IsRTTEnabled()
	{
		return m_commsLayer.IsRTTEnabled();
	}

	public RTTConnectionStatus GetConnectionStatus()
	{
		return m_commsLayer.GetConnectionStatus();
	}

	public void RegisterRTTEventCallback(RTTCallback in_callback)
	{
		m_commsLayer.RegisterRTTCallback(ServiceName.Event, in_callback);
	}

	public void DeregisterRTTEventCallback()
	{
		m_commsLayer.DeregisterRTTCallback(ServiceName.Event);
	}

	public void RegisterRTTChatCallback(RTTCallback in_callback)
	{
		m_commsLayer.RegisterRTTCallback(ServiceName.Chat, in_callback);
	}

	public void DeregisterRTTChatCallback()
	{
		m_commsLayer.DeregisterRTTCallback(ServiceName.Chat);
	}

	public void RegisterRTTPresenceCallback(RTTCallback in_callback)
	{
		m_commsLayer.RegisterRTTCallback(ServiceName.Presence, in_callback);
	}

	public void DeregisterRTTPresenceCallback()
	{
		m_commsLayer.DeregisterRTTCallback(ServiceName.Presence);
	}

	public void RegisterRTTMessagingCallback(RTTCallback in_callback)
	{
		m_commsLayer.RegisterRTTCallback(ServiceName.Messaging, in_callback);
	}

	public void DeregisterRTTMessagingCallback()
	{
		m_commsLayer.DeregisterRTTCallback(ServiceName.Messaging);
	}

	public void RegisterRTTLobbyCallback(RTTCallback in_callback)
	{
		m_commsLayer.RegisterRTTCallback(ServiceName.Lobby, in_callback);
	}

	public void DeregisterRTTLobbyCallback()
	{
		m_commsLayer.DeregisterRTTCallback(ServiceName.Lobby);
	}

	public void RegisterRTTAsyncMatchCallback(RTTCallback in_callback)
	{
		m_commsLayer.RegisterRTTCallback(ServiceName.AsyncMatch, in_callback);
	}

	public void RegisterRTTBlockchainRefresh(RTTCallback in_callback)
	{
		m_commsLayer.RegisterRTTCallback(ServiceName.UserItems, in_callback);
	}

	public void DeregisterRTTBlockchainRefresh()
	{
		m_commsLayer.DeregisterRTTCallback(ServiceName.UserItems);
	}

	public void DeregisterRTTAsyncMatchCallback()
	{
		m_commsLayer.DeregisterRTTCallback(ServiceName.AsyncMatch);
	}

	public void DeregisterAllRTTCallbacks()
	{
		m_commsLayer.DeregisterAllRTTCallbacks();
	}

	public void SetRTTHeartBeatSeconds(int in_value)
	{
		m_commsLayer.SetRTTHeartBeatSeconds(in_value);
	}

	public void RequestClientConnection(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.RTTRegistration, ServiceOperation.RequestClientConnection, null, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public string getRTTConnectionID()
	{
		return m_commsLayer.RTTConnectionID;
	}
}
