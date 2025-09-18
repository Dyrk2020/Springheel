using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using BrainCloud.JsonFx.Json;

namespace BrainCloud.Internal;

internal sealed class RelayComms
{
	private enum EventType
	{
		SocketError,
		SocketConnected,
		SocketData,
		ConnectSuccess,
		ConnectFailure,
		Relay,
		System
	}

	private class Event
	{
		public EventType type;

		public string message;

		public short netId;

		public byte[] data;
	}

	private class UDPPacket
	{
		public DateTime TimeSinceFirstSend { get; private set; }

		public DateTime LastTimeSent { get; private set; }

		public int TimeInterval { get; private set; }

		public byte[] RawData { get; private set; }

		public int Id { get; private set; }

		public byte NetId { get; private set; }

		public UDPPacket(byte[] in_data, int in_channel, int in_packetId, byte in_netId)
		{
			LastTimeSent = DateTime.Now;
			TimeSinceFirstSend = DateTime.Now;
			TimeInterval = ((in_channel <= 1) ? 50 : ((in_channel == 2) ? 150 : 250));
			RawData = in_data;
			Id = in_packetId;
			NetId = in_netId;
		}

		public void UpdateTimeIntervalSent()
		{
			LastTimeSent = DateTime.Now;
			TimeInterval = Math.Min((int)((float)TimeInterval * 1.25f), 500);
		}
	}

	public const int MAX_PACKETSIZE = 1024;

	public const byte MAX_PLAYERS = 40;

	public const byte INVALID_NET_ID = 40;

	public const byte CL2RS_CONNECT = 0;

	public const byte CL2RS_DISCONNECT = 1;

	public const byte CL2RS_RELAY = 2;

	public const byte CL2RS_ACK = 3;

	public const byte CL2RS_PING = 4;

	public const byte CL2RS_RSMG_ACK = 5;

	public const byte RS2CL_RSMG = 0;

	public const byte RS2CL_DISCONNECT = 1;

	public const byte RS2CL_RELAY = 2;

	public const byte RS2CL_ACK = 3;

	public const byte RS2CL_PONG = 4;

	private const int MAX_RSMG_HISTORY = 50;

	private RelayConnectOptions m_connectOptions;

	private RelayConnectionType m_connectionType;

	private bool m_bIsConnected;

	private DateTime m_lastNowMS;

	private int m_timeSinceLastPingRequest;

	private int m_pingInterval = 1000;

	private DateTime m_lastRecvTime;

	private const int MAX_PACKET_ID_HISTORY = 600;

	private const int MAX_RELIABLE_RESEND_INTERVAL = 500;

	private const int MAX_PACKET_ID = 4095;

	private const int MAX_CHANNELS = 4;

	private const int PACKET_LOWER_THRESHOLD = 1023;

	private const int PACKET_HIGHER_THRESHOLD = 3071;

	private string m_ownerCxId = "";

	private Dictionary<string, int> m_cxIdToNetId = new Dictionary<string, int>();

	private Dictionary<int, string> m_netIdToCxId = new Dictionary<int, string>();

	private int m_netId = 40;

	private BrainCloudWebSocket m_webSocket;

	private TcpClient m_tcpClient;

	private NetworkStream m_tcpStream;

	private byte[] m_tcpReadBuffer = new byte[1024];

	private const int SIZE_OF_LENGTH_PREFIX_BYTE_ARRAY = 2;

	private int m_tcpBytesRead;

	private int m_tcpBytesToRead;

	private byte[] m_tcpHeaderReadBuffer = new byte[2];

	private object fLock = new object();

	private Queue<byte[]> fToSend = new Queue<byte[]>();

	private UdpClient m_udpClient;

	private List<int> m_rsmgHistory = new List<int>();

	private Dictionary<ulong, int> m_sendPacketId = new Dictionary<ulong, int>();

	private Dictionary<ulong, int> m_recvPacketId = new Dictionary<ulong, int>();

	private Dictionary<ulong, UDPPacket> m_reliables = new Dictionary<ulong, UDPPacket>();

	private Dictionary<ulong, List<UDPPacket>> m_orderedReliablePackets = new Dictionary<ulong, List<UDPPacket>>();

	private bool m_resendConnectRequest;

	private DateTime m_lastConnectResendTime = DateTime.Now;

	private const int CONTROL_BYTE_HEADER_LENGTH = 1;

	private const int SIZE_OF_RELIABLE_FLAGS = 2;

	public const ushort RELIABLE_BIT = 32768;

	public const ushort ORDERED_BIT = 16384;

	private BrainCloudClient m_clientRef;

	private long m_sentPing = DateTime.Now.Ticks;

	private byte[] DISCONNECT_ARR = new byte[1] { 1 };

