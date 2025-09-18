using BrainCloud.Internal;

namespace BrainCloud;

public class BrainCloudRelay
{
	public const ulong TO_ALL_PLAYERS = 1099511627775uL;

	public const int MAX_PLAYERS = 40;

	public const int CHANNEL_HIGH_PRIORITY_1 = 0;

	public const int CHANNEL_HIGH_PRIORITY_2 = 1;

	public const int CHANNEL_NORMAL_PRIORITY = 2;

	public const int CHANNEL_LOW_PRIORITY = 3;

	private RelayComms m_commsLayer;

	private BrainCloudClient m_clientRef;

	public long LastPing => m_commsLayer.Ping;

	public string OwnerProfileId => m_commsLayer.GetOwnerProfileId();

	public string OwnerCxId => m_commsLayer.GetOwnerCxId();

	internal BrainCloudRelay(RelayComms in_comms, BrainCloudClient in_client)
	{
		m_commsLayer = in_comms;
		m_clientRef = in_client;
	}

	public string GetOwnerProfileId()
	{
		return m_commsLayer.GetOwnerProfileId();
	}

	public string GetProfileIdForNetId(short netId)
	{
		return m_commsLayer.GetProfileIdForNetId(netId);
	}

	public short GetNetIdForProfileId(string profileId)
	{
		return m_commsLayer.GetNetIdForProfileId(profileId);
	}

	public string GetOwnerCxId()
	{
		return m_commsLayer.GetOwnerCxId();
	}

	public string GetCxIdForNetId(short netId)
	{
		return m_commsLayer.GetCxIdForNetId(netId);
	}

	public short GetNetIdForCxId(string cxId)
	{
		return m_commsLayer.GetNetIdForCxId(cxId);
	}

	public void Connect(RelayConnectionType in_connectionType, RelayConnectOptions in_options, SuccessCallback in_success = null, FailureCallback in_failure = null, object cb_object = null)
	{
		m_commsLayer.Connect(in_connectionType, in_options, in_success, in_failure, cb_object);
	}

	public void Disconnect()
	{
		m_commsLayer.Disconnect();
	}

	public bool IsConnected()
	{
		return m_commsLayer.IsConnected();
	}

	public void RegisterRelayCallback(RelayCallback in_callback)
	{
		m_commsLayer.RegisterRelayCallback(in_callback);
	}

	public void DeregisterRelayCallback()
	{
		m_commsLayer.DeregisterRelayCallback();
	}

	public void RegisterSystemCallback(RelaySystemCallback in_callback)
	{
		m_commsLayer.RegisterSystemCallback(in_callback);
	}

	public void DeregisterSystemCallback()
	{
		m_commsLayer.DeregisterSystemCallback();
	}

	public void Send(byte[] in_data, ulong to_netId, bool in_reliable = true, bool in_ordered = true, int in_channel = 0)
	{
		switch (to_netId)
		{
		case 1099511627775uL:
			SendToAll(in_data, in_reliable, in_ordered, in_channel);
			break;
		default:
		{
			string message = "Invalid NetId: " + to_netId;
			m_commsLayer.QueueError(message);
			break;
		}
		case 0uL:
		case 1uL:
		case 2uL:
		case 3uL:
		case 4uL:
		case 5uL:
		case 6uL:
		case 7uL:
		case 8uL:
		case 9uL:
		case 10uL:
		case 11uL:
		case 12uL:
		case 13uL:
		case 14uL:
		case 15uL:
		case 16uL:
		case 17uL:
		case 18uL:
		case 19uL:
		case 20uL:
		case 21uL:
		case 22uL:
		case 23uL:
		case 24uL:
		case 25uL:
		case 26uL:
		case 27uL:
		case 28uL:
		case 29uL:
		case 30uL:
		case 31uL:
		case 32uL:
		case 33uL:
		case 34uL:
		case 35uL:
		case 36uL:
		case 37uL:
		case 38uL:
		case 39uL:
		{
			ulong in_playerMask = (ulong)(1L << (int)to_netId);
			m_commsLayer.Send(in_data, in_playerMask, in_reliable, in_ordered, in_channel);
			break;
		}
		}
	}

	public void SendToPlayers(byte[] in_data, ulong in_playerMask, bool in_reliable = true, bool in_ordered = true, int in_channel = 0)
	{
		m_commsLayer.Send(in_data, in_playerMask, in_reliable, in_ordered, in_channel);
	}

	public void SendToAll(byte[] in_data, bool in_reliable = true, bool in_ordered = true, int in_channel = 0)
	{
		string profileId = m_clientRef.AuthenticationService.ProfileId;
		short netIdForProfileId = GetNetIdForProfileId(profileId);
		ulong num = (ulong)(~(1L << (int)netIdForProfileId));
		ulong in_playerMask = 0xFFFFFFFFFFL & num;
		m_commsLayer.Send(in_data, in_playerMask, in_reliable, in_ordered, in_channel);
	}

	public void SetPingInterval(float in_interval)
	{
		m_commsLayer.SetPingInterval(in_interval);
	}
}
