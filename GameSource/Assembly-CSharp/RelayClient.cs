using System;
using System.Collections.Generic;
using System.Text;
using Telepathy;
using UnityEngine;

public sealed class RelayClient : MonoBehaviour
{
	private static RelayClient instance;

	private List<Client> relayClients = new List<Client>();

	public static RelayClient Instance
	{
		get
		{
			if (instance == null)
			{
				GameObject obj = new GameObject("RelayClient", typeof(RelayClient));
				instance = obj.GetComponent<RelayClient>();
				UnityEngine.Object.DontDestroyOnLoad(obj);
			}
			return instance;
		}
	}

	private RelayClient()
	{
	}

	public void AddOrRemoveMPSDPlayer(bool isAdding, string playerId, string playerName, string xToken, string lobbyCode, string gamesparksLobby, string secureDeviceAddress, Action<bool, string> result)
	{
		string messageID = (isAdding ? "AddPlayerToMPSD" : "RemovePlayerFromMPSD");
		try
		{
			if (isAdding)
			{
				Logger.Log($"[ID = {messageID}] Adding player to MPSD...", GetType().Name, ConsoleColor.Yellow);
			}
			else
			{
				Logger.Log($"[ID = {messageID}] Removing player from MPSD...", GetType().Name, ConsoleColor.Yellow);
			}
			Client relayClient = new Client(8192);
			relayClient.OnConnected = delegate
			{
				Logger.Log($"[ID = {messageID}] Connected to Server", GetType().Name, ConsoleColor.Yellow);
				string text = JsonUtility.ToJson(new MPSDRequestMessage(messageID)
				{
					RuntimePlatform = Application.platform,
					PlayerID = playerId,
					PlayerName = playerName,
					XToken = xToken,
					LobbyCode = lobbyCode,
					GamesparksLobby = gamesparksLobby,
					SecureDeviceAddress = secureDeviceAddress
				});
				string text2 = CryptoEngine.AesEncrypt(text);
				if (!string.IsNullOrEmpty(text2))
				{
					byte[] bytes = Encoding.ASCII.GetBytes(text2);
					ArraySegment<byte> message = new ArraySegment<byte>(bytes);
					relayClient.Send(message);
					Logger.Log($"[ID = {messageID}] Sent: ClearText.Length ({text.Length}), CipherText.Length ({text2.Length}), ArraySegment.Count ({message.Count}), ClearText ({text})", GetType().Name, ConsoleColor.Yellow);
				}
				else
				{
					DisconnectClient(relayClient);
				}
			};
			relayClient.OnData = delegate(ArraySegment<byte> messageBytes)
			{
				string text = Encoding.ASCII.GetString(messageBytes.Array, messageBytes.Offset, messageBytes.Count);
				string text2 = CryptoEngine.AesDecrypt(text);
				if (!string.IsNullOrEmpty(text2))
				{
					Logger.Log($"[ID = {messageID}] Received: MessageBytes.Count ({messageBytes.Count}), CipherText.Length ({text.Length}), ClearText.Length ({text2.Length}), ClearText ({text2})", GetType().Name, ConsoleColor.Yellow);
					if (RelayMessage.ToMessage(text2).ID.Equals(messageID))
					{
						MPSDResponseMessage mPSDResponseMessage = MPSDResponseMessage.ToMessage(text2);
						if (result != null)
						{
							result(mPSDResponseMessage.IsSuccess, mPSDResponseMessage.Response);
							result = null;
						}
					}
				}
				DisconnectClient(relayClient);
			};
			relayClient.OnDisconnected = delegate
			{
				Logger.Log($"[ID = {messageID}] Disconnected from Server", GetType().Name, ConsoleColor.Yellow);
				if (result != null)
				{
					result(arg1: false, null);
					result = null;
				}
				if (relayClient != null)
				{
					relayClients.Remove(relayClient);
					relayClient = null;
				}
			};
			relayClient.Connect(RelayConstants.SERVER_ADDRESS, RelayConstants.SERVER_PORT);
			relayClients.Add(relayClient);
		}
		catch (Exception ex)
		{
			Logger.Log($"[ID = {messageID}] Caught exception: {ex.Message}", GetType().Name, ConsoleColor.Yellow);
			Debug.LogException(ex);
		}
	}

	private void DisconnectClient(Client relayClient)
	{
		if (relayClient != null && relayClient.Connected)
		{
			relayClient.Disconnect();
		}
	}

	private void Update()
	{
		for (int num = relayClients.Count - 1; num >= 0; num--)
		{
			relayClients[num]?.Tick(100);
		}
	}
}