	private byte[] CONNECT_ARR = new byte[1];

	private SuccessCallback m_connectedSuccessCallback;

	private FailureCallback m_connectionFailureCallback;

	private object m_connectedObj;

	private RelayCallback m_registeredRelayCallback;

	private RelaySystemCallback m_registeredSystemCallback;

	private List<Event> m_events = new List<Event>();

	public long Ping { get; private set; }

	public RelayComms(BrainCloudClient in_client)
	{
		m_clientRef = in_client;
	}

	public void Connect(RelayConnectionType in_connectionType, RelayConnectOptions in_options, SuccessCallback in_success = null, FailureCallback in_failure = null, object cb_object = null)
	{
		Ping = 999L;
		if (!IsConnected())
		{
			m_connectOptions = in_options;
			m_connectedSuccessCallback = in_success;
			m_connectionFailureCallback = in_failure;
			m_connectedObj = cb_object;
			m_connectionType = in_connectionType;
			startReceivingRSConnectionAsync();
		}
	}

	public void Disconnect()
	{
		if (IsConnected())
		{
			send(buildDisconnectRequest());
		}
		disconnect();
	}

	public bool IsConnected()
	{
		return m_bIsConnected;
	}

	public void RegisterRelayCallback(RelayCallback in_callback)
	{
		m_registeredRelayCallback = in_callback;
	}

	public void DeregisterRelayCallback()
	{
		m_registeredRelayCallback = null;
	}

	public void RegisterSystemCallback(RelaySystemCallback in_callback)
	{
		m_registeredSystemCallback = in_callback;
	}

	public void DeregisterSystemCallback()
	{
		m_registeredSystemCallback = null;
	}

	public void QueueError(string message)
	{
		queueErrorEvent(message);
	}

	public void Send(byte[] in_data, ulong in_playerMask, bool in_reliable, bool in_ordered, int in_channel)
	{
		if (!IsConnected())
		{
			return;
		}
		if (in_data.Length > 1024)
		{
			disconnect();
			queueErrorEvent("Packet too big: " + in_data.Length + " > max " + 1024);
			return;
		}
		byte[] a = new byte[1] { 2 };
		ushort num = 0;
		if (in_reliable)
		{
			num |= 0x8000;
		}
		if (in_ordered)
		{
			num |= 0x4000;
		}
		num |= (ushort)((in_channel << 12) & 0x3000);
		ulong num2 = 0uL;
		int i = 0;
		for (int num3 = 40; i < num3; i++)
		{
			num2 |= ((in_playerMask >> 40 - i - 1) & 1) << i;
		}
		num2 = (num2 << 8) & 0xFFFFFFFFFF00L;
		ulong key = (((ulong)num << 48) & 0xFFFF000000000000uL) | num2;
		int num4 = 0;
		if (m_sendPacketId.ContainsKey(key))
		{
			num4 = m_sendPacketId[key];
		}
		m_sendPacketId[key] = (num4 + 1) & 0xFFF;
		num |= (ushort)num4;
		ushort number = (ushort)((num2 >> 32) & 0xFFFF);
		ushort number2 = (ushort)((num2 >> 16) & 0xFFFF);
		ushort number3 = (ushort)(num2 & 0xFFFF);
		fromShortBE(num, out var @byte, out var byte2);
		fromShortBE(number, out var byte3, out var byte4);
		fromShortBE(number2, out var byte5, out var byte6);
		fromShortBE(number3, out var byte7, out var byte8);
		byte[] array = new byte[8] { @byte, byte2, byte3, byte4, byte5, byte6, byte7, byte8 };
		byte[] a2 = concatenateByteArrays(a, array);
		byte[] in_data2 = concatenateByteArrays(a2, in_data);
		send(in_data2);
		if (in_reliable && m_connectionType == RelayConnectionType.UDP)
		{
			UDPPacket value = new UDPPacket(in_data2, in_channel, num4, 0);
			ulong key2 = BitConverter.ToUInt64(array, 0);
			m_reliables[key2] = value;
		}
	}

	public void SetPingInterval(float in_interval)
	{
		m_timeSinceLastPingRequest = 0;
		m_pingInterval = (int)(in_interval * 1000f);
	}

	public string GetOwnerProfileId()
	{
		string[] array = m_ownerCxId.Split(':');
		if (array.Length != 3)
		{
			return "";
		}
		return array[1];
	}

	public string GetProfileIdForNetId(short netId)
	{
		if (m_netIdToCxId.ContainsKey(netId))
		{
			string[] array = m_netIdToCxId[netId].Split(':');
			if (array.Length != 3)
			{
				return null;
			}
			return array[1];
		}
		return null;
	}

