using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;

namespace MLAPI.Relay.Transports;

public class UnetRelayTransport : INetworkTransport
{
	public class ForceDisconnectedData
	{
		public int hostId;

		public int connectionId;

		public byte channelId;

		public byte[] buffer;

		public int size;
	}

	private enum MessageType
	{
		StartServer,
		ConnectToServer,
		Data,
		ClientDisconnect,
		AddressReport,
		ServerDummyPing
	}

	private ulong totalSentBytes;

	private ulong totalReceivedBytes;

	private byte defaultChannelId;

	private int relayConnectionId;

	private bool isClient;

	private string address;

	private ushort port;

	private List<ChannelQOS> channels = new List<ChannelQOS>();

	private readonly byte[] disconnectBuffer = new byte[9] { 0, 0, 0, 0, 0, 0, 0, 0, 3 };

	private readonly byte[] serverDummyPingBuffer = new byte[1] { 5 };

	private Coroutine serverPingCoroutine;

	private List<ForceDisconnectedData> queuedForceDisconnects = new List<ForceDisconnectedData>();

	public ulong TotalSentBytes => totalSentBytes;

	public ulong TotalReceivedBytes => totalReceivedBytes;

	public bool Enabled => GameSettings.GetInstance().UseUnityRelay;

	public bool IsServer => GameSettings.GetInstance().StartAsHost;

	public bool IsStarted => NetworkTransport.IsStarted;

	public string RelayAddress => RelayConstants.SERVER_ADDRESS;

	public ushort RelayPort => (ushort)RelayConstants.SERVER_PORT;

	public bool RemoteEndpointReported { get; set; }

	public event Action<IPEndPoint> OnRemoteEndpointReported;

	public event Action OnRelayServerDisconnected;

	public UnetRelayTransport()
	{
		Logger.Log("Transport initialized", GetType().Name, ConsoleColor.Cyan);
	}

	public int Connect(int hostId, string serverAddress, int serverPort, int exceptionConnectionId, out byte error)
	{
		if (!Enabled)
		{
			return NetworkTransport.Connect(hostId, serverAddress, serverPort, exceptionConnectionId, out error);
		}
		isClient = true;
		address = serverAddress;
		port = (ushort)serverPort;
		relayConnectionId = NetworkTransport.Connect(hostId, RelayAddress, RelayPort, exceptionConnectionId, out error);
		return relayConnectionId;
	}

	public int ConnectWithSimulator(int hostId, string serverAddress, int serverPort, int exceptionConnectionId, out byte error, ConnectionSimulatorConfig conf)
	{
		if (!Enabled)
		{
			return NetworkTransport.ConnectWithSimulator(hostId, serverAddress, serverPort, exceptionConnectionId, out error, conf);
		}
		isClient = true;
		address = serverAddress;
		port = (ushort)serverPort;
		relayConnectionId = NetworkTransport.ConnectWithSimulator(hostId, RelayAddress, RelayPort, exceptionConnectionId, out error, conf);
		return relayConnectionId;
	}

	public int ConnectEndPoint(int hostId, EndPoint endPoint, int exceptionConnectionId, out byte error)
	{
		if (!Enabled)
		{
			return NetworkTransport.ConnectEndPoint(hostId, endPoint, exceptionConnectionId, out error);
		}
		isClient = true;
		address = ((IPEndPoint)endPoint).Address.ToString();
		port = (ushort)((IPEndPoint)endPoint).Port;
		relayConnectionId = NetworkTransport.Connect(hostId, RelayAddress, RelayPort, exceptionConnectionId, out error);
		return relayConnectionId;
	}

	private void SetChannelsFromTopology(HostTopology topology)
	{
		channels = topology.DefaultConfig.Channels;
	}

