using System.Runtime.InteropServices;
using GameEvent;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class playerJoinIndicator : NetworkBehaviour, IGameEventListener
{
	private enum IndicatorState
	{
		PRESSA,
		CHOOSECHARACTER,
		PICKLEVEL,
		READY
	}

	public Text Press;

	public Text PickLevel;

	public Text ChooseCharacter;

	public Text AnimalName;

	public Text Ready;

	public Text AnimalNameShadow;

	public Text Joining;

	public MultiControllerButton multiControllerButton;

	[SyncVar]
	private Color color = Color.white;

	[SyncVar]
	private Character.Animals animal;

	[SyncVar]
	private IndicatorState state;

	public bool altSkin;

	private static int kCmdCmdPressEnabled;

	private static int kRpcRpcPressEnabled;

	private static int kCmdCmdChooseCharacterEnabled;

	private static int kRpcRpcChooseCharacterEnabled;

	private static int kCmdCmdPickLevelEnabled;

	private static int kRpcRpcPickLevelEnabled;

	private static int kCmdCmdReadyEnabled;

	private static int kRpcRpcReadyEnabled;

	private static int kCmdCmdsetAnimalName;

	private static int kRpcRpcsetAnimalName;

	private static int kCmdCmdsetTintColor;

	private static int kRpcRpcsetTintColor;

	public Color Networkcolor
	{
		get
		{
			return color;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref color, 1u);
		}
	}

	public Character.Animals Networkanimal
	{
		get
		{
			return animal;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref animal, 2u);
		}
	}

	public IndicatorState Networkstate
	{
		get
		{
			return state;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref state, 4u);
		}
	}

	public void Awake()
	{
		PressEnabled();
	}

	private void Start()
	{
		switch (state)
		{
		case IndicatorState.PRESSA:
			PressEnabled();
			break;
		case IndicatorState.CHOOSECHARACTER:
			ChooseCharacterEnabled();
			break;
		case IndicatorState.PICKLEVEL:
			PickLevelEnabled();
			break;
		case IndicatorState.READY:
			ReadyEnabled();
			break;
		}
		setTintColor(color);
		ChangeListener(adding: true);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<LanguageChangeEvent>(this, adding);
	}

	private void DisableAll()
	{
		Press.enabled = false;
		PickLevel.enabled = false;
		ChooseCharacter.enabled = false;
		AnimalName.enabled = false;
		AnimalNameShadow.enabled = false;
		Ready.enabled = false;
		multiControllerButton.Hidden = true;
		Joining.enabled = false;
	}

	public void PressEnabled()
	{
		DisableAll();
		Press.enabled = true;
		multiControllerButton.Hidden = false;
		if (base.hasAuthority)
		{
			CallCmdPressEnabled();
		}
	}

	[Command]
	private void CmdPressEnabled()
	{
		Networkstate = IndicatorState.PRESSA;
		CallRpcPressEnabled();
	}

	[ClientRpc]
	private void RpcPressEnabled()
	{
		if (!base.hasAuthority)
		{
			PressEnabled();
		}
	}

	public void ChooseCharacterEnabled()
	{
		DisableAll();
		ChooseCharacter.enabled = true;
		multiControllerButton.Hidden = true;
		if (base.hasAuthority)
		{
			CallCmdChooseCharacterEnabled();
		}
	}

	[Command]
	private void CmdChooseCharacterEnabled()
	{
		Networkstate = IndicatorState.CHOOSECHARACTER;
		CallRpcChooseCharacterEnabled();
	}

	[ClientRpc]
	private void RpcChooseCharacterEnabled()
	{
		if (!base.hasAuthority)
		{
			ChooseCharacterEnabled();
		}
	}

	public void PickLevelEnabled()
	{
		DisableAll();
		PickLevel.enabled = true;
		AnimalName.enabled = true;
		AnimalNameShadow.enabled = true;
		if (base.hasAuthority)
		{
			CallCmdPickLevelEnabled();
		}
	}

	[Command]
	private void CmdPickLevelEnabled()
	{
		Networkstate = IndicatorState.PICKLEVEL;
		CallRpcPickLevelEnabled();
	}

	[ClientRpc]
	private void RpcPickLevelEnabled()
	{
		if (!base.hasAuthority)
		{
			PickLevelEnabled();
		}
	}

	public void ReadyEnabled()
	{
		DisableAll();
		Ready.enabled = true;
		AnimalName.enabled = true;
		AnimalNameShadow.enabled = true;
		if (base.hasAuthority)
		{
			CallCmdReadyEnabled();
		}
	}

	[Command]
	private void CmdReadyEnabled()
	{
		Networkstate = IndicatorState.READY;
		CallRpcReadyEnabled();
	}

	[ClientRpc]
	private void RpcReadyEnabled()
	{
		if (!base.hasAuthority)
		{
			ReadyEnabled();
		}
	}

	public void setAnimalName(Character.Animals animal, bool altSkin)
	{
		string localizedAnimal = Character.GetLocalizedAnimal(animal, altSkin);
		AnimalName.text = localizedAnimal;
		AnimalNameShadow.text = localizedAnimal;
		this.altSkin = altSkin;
		if (base.hasAuthority)
		{
			CallCmdsetAnimalName(animal, altSkin);
		}
	}

	[Command]
	private void CmdsetAnimalName(Character.Animals animal, bool altSkin)
	{
		Networkanimal = animal;
		CallRpcsetAnimalName(animal, altSkin);
	}

	[ClientRpc]
	private void RpcsetAnimalName(Character.Animals animal, bool altSkin)
	{
		if (!base.hasAuthority)
		{
			setAnimalName(animal, altSkin);
		}
	}

	public void setTintColor(Color color)
	{
		Press.color = color;
		PickLevel.color = color;
		ChooseCharacter.color = color;
		AnimalName.color = color;
		Ready.color = color;
		if (base.hasAuthority)
		{
			CallCmdsetTintColor(color);
		}
	}

	[Command]
	private void CmdsetTintColor(Color color)
	{
		Networkcolor = color;
		CallRpcsetTintColor(color);
	}

	[ClientRpc]
	private void RpcsetTintColor(Color color)
	{
		if (!base.hasAuthority)
		{
			setTintColor(color);
		}
	}

	public void localJoining()
	{
		DisableAll();
		Joining.enabled = true;
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e.GetType() == typeof(LanguageChangeEvent) && animal != Character.Animals.NONE)
		{
			setAnimalName(animal, altSkin);
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdPressEnabled(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPressEnabled called on client.");
		}
		else
		{
			((playerJoinIndicator)obj).CmdPressEnabled();
		}
	}

	protected static void InvokeCmdCmdChooseCharacterEnabled(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChooseCharacterEnabled called on client.");
		}
		else
		{
			((playerJoinIndicator)obj).CmdChooseCharacterEnabled();
		}
	}

	protected static void InvokeCmdCmdPickLevelEnabled(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPickLevelEnabled called on client.");
		}
		else
		{
			((playerJoinIndicator)obj).CmdPickLevelEnabled();
		}
	}

	protected static void InvokeCmdCmdReadyEnabled(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdReadyEnabled called on client.");
		}
		else
		{
			((playerJoinIndicator)obj).CmdReadyEnabled();
		}
	}

	protected static void InvokeCmdCmdsetAnimalName(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdsetAnimalName called on client.");
		}
		else
		{
			((playerJoinIndicator)obj).CmdsetAnimalName((Character.Animals)reader.ReadInt32(), reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdsetTintColor(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdsetTintColor called on client.");
		}
		else
		{
			((playerJoinIndicator)obj).CmdsetTintColor(reader.ReadColor());
		}
	}

	public void CallCmdPressEnabled()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdPressEnabled called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdPressEnabled();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdPressEnabled);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdPressEnabled");
	}

	public void CallCmdChooseCharacterEnabled()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdChooseCharacterEnabled called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdChooseCharacterEnabled();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdChooseCharacterEnabled);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdChooseCharacterEnabled");
	}

	public void CallCmdPickLevelEnabled()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdPickLevelEnabled called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdPickLevelEnabled();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdPickLevelEnabled);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdPickLevelEnabled");
	}

	public void CallCmdReadyEnabled()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdReadyEnabled called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdReadyEnabled();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdReadyEnabled);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdReadyEnabled");
	}

	public void CallCmdsetAnimalName(Character.Animals animal, bool altSkin)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdsetAnimalName called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdsetAnimalName(animal, altSkin);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdsetAnimalName);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write((int)animal);
		networkWriter.Write(altSkin);
		SendCommandInternal(networkWriter, 0, "CmdsetAnimalName");
	}

	public void CallCmdsetTintColor(Color color)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdsetTintColor called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdsetTintColor(color);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdsetTintColor);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(color);
		SendCommandInternal(networkWriter, 0, "CmdsetTintColor");
	}

	protected static void InvokeRpcRpcPressEnabled(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPressEnabled called on server.");
		}
		else
		{
			((playerJoinIndicator)obj).RpcPressEnabled();
		}
	}

	protected static void InvokeRpcRpcChooseCharacterEnabled(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcChooseCharacterEnabled called on server.");
		}
		else
		{
			((playerJoinIndicator)obj).RpcChooseCharacterEnabled();
		}
	}

	protected static void InvokeRpcRpcPickLevelEnabled(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPickLevelEnabled called on server.");
		}
		else
		{
			((playerJoinIndicator)obj).RpcPickLevelEnabled();
		}
	}

	protected static void InvokeRpcRpcReadyEnabled(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcReadyEnabled called on server.");
		}
		else
		{
			((playerJoinIndicator)obj).RpcReadyEnabled();
		}
	}

	protected static void InvokeRpcRpcsetAnimalName(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcsetAnimalName called on server.");
		}
		else
		{
			((playerJoinIndicator)obj).RpcsetAnimalName((Character.Animals)reader.ReadInt32(), reader.ReadBoolean());
		}
	}

	protected static void InvokeRpcRpcsetTintColor(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcsetTintColor called on server.");
		}
		else
		{
			((playerJoinIndicator)obj).RpcsetTintColor(reader.ReadColor());
		}
	}

	public void CallRpcPressEnabled()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcPressEnabled called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcPressEnabled);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcPressEnabled");
	}

	public void CallRpcChooseCharacterEnabled()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcChooseCharacterEnabled called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcChooseCharacterEnabled);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcChooseCharacterEnabled");
	}

	public void CallRpcPickLevelEnabled()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcPickLevelEnabled called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcPickLevelEnabled);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcPickLevelEnabled");
	}

	public void CallRpcReadyEnabled()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcReadyEnabled called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcReadyEnabled);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcReadyEnabled");
	}

	public void CallRpcsetAnimalName(Character.Animals animal, bool altSkin)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcsetAnimalName called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcsetAnimalName);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write((int)animal);
		networkWriter.Write(altSkin);
		SendRPCInternal(networkWriter, 0, "RpcsetAnimalName");
	}

	public void CallRpcsetTintColor(Color color)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcsetTintColor called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcsetTintColor);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(color);
		SendRPCInternal(networkWriter, 0, "RpcsetTintColor");
	}

	static playerJoinIndicator()
	{
		kCmdCmdPressEnabled = -487772286;
		NetworkBehaviour.RegisterCommandDelegate(typeof(playerJoinIndicator), kCmdCmdPressEnabled, InvokeCmdCmdPressEnabled);
		kCmdCmdChooseCharacterEnabled = 413114643;
		NetworkBehaviour.RegisterCommandDelegate(typeof(playerJoinIndicator), kCmdCmdChooseCharacterEnabled, InvokeCmdCmdChooseCharacterEnabled);
		kCmdCmdPickLevelEnabled = 1414972898;
		NetworkBehaviour.RegisterCommandDelegate(typeof(playerJoinIndicator), kCmdCmdPickLevelEnabled, InvokeCmdCmdPickLevelEnabled);
		kCmdCmdReadyEnabled = 1355951650;
		NetworkBehaviour.RegisterCommandDelegate(typeof(playerJoinIndicator), kCmdCmdReadyEnabled, InvokeCmdCmdReadyEnabled);
		kCmdCmdsetAnimalName = 932758341;
		NetworkBehaviour.RegisterCommandDelegate(typeof(playerJoinIndicator), kCmdCmdsetAnimalName, InvokeCmdCmdsetAnimalName);
		kCmdCmdsetTintColor = 793632970;
		NetworkBehaviour.RegisterCommandDelegate(typeof(playerJoinIndicator), kCmdCmdsetTintColor, InvokeCmdCmdsetTintColor);
		kRpcRpcPressEnabled = -404340136;
		NetworkBehaviour.RegisterRpcDelegate(typeof(playerJoinIndicator), kRpcRpcPressEnabled, InvokeRpcRpcPressEnabled);
		kRpcRpcChooseCharacterEnabled = -2045271959;
		NetworkBehaviour.RegisterRpcDelegate(typeof(playerJoinIndicator), kRpcRpcChooseCharacterEnabled, InvokeRpcRpcChooseCharacterEnabled);
		kRpcRpcPickLevelEnabled = 1044282808;
		NetworkBehaviour.RegisterRpcDelegate(typeof(playerJoinIndicator), kRpcRpcPickLevelEnabled, InvokeRpcRpcPickLevelEnabled);
		kRpcRpcReadyEnabled = 1439383800;
		NetworkBehaviour.RegisterRpcDelegate(typeof(playerJoinIndicator), kRpcRpcReadyEnabled, InvokeRpcRpcReadyEnabled);
		kRpcRpcsetAnimalName = -775812305;
		NetworkBehaviour.RegisterRpcDelegate(typeof(playerJoinIndicator), kRpcRpcsetAnimalName, InvokeRpcRpcsetAnimalName);
		kRpcRpcsetTintColor = 877065120;
		NetworkBehaviour.RegisterRpcDelegate(typeof(playerJoinIndicator), kRpcRpcsetTintColor, InvokeRpcRpcsetTintColor);
		NetworkCRC.RegisterBehaviour("playerJoinIndicator", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(color);
			writer.Write((int)animal);
			writer.Write((int)state);
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
			writer.Write(color);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write((int)animal);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write((int)state);
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
			color = reader.ReadColor();
			animal = (Character.Animals)reader.ReadInt32();
			state = (IndicatorState)reader.ReadInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			color = reader.ReadColor();
		}
		if ((num & 2) != 0)
		{
			animal = (Character.Animals)reader.ReadInt32();
		}
		if ((num & 4) != 0)
		{
			state = (IndicatorState)reader.ReadInt32();
		}
	}

	public override void PreStartClient()
	{
	}
}