	public short GetNetIdForProfileId(string profileId)
	{
		foreach (KeyValuePair<string, int> item in m_cxIdToNetId)
		{
			string[] array = item.Key.Split(':');
			if (array.Length == 3 && profileId == array[1])
			{
				return (short)item.Value;
			}
		}
		return 40;
	}

	public string GetOwnerCxId()
	{
		return m_ownerCxId;
	}

	public string GetCxIdForNetId(short netId)
	{
		return m_netIdToCxId[netId];
	}

	public short GetNetIdForCxId(string cxId)
	{
		if (m_cxIdToNetId.ContainsKey(cxId))
		{
			return (short)m_cxIdToNetId[cxId];
		}
		return 40;
	}

	public void Update()
	{
		if (m_connectionType == RelayConnectionType.UDP && m_resendConnectRequest && (DateTime.Now - m_lastConnectResendTime).TotalSeconds > 0.5)
		{
			send(buildConnectionRequest());
			m_lastConnectResendTime = DateTime.Now;
		}
		DateTime now = DateTime.Now;
		if (IsConnected())
		{
			m_timeSinceLastPingRequest += (now - m_lastNowMS).Milliseconds;
			m_lastNowMS = now;
			if (m_timeSinceLastPingRequest >= m_pingInterval)
			{
				m_timeSinceLastPingRequest = 0;
				ping();
			}
			if (m_connectionType == RelayConnectionType.UDP)
			{
				foreach (KeyValuePair<ulong, UDPPacket> reliable in m_reliables)
				{
					UDPPacket value = reliable.Value;
					if ((value.TimeSinceFirstSend - now).Milliseconds > 10000)
					{
						disconnect();
						queueErrorEvent("Relay disconnected, too many packet lost");
						break;
					}
					if ((value.LastTimeSent - now).Milliseconds > value.TimeInterval)
					{
						value.UpdateTimeIntervalSent();
						send(value.RawData);
					}
				}
			}
		}
		if (m_connectionType == RelayConnectionType.UDP && (now - m_lastRecvTime).Milliseconds > 10000)
		{
			disconnect();
			queueErrorEvent("Relay Socket Timeout");
		}
		for (int i = 0; i < 10; i++)
		{
			if (m_events.Count <= 0)
			{
				break;
			}
			List<Event> events;
			lock (m_events)
			{
				events = m_events;
				m_events = new List<Event>();
			}
			for (int j = 0; j < events.Count; j++)
			{
				Event obj = events[j];
				switch (obj.type)
				{
				case EventType.SocketData:
					m_lastRecvTime = DateTime.Now;
					onRecv(obj.data);
					break;
				case EventType.SocketError:
					disconnect();
					queueErrorEvent(obj.message);
					break;
				case EventType.SocketConnected:
					m_lastNowMS = DateTime.Now;
					m_lastRecvTime = DateTime.Now;
					send(buildConnectionRequest());
					if (m_connectionType == RelayConnectionType.UDP)
					{
						m_resendConnectRequest = true;
						m_lastConnectResendTime = DateTime.Now;
					}
					break;
				case EventType.ConnectSuccess:
					if (m_connectedSuccessCallback != null)
					{
						m_connectedSuccessCallback(obj.message, m_connectedObj);
					}
					break;
				case EventType.ConnectFailure:
					if (m_connectionFailureCallback != null)
					{
						events.Clear();
						lock (m_events)
						{
							m_events.Clear();
						}
						FailureCallback connectionFailureCallback = m_connectionFailureCallback;
						object connectedObj = m_connectedObj;
						m_connectionFailureCallback = null;
						m_connectedObj = null;
						connectionFailureCallback(400, -1, buildRSRequestError(obj.message), connectedObj);
					}
					break;
				case EventType.System:
					if (m_registeredSystemCallback != null)
					{
						m_registeredSystemCallback(obj.message);
					}
					break;
				case EventType.Relay:
					if (m_registeredRelayCallback != null)
					{
						byte[] array = new byte[obj.data.Length - 11];
						Buffer.BlockCopy(obj.data, 11, array, 0, array.Length);
						m_registeredRelayCallback(obj.netId, array);
					}
					break;
				}
			}
		}
	}

	public void ping()
	{
		m_sentPing = DateTime.Now.Ticks;
		short number = Convert.ToInt16((double)Ping * 0.0001);
		fromShortBE(number, out var @byte, out var byte2);
		byte[] b = new byte[2] { @byte, byte2 };
		byte b2 = Convert.ToByte((byte)4);
		byte[] a = new byte[1] { b2 };
		byte[] in_data = concatenateByteArrays(a, b);
		send(in_data);
	}

