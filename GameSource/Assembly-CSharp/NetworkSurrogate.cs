using System;
using System.Runtime.InteropServices;
using GameEvent;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkSurrogate : NetworkBehaviour, IGameEventListener
{
	[SyncVar]
	private bool boolVal;

	[SyncVar]
	private int intVal;

	[SyncVar]
	private float floatVal;

	[SyncVar]
	private string stringVal;

	private bool triggerVal;

	[HideInInspector]
	public int linkAttempts;

	public bool IsTwoWay = true;

	private static int kCmdCmdSetBool;

	private static int kCmdCmdSetInt;

	private static int kCmdCmdSetFloat;

	private static int kCmdCmdSetString;

	private static int kCmdCmdSetTrigger;

	private static int kRpcRpcSetTrigger;

	public bool BoolVal
	{
		get
		{
			return boolVal;
		}
		set
		{
			if (base.hasAuthority)
			{
				CallCmdSetBool(value);
			}
			else if (IsTwoWay)
			{
				MsgSetNetworkSurrogateVal msgSetNetworkSurrogateVal = new MsgSetNetworkSurrogateVal();
				msgSetNetworkSurrogateVal.BoolVal = value;
				msgSetNetworkSurrogateVal.ValueType = MsgSetNetworkSurrogateVal.BOOL;
				msgSetNetworkSurrogateVal.NetSurrogateID = base.netId;
				NetworkManager.singleton.client.Send(NetMsgTypes.SetNetworkSurrogateVal, msgSetNetworkSurrogateVal);
			}
		}
	}

	public int IntVal
	{
		get
		{
			return intVal;
		}
		set
		{
			if (base.hasAuthority)
			{
				CallCmdSetInt(value);
			}
			else if (IsTwoWay)
			{
				MsgSetNetworkSurrogateVal msgSetNetworkSurrogateVal = new MsgSetNetworkSurrogateVal();
				msgSetNetworkSurrogateVal.IntVal = value;
				msgSetNetworkSurrogateVal.ValueType = MsgSetNetworkSurrogateVal.INT;
				msgSetNetworkSurrogateVal.NetSurrogateID = base.netId;
				NetworkManager.singleton.client.Send(NetMsgTypes.SetNetworkSurrogateVal, msgSetNetworkSurrogateVal);
			}
		}
	}

	public float FloatVal
	{
		get
		{
			return floatVal;
		}
		set
		{
			if (base.hasAuthority)
			{
				CallCmdSetFloat(value);
			}
			else if (IsTwoWay)
			{
				MsgSetNetworkSurrogateVal msgSetNetworkSurrogateVal = new MsgSetNetworkSurrogateVal();
				msgSetNetworkSurrogateVal.FloatVal = value;
				msgSetNetworkSurrogateVal.ValueType = MsgSetNetworkSurrogateVal.FLOAT;
				msgSetNetworkSurrogateVal.NetSurrogateID = base.netId;
				NetworkManager.singleton.client.Send(NetMsgTypes.SetNetworkSurrogateVal, msgSetNetworkSurrogateVal);
			}
		}
	}

	public string StringVal
	{
		get
		{
			return stringVal;
		}
		set
		{
			if (base.hasAuthority)
			{
				CallCmdSetString(value);
			}
			else if (IsTwoWay)
			{
				MsgSetNetworkSurrogateVal msgSetNetworkSurrogateVal = new MsgSetNetworkSurrogateVal();
				msgSetNetworkSurrogateVal.StringVal = value;
				msgSetNetworkSurrogateVal.ValueType = MsgSetNetworkSurrogateVal.STRING;
				msgSetNetworkSurrogateVal.NetSurrogateID = base.netId;
				NetworkManager.singleton.client.Send(NetMsgTypes.SetNetworkSurrogateVal, msgSetNetworkSurrogateVal);
			}
		}
	}

	public bool TriggerVal
	{
		get
		{
			if (triggerVal)
			{
				triggerVal = false;
				return true;
			}
			return false;
		}
		set
		{
			if (base.hasAuthority)
			{
				CallCmdSetTrigger(value);
			}
			else if (IsTwoWay)
			{
				MsgSetNetworkSurrogateVal msgSetNetworkSurrogateVal = new MsgSetNetworkSurrogateVal();
				msgSetNetworkSurrogateVal.ValueType = MsgSetNetworkSurrogateVal.TRIGGER;
				msgSetNetworkSurrogateVal.NetSurrogateID = base.netId;
				NetworkManager.singleton.client.Send(NetMsgTypes.SetNetworkSurrogateVal, msgSetNetworkSurrogateVal);
			}
		}
	}

	public bool NetworkboolVal
	{
		get
		{
			return boolVal;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref boolVal, 1u);
		}
	}

	public int NetworkintVal
	{
		get
		{
			return intVal;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref intVal, 2u);
		}
	}

	public float NetworkfloatVal
	{
		get
		{
			return floatVal;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref floatVal, 4u);
		}
	}

	public string NetworkstringVal
	{
		get
		{
			return stringVal;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref stringVal, 8u);
		}
	}

	private void Start()
	{
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding: true);
	}

	private void OnDestroy()
	{
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding: false);
	}

	public void Spawn(GameObject obj)
	{
		if (base.hasAuthority)
		{
			NetworkServer.Spawn(obj);
		}
	}

	public void SpawnWithClientAuthority(GameObject obj, GameObject player)
	{
		if (base.hasAuthority)
		{
			NetworkServer.SpawnWithClientAuthority(obj, player);
		}
	}

	public void SpawnWithClientAuthority(GameObject obj, NetworkConnection conn)
	{
		if (base.hasAuthority)
		{
			NetworkServer.SpawnWithClientAuthority(obj, conn);
		}
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (!base.hasAuthority || !(type == typeof(NetworkMessageReceivedEvent)))
		{
			return;
		}
		NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
		if (networkMessageReceivedEvent.Message.msgType != NetMsgTypes.SetNetworkSurrogateVal)
		{
			return;
		}
		MsgSetNetworkSurrogateVal msgSetNetworkSurrogateVal = networkMessageReceivedEvent.ReadMessage as MsgSetNetworkSurrogateVal;
		if (msgSetNetworkSurrogateVal.NetSurrogateID == base.netId)
		{
			if (msgSetNetworkSurrogateVal.ValueType == MsgSetNetworkSurrogateVal.BOOL)
			{
				BoolVal = msgSetNetworkSurrogateVal.BoolVal;
			}
			if (msgSetNetworkSurrogateVal.ValueType == MsgSetNetworkSurrogateVal.INT)
			{
				IntVal = msgSetNetworkSurrogateVal.IntVal;
			}
			if (msgSetNetworkSurrogateVal.ValueType == MsgSetNetworkSurrogateVal.FLOAT)
			{
				FloatVal = msgSetNetworkSurrogateVal.FloatVal;
			}
			if (msgSetNetworkSurrogateVal.ValueType == MsgSetNetworkSurrogateVal.STRING)
			{
				StringVal = msgSetNetworkSurrogateVal.StringVal;
			}
			if (msgSetNetworkSurrogateVal.ValueType == MsgSetNetworkSurrogateVal.TRIGGER)
			{
				TriggerVal = true;
			}
		}
	}

	[Command]
	private void CmdSetBool(bool value)
	{
		NetworkboolVal = value;
	}

	[Command]
	private void CmdSetInt(int value)
	{
		NetworkintVal = value;
	}

	[Command]
	private void CmdSetFloat(float value)
	{
		NetworkfloatVal = value;
	}

	[Command]
	private void CmdSetString(string value)
	{
		NetworkstringVal = value;
	}

	[Command]
	private void CmdSetTrigger(bool value)
	{
		CallRpcSetTrigger(value: true);
	}

	[ClientRpc]
	private void RpcSetTrigger(bool value)
	{
		triggerVal = value;
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdSetBool(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetBool called on client.");
		}
		else
		{
			((NetworkSurrogate)obj).CmdSetBool(reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdSetInt(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetInt called on client.");
		}
		else
		{
			((NetworkSurrogate)obj).CmdSetInt((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdSetFloat(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetFloat called on client.");
		}
		else
		{
			((NetworkSurrogate)obj).CmdSetFloat(reader.ReadSingle());
		}
	}

	protected static void InvokeCmdCmdSetString(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetString called on client.");
		}
		else
		{
			((NetworkSurrogate)obj).CmdSetString(reader.ReadString());
		}
	}

	protected static void InvokeCmdCmdSetTrigger(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetTrigger called on client.");
		}
		else
		{
			((NetworkSurrogate)obj).CmdSetTrigger(reader.ReadBoolean());
		}
	}

	public void CallCmdSetBool(bool value)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetBool called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetBool(value);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetBool);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(value);
		SendCommandInternal(networkWriter, 0, "CmdSetBool");
	}

	public void CallCmdSetInt(int value)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetInt called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetInt(value);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetInt);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)value);
		SendCommandInternal(networkWriter, 0, "CmdSetInt");
	}

	public void CallCmdSetFloat(float value)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetFloat called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetFloat(value);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetFloat);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(value);
		SendCommandInternal(networkWriter, 0, "CmdSetFloat");
	}

	public void CallCmdSetString(string value)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetString called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetString(value);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetString);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(value);
		SendCommandInternal(networkWriter, 0, "CmdSetString");
	}

	public void CallCmdSetTrigger(bool value)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetTrigger called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetTrigger(value);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetTrigger);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(value);
		SendCommandInternal(networkWriter, 0, "CmdSetTrigger");
	}

	protected static void InvokeRpcRpcSetTrigger(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetTrigger called on server.");
		}
		else
		{
			((NetworkSurrogate)obj).RpcSetTrigger(reader.ReadBoolean());
		}
	}

	public void CallRpcSetTrigger(bool value)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSetTrigger called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSetTrigger);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(value);
		SendRPCInternal(networkWriter, 0, "RpcSetTrigger");
	}

	static NetworkSurrogate()
	{
		kCmdCmdSetBool = -780720286;
		NetworkBehaviour.RegisterCommandDelegate(typeof(NetworkSurrogate), kCmdCmdSetBool, InvokeCmdCmdSetBool);
		kCmdCmdSetInt = -1272103817;
		NetworkBehaviour.RegisterCommandDelegate(typeof(NetworkSurrogate), kCmdCmdSetInt, InvokeCmdCmdSetInt);
		kCmdCmdSetFloat = 1571079396;
		NetworkBehaviour.RegisterCommandDelegate(typeof(NetworkSurrogate), kCmdCmdSetFloat, InvokeCmdCmdSetFloat);
		kCmdCmdSetString = 1838485129;
		NetworkBehaviour.RegisterCommandDelegate(typeof(NetworkSurrogate), kCmdCmdSetString, InvokeCmdCmdSetString);
		kCmdCmdSetTrigger = 1980331584;
		NetworkBehaviour.RegisterCommandDelegate(typeof(NetworkSurrogate), kCmdCmdSetTrigger, InvokeCmdCmdSetTrigger);
		kRpcRpcSetTrigger = -155892074;
		NetworkBehaviour.RegisterRpcDelegate(typeof(NetworkSurrogate), kRpcRpcSetTrigger, InvokeRpcRpcSetTrigger);
		NetworkCRC.RegisterBehaviour("NetworkSurrogate", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(boolVal);
			writer.WritePackedUInt32((uint)intVal);
			writer.Write(floatVal);
			writer.Write(stringVal);
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
			writer.Write(boolVal);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)intVal);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(floatVal);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(stringVal);
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
			boolVal = reader.ReadBoolean();
			intVal = (int)reader.ReadPackedUInt32();
			floatVal = reader.ReadSingle();
			stringVal = reader.ReadString();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			boolVal = reader.ReadBoolean();
		}
		if ((num & 2) != 0)
		{
			intVal = (int)reader.ReadPackedUInt32();
		}
		if ((num & 4) != 0)
		{
			floatVal = reader.ReadSingle();
		}
		if ((num & 8) != 0)
		{
			stringVal = reader.ReadString();
		}
	}

	public override void PreStartClient()
	{
	}
}