	public int AddHost(HostTopology topology)
	{
		if (!Enabled)
		{
			return NetworkTransport.AddHost(topology);
		}
		bool isServer = IsServer;
		isClient = !isServer;
		defaultChannelId = topology.DefaultConfig.AddChannel(QosType.ReliableSequenced);
		SetChannelsFromTopology(topology);
		int num = NetworkTransport.AddHost(topology);
		if (isServer)
		{
			relayConnectionId = NetworkTransport.Connect(num, RelayAddress, RelayPort, 0, out var _);
		}
		return num;
	}

	public int AddHost(HostTopology topology, int port)
	{
		if (!Enabled)
		{
			return NetworkTransport.AddHost(topology, port);
		}
		bool isServer = IsServer;
		isClient = !isServer;
		defaultChannelId = topology.DefaultConfig.AddChannel(QosType.ReliableSequenced);
		SetChannelsFromTopology(topology);
		int num = NetworkTransport.AddHost(topology, port);
		if (isServer)
		{
			relayConnectionId = NetworkTransport.Connect(num, RelayAddress, RelayPort, 0, out var _);
		}
		return num;
	}

	public int AddHost(HostTopology topology, int port, string ip)
	{
		if (!Enabled)
		{
			return NetworkTransport.AddHost(topology, port, ip);
		}
		bool isServer = IsServer;
		isClient = !isServer;
		defaultChannelId = topology.DefaultConfig.AddChannel(QosType.ReliableSequenced);
		SetChannelsFromTopology(topology);
		int num = NetworkTransport.AddHost(topology, port, ip);
		if (isServer)
		{
			relayConnectionId = NetworkTransport.Connect(num, RelayAddress, RelayPort, 0, out var _);
		}
		return num;
	}

	public int AddHostWithSimulator(HostTopology topology, int minTimeout, int maxTimeout, int port, string ip)
	{
		if (!Enabled)
		{
			return NetworkTransport.AddHostWithSimulator(topology, minTimeout, maxTimeout);
		}
		bool isServer = IsServer;
		isClient = !isServer;
		defaultChannelId = topology.DefaultConfig.AddChannel(QosType.ReliableSequenced);
		SetChannelsFromTopology(topology);
		int num = NetworkTransport.AddHostWithSimulator(topology, minTimeout, maxTimeout, port, ip);
		if (isServer)
		{
			relayConnectionId = NetworkTransport.Connect(num, RelayAddress, RelayPort, 0, out var _);
		}
		return num;
	}

	public int AddHostWithSimulator(HostTopology topology, int minTimeout, int maxTimeout)
	{
		if (!Enabled)
		{
			return NetworkTransport.AddHostWithSimulator(topology, minTimeout, maxTimeout);
		}
		bool isServer = IsServer;
		isClient = !isServer;
		defaultChannelId = topology.DefaultConfig.AddChannel(QosType.ReliableSequenced);
		SetChannelsFromTopology(topology);
		int num = NetworkTransport.AddHostWithSimulator(topology, minTimeout, maxTimeout);
		if (isServer)
		{
			relayConnectionId = NetworkTransport.Connect(num, RelayAddress, RelayPort, 0, out var _);
		}
		return num;
	}

	public int AddHostWithSimulator(HostTopology topology, int minTimeout, int maxTimeout, int port)
	{
		if (!Enabled)
		{
			return NetworkTransport.AddHostWithSimulator(topology, minTimeout, maxTimeout, port);
		}
		bool isServer = IsServer;
		isClient = !isServer;
		SetChannelsFromTopology(topology);
		int num = NetworkTransport.AddHostWithSimulator(topology, minTimeout, maxTimeout, port);
		if (isServer)
		{
			relayConnectionId = NetworkTransport.Connect(num, RelayAddress, RelayPort, 0, out var _);
		}
		return num;
	}