	private byte[] buildConnectionRequest()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["cxId"] = m_clientRef.RTTConnectionID;
		dictionary["lobbyId"] = m_connectOptions.lobbyId;
		dictionary["passcode"] = m_connectOptions.passcode;
		dictionary["version"] = m_clientRef.BrainCloudClientVersion;
		return concatenateByteArrays(CONNECT_ARR, Encoding.ASCII.GetBytes(JsonWriter.Serialize(dictionary)));
	}

	private string buildRSRequestError(string in_statusMessage)
	{
		return JsonWriter.Serialize(new Dictionary<string, object>
		{
			["status"] = 403,
			["reason_code"] = 90300,
			["status_message"] = in_statusMessage,
			["severity"] = "ERROR"
		});
	}

	private byte[] buildDisconnectRequest()
	{
		return DISCONNECT_ARR;
	}

	private void disconnect()
	{
		m_bIsConnected = false;
		m_connectedSuccessCallback = null;
		m_connectedObj = null;
		m_resendConnectRequest = false;
		m_connectionType = RelayConnectionType.INVALID;
		m_cxIdToNetId.Clear();
		m_netIdToCxId.Clear();
		m_ownerCxId = "";
		m_netId = 40;
		if (m_webSocket != null)
		{
			m_webSocket.Close();
		}
		m_webSocket = null;
		if (m_tcpStream != null)
		{
			m_tcpStream.Dispose();
		}
		m_tcpStream = null;
		if (m_tcpClient != null)
		{
			m_tcpClient.Client.Close(0);
			m_tcpClient.Close();
			fToSend.Clear();
		}
		m_tcpClient = null;
		if (m_udpClient != null)
		{
			m_udpClient.Close();
		}
		m_udpClient = null;
		m_sendPacketId.Clear();
		m_recvPacketId.Clear();
		m_reliables.Clear();
		m_orderedReliablePackets.Clear();
	}

	private byte[] appendSizeBytes(byte[] in_data)
	{
		short number = Convert.ToInt16(in_data.Length + 2);
		fromShortBE(number, out var @byte, out var byte2);
		byte[] a = new byte[2] { @byte, byte2 };
		return concatenateByteArrays(a, in_data);
	}

	private void parseHeaderData(byte[] in_data, out bool out_reliable, out bool out_ordered, out int out_channel, out int out_packetId)
	{
		ushort num = toShortBE(in_data);
		out_reliable = (num & 0x8000) == 32768;
		out_ordered = (num & 0x4000) == 16384;
		out_channel = (num >> 12) & 3;
		out_packetId = num & 0xFFF;
	}

	private bool send(byte[] in_data)
	{
		bool result = false;
		switch (m_connectionType)
		{
		case RelayConnectionType.WEBSOCKET:
			if (m_webSocket == null)
			{
				return result;
			}
			break;
		case RelayConnectionType.TCP:
			if (m_tcpClient == null)
			{
				return result;
			}
			break;
		case RelayConnectionType.UDP:
			if (m_udpClient == null)
			{
				return result;
			}
			break;
		}
		try
		{
			byte[] array = appendSizeBytes(in_data);
			switch (m_connectionType)
			{
			case RelayConnectionType.WEBSOCKET:
				m_webSocket.SendAsync(array);
				result = true;
				break;
			case RelayConnectionType.TCP:
				tcpWrite(array);
				result = true;
				break;
			case RelayConnectionType.UDP:
				m_udpClient.SendAsync(array, array.Length);
				result = true;
				break;
			}
		}
		catch (Exception ex)
		{
			if (m_clientRef.LoggingEnabled)
			{
				m_clientRef.Log("send exception: " + ex);
			}
			queueSocketErrorEvent(ex.ToString());
		}
		return result;
	}

	private void startReceivingRSConnectionAsync()
	{
		bool ssl = m_connectOptions.ssl;
		string host = m_connectOptions.host;
		int port = m_connectOptions.port;
		switch (m_connectionType)
		{
		case RelayConnectionType.WEBSOCKET:
			connectWebSocket(host, port, ssl);
			break;
		case RelayConnectionType.TCP:
			connectTCPAsync(host, port);
			break;
		case RelayConnectionType.UDP:
			connectUDPAsync(host, port);
			break;
		}
	}

	private void WebSocket_OnClose(BrainCloudWebSocket sender, int code, string reason)
	{
		if (m_clientRef.LoggingEnabled)
		{
			m_clientRef.Log("Relay: Connection closed: " + reason);
		}
		queueErrorEvent(reason);
	}

	private void Websocket_OnOpen(BrainCloudWebSocket accepted)
	{
		if (m_clientRef.LoggingEnabled)
		{
			m_clientRef.Log("Relay: Connection established.");
		}
		queueSocketConnectedEvent();
	}

	private void WebSocket_OnMessage(BrainCloudWebSocket sender, byte[] data)
	{
		queueSocketDataEvent(data, data.Length);
	}

	private void WebSocket_OnError(BrainCloudWebSocket sender, string message)
	{
		if (m_clientRef.LoggingEnabled)
		{
			m_clientRef.Log("Relay Error: " + message);
		}
		queueErrorEvent(message);
	}

	private void sendRSMGAck(int rsmgPacketId)
	{
		byte[] array = new byte[3] { 5, 0, 0 };
		fromShortBE((short)rsmgPacketId, out var @byte, out var byte2);
		array[1] = @byte;
		array[2] = byte2;
		send(array);
	}

	private void sendAck(byte[] in_data)
	{
		byte[] array = new byte[9] { 3, 0, 0, 0, 0, 0, 0, 0, 0 };
		Buffer.BlockCopy(in_data, 3, array, 1, 8);
		send(array);
	}

	private void onRSMG(byte[] in_data, int in_lengthOfData)
	{
		int num = BitConverter.ToUInt16(new byte[2]
		{
			BitConverter.IsLittleEndian ? in_data[4] : in_data[3],
			BitConverter.IsLittleEndian ? in_data[3] : in_data[4]
		}, 0);
		if (m_connectionType == RelayConnectionType.UDP)
		{
			sendRSMGAck(num);
			for (int i = 0; i < m_rsmgHistory.Count; i++)
			{
				if (m_rsmgHistory[i] == num)
				{
					if (m_clientRef.LoggingEnabled)
					{
						m_clientRef.Log("Duplicated System Msg: " + num);
					}
					return;
				}
			}
			m_rsmgHistory.Add(num);
			while (m_rsmgHistory.Count > 50)
			{
				m_rsmgHistory.RemoveAt(0);
			}
		}
		int num2 = 5;
		int num3 = in_lengthOfData - num2;
		if (num3 == 0)
		{
			queueErrorEvent("RSMG cannot be empty");
			return;
		}
		string text = Encoding.ASCII.GetString(in_data, num2, num3);
		if (m_clientRef.LoggingEnabled)
		{
			m_clientRef.Log("Relay System Msg: " + text);
		}
		Dictionary<string, object> dictionary = (Dictionary<string, object>)JsonReader.Deserialize(text);
		switch (dictionary["op"] as string)
		{
		case "CONNECT":
		{
			int num4 = (int)dictionary["netId"];
			string text2 = dictionary["cxId"] as string;
			m_cxIdToNetId[text2] = num4;
			m_netIdToCxId[num4] = text2;
			if (text2 == m_clientRef.RTTConnectionID && !m_bIsConnected)
			{
				m_netId = num4;
				m_ownerCxId = dictionary["ownerCxId"] as string;
				m_bIsConnected = true;
				m_lastNowMS = DateTime.Now;
				m_resendConnectRequest = false;
				queueConnectSuccessEvent(text);
			}
			break;
		}
		case "NET_ID":
		{
			int num5 = (int)dictionary["netId"];
			string text3 = dictionary["cxId"] as string;
			m_cxIdToNetId[text3] = num5;
			m_netIdToCxId[num5] = text3;
			break;
		}
		case "MIGRATE_OWNER":
			m_ownerCxId = dictionary["cxId"] as string;
			break;
		case "DISCONNECT":
			if (dictionary["cxId"] as string == m_clientRef.RTTService.getRTTConnectionID())
			{
				disconnect();
				queueErrorEvent("Disconnected by server");
				return;
			}
			break;
		}
		queueSystemEvent(text);
	}

	private void onPong()
	{
		Ping = DateTime.Now.Ticks - m_sentPing;
		if (m_clientRef.LoggingEnabled)
		{
			m_clientRef.Log("Relay LastPing: " + (float)Ping * 0.0001f + "ms");
		}
	}

	private void onRecv(byte[] in_data)
	{
		if (in_data.Length < 3)
		{
			queueErrorEvent("packet cannot be smaller than 3 bytes");
			return;
		}
		byte b = in_data[2];
		switch (b)
		{
		case 0:
			if (in_data.Length < 5)
			{
				queueErrorEvent("packet cannot be smaller than 5 bytes");
			}
			else
			{
				onRSMG(in_data, in_data.Length);
			}
			break;
		case 1:
			disconnect();
			queueErrorEvent("Relay: Disconnected by server");
			break;
		case 4:
			onPong();
			break;
		case 3:
			if (in_data.Length < 11)
			{
				queueErrorEvent("ack packet cannot be smaller than 11 bytes");
			}
			else if (m_connectionType == RelayConnectionType.UDP)
			{
				onUDPAcknowledge(in_data);
			}
			break;
		case 2:
			if (in_data.Length < 11)
			{
				queueErrorEvent("Relay packets cannot be smaller than 11 bytes");
				break;
			}
			if (m_clientRef.LoggingEnabled)
			{
				m_clientRef.Log("RELAY RECV: " + in_data.Length + " bytes, msg: " + Encoding.ASCII.GetString(in_data, 11, in_data.Length - 11));
			}
			onRelay(in_data);
			break;
		default:
			disconnect();
			queueErrorEvent("Relay Recv Error: Unknown control byte: " + b);
			break;
		}
	}

	private bool packetLE(int a, int b)
	{
		if (a > 3071 && b <= 1023)
		{
			return true;
		}
		if (b > 3071 && a <= 1023)
		{
			return false;
		}
		return a <= b;
	}

	private void onRelay(byte[] in_data)
	{
		ushort num = BitConverter.ToUInt16(new byte[2]
		{
			BitConverter.IsLittleEndian ? in_data[4] : in_data[3],
			BitConverter.IsLittleEndian ? in_data[3] : in_data[4]
		}, 0);
		ushort num2 = BitConverter.ToUInt16(new byte[2]
		{
			BitConverter.IsLittleEndian ? in_data[6] : in_data[5],
			BitConverter.IsLittleEndian ? in_data[5] : in_data[6]
		}, 0);
		ushort num3 = BitConverter.ToUInt16(new byte[2]
		{
			BitConverter.IsLittleEndian ? in_data[8] : in_data[7],
			BitConverter.IsLittleEndian ? in_data[7] : in_data[8]
		}, 0);
		ushort num4 = BitConverter.ToUInt16(new byte[2]
		{
			BitConverter.IsLittleEndian ? in_data[10] : in_data[9],
			BitConverter.IsLittleEndian ? in_data[9] : in_data[10]
		}, 0);
		ulong key = ((((ulong)num << 48) & 0xFFFF000000000000uL) | (((ulong)num2 << 32) & 0xFFFF00000000L) | (((ulong)num3 << 16) & 0xFFFF0000u) | ((ulong)num4 & 0xFFFFuL)) & 0xF000FFFFFFFFFFFFuL;
		bool flag = (((num & 0x8000) != 0) ? true : false);
		bool flag2 = (((num & 0x4000) != 0) ? true : false);
		int in_channel = (num >> 12) & 3;
		int num5 = num & 0xFFF;
		byte b = (byte)(num4 & 0xFF);
		if (m_connectionType == RelayConnectionType.UDP)
		{
			if (flag)
			{
				sendAck(in_data);
			}
			if (flag2)
			{
				int num6 = 4095;
				if (m_recvPacketId.ContainsKey(key))
				{
					num6 = m_recvPacketId[key];
				}
				if (flag)
				{
					if (packetLE(num5, num6))
					{
						if (m_clientRef.LoggingEnabled)
						{
							m_clientRef.Log("Duplicated packet from " + b + ". got " + num5);
						}
						return;
					}
					if (!m_orderedReliablePackets.ContainsKey(key))
					{
						m_orderedReliablePackets[key] = new List<UDPPacket>();
					}
					List<UDPPacket> list = m_orderedReliablePackets[key];
					if (num5 != ((num6 + 1) & 0xFFF))
					{
						if (list.Count > 600)
						{
							disconnect();
							queueErrorEvent("Relay disconnected, too many queued out of order packets.");
							return;
						}
						int i;
						for (i = 0; i < list.Count; i++)
						{
							UDPPacket uDPPacket = list[i];
							if (uDPPacket.Id == num5)
							{
								if (m_clientRef.LoggingEnabled)
								{
									m_clientRef.Log("Duplicated packet from " + b + ". got " + num5);
								}
								return;
							}
							if (packetLE(num5, uDPPacket.Id))
							{
								break;
							}
						}
						UDPPacket item = new UDPPacket(in_data, in_channel, num5, b);
						list.Insert(i, item);
						if (m_clientRef.LoggingEnabled)
						{
							m_clientRef.Log("Queuing out of order reliable from " + b + ". got " + num5);
						}
						return;
					}
					m_recvPacketId[key] = num5;
					queueRelayEvent(b, in_data);
					while (list.Count > 0)
					{
						UDPPacket uDPPacket2 = list[0];
						if (uDPPacket2.Id == ((num5 + 1) & 0xFFF))
						{
							queueRelayEvent(uDPPacket2.NetId, uDPPacket2.RawData);
							list.RemoveAt(0);
							num5 = uDPPacket2.Id;
							m_recvPacketId[key] = num5;
							continue;
						}
						break;
					}
					return;
				}
				if (packetLE(num5, num6))
				{
					if (m_clientRef.LoggingEnabled)
					{
						m_clientRef.Log("Out of order packet from " + b + ". Expecting " + ((num6 + 1) & 0xFFF) + ", got " + num5);
					}
					return;
				}
				m_recvPacketId[key] = num5;
			}
		}
		queueRelayEvent(b, in_data);
	}

	private void onUDPAcknowledge(byte[] in_data)
	{
		ulong key = BitConverter.ToUInt64(in_data, 3);
		m_reliables.Remove(key);
		if (m_clientRef.LoggingEnabled)
		{
			m_clientRef.Log("RELAY RECV ACK: " + key);
		}
	}

	private void onUDPRecv(IAsyncResult result)
	{
		try
		{
			UdpClient udpClient = result.AsyncState as UdpClient;
			string host = m_connectOptions.host;
			IPEndPoint remoteEP = new IPEndPoint(port: m_connectOptions.port, address: IPAddress.Parse(host));
			if (udpClient != null)
			{
				byte[] array = udpClient.EndReceive(result, ref remoteEP);
				queueSocketDataEvent(array, array.Length);
				udpClient.BeginReceive(onUDPRecv, udpClient);
			}
		}
		catch (Exception ex)
		{
			queueErrorEvent(ex.ToString());
		}
	}

	private void tcpWrite(byte[] message)
	{
		try
		{
			lock (fLock)
			{
				fToSend.Enqueue(message);
				if (1 == fToSend.Count)
				{
					m_tcpStream.BeginWrite(message, 0, message.Length, tcpFinishWrite, null);
				}
			}
		}
		catch (Exception ex)
		{
			queueErrorEvent(ex.ToString());
		}
	}

	private void tcpFinishWrite(IAsyncResult result)
	{
		try
		{
			m_tcpStream.EndWrite(result);
			lock (fLock)
			{
				fToSend.Dequeue();
				if (fToSend.Count > 0)
				{
					byte[] array = fToSend.Peek();
					m_tcpStream.BeginWrite(array, 0, array.Length, tcpFinishWrite, null);
				}
			}
		}
		catch (Exception ex)
		{
			queueErrorEvent(ex.ToString());
		}
	}

	private void onTCPReadHeader(IAsyncResult ar)
	{
		try
		{
			int num = m_tcpStream.EndRead(ar);
			if (num == 0)
			{
				queueErrorEvent("Server Closed Connection");
			}
			else if (m_tcpStream != null && num == 2)
			{
				m_tcpBytesRead = 0;
				if (BitConverter.IsLittleEndian)
				{
					Array.Reverse(m_tcpHeaderReadBuffer);
				}
				m_tcpBytesToRead = BitConverter.ToInt16(m_tcpHeaderReadBuffer, 0);
				m_tcpBytesToRead -= 2;
				m_tcpStream.BeginRead(m_tcpReadBuffer, 2, m_tcpBytesToRead, onTCPFinishRead, null);
			}
		}
		catch (Exception ex)
		{
			queueErrorEvent(ex.ToString());
		}
	}

	private void onTCPFinishRead(IAsyncResult result)
	{
		try
		{
			if (m_tcpStream == null)
			{
				return;
			}
			int num = m_tcpStream.EndRead(result);
			if (num == 0)
			{
				queueErrorEvent("Server Closed Connection");
				return;
			}
			m_tcpBytesRead += num;
			if (m_tcpBytesRead < m_tcpBytesToRead)
			{
				m_tcpStream.BeginRead(m_tcpReadBuffer, m_tcpBytesRead, m_tcpBytesToRead - m_tcpBytesRead, onTCPFinishRead, null);
				return;
			}
			if (m_tcpBytesRead != m_tcpBytesToRead)
			{
				queueErrorEvent("Incorrect Bytes Read " + m_tcpBytesRead + " " + m_tcpBytesToRead);
				return;
			}
			queueSocketDataEvent(m_tcpReadBuffer, m_tcpBytesToRead + 2);
			m_tcpBytesToRead = 0;
			m_tcpStream.BeginRead(m_tcpHeaderReadBuffer, 0, 2, onTCPReadHeader, null);
		}
		catch (Exception ex)
		{
			queueErrorEvent(ex.ToString());
		}
	}

	private void connectWebSocket(string in_host, int in_port, bool in_sslEnabled)
	{
		string url = (in_sslEnabled ? "wss://" : "ws://") + in_host + ":" + in_port;
		m_webSocket = new BrainCloudWebSocket(url);
		m_webSocket.OnClose += WebSocket_OnClose;
		m_webSocket.OnOpen += Websocket_OnOpen;
		m_webSocket.OnMessage += WebSocket_OnMessage;
		m_webSocket.OnError += WebSocket_OnError;
	}

	private async void connectTCPAsync(string host, int port)
	{
		if (await Task.Run(async delegate
		{
			try
			{
				m_tcpClient = new TcpClient();
				m_tcpClient.NoDelay = true;
				m_tcpClient.Client.NoDelay = true;
				if (m_clientRef.LoggingEnabled)
				{
					m_clientRef.Log("Starting TCP connect ASYNC " + m_tcpClient.Connected + " s:" + m_tcpClient.Client.Connected);
				}
				await m_tcpClient.ConnectAsync(host, port);
			}
			catch (Exception ex)
			{
				queueErrorEvent(ex.ToString());
				return false;
			}
			return true;
		}))
		{
			m_tcpStream = m_tcpClient.GetStream();
			queueSocketConnectedEvent();
			m_tcpBytesToRead = 0;
			m_tcpStream.BeginRead(m_tcpHeaderReadBuffer, 0, 2, onTCPReadHeader, null);
			if (m_clientRef.LoggingEnabled)
			{
				m_clientRef.Log("Connected! ASYNC " + m_tcpClient.Connected + " s:" + m_tcpClient.Client.Connected);
			}
		}
	}

	private void initUDPConnection()
	{
		m_udpClient = new UdpClient();
		m_sendPacketId.Clear();
		m_recvPacketId.Clear();
		m_reliables.Clear();
		m_rsmgHistory.Clear();
		m_orderedReliablePackets.Clear();
	}

	private void connectUDPAsync(string host, int port)
	{
		try
		{
			SocketAsyncEventArgs e = new SocketAsyncEventArgs();
			e.Completed += OnUDPConnected;
			e.RemoteEndPoint = new DnsEndPoint(host, port);
			initUDPConnection();
			m_udpClient.Client.ConnectAsync(e);
		}
		catch (Exception ex)
		{
			queueErrorEvent(ex.ToString());
		}
	}

	private void OnUDPConnected(object sender, SocketAsyncEventArgs args)
	{
		queueSocketConnectedEvent();
		m_udpClient.BeginReceive(onUDPRecv, m_udpClient);
	}

	private void queueConnectSuccessEvent(string jsonString)
	{
		Event obj = new Event();
		obj.type = EventType.ConnectSuccess;
		obj.message = jsonString;
		lock (m_events)
		{
			m_events.Add(obj);
		}
	}

	private void queueSocketErrorEvent(string message)
	{
		Event obj = new Event();
		obj.type = EventType.SocketError;
		obj.message = message;
		lock (m_events)
		{
			m_events.Add(obj);
		}
	}

	private void queueSocketConnectedEvent()
	{
		Event obj = new Event();
		obj.type = EventType.SocketConnected;
		lock (m_events)
		{
			m_events.Add(obj);
		}
	}

	private void queueSocketDataEvent(byte[] in_data, int length)
	{
		Event obj = new Event();
		obj.type = EventType.SocketData;
		obj.data = new byte[length];
		Buffer.BlockCopy(in_data, 0, obj.data, 0, length);
		lock (m_events)
		{
			m_events.Add(obj);
		}
	}

	private void queueErrorEvent(string message)
	{
		Event obj = new Event();
		obj.type = EventType.ConnectFailure;
		obj.message = message;
		lock (m_events)
		{
			m_events.Add(obj);
		}
	}

	private void queueSystemEvent(string jsonString)
	{
		Event obj = new Event();
		obj.type = EventType.System;
		obj.message = jsonString;
		lock (m_events)
		{
			m_events.Add(obj);
		}
	}

	private void queueRelayEvent(short netId, byte[] data)
	{
		Event obj = new Event();
		obj.type = EventType.Relay;
		obj.netId = netId;
		obj.data = data;
		lock (m_events)
		{
			m_events.Add(obj);
		}
	}

	private byte[] concatenateByteArrays(byte[] a, byte[] b)
	{
		byte[] array = new byte[a.Length + b.Length];
		Buffer.BlockCopy(a, 0, array, 0, a.Length);
		Buffer.BlockCopy(b, 0, array, a.Length, b.Length);
		return array;
	}

	private void fromShortBE(short number, out byte byte1, out byte byte2)
	{
		byte1 = (byte)(number >> 8);
		byte2 = (byte)number;
	}

	private void fromShortBE(ushort number, out byte byte1, out byte byte2)
	{
		byte1 = (byte)(number >> 8);
		byte2 = (byte)number;
	}

	private ushort toShortBE(byte[] byteArr)
	{
		int num = 3;
		bool isLittleEndian = BitConverter.IsLittleEndian;
		return BitConverter.ToUInt16(new byte[2]
		{
			isLittleEndian ? byteArr[num + 1] : byteArr[num],
			isLittleEndian ? byteArr[num] : byteArr[num + 1]
		}, 0);
	}
}
