using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkPlayerTracker : NetworkBehaviour
{
	[Serializable]
	public struct NetPlayerInfo
	{
		public uint LobbyNetID;

		public uint GameNetID;

		public int NetworkNumber;

		public NetPlayerInfo(int networkNumber)
		{
			NetworkNumber = networkNumber;
			LobbyNetID = 0u;
			GameNetID = 0u;
		}
	}

	public class SyncListNetPlayerInfo : SyncListStruct<NetPlayerInfo>
	{
		public override void SerializeItem(NetworkWriter writer, NetPlayerInfo item)
		{
			writer.WritePackedUInt32(item.LobbyNetID);
			writer.WritePackedUInt32(item.GameNetID);
			writer.WritePackedUInt32((uint)item.NetworkNumber);
		}

		public override NetPlayerInfo DeserializeItem(NetworkReader reader)
		{
			return new NetPlayerInfo
			{
				LobbyNetID = reader.ReadPackedUInt32(),
				GameNetID = reader.ReadPackedUInt32(),
				NetworkNumber = (int)reader.ReadPackedUInt32()
			};
		}
	}

	[SyncVar]
	private int waitingForIDs;

	[SerializeField]
	private SyncListNetPlayerInfo playerInfo;

	private static int kListplayerInfo;

	public bool WaitingForIDs => waitingForIDs > 0;

	public int NumPlayers => playerInfo.Count;

	public bool WaitingForGamePlayerInit
	{
		get
		{
			for (int i = 0; i != playerInfo.Count; i++)
			{
				GameObject gameObject = ClientScene.FindLocalObject(new NetworkInstanceId(playerInfo[i].GameNetID));
				if (gameObject == null)
				{
					return true;
				}
				if (!gameObject.GetComponent<GamePlayer>().Initialized)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool WaitingForLobbyPlayerInit
	{
		get
		{
			for (int i = 0; i != playerInfo.Count; i++)
			{
				GameObject gameObject = ClientScene.FindLocalObject(new NetworkInstanceId(playerInfo[i].LobbyNetID));
				if (gameObject == null)
				{
					return true;
				}
				if (!gameObject.GetComponent<LobbyPlayer>().Initialized)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool WaitingForSceneInit
	{
		get
		{
			for (int i = 0; i != playerInfo.Count; i++)
			{
				GameObject gameObject = ClientScene.FindLocalObject(new NetworkInstanceId(playerInfo[i].GameNetID));
				if (!(gameObject == null))
				{
					GamePlayer component = gameObject.GetComponent<GamePlayer>();
					if (!component.SceneInitDone || !component.Initialized)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public bool WaitingForSetupStart
	{
		get
		{
			for (int i = 0; i != playerInfo.Count; i++)
			{
				GameObject gameObject = ClientScene.FindLocalObject(new NetworkInstanceId(playerInfo[i].GameNetID));
				if (!(gameObject == null))
				{
					GamePlayer component = gameObject.GetComponent<GamePlayer>();
					if (!component.SetupStartDone || !component.Initialized)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public int NetworkwaitingForIDs
	{
		get
		{
			return waitingForIDs;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref waitingForIDs, 1u);
		}
	}

	private void Awake()
	{
		playerInfo = new SyncListNetPlayerInfo();
		playerInfo.InitializeBehaviour(this, kListplayerInfo);
	}

	public NetPlayerInfo GetPlayerInfo(int networkNumber)
	{
		for (int i = 0; i != playerInfo.Count; i++)
		{
			NetPlayerInfo result = playerInfo[i];
			if (result.NetworkNumber == networkNumber)
			{
				return result;
			}
		}
		return new NetPlayerInfo(networkNumber);
	}

	public GamePlayer GetGamePlayer(int networkNumber)
	{
		for (int i = 0; i != playerInfo.Count; i++)
		{
			NetPlayerInfo netPlayerInfo = playerInfo[i];
			if (netPlayerInfo.NetworkNumber == networkNumber)
			{
				if (netPlayerInfo.GameNetID == 0)
				{
					return null;
				}
				GameObject gameObject = ClientScene.FindLocalObject(new NetworkInstanceId(netPlayerInfo.GameNetID));
				if (gameObject == null)
				{
					return null;
				}
				return gameObject.GetComponent<GamePlayer>();
			}
		}
		return null;
	}

	public LobbyPlayer GetLobbyPlayer(int networkNumber)
	{
		for (int i = 0; i != playerInfo.Count; i++)
		{
			NetPlayerInfo netPlayerInfo = playerInfo[i];
			if (netPlayerInfo.NetworkNumber == networkNumber)
			{
				if (netPlayerInfo.LobbyNetID == 0)
				{
					return null;
				}
				return ClientScene.FindLocalObject(new NetworkInstanceId(netPlayerInfo.LobbyNetID)).GetComponent<LobbyPlayer>();
			}
		}
		return null;
	}

	public IEnumerable<uint> GetAllGameNetIDs()
	{
		int i = 0;
		while (i < playerInfo.Count)
		{
			yield return playerInfo[i].GameNetID;
			int num = i + 1;
			i = num;
		}
	}

	public NetPlayerInfo GetPlayerInfoByIndex(int i)
	{
		return playerInfo[i];
	}

	public void AddLobbyPlayer(LobbyPlayer lobbyP)
	{
		if (!base.hasAuthority)
		{
			Debug.LogWarning("Trying to add lobby player from the client");
		}
		bool flag = false;
		NetPlayerInfo netPlayerInfo = new NetPlayerInfo(lobbyP.networkNumber);
		for (int i = 0; i != playerInfo.Count; i++)
		{
			if (playerInfo[i].NetworkNumber == lobbyP.networkNumber)
			{
				netPlayerInfo = playerInfo[i];
				flag = true;
				break;
			}
		}
		if (!lobbyP.netId.IsEmpty())
		{
			Debug.Log("Net id added for lobby player");
			NetworkServer.SendToAll(NetMsgTypes.NetworkClientConnected, new MsgNetworkClientConnected());
			netPlayerInfo.LobbyNetID = lobbyP.netId.Value;
			if (!flag)
			{
				playerInfo.Add(netPlayerInfo);
				return;
			}
			for (int j = 0; j != playerInfo.Count; j++)
			{
				if (playerInfo[j].NetworkNumber == netPlayerInfo.NetworkNumber)
				{
					playerInfo[j] = netPlayerInfo;
					break;
				}
			}
		}
		else
		{
			Debug.Log("Waiting for netid for lobby player " + lobbyP.networkNumber);
			StartCoroutine(waitForLobbyNetID(lobbyP));
		}
	}

	public void AddGamePlayer(GamePlayer gameP)
	{
		if (!base.hasAuthority)
		{
			Debug.LogWarning("Trying to add game player from the client");
		}
		bool flag = false;
		NetPlayerInfo netPlayerInfo = new NetPlayerInfo(gameP.networkNumber);
		for (int i = 0; i != playerInfo.Count; i++)
		{
			if (playerInfo[i].NetworkNumber == gameP.networkNumber)
			{
				netPlayerInfo = playerInfo[i];
				flag = true;
				break;
			}
		}
		if (!gameP.netId.IsEmpty())
		{
			Debug.Log("Net id " + gameP.netId.ToString() + " added for game player " + netPlayerInfo.NetworkNumber);
			netPlayerInfo.GameNetID = gameP.netId.Value;
			if (!flag)
			{
				playerInfo.Add(netPlayerInfo);
				return;
			}
			for (int j = 0; j != playerInfo.Count; j++)
			{
				if (playerInfo[j].NetworkNumber == netPlayerInfo.NetworkNumber)
				{
					playerInfo[j] = netPlayerInfo;
					break;
				}
			}
		}
		else
		{
			Debug.Log("Waiting for netid for game player " + gameP.networkNumber);
			StartCoroutine(waitForGameNetID(gameP));
		}
	}

	public void RemovePlayer(int networkNumber)
	{
		int num = -1;
		for (int i = 0; i != playerInfo.Count; i++)
		{
			if (playerInfo[i].NetworkNumber == networkNumber)
			{
				num = i;
				break;
			}
		}
		if (num >= 0)
		{
			playerInfo.RemoveAt(num);
		}
	}

	public void RemoveGamePlayer(int networkNumber)
	{
		for (int i = 0; i != playerInfo.Count; i++)
		{
			NetPlayerInfo value = playerInfo[i];
			if (value.NetworkNumber == networkNumber)
			{
				value.GameNetID = 0u;
				playerInfo[i] = value;
				break;
			}
		}
	}

	public void RemoveLobbyPlayer(int networkNumber)
	{
		for (int i = 0; i != playerInfo.Count; i++)
		{
			NetPlayerInfo value = playerInfo[i];
			if (value.NetworkNumber == networkNumber)
			{
				value.LobbyNetID = 0u;
				playerInfo[i] = value;
				break;
			}
		}
	}

	private IEnumerator waitForLobbyNetID(LobbyPlayer lobbyP)
	{
		uint value = lobbyP.netId.Value;
		NetworkwaitingForIDs = waitingForIDs + 1;
		while (value == 0)
		{
			yield return null;
			value = lobbyP.netId.Value;
		}
		NetworkwaitingForIDs = waitingForIDs - 1;
		Debug.Log("Done waiting for lobby netid, trying to add again");
		AddLobbyPlayer(lobbyP);
	}

	private IEnumerator waitForGameNetID(GamePlayer gameP)
	{
		uint val = 0u;
		NetworkwaitingForIDs = waitingForIDs + 1;
		while (val == 0 && gameP != null)
		{
			val = gameP.netId.Value;
			yield return null;
		}
		Debug.Log("Done waiting for game netid, trying to add again");
		NetworkwaitingForIDs = waitingForIDs - 1;
		AddGamePlayer(gameP);
	}

	public GameObject[] GetPlayerObjects()
	{
		GameObject[] array = new GameObject[playerInfo.Count];
		for (int i = 0; i != playerInfo.Count; i++)
		{
			GameObject gameObject = ClientScene.FindLocalObject(new NetworkInstanceId(playerInfo[i].GameNetID));
			array[i] = gameObject;
		}
		return array;
	}

	public NetworkPlayerTracker()
	{
		playerInfo = new SyncListNetPlayerInfo();
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeSyncListplayerInfo(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("SyncList playerInfo called on server.");
		}
		else
		{
			((NetworkPlayerTracker)obj).playerInfo.HandleMsg(reader);
		}
	}

	static NetworkPlayerTracker()
	{
		kListplayerInfo = -1668477030;
		NetworkBehaviour.RegisterSyncListDelegate(typeof(NetworkPlayerTracker), kListplayerInfo, InvokeSyncListplayerInfo);
		NetworkCRC.RegisterBehaviour("NetworkPlayerTracker", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)waitingForIDs);
			GeneratedNetworkCode._WriteStructSyncListNetPlayerInfo_NetworkPlayerTracker(writer, playerInfo);
			return true;
		}
		bool flag = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)waitingForIDs);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			GeneratedNetworkCode._WriteStructSyncListNetPlayerInfo_NetworkPlayerTracker(writer, playerInfo);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			waitingForIDs = (int)reader.ReadPackedUInt32();
			GeneratedNetworkCode._ReadStructSyncListNetPlayerInfo_NetworkPlayerTracker(reader, playerInfo);
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			waitingForIDs = (int)reader.ReadPackedUInt32();
		}
		if ((num & 2) != 0)
		{
			GeneratedNetworkCode._ReadStructSyncListNetPlayerInfo_NetworkPlayerTracker(reader, playerInfo);
		}
	}

	public override void PreStartClient()
	{
	}
}