	public int AddWebsocketHost(HostTopology topology, int port)
	{
		if (!Enabled)
		{
			return NetworkTransport.AddWebsocketHost(topology, port);
		}
		bool isServer = IsServer;
		isClient = !isServer;
		defaultChannelId = topology.DefaultConfig.AddChannel(QosType.ReliableSequenced);
		SetChannelsFromTopology(topology);
		int num = NetworkTransport.AddWebsocketHost(topology, port);
		if (isServer)
		{
			relayConnectionId = NetworkTransport.Connect(num, RelayAddress, RelayPort, 0, out var _);
		}
		return num;
	}

	public int AddWebsocketHost(HostTopology topology, int port, string ip)
	{
		if (!Enabled)
		{
			return NetworkTransport.AddWebsocketHost(topology, port, ip);
		}
		bool isServer = IsServer;
		isClient = !isServer;
		defaultChannelId = topology.DefaultConfig.AddChannel(QosType.ReliableSequenced);
		SetChannelsFromTopology(topology);
		int num = NetworkTransport.AddWebsocketHost(topology, port, ip);
		if (isServer)
		{
			relayConnectionId = NetworkTransport.Connect(num, RelayAddress, RelayPort, 0, out var _);
		}
		return num;
	}

	public bool Disconnect(int hostId, int connectionId, out byte error)
	{
		if (!Enabled)
		{
			return NetworkTransport.Disconnect(hostId, connectionId, out error);
		}
		if (!isClient)
		{
			for (byte b = 0; b < 8; b++)
			{
				disconnectBuffer[b] = (byte)((ulong)connectionId >> b * 8);
			}
			ForceDisconnectedData forceDisconnectedData = new ForceDisconnectedData
			{
				hostId = hostId,
				connectionId = connectionId,
				channelId = defaultChannelId,
				buffer = new byte[disconnectBuffer.Length],
				size = 9
			};
			Buffer.BlockCopy(disconnectBuffer, 0, forceDisconnectedData.buffer, 0, 9);
			queuedForceDisconnects.Add(forceDisconnectedData);
			return NetworkTransport.Send(hostId, relayConnectionId, defaultChannelId, disconnectBuffer, 9, out error);
		}
		return NetworkTransport.Disconnect(hostId, connectionId, out error);
	}

	public bool Send(int hostId, int connectionId, int channelId, byte[] buffer, int size, out byte error)
	{
		if (!Enabled)
		{
			return NetworkTransport.Send(hostId, connectionId, channelId, buffer, size, out error);
		}
		size++;
		if (!isClient)
		{
			size += 8;
			if (buffer.Length < size)
			{
				Logger.Log($"Resizing 'buffer' array as buffer.Length ({buffer.Length}) < size ({size})", GetType().Name, ConsoleColor.Cyan);
				byte[] array = new byte[size];
				Buffer.BlockCopy(buffer, 0, array, 0, buffer.Length);
				buffer = array;
			}
			int num = size - 9;
			for (byte b = 0; b < 8; b++)
			{
				buffer[num + b] = (byte)((ulong)connectionId >> b * 8);
			}
		}
		buffer[size - 1] = 2;
		totalSentBytes += (ulong)size;
		return NetworkTransport.Send(hostId, relayConnectionId, channelId, buffer, size, out error);
	}

	public bool QueueMessageForSending(int hostId, int connectionId, int channelId, byte[] buffer, int size, out byte error)
	{
		if (!Enabled)
		{
			return NetworkTransport.QueueMessageForSending(hostId, connectionId, channelId, buffer, size, out error);
		}
		size++;
		if (!isClient)
		{
			size += 8;
			if (buffer.Length < size)
			{
				Logger.Log($"Resizing 'buffer' array as buffer.Length ({buffer.Length}) < size ({size})", GetType().Name, ConsoleColor.Cyan);
				byte[] array = new byte[size];
				Buffer.BlockCopy(buffer, 0, array, 0, buffer.Length);
				buffer = array;
			}
			int num = size - 9;
			for (byte b = 0; b < 8; b++)
			{
				buffer[num + b] = (byte)((ulong)connectionId >> b * 8);
			}
		}
		buffer[size - 1] = 2;
		return NetworkTransport.QueueMessageForSending(hostId, relayConnectionId, channelId, buffer, size, out error);
	}

