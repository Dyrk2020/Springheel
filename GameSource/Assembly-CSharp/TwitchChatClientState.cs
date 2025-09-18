using System.Runtime.InteropServices;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

public class TwitchChatClientState : NetworkBehaviour
{
	public struct VoteState
	{
		public int pickableIndex;

		public int votes;

		public bool newVotes;

		public VoteState(int pickableIndex, int votes, bool newVotes)
		{
			this.pickableIndex = pickableIndex;
			this.votes = votes;
			this.newVotes = newVotes;
		}
	}

	public class SyncListVoteState : SyncListStruct<VoteState>
	{
		public override void SerializeItem(NetworkWriter writer, VoteState item)
		{
			writer.WritePackedUInt32((uint)item.pickableIndex);
			writer.WritePackedUInt32((uint)item.votes);
			writer.Write(item.newVotes);
		}

		public override VoteState DeserializeItem(NetworkReader reader)
		{
			return new VoteState
			{
				pickableIndex = (int)reader.ReadPackedUInt32(),
				votes = (int)reader.ReadPackedUInt32(),
				newVotes = reader.ReadBoolean()
			};
		}
	}

	public SyncListVoteState SyncListVoteStates;

	[SyncVar]
	public bool showTwitchVoteWidget;

	[SyncVar]
	public string channelName;

	[SyncVar]
	public int NumberOfVotes;

	public SyncListInt playersWithTwitchItem;

	private static int kListSyncListVoteStates;

	private static int kListplayersWithTwitchItem;

	private static int kRpcRpcUserVotedMessage;

	private static int kRpcRpcClearUserVotedMessages;

	private static int kRpcRpcDistributeVoteIntoPartyBox;

	private static int kRpcRpcUserPlacedTwitchPiece;

