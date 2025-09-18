using System.Collections;
using System.Runtime.InteropServices;
using GameEvent;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class GamePlayer : NetworkBehaviour
{
	public LobbyCursor CursorPrefab;

	public Character CharacterPrefab;

	[SyncVar]
	private NetworkInstanceId characterNetID;

	[SyncVar]
	private NetworkInstanceId cursorNetID;

	public Cursor CursorInstance;

	public PartyPickCursor PartyPickCursor;

	public Character CharacterInstance;

	public Player LocalPlayer;

	public Controller Control;

	public bool WasKicked;

	[SyncVar]
	public int networkNumber;

	[SyncVar]
	public int localNumber;

	[SyncVar]
	public Character.Animals PickedAnimal;

	[SyncVar(hook = "OnNameChanged")]
	public string playerName;

	public SyncListInt characterOutfitsList;

	[SyncVar]
	public Color PlayerColor;

	[SyncVar]
	public GameControl.GamePhase InPhase;

	[SyncVar]
	public int Handicap = 1;

	public int TurnOrder;

	public bool SceneInitDone;

	public bool SetupStartDone;

	public bool Initialized;

	[SyncVar]
	private bool InitializedByLocalPlayer;

	public int lives;

	public GameControl.GamePhase currentFreePlayPhase;

	private static int kListcharacterOutfitsList;

	private static int kCmdCmdSetInPhase;

	private static int kCmdCmdSetPlayerNumber;

	private static int kCmdCmdAssignCursor;

	private static int kRpcRpcAssignCursor;

	private static int kCmdCmdRemoveCursor;

	private static int kRpcRpcRemoveCursor;

	private static int kCmdCmdAssignCharacter;

	private static int kRpcRpcAssignCharacter;

	private static int kCmdCmdSetPlayerName;

	private static int kCmdCmdSetPlayerHandicap;

	private static int kCmdCmdSetSceneInitDone;

	private static int kCmdCmdSetupStartDone;

	private static int kCmdCmdSetInitializedByLocalPlayer;

	private static int kCmdCmdSetCurrentFreePlayPhase;

	private static int kCmdCmdRemovePlayer;

	private static int kRpcRpcRemovePlayer;

	private static int kRpcRpcKillPlayer;

	public string ValidatedDisplayName { get; private set; } = "";

	private bool MagicalUnityEventHappened
	{
		get
		{
			if (!InitializedByLocalPlayer)
			{
				return base.connectionToServer != null;
			}
			return true;
		}
	}

	public bool IsLocalPlayer
	{
		get
		{
			if (!Initialized)
			{
				Debug.LogError("GamePlayer.IsLocalPlayer: GamePlayer was not initialized");
				return false;
			}
			if (LobbyManager.instance.IsInOnlineGame)
			{
				return base.isLocalPlayer;
			}
			return true;
		}
	}

	public bool IsWearingSkin
	{
		get
		{
			if (characterOutfitsList != null && characterOutfitsList.Count > 3)
			{
				return characterOutfitsList[3] != -1;
			}
			return false;
		}
	}

	public NetworkInstanceId NetworkcharacterNetID
	{
		get
		{
			return characterNetID;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref characterNetID, 1u);
		}
	}

	public NetworkInstanceId NetworkcursorNetID
	{
		get
		{
			return cursorNetID;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref cursorNetID, 2u);
		}
	}

	public int NetworknetworkNumber
	{
		get
		{
			return networkNumber;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref networkNumber, 4u);
		}
	}

	public int NetworklocalNumber
	{
		get
		{
			return localNumber;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref localNumber, 8u);
		}
	}

	public Character.Animals NetworkPickedAnimal
	{
		get
		{
			return PickedAnimal;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref PickedAnimal, 16u);
		}
	}

	public string NetworkplayerName
	{
		get
		{
			return playerName;
		}
		[param: In]
		set
		{
			ref string fieldValue = ref playerName;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				OnNameChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 32u);
		}
	}

	public Color NetworkPlayerColor
	{
		get
		{
			return PlayerColor;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref PlayerColor, 128u);
		}
	}

	public GameControl.GamePhase NetworkInPhase
	{
		get
		{
			return InPhase;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref InPhase, 256u);
		}
	}

	public int NetworkHandicap
	{
		get
		{
			return Handicap;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref Handicap, 512u);
		}
	}

	public bool NetworkInitializedByLocalPlayer
	{
		get
		{
			return InitializedByLocalPlayer;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref InitializedByLocalPlayer, 1024u);
		}
	}

	private void Awake()
	{
		characterOutfitsList = new SyncListInt();
		characterOutfitsList.InitializeBehaviour(this, kListcharacterOutfitsList);
	}

	private void Start()
	{
		Object.DontDestroyOnLoad(base.gameObject);
		StartCoroutine(InitPlayer());
	}

	private IEnumerator InitPlayer()
	{
		if (!LobbyManager.instance.IsHost)
		{
			int framesWaited = 0;
			if (!MagicalUnityEventHappened)
			{
				Debug.Log("GamePlayer.InitPlayer(): Waiting for magical Unity event.");
				while (!MagicalUnityEventHappened)
				{
					framesWaited++;
					yield return null;
				}
				Debug.Log("GamePlayer: Magical Unity event happened - waited " + framesWaited + " frames.");
			}
			else
			{
				Debug.Log("GamePlayer: Did not need to wait for magical Unity event.");
			}
		}
		if (base.isLocalPlayer || !LobbyManager.instance.IsInOnlineGame)
		{
			PlayerManager instance = PlayerManager.GetInstance();
			NetworklocalNumber = 0;
			foreach (Player item in instance)
			{
				if (item != null && item.AssociatedLobbyPlayer.networkNumber == networkNumber)
				{
					NetworklocalNumber = item.Number;
					CallCmdSetPlayerNumber(item.Number);
					LocalPlayer = item;
					LocalPlayer.AssociatedGamePlayer = this;
					Debug.Log("Game player associated with local player " + localNumber);
					break;
				}
			}
			if (localNumber == 0)
			{
				Debug.LogError("Could not find local player for networked game player.");
			}
			else
			{
				LocalPlayer = instance.GetPlayer(localNumber);
				CallCmdSetPlayerName(LocalPlayer.AssociatedLobbyPlayer.playerName);
				CallCmdSetPlayerHandicap(LocalPlayer.AssociatedLobbyPlayer.handicap);
				LocalPlayer.AssociatedLobbyPlayer.EmoteSystem.GamePlayer = this;
				LocalPlayer.AssociatedLobbyPlayer.EmoteSystem.characterPortrait.sprite = CharacterSpriteManager.GetInstance().GetCharaterPortrait(LocalPlayer.AssociatedLobbyPlayer.PickedAnimal);
				Control = LocalPlayer.UseController;
				CallCmdSetInitializedByLocalPlayer();
			}
		}
		else
		{
			GameObject gameObject = ClientScene.FindLocalObject(characterNetID);
			if (gameObject != null)
			{
				Character component = gameObject.GetComponent<Character>();
				if (component != null)
				{
					CharacterInstance = component;
					CharacterInstance.AssociatedGamePlayer = this;
				}
			}
			GameObject gameObject2 = ClientScene.FindLocalObject(cursorNetID);
			if (gameObject2 != null)
			{
				Cursor component2 = gameObject2.GetComponent<Cursor>();
				if (component2 != null)
				{
					CursorInstance = component2;
					CursorInstance.AssociatedGamePlayer = this;
				}
			}
		}
		Initialized = true;
	}

	public void SetInPhase(GameControl.GamePhase phase)
	{
		CallCmdSetInPhase(phase);
	}

	[Command]
	private void CmdSetInPhase(GameControl.GamePhase phase)
	{
		NetworkInPhase = phase;
	}

	[Command]
	private void CmdSetPlayerNumber(int number)
	{
		NetworklocalNumber = number;
	}

	[Command]
	public void CmdAssignCursor(GameObject cursorObj, int networkNumber, int localNumber)
	{
		if (!(cursorObj != null))
		{
			return;
		}
		Cursor component = cursorObj.GetComponent<Cursor>();
		if (!(component != null))
		{
			return;
		}
		component.NetworknetworkNumber = networkNumber;
		if (component.GetType() == typeof(PartyPickCursor))
		{
			PartyPickCursor = (PartyPickCursor)component;
		}
		else
		{
			CursorInstance = component;
			cursorObj.GetComponent<NetworkIdentity>().AssignClientAuthority(base.connectionToClient);
			NetworkcursorNetID = cursorObj.GetComponent<NetworkIdentity>().netId;
			component.CallCmdSetLocalPlayerID(localNumber);
			if (IsLocalPlayer && Control != null)
			{
				CursorInstance.SetLocalController(Control);
			}
		}
		CallRpcAssignCursor(cursorObj, networkNumber, localNumber);
	}

	[ClientRpc]
	private void RpcAssignCursor(GameObject go, int networkNumber, int localNumber)
	{
		if (networkNumber != this.networkNumber || !(go != null))
		{
			return;
		}
		Cursor component = go.GetComponent<Cursor>();
		component.AssociatedGamePlayer = this;
		if (component.GetType() == typeof(PartyPickCursor))
		{
			PartyPickCursor = (PartyPickCursor)component;
			return;
		}
		CursorInstance = component;
		if (IsLocalPlayer && Control != null)
		{
			CursorInstance.SetLocalController(Control);
		}
	}

	[Command]
	public void CmdRemoveCursor()
	{
		if (CursorInstance != null)
		{
			CursorInstance.NetworknetworkNumber = 0;
			CursorInstance.CallCmdSetLocalPlayerID(0);
			CursorInstance.GetComponent<NetworkIdentity>().RemoveClientAuthority(base.connectionToClient);
			CallRpcRemoveCursor(networkNumber);
			CursorInstance = null;
		}
	}

	[ClientRpc]
	private void RpcRemoveCursor(int networkNumber)
	{
		if (networkNumber == this.networkNumber)
		{
			CursorInstance = null;
			LocalPlayer.PlayerCursor = null;
		}
	}

	[Command]
	public void CmdAssignCharacter(GameObject charObj, int networkNumber, int localNumber)
	{
		if (!(charObj != null))
		{
			return;
		}
		Character component = charObj.GetComponent<Character>();
		if (component != null)
		{
			component.NetworknetworkNumber = networkNumber;
			CharacterInstance = component;
			if (IsLocalPlayer && Control != null)
			{
				CharacterInstance.SetLocalController(Control);
			}
			charObj.GetComponent<NetworkIdentity>().AssignClientAuthority(base.connectionToClient);
			NetworkcharacterNetID = charObj.GetComponent<NetworkIdentity>().netId;
			component.CallCmdSetLocalPlayerID(localNumber);
			component.CallCmdSetPicked(picked: true);
			CallRpcAssignCharacter(charObj, networkNumber, localNumber);
		}
	}

	[ClientRpc]
	private void RpcAssignCharacter(GameObject go, int networkNumber, int localNumber)
	{
		if (networkNumber == this.networkNumber && go != null)
		{
			Character component = go.GetComponent<Character>();
			component.AssociatedGamePlayer = this;
			component.PlayerColor = PlayerColor;
			CharacterInstance = component;
			if (IsLocalPlayer && Control != null)
			{
				CharacterInstance.SetLocalController(Control);
			}
		}
	}

	[Command]
	private void CmdSetPlayerName(string newPlayerName)
	{
		OnNameChanged(newPlayerName);
		NetworkplayerName = newPlayerName;
		NetworkPlayerColor = GameSettings.GetInstance().PlayerColors[networkNumber - 1];
	}

	[Command]
	private void CmdSetPlayerHandicap(int newHandicap)
	{
		NetworkHandicap = newHandicap;
	}

	[Command]
	public void CmdSetSceneInitDone(bool done)
	{
		Debug.Log("Scene Init " + (done ? "" : "not ") + "done for net player " + networkNumber);
		SceneInitDone = done;
	}

	[Command]
	public void CmdSetupStartDone()
	{
		Debug.Log("Setup Start done for net player " + networkNumber);
		SetupStartDone = true;
	}

	[Command]
	private void CmdSetInitializedByLocalPlayer()
	{
		NetworkInitializedByLocalPlayer = true;
	}

	public void RunAfterInitialized(UnityAction onConditionMet)
	{
		this.StartCoroutineWithCondition(() => Initialized, onConditionMet);
	}

	[Command]
	public void CmdSetCurrentFreePlayPhase(GameControl.GamePhase phase)
	{
		currentFreePlayPhase = phase;
	}

	public void RemovePlayer()
	{
		if (base.hasAuthority)
		{
			LocalPlayer.LoggedOut = true;
			CallCmdRemovePlayer();
		}
	}

	[Command]
	private void CmdRemovePlayer()
	{
		CallRpcRemovePlayer();
	}

	[ClientRpc]
	private void RpcRemovePlayer()
	{
		GameEventManager.SendEvent(new GamePlayerRemovedEvent(networkNumber));
	}

	[ClientRpc]
	public void RpcKillPlayer(string deathCause)
	{
		if (CharacterInstance != null)
		{
			CharacterInstance.KillCharacter(deathCause, deathFreezeOn: false, 0);
		}
	}

	private void OnNameChanged(string newName)
	{
		NetworkplayerName = newName;
		ValidatedDisplayName = "";
		ValidatedDisplayName = newName;
		ApplyNameTarget(newName);
	}

	private void ApplyNameTarget(string nameToDisplay)
	{
		if (CharacterInstance != null && CharacterInstance.nameTag != null)
		{
			CharacterInstance.nameTag.setNameBoxText(nameToDisplay, CharacterInstance);
		}
	}

	public GamePlayer()
	{
		characterOutfitsList = new SyncListInt();
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeSyncListcharacterOutfitsList(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("SyncList characterOutfitsList called on server.");
		}
		else
		{
			((GamePlayer)obj).characterOutfitsList.HandleMsg(reader);
		}
	}

	protected static void InvokeCmdCmdSetInPhase(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetInPhase called on client.");
		}
		else
		{
			((GamePlayer)obj).CmdSetInPhase((GameControl.GamePhase)reader.ReadInt32());
		}
	}

	protected static void InvokeCmdCmdSetPlayerNumber(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetPlayerNumber called on client.");
		}
		else
		{
			((GamePlayer)obj).CmdSetPlayerNumber((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdAssignCursor(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAssignCursor called on client.");
		}
		else
		{
			((GamePlayer)obj).CmdAssignCursor(reader.ReadGameObject(), (int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdRemoveCursor(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRemoveCursor called on client.");
		}
		else
		{
			((GamePlayer)obj).CmdRemoveCursor();
		}
	}

	protected static void InvokeCmdCmdAssignCharacter(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAssignCharacter called on client.");
		}
		else
		{
			((GamePlayer)obj).CmdAssignCharacter(reader.ReadGameObject(), (int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdSetPlayerName(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetPlayerName called on client.");
		}
		else
		{
			((GamePlayer)obj).CmdSetPlayerName(reader.ReadString());
		}
	}

	protected static void InvokeCmdCmdSetPlayerHandicap(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetPlayerHandicap called on client.");
		}
		else
		{
			((GamePlayer)obj).CmdSetPlayerHandicap((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdSetSceneInitDone(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetSceneInitDone called on client.");
		}
		else
		{
			((GamePlayer)obj).CmdSetSceneInitDone(reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdSetupStartDone(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetupStartDone called on client.");
		}
		else
		{
			((GamePlayer)obj).CmdSetupStartDone();
		}
	}

	protected static void InvokeCmdCmdSetInitializedByLocalPlayer(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetInitializedByLocalPlayer called on client.");
		}
		else
		{
			((GamePlayer)obj).CmdSetInitializedByLocalPlayer();
		}
	}

	protected static void InvokeCmdCmdSetCurrentFreePlayPhase(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetCurrentFreePlayPhase called on client.");
		}
		else
		{
			((GamePlayer)obj).CmdSetCurrentFreePlayPhase((GameControl.GamePhase)reader.ReadInt32());
		}
	}

	protected static void InvokeCmdCmdRemovePlayer(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRemovePlayer called on client.");
		}
		else
		{
			((GamePlayer)obj).CmdRemovePlayer();
		}
	}

	public void CallCmdSetInPhase(GameControl.GamePhase phase)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetInPhase called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetInPhase(phase);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetInPhase);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write((int)phase);
		SendCommandInternal(networkWriter, 0, "CmdSetInPhase");
	}

	public void CallCmdSetPlayerNumber(int number)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetPlayerNumber called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetPlayerNumber(number);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetPlayerNumber);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)number);
		SendCommandInternal(networkWriter, 0, "CmdSetPlayerNumber");
	}

	public void CallCmdAssignCursor(GameObject cursorObj, int networkNumber, int localNumber)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdAssignCursor called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdAssignCursor(cursorObj, networkNumber, localNumber);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdAssignCursor);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(cursorObj);
		networkWriter.WritePackedUInt32((uint)networkNumber);
		networkWriter.WritePackedUInt32((uint)localNumber);
		SendCommandInternal(networkWriter, 0, "CmdAssignCursor");
	}

	public void CallCmdRemoveCursor()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRemoveCursor called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRemoveCursor();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRemoveCursor);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdRemoveCursor");
	}

	public void CallCmdAssignCharacter(GameObject charObj, int networkNumber, int localNumber)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdAssignCharacter called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdAssignCharacter(charObj, networkNumber, localNumber);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdAssignCharacter);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(charObj);
		networkWriter.WritePackedUInt32((uint)networkNumber);
		networkWriter.WritePackedUInt32((uint)localNumber);
		SendCommandInternal(networkWriter, 0, "CmdAssignCharacter");
	}

	public void CallCmdSetPlayerName(string newPlayerName)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetPlayerName called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetPlayerName(newPlayerName);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetPlayerName);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(newPlayerName);
		SendCommandInternal(networkWriter, 0, "CmdSetPlayerName");
	}

	public void CallCmdSetPlayerHandicap(int newHandicap)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetPlayerHandicap called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetPlayerHandicap(newHandicap);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetPlayerHandicap);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)newHandicap);
		SendCommandInternal(networkWriter, 0, "CmdSetPlayerHandicap");
	}

	public void CallCmdSetSceneInitDone(bool done)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetSceneInitDone called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetSceneInitDone(done);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetSceneInitDone);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(done);
		SendCommandInternal(networkWriter, 0, "CmdSetSceneInitDone");
	}

	public void CallCmdSetupStartDone()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetupStartDone called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetupStartDone();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetupStartDone);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdSetupStartDone");
	}

	public void CallCmdSetInitializedByLocalPlayer()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetInitializedByLocalPlayer called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetInitializedByLocalPlayer();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetInitializedByLocalPlayer);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdSetInitializedByLocalPlayer");
	}

	public void CallCmdSetCurrentFreePlayPhase(GameControl.GamePhase phase)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetCurrentFreePlayPhase called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetCurrentFreePlayPhase(phase);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetCurrentFreePlayPhase);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write((int)phase);
		SendCommandInternal(networkWriter, 0, "CmdSetCurrentFreePlayPhase");
	}

	public void CallCmdRemovePlayer()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRemovePlayer called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRemovePlayer();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRemovePlayer);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdRemovePlayer");
	}

	protected static void InvokeRpcRpcAssignCursor(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAssignCursor called on server.");
		}
		else
		{
			((GamePlayer)obj).RpcAssignCursor(reader.ReadGameObject(), (int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcRemoveCursor(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRemoveCursor called on server.");
		}
		else
		{
			((GamePlayer)obj).RpcRemoveCursor((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcAssignCharacter(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAssignCharacter called on server.");
		}
		else
		{
			((GamePlayer)obj).RpcAssignCharacter(reader.ReadGameObject(), (int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcRemovePlayer(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRemovePlayer called on server.");
		}
		else
		{
			((GamePlayer)obj).RpcRemovePlayer();
		}
	}

	protected static void InvokeRpcRpcKillPlayer(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcKillPlayer called on server.");
		}
		else
		{
			((GamePlayer)obj).RpcKillPlayer(reader.ReadString());
		}
	}

	public void CallRpcAssignCursor(GameObject go, int networkNumber, int localNumber)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcAssignCursor called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcAssignCursor);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(go);
		networkWriter.WritePackedUInt32((uint)networkNumber);
		networkWriter.WritePackedUInt32((uint)localNumber);
		SendRPCInternal(networkWriter, 0, "RpcAssignCursor");
	}

	public void CallRpcRemoveCursor(int networkNumber)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcRemoveCursor called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcRemoveCursor);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)networkNumber);
		SendRPCInternal(networkWriter, 0, "RpcRemoveCursor");
	}

	public void CallRpcAssignCharacter(GameObject go, int networkNumber, int localNumber)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcAssignCharacter called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcAssignCharacter);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(go);
		networkWriter.WritePackedUInt32((uint)networkNumber);
		networkWriter.WritePackedUInt32((uint)localNumber);
		SendRPCInternal(networkWriter, 0, "RpcAssignCharacter");
	}

	public void CallRpcRemovePlayer()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcRemovePlayer called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcRemovePlayer);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcRemovePlayer");
	}

	public void CallRpcKillPlayer(string deathCause)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcKillPlayer called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcKillPlayer);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(deathCause);
		SendRPCInternal(networkWriter, 0, "RpcKillPlayer");
	}

	static GamePlayer()
	{
		kCmdCmdSetInPhase = -754704185;
		NetworkBehaviour.RegisterCommandDelegate(typeof(GamePlayer), kCmdCmdSetInPhase, InvokeCmdCmdSetInPhase);
		kCmdCmdSetPlayerNumber = -979038535;
		NetworkBehaviour.RegisterCommandDelegate(typeof(GamePlayer), kCmdCmdSetPlayerNumber, InvokeCmdCmdSetPlayerNumber);
		kCmdCmdAssignCursor = 1854975128;
		NetworkBehaviour.RegisterCommandDelegate(typeof(GamePlayer), kCmdCmdAssignCursor, InvokeCmdCmdAssignCursor);
		kCmdCmdRemoveCursor = -959029139;
		NetworkBehaviour.RegisterCommandDelegate(typeof(GamePlayer), kCmdCmdRemoveCursor, InvokeCmdCmdRemoveCursor);
		kCmdCmdAssignCharacter = -911441241;
		NetworkBehaviour.RegisterCommandDelegate(typeof(GamePlayer), kCmdCmdAssignCharacter, InvokeCmdCmdAssignCharacter);
		kCmdCmdSetPlayerName = 584436219;
		NetworkBehaviour.RegisterCommandDelegate(typeof(GamePlayer), kCmdCmdSetPlayerName, InvokeCmdCmdSetPlayerName);
		kCmdCmdSetPlayerHandicap = 1630225192;
		NetworkBehaviour.RegisterCommandDelegate(typeof(GamePlayer), kCmdCmdSetPlayerHandicap, InvokeCmdCmdSetPlayerHandicap);
		kCmdCmdSetSceneInitDone = 792998415;
		NetworkBehaviour.RegisterCommandDelegate(typeof(GamePlayer), kCmdCmdSetSceneInitDone, InvokeCmdCmdSetSceneInitDone);
		kCmdCmdSetupStartDone = -1721464902;
		NetworkBehaviour.RegisterCommandDelegate(typeof(GamePlayer), kCmdCmdSetupStartDone, InvokeCmdCmdSetupStartDone);
		kCmdCmdSetInitializedByLocalPlayer = 383731088;
		NetworkBehaviour.RegisterCommandDelegate(typeof(GamePlayer), kCmdCmdSetInitializedByLocalPlayer, InvokeCmdCmdSetInitializedByLocalPlayer);
		kCmdCmdSetCurrentFreePlayPhase = -1077327151;
		NetworkBehaviour.RegisterCommandDelegate(typeof(GamePlayer), kCmdCmdSetCurrentFreePlayPhase, InvokeCmdCmdSetCurrentFreePlayPhase);
		kCmdCmdRemovePlayer = -595662856;
		NetworkBehaviour.RegisterCommandDelegate(typeof(GamePlayer), kCmdCmdRemovePlayer, InvokeCmdCmdRemovePlayer);
		kRpcRpcAssignCursor = 1938407278;
		NetworkBehaviour.RegisterRpcDelegate(typeof(GamePlayer), kRpcRpcAssignCursor, InvokeRpcRpcAssignCursor);
		kRpcRpcRemoveCursor = -875596989;
		NetworkBehaviour.RegisterRpcDelegate(typeof(GamePlayer), kRpcRpcRemoveCursor, InvokeRpcRpcRemoveCursor);
		kRpcRpcAssignCharacter = 2124642321;
		NetworkBehaviour.RegisterRpcDelegate(typeof(GamePlayer), kRpcRpcAssignCharacter, InvokeRpcRpcAssignCharacter);
		kRpcRpcRemovePlayer = -512230706;
		NetworkBehaviour.RegisterRpcDelegate(typeof(GamePlayer), kRpcRpcRemovePlayer, InvokeRpcRpcRemovePlayer);
		kRpcRpcKillPlayer = -851627928;
		NetworkBehaviour.RegisterRpcDelegate(typeof(GamePlayer), kRpcRpcKillPlayer, InvokeRpcRpcKillPlayer);
		kListcharacterOutfitsList = 116537018;
		NetworkBehaviour.RegisterSyncListDelegate(typeof(GamePlayer), kListcharacterOutfitsList, InvokeSyncListcharacterOutfitsList);
		NetworkCRC.RegisterBehaviour("GamePlayer", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(characterNetID);
			writer.Write(cursorNetID);
			writer.WritePackedUInt32((uint)networkNumber);
			writer.WritePackedUInt32((uint)localNumber);
			writer.Write((int)PickedAnimal);
			writer.Write(playerName);
			SyncListInt.WriteInstance(writer, characterOutfitsList);
			writer.Write(PlayerColor);
			writer.Write((int)InPhase);
			writer.WritePackedUInt32((uint)Handicap);
			writer.Write(InitializedByLocalPlayer);
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
			writer.Write(characterNetID);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(cursorNetID);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)networkNumber);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)localNumber);
		}
		if ((base.syncVarDirtyBits & 0x10) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write((int)PickedAnimal);
		}
		if ((base.syncVarDirtyBits & 0x20) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(playerName);
		}
		if ((base.syncVarDirtyBits & 0x40) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			SyncListInt.WriteInstance(writer, characterOutfitsList);
		}
		if ((base.syncVarDirtyBits & 0x80) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(PlayerColor);
		}
		if ((base.syncVarDirtyBits & 0x100) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write((int)InPhase);
		}
		if ((base.syncVarDirtyBits & 0x200) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)Handicap);
		}
		if ((base.syncVarDirtyBits & 0x400) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(InitializedByLocalPlayer);
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
			characterNetID = reader.ReadNetworkId();
			cursorNetID = reader.ReadNetworkId();
			networkNumber = (int)reader.ReadPackedUInt32();
			localNumber = (int)reader.ReadPackedUInt32();
			PickedAnimal = (Character.Animals)reader.ReadInt32();
			playerName = reader.ReadString();
			SyncListInt.ReadReference(reader, characterOutfitsList);
			PlayerColor = reader.ReadColor();
			InPhase = (GameControl.GamePhase)reader.ReadInt32();
			Handicap = (int)reader.ReadPackedUInt32();
			InitializedByLocalPlayer = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			characterNetID = reader.ReadNetworkId();
		}
		if ((num & 2) != 0)
		{
			cursorNetID = reader.ReadNetworkId();
		}
		if ((num & 4) != 0)
		{
			networkNumber = (int)reader.ReadPackedUInt32();
		}
		if ((num & 8) != 0)
		{
			localNumber = (int)reader.ReadPackedUInt32();
		}
		if ((num & 0x10) != 0)
		{
			PickedAnimal = (Character.Animals)reader.ReadInt32();
		}
		if ((num & 0x20) != 0)
		{
			OnNameChanged(reader.ReadString());
		}
		if ((num & 0x40) != 0)
		{
			SyncListInt.ReadReference(reader, characterOutfitsList);
		}
		if ((num & 0x80) != 0)
		{
			PlayerColor = reader.ReadColor();
		}
		if ((num & 0x100) != 0)
		{
			InPhase = (GameControl.GamePhase)reader.ReadInt32();
		}
		if ((num & 0x200) != 0)
		{
			Handicap = (int)reader.ReadPackedUInt32();
		}
		if ((num & 0x400) != 0)
		{
			InitializedByLocalPlayer = reader.ReadBoolean();
		}
	}

	public override void PreStartClient()
	{
	}
}