	public bool SendQueuedMessages(int hostId, int connectionId, out byte error)
	{
		if (!Enabled)
		{
			return NetworkTransport.SendQueuedMessages(hostId, connectionId, out error);
		}
		return NetworkTransport.SendQueuedMessages(hostId, relayConnectionId, out error);
	}

	public NetworkEventType ReceiveFromHost(int hostId, out int connectionId, out int channelId, byte[] buffer, int bufferSize, out int receivedSize, out byte error)
	{
		if (!Enabled)
		{
			return NetworkTransport.ReceiveFromHost(hostId, out connectionId, out channelId, buffer, bufferSize, out receivedSize, out error);
		}
		if (queuedForceDisconnects.Count > 0)
		{
			ForceDisconnectedData forceDisconnectedData = queuedForceDisconnects[0];
			queuedForceDisconnects.RemoveAt(0);
			NetworkEventType networkEventType = NetworkEventType.DisconnectEvent;
			connectionId = forceDisconnectedData.connectionId;
			channelId = forceDisconnectedData.channelId;
			receivedSize = forceDisconnectedData.size;
			error = 0;
			return BaseReceive(networkEventType, hostId, ref connectionId, ref channelId, forceDisconnectedData.buffer, forceDisconnectedData.size, ref receivedSize, ref error);
		}
		NetworkEventType networkEventType2 = NetworkTransport.ReceiveFromHost(hostId, out connectionId, out channelId, buffer, bufferSize, out receivedSize, out error);
		return BaseReceive(networkEventType2, hostId, ref connectionId, ref channelId, buffer, bufferSize, ref receivedSize, ref error);
	}

	public NetworkEventType Receive(out int hostId, out int connectionId, out int channelId, byte[] buffer, int bufferSize, out int receivedSize, out byte error)
	{
		if (!Enabled)
		{
			return NetworkTransport.Receive(out hostId, out connectionId, out channelId, buffer, bufferSize, out receivedSize, out error);
		}
		NetworkEventType networkEventType = NetworkTransport.Receive(out hostId, out connectionId, out channelId, buffer, bufferSize, out receivedSize, out error);
		return BaseReceive(networkEventType, hostId, ref connectionId, ref channelId, buffer, bufferSize, ref receivedSize, ref error);
	}

	private IEnumerator DummyPingCoroutine(int hostId, int connectionId)
	{
		WaitForSeconds waiter = new WaitForSeconds(1f);
		int failCount = 0;
		while (true)
		{
			failCount = ((!NetworkTransport.Send(hostId, connectionId, defaultChannelId, serverDummyPingBuffer, 1, out var _)) ? (failCount + 1) : 0);
			if (failCount >= 3)
			{
				break;
			}
			yield return waiter;
		}
		this.OnRelayServerDisconnected?.Invoke();
	}