	public bool NetworkshowTwitchVoteWidget
	{
		get
		{
			return showTwitchVoteWidget;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref showTwitchVoteWidget, 2u);
		}
	}

	public string NetworkchannelName
	{
		get
		{
			return channelName;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref channelName, 4u);
		}
	}

	public int NetworkNumberOfVotes
	{
		get
		{
			return NumberOfVotes;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref NumberOfVotes, 8u);
		}
	}

	private void Awake()
	{
		if (TwitchChatController.instance != null)
		{
			TwitchChatController.instance.twitchChatClientState = this;
		}
		Debug.Log("Spawned Twitch Chat ClientState...");
		SyncListVoteStates = new SyncListVoteState();
		playersWithTwitchItem = new SyncListInt();
		SyncListVoteStates.InitializeBehaviour(this, kListSyncListVoteStates);
		playersWithTwitchItem.InitializeBehaviour(this, kListplayersWithTwitchItem);
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	[ClientRpc]
	public void RpcUserVotedMessage(string username)
	{
		TwitchChatController.instance.EnqueueVotedUsername(username);
	}

	[ClientRpc]
	public void RpcClearUserVotedMessages()
	{
		if (TwitchChatController.PlatformHasTwitchIntegration)
		{
			TwitchChatController.instance.ClearVotedUsernames();
		}
	}

	[ClientRpc]
	public void RpcDistributeVoteIntoPartyBox(int numberToPutIntoPartyBox)
	{
		if (TwitchChatController.PlatformHasTwitchIntegration)
		{
			TwitchChatController.instance.DistributeVotes(numberToPutIntoPartyBox);
		}
	}

	[ClientRpc]
	public void RpcUserPlacedTwitchPiece(string[] fireworksUsernames, Vector3 position)
	{
		if (TwitchChatController.PlatformHasTwitchIntegration)
		{
			TwitchChatController.instance.SpawnNameFireworks(fireworksUsernames, position);
		}
	}

	public TwitchChatClientState()
	{
		SyncListVoteStates = new SyncListVoteState();
		playersWithTwitchItem = new SyncListInt();
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeSyncListSyncListVoteStates(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("SyncList SyncListVoteStates called on server.");
		}
		else
		{
			((TwitchChatClientState)obj).SyncListVoteStates.HandleMsg(reader);
		}
	}

	protected static void InvokeSyncListplayersWithTwitchItem(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("SyncList playersWithTwitchItem called on server.");
		}
		else
		{
			((TwitchChatClientState)obj).playersWithTwitchItem.HandleMsg(reader);
		}
	}

	protected static void InvokeRpcRpcUserVotedMessage(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUserVotedMessage called on server.");
		}
		else
		{
			((TwitchChatClientState)obj).RpcUserVotedMessage(reader.ReadString());
		}
	}

	protected static void InvokeRpcRpcClearUserVotedMessages(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcClearUserVotedMessages called on server.");
		}
		else
		{
			((TwitchChatClientState)obj).RpcClearUserVotedMessages();
		}
	}

	protected static void InvokeRpcRpcDistributeVoteIntoPartyBox(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDistributeVoteIntoPartyBox called on server.");
		}
		else
		{
			((TwitchChatClientState)obj).RpcDistributeVoteIntoPartyBox((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcUserPlacedTwitchPiece(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUserPlacedTwitchPiece called on server.");
		}
		else
		{
			((TwitchChatClientState)obj).RpcUserPlacedTwitchPiece(GeneratedNetworkCode._ReadArrayString_None(reader), reader.ReadVector3());
		}
	}

	public void CallRpcUserVotedMessage(string username)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcUserVotedMessage called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcUserVotedMessage);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(username);
		SendRPCInternal(networkWriter, 0, "RpcUserVotedMessage");
	}

	public void CallRpcClearUserVotedMessages()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcClearUserVotedMessages called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcClearUserVotedMessages);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcClearUserVotedMessages");
	}

	public void CallRpcDistributeVoteIntoPartyBox(int numberToPutIntoPartyBox)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcDistributeVoteIntoPartyBox called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcDistributeVoteIntoPartyBox);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)numberToPutIntoPartyBox);
		SendRPCInternal(networkWriter, 0, "RpcDistributeVoteIntoPartyBox");
	}

	public void CallRpcUserPlacedTwitchPiece(string[] fireworksUsernames, Vector3 position)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcUserPlacedTwitchPiece called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcUserPlacedTwitchPiece);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WriteArrayString_None(networkWriter, fireworksUsernames);
		networkWriter.Write(position);
		SendRPCInternal(networkWriter, 0, "RpcUserPlacedTwitchPiece");
	}

	static TwitchChatClientState()
	{
		kRpcRpcUserVotedMessage = -1845595095;
		NetworkBehaviour.RegisterRpcDelegate(typeof(TwitchChatClientState), kRpcRpcUserVotedMessage, InvokeRpcRpcUserVotedMessage);
		kRpcRpcClearUserVotedMessages = -1683067553;
		NetworkBehaviour.RegisterRpcDelegate(typeof(TwitchChatClientState), kRpcRpcClearUserVotedMessages, InvokeRpcRpcClearUserVotedMessages);
		kRpcRpcDistributeVoteIntoPartyBox = -1501931743;
		NetworkBehaviour.RegisterRpcDelegate(typeof(TwitchChatClientState), kRpcRpcDistributeVoteIntoPartyBox, InvokeRpcRpcDistributeVoteIntoPartyBox);
		kRpcRpcUserPlacedTwitchPiece = -339576926;
		NetworkBehaviour.RegisterRpcDelegate(typeof(TwitchChatClientState), kRpcRpcUserPlacedTwitchPiece, InvokeRpcRpcUserPlacedTwitchPiece);
		kListSyncListVoteStates = 207860418;
		NetworkBehaviour.RegisterSyncListDelegate(typeof(TwitchChatClientState), kListSyncListVoteStates, InvokeSyncListSyncListVoteStates);
		kListplayersWithTwitchItem = -179455551;
		NetworkBehaviour.RegisterSyncListDelegate(typeof(TwitchChatClientState), kListplayersWithTwitchItem, InvokeSyncListplayersWithTwitchItem);
		NetworkCRC.RegisterBehaviour("TwitchChatClientState", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			GeneratedNetworkCode._WriteStructSyncListVoteState_TwitchChatClientState(writer, SyncListVoteStates);
			writer.Write(showTwitchVoteWidget);
			writer.Write(channelName);
			writer.WritePackedUInt32((uint)NumberOfVotes);
			SyncListInt.WriteInstance(writer, playersWithTwitchItem);
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
			GeneratedNetworkCode._WriteStructSyncListVoteState_TwitchChatClientState(writer, SyncListVoteStates);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(showTwitchVoteWidget);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(channelName);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)NumberOfVotes);
		}
		if ((base.syncVarDirtyBits & 0x10) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			SyncListInt.WriteInstance(writer, playersWithTwitchItem);
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
			GeneratedNetworkCode._ReadStructSyncListVoteState_TwitchChatClientState(reader, SyncListVoteStates);
			showTwitchVoteWidget = reader.ReadBoolean();
			channelName = reader.ReadString();
			NumberOfVotes = (int)reader.ReadPackedUInt32();
			SyncListInt.ReadReference(reader, playersWithTwitchItem);
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			GeneratedNetworkCode._ReadStructSyncListVoteState_TwitchChatClientState(reader, SyncListVoteStates);
		}
		if ((num & 2) != 0)
		{
			showTwitchVoteWidget = reader.ReadBoolean();
		}
		if ((num & 4) != 0)
		{
			channelName = reader.ReadString();
		}
		if ((num & 8) != 0)
		{
			NumberOfVotes = (int)reader.ReadPackedUInt32();
		}
		if ((num & 0x10) != 0)
		{
			SyncListInt.ReadReference(reader, playersWithTwitchItem);
		}
	}

	public override void PreStartClient()
	{
	}
}