	private NetworkEventType BaseReceive(NetworkEventType @event, int hostId, ref int connectionId, ref int channelId, byte[] buffer, int bufferSize, ref int receivedSize, ref byte error)
	{
		totalReceivedBytes += (ulong)receivedSize;
		switch (@event)
		{
		case NetworkEventType.DataEvent:
			switch ((MessageType)buffer[receivedSize - 1])
			{
			case MessageType.AddressReport:
			{
				byte[] array = new byte[16];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = buffer[i];
				}
				ushort num4 = (ushort)(buffer[16] | (buffer[17] << 8));
				IPEndPoint iPEndPoint = new IPEndPoint(new IPAddress(array), num4);
				Logger.Log($"Connected to Relay Server with resolved endpoint ({iPEndPoint.ToString()})", "UnetRelayTransport", ConsoleColor.Cyan);
				if (Matchmaker.Instance.IsLobbyOwner())
				{
					Matchmaker.Instance.CurrentLobby.SetLobbyPort(iPEndPoint.Port);
				}
				RemoteEndpointReported = true;
				this.OnRemoteEndpointReported?.Invoke(iPEndPoint);
				serverPingCoroutine = Matchmaker.Instance.StartCoroutine(DummyPingCoroutine(hostId, connectionId));
				break;
			}
			case MessageType.ConnectToServer:
				if (!isClient)
				{
					ulong num3 = buffer[receivedSize - 9] | ((ulong)buffer[receivedSize - 8] << 8) | ((ulong)buffer[receivedSize - 7] << 16) | ((ulong)buffer[receivedSize - 6] << 24) | ((ulong)buffer[receivedSize - 5] << 32) | ((ulong)buffer[receivedSize - 4] << 40) | ((ulong)buffer[receivedSize - 3] << 48) | ((ulong)buffer[receivedSize - 2] << 56);
					connectionId = (int)num3;
				}
				return NetworkEventType.ConnectEvent;
			case MessageType.Data:
				if (isClient)
				{
					receivedSize--;
				}
				else
				{
					receivedSize -= 9;
					ulong num2 = buffer[receivedSize] | ((ulong)buffer[receivedSize + 1] << 8) | ((ulong)buffer[receivedSize + 2] << 16) | ((ulong)buffer[receivedSize + 3] << 24) | ((ulong)buffer[receivedSize + 4] << 32) | ((ulong)buffer[receivedSize + 5] << 40) | ((ulong)buffer[receivedSize + 6] << 48) | ((ulong)buffer[receivedSize + 7] << 56);
					connectionId = (int)num2;
				}
				return NetworkEventType.DataEvent;
			case MessageType.ClientDisconnect:
			{
				ulong num = buffer[0] | ((ulong)buffer[1] << 8) | ((ulong)buffer[2] << 16) | ((ulong)buffer[3] << 24) | ((ulong)buffer[4] << 32) | ((ulong)buffer[5] << 40) | ((ulong)buffer[6] << 48) | ((ulong)buffer[7] << 56);
				connectionId = (int)num;
				return NetworkEventType.DisconnectEvent;
			}
			}
			break;
		case NetworkEventType.ConnectEvent:
			if (isClient)
			{
				IPAddress iPAddress = IPAddress.Parse(address);
				byte[] array2;
				if (iPAddress.AddressFamily == AddressFamily.InterNetworkV6)
				{
					array2 = iPAddress.GetAddressBytes();
				}
				else if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
				{
					byte[] addressBytes = iPAddress.GetAddressBytes();
					array2 = new byte[16]
					{
						0,
						0,
						0,
						0,
						0,
						0,
						0,
						0,
						0,
						0,
						255,
						255,
						addressBytes[0],
						addressBytes[1],
						addressBytes[2],
						addressBytes[3]
					};
				}
				else
				{
					array2 = null;
				}
				for (int j = 0; j < array2.Length; j++)
				{
					buffer[j] = array2[j];
				}
				for (byte b = 0; b < 2; b++)
				{
					buffer[16 + b] = (byte)(port >> b * 8);
				}
				buffer[18] = (byte)Application.platform;
				string lobbyCode = Matchmaker.Instance.CurrentLobby.GetLobbyCode();
				byte[] bytes = Encoding.ASCII.GetBytes(lobbyCode);
				for (int k = 0; k < bytes.Length; k++)
				{
					buffer[19 + k] = bytes[k];
				}
				buffer[23] = 1;
				NetworkTransport.Send(hostId, connectionId, defaultChannelId, buffer, 24, out error);
			}
			else
			{
				string lobbyCode2 = Matchmaker.Instance.CurrentLobby.GetLobbyCode();
				byte[] bytes2 = Encoding.ASCII.GetBytes(lobbyCode2);
				for (int l = 0; l < bytes2.Length; l++)
				{
					buffer[l] = bytes2[l];
				}
				buffer[bytes2.Length] = (byte)Application.platform;
				buffer[bytes2.Length + 1] = 0;
				NetworkTransport.Send(hostId, connectionId, defaultChannelId, buffer, bytes2.Length + 1 + 1, out error);
			}
			return NetworkEventType.Nothing;
		case NetworkEventType.DisconnectEvent:
			if (error == 10)
			{
				Debug.LogError("[MLAPI.Relay] The MLAPI Relay detected a CRC mismatch. This could be due to the maxClients or other connectionConfig settings not being the same");
			}
			return NetworkEventType.DisconnectEvent;
		}
		return @event;
	}

	public void Init()
	{
		NetworkTransport.Init();
	}

	public void Init(GlobalConfig config)
	{
		NetworkTransport.Init(config);
	}

	public void Shutdown()
	{
		RemoteEndpointReported = false;
		NetworkTransport.Shutdown();
	}

	public void ConnectAsNetworkHost(int hostId, string address, int port, NetworkID network, SourceID source, NodeID node, out byte error)
	{
		NetworkTransport.ConnectAsNetworkHost(hostId, address, port, network, source, node, out error);
	}

	public int ConnectToNetworkPeer(int hostId, string address, int port, int specialConnectionId, int relaySlotId, NetworkID network, SourceID source, NodeID node, out byte error)
	{
		return NetworkTransport.ConnectToNetworkPeer(hostId, address, port, specialConnectionId, relaySlotId, network, source, node, out error);
	}

	public bool DoesEndPointUsePlatformProtocols(EndPoint endPoint)
	{
		return NetworkTransport.DoesEndPointUsePlatformProtocols(endPoint);
	}

	public bool RemoveHost(int hostId)
	{
		RemoteEndpointReported = false;
		if (serverPingCoroutine != null)
		{
			Matchmaker.Instance.StopCoroutine(serverPingCoroutine);
			serverPingCoroutine = null;
		}
		return NetworkTransport.RemoveHost(hostId);
	}

	public NetworkEventType ReceiveRelayEventFromHost(int hostId, out byte error)
	{
		return NetworkTransport.ReceiveRelayEventFromHost(hostId, out error);
	}

	public int GetCurrentRTT(int hostId, int connectionId, out byte error)
	{
		return NetworkTransport.GetCurrentRTT(hostId, connectionId, out error);
	}

	public void GetConnectionInfo(int hostId, int connectionId, out string address, out int port, out NetworkID network, out NodeID dstNode, out byte error)
	{
		NetworkTransport.GetConnectionInfo(hostId, connectionId, out address, out port, out network, out dstNode, out error);
	}

	public void SetBroadcastCredentials(int hostId, int key, int version, int subversion, out byte error)
	{
		NetworkTransport.SetBroadcastCredentials(hostId, key, version, subversion, out error);
	}

	public bool StartBroadcastDiscovery(int hostId, int broadcastPort, int key, int version, int subversion, byte[] buffer, int size, int timeout, out byte error)
	{
		return NetworkTransport.StartBroadcastDiscovery(hostId, broadcastPort, key, version, subversion, buffer, size, timeout, out error);
	}

	public void GetBroadcastConnectionInfo(int hostId, out string address, out int port, out byte error)
	{
		NetworkTransport.GetBroadcastConnectionInfo(hostId, out address, out port, out error);
	}

	public void GetBroadcastConnectionMessage(int hostId, byte[] buffer, int bufferSize, out int receivedSize, out byte error)
	{
		NetworkTransport.GetBroadcastConnectionMessage(hostId, buffer, bufferSize, out receivedSize, out error);
	}

	public void StopBroadcastDiscovery()
	{
		NetworkTransport.StopBroadcastDiscovery();
	}

	public void SetPacketStat(int direction, int packetStatId, int numMsgs, int numBytes)
	{
		NetworkTransport.SetPacketStat(direction, packetStatId, numMsgs, numBytes);
	}
}
