using System.Collections;
using System.Runtime.InteropServices;
using GameEvent;
using I2.Loc;
using Steamworks;
using Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class LobbyPlayer : NetworkLobbyPlayer, IGameEventListener, InputReceiver
{
	public enum Status
	{
		INACTIVE,
		CURSOR,
		CHARACTER,
		READY,
		COUCH
	}

	public enum SocialPlatform
	{
		Undefined,
		Steam,
		PSN,
		XboxLive,
		Nintendo,
		Android,
		Origin,
		WeGame
	}

	public LobbyCursor LobbyCursorPrefab;

	public EmoteSystem EmoteSystePrefab;

	[SyncVar]
	public bool LockedForLoad;

	[SyncVar]
	private NetworkInstanceId characterNetID;

	[SyncVar]
	private NetworkInstanceId cursorNetID;

	[SyncVar]
	public ushort playerNodeID;

	private Character characterInstance;

	private Cursor cursorInstance;

	[SyncVar]
	public bool IsHost;

	[SyncVar]
	public string GSID;

	[SyncVar]
	public string platformUniqueID;

	[SyncVar]
	public SocialPlatform platform;

	[SyncVar]
	public bool hasVerifiedSocialAccount;

	public string lastVerifiedSocialAccountCheck;

	public bool inMenu;

	private Character requestedCharacterInstance;

	public bool characterUnpickRequested;

	protected bool realIsLocalPlayer;

	public Player LocalPlayer;

	public EmoteSystem EmoteSystem;

	[SyncVar]
	public int networkNumber;

	[SyncVar]
	public int localNumber;

	[SyncVar]
	public Character.Animals PickedAnimal;

	[SyncVar]
	private Status playerStatus;

	[SyncVar]
	public Color PlayerColor;

	public SyncListInt characterOutfitsList;

	[SyncVar]
	public ulong SteamID;

	[SyncVar]
	public LobbyManager.ConnectionQuality ConnectionQuality;

	[SyncVar]
	public int handicap = 100;

	public bool VotedToKick;

	public bool WasKicked;

	[SyncVar(hook = "OnNameChanged")]
	public string playerName;

	public bool Muted;

	public uint netid;

	[SyncVar]
	public bool MainUser;

	[SyncVar]
	public double SkillMean;

	[SyncVar]
	public double SkillStdDev;

	public bool Initialized;

	[SyncVar]
	private bool InitializedByLocalPlayer;

	private static int kListcharacterOutfitsList;

	private static int kCmdCmdSetMainUser;

	private static int kCmdCmdSetSteamID;

	private static int kCmdCmdSetNodeID;

	private static int kCmdCmdSetPlayerNumber;

	private static int kCmdCmdSetPlayerInfo;

	private static int kCmdCmdSetPlayerHandicap;

	private static int kCmdCmdSetCursorInstance;

	private static int kRpcRpcSetCursorInstance;

	private static int kCmdCmdSetCharacterInstance;

	private static int kRpcRpcSetCharacterInstance;

	private static int kCmdCmdAssignCursor;

	private static int kRpcRpcAssignCursor;

	private static int kCmdCmdRemoveCursor;

	private static int kRpcRpcRemoveCursor;

	private static int kCmdCmdAssignCharacter;

	private static int kRpcRpcAssignCharacter;

	private static int kCmdCmdRemoveCharacter;

	private static int kRpcRpcRemoveCharacter;

	private static int kRpcRpcSwitchToCharacter;

	private static int kCmdCmdSwitchToCursor;

	private static int kRpcRpcSwitchToCursor;

	private static int kRpcRpcClearInstances;

	private static int kCmdCmdSendCharUnpicked;

	private static int kCmdCmdSendJoinMessage;

	private static int kRpcRpcShowJoinMessage;

	private static int kCmdCmdPlayerPickedCharacter;

	private static int kCmdCmdRequestPickCharacter;

	private static int kRpcRpcRequestPickResponse;

	private static int kCmdCmdSetOutfitsFromArray;

	private static int kCmdCmdSetPlayerStatus;

	private static int kCmdCmdSetInitializedByLocalPlayer;

	private static int kCmdCmdSetGSID;

	private static int kCmdCmdSetPlatformUniqueID;

	private static int kCmdCmdSetHasVerifiedSocialAccount;

	private static int kCmdCmdIShouldNotBeHere;

	public static SocialPlatform LocalMachinePlatform => SocialPlatform.Steam;

	public Character CharacterInstance
	{
		get
		{
			return characterInstance;
		}
		set
		{
			characterInstance = value;
			if (base.hasAuthority)
			{
				if (value != null)
				{
					CallCmdSetCharacterInstance(value.netId);
				}
				else
				{
					CallCmdSetCharacterInstance(new NetworkInstanceId(0u));
				}
			}
		}
	}

	public Cursor CursorInstance
	{
		get
		{
			return cursorInstance;
		}
		set
		{
			cursorInstance = value;
			if (base.hasAuthority)
			{
				if (value != null)
				{
					CallCmdSetCursorInstance(value.netId);
				}
				else
				{
					CallCmdSetCursorInstance(new NetworkInstanceId(0u));
				}
			}
		}
	}

	public Status PlayerStatus
	{
		get
		{
			return playerStatus;
		}
		set
		{
			if (base.isLocalPlayer || base.hasAuthority)
			{
				CallCmdSetPlayerStatus(value);
			}
			NetworkplayerStatus = value;
		}
	}

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
				Debug.LogError("LobbyPlayer.IsLocalPlayer: LobbyPlayer was not initialized");
				return base.isLocalPlayer;
			}
			if (LobbyManager.instance.IsInOnlineGame)
			{
				return realIsLocalPlayer;
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

	public bool NetworkLockedForLoad
	{
		get
		{
			return LockedForLoad;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref LockedForLoad, 1u);
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
			SetSyncVar(value, ref characterNetID, 2u);
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
			SetSyncVar(value, ref cursorNetID, 4u);
		}
	}

	public ushort NetworkplayerNodeID
	{
		get
		{
			return playerNodeID;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref playerNodeID, 8u);
		}
	}

	public bool NetworkIsHost
	{
		get
		{
			return IsHost;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref IsHost, 16u);
		}
	}

	public string NetworkGSID
	{
		get
		{
			return GSID;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref GSID, 32u);
		}
	}

	public string NetworkplatformUniqueID
	{
		get
		{
			return platformUniqueID;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref platformUniqueID, 64u);
		}
	}

	public SocialPlatform Networkplatform
	{
		get
		{
			return platform;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref platform, 128u);
		}
	}

	public bool NetworkhasVerifiedSocialAccount
	{
		get
		{
			return hasVerifiedSocialAccount;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref hasVerifiedSocialAccount, 256u);
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
			SetSyncVar(value, ref networkNumber, 512u);
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
			SetSyncVar(value, ref localNumber, 1024u);
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
			SetSyncVar(value, ref PickedAnimal, 2048u);
		}
	}

	public Status NetworkplayerStatus
	{
		get
		{
			return playerStatus;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref playerStatus, 4096u);
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
			SetSyncVar(value, ref PlayerColor, 8192u);
		}
	}

	public ulong NetworkSteamID
	{
		get
		{
			return SteamID;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref SteamID, 32768u);
		}
	}

	public LobbyManager.ConnectionQuality NetworkConnectionQuality
	{
		get
		{
			return ConnectionQuality;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref ConnectionQuality, 65536u);
		}
	}

	public int Networkhandicap
	{
		get
		{
			return handicap;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref handicap, 131072u);
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
			SetSyncVar(value, ref fieldValue, 262144u);
		}
	}

	public bool NetworkMainUser
	{
		get
		{
			return MainUser;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref MainUser, 524288u);
		}
	}

	public double NetworkSkillMean
	{
		get
		{
			return SkillMean;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref SkillMean, 1048576u);
		}
	}

	public double NetworkSkillStdDev
	{
		get
		{
			return SkillStdDev;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref SkillStdDev, 2097152u);
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
			SetSyncVar(value, ref InitializedByLocalPlayer, 4194304u);
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
				Debug.Log("LobbyPlayer.InitPlayer(): Waiting for magical Unity event.");
				while (!MagicalUnityEventHappened)
				{
					framesWaited++;
					yield return null;
				}
				Debug.Log("LobbyPlayer: Magical Unity event happened - waited " + framesWaited + " frames.");
			}
			else
			{
				Debug.Log("LobbyPlayer: Did not need to wait for magical Unity event.");
			}
		}
		base.transform.position = Vector3.zero;
		if (base.isLocalPlayer)
		{
			realIsLocalPlayer = base.isLocalPlayer;
			PlayerManager instance = PlayerManager.GetInstance();
			for (int i = 0; i != GameSettings.GetInstance().MaxPlayers; i++)
			{
				Player player = instance.GetPlayer(i + 1);
				if (player != null && !player.Connected)
				{
					NetworklocalNumber = player.Number;
					CallCmdSetPlayerNumber(player.Number);
					string text = "";
					if (SteamManager.Initialized)
					{
						text = SteamFriends.GetPersonaName();
						LocalSetPlatformUniqueID(SteamUser.GetSteamID().m_SteamID.ToString());
					}
					if (i > 0)
					{
						text = text + " " + (i + 1);
					}
					else
					{
						NetworkMainUser = true;
					}
					NetworkplayerName = text;
					double skillMean = 25.0;
					double skillStdDev = 8.333333333333334;
					SaveFileData saveFileDataForLocalPlayer = StatTracker.Instance.GetSaveFileDataForLocalPlayer(player.Number);
					if (saveFileDataForLocalPlayer != null)
					{
						skillMean = saveFileDataForLocalPlayer.SkillMean;
						skillStdDev = saveFileDataForLocalPlayer.SkillStdDev;
					}
					CallCmdSetPlayerInfo(text, skillMean, skillStdDev, LobbyManager.instance.IsHost, null, LocalMachinePlatform);
					player.Connected = true;
					LocalPlayer = player;
					LocalPlayer.AssociatedLobbyPlayer = this;
					break;
				}
			}
			if (localNumber == 0)
			{
				Debug.LogWarning("No unconnected players for lobby player");
			}
			EmoteSystem = Object.Instantiate(EmoteSystePrefab);
			LocalPlayer.UseController.AddReceiver(EmoteSystem);
			EmoteSystem.LobbyPlayer = this;
			Object.DontDestroyOnLoad(EmoteSystem.gameObject);
			if (LocalPlayer.UseController.GetControllerType() == Controller.ControllerType.KEYBOARD)
			{
				GameState.ChatSystem.keyBoardLobbyPlayer = this;
			}
			ChangeListener(addRemove: true);
			if (SteamManager.Initialized)
			{
				CallCmdSetSteamID(SteamUser.GetSteamID().m_SteamID);
			}
			if (GameSettings.GetInstance().matchInfo != null)
			{
				NetworkplayerNodeID = (ushort)GameSettings.GetInstance().matchInfo.nodeId;
				CallCmdSetNodeID(playerNodeID);
			}
			CallCmdSetMainUser(MainUser);
			if (MainUser)
			{
				CallCmdSendJoinMessage();
			}
			StartCoroutine(checkQuality());
			Initialized = true;
			CallCmdSetInitializedByLocalPlayer();
		}
		else
		{
			Initialized = true;
			FindLobbyObjects();
		}
		GameEventManager.SendEvent(new LobbyPlayerCreatedEvent(base.gameObject));
	}

	public void FindLobbyObjects()
	{
		GameObject gameObject = ClientScene.FindLocalObject(characterNetID);
		if (gameObject != null)
		{
			Character component = gameObject.GetComponent<Character>();
			if (component != null)
			{
				characterInstance = component;
				characterInstance.AssociatedLobbyPlayer = this;
				if (IsLocalPlayer && LocalPlayer != null && LocalPlayer.UseController != null)
				{
					characterInstance.SetLocalController(LocalPlayer.UseController);
				}
			}
		}
		GameObject gameObject2 = ClientScene.FindLocalObject(cursorNetID);
		if (!(gameObject2 != null))
		{
			return;
		}
		Cursor component2 = gameObject2.GetComponent<Cursor>();
		if (component2 != null)
		{
			cursorInstance = component2;
			cursorInstance.AssociatedLobbyPlayer = this;
			cursorInstance.transform.SetParent(base.transform);
			if (IsLocalPlayer && LocalPlayer != null && LocalPlayer.UseController != null)
			{
				cursorInstance.SetLocalController(LocalPlayer.UseController);
			}
		}
	}

	private void Update()
	{
		netid = base.netId.Value;
	}

	private void OnDestroy()
	{
		Debug.LogWarning("LobbyPlayer.OnDestroy for player " + networkNumber);
		if (LocalPlayer == null && MainUser && LobbyManagerManager.LastSceneLoaded != "MainMenu")
		{
			Debug.Log("[Net] " + playerName + " left the lobby");
			if (!GameSettings.GetInstance().StartLocal)
			{
				ChatMessageDetails chatMessageDetails = new ChatMessageDetails(Character.Animals.NONE, playerName, Color.white, null, EmoteMeanings.CHAT_Text, networkNumber);
				chatMessageDetails.isChatMessage = false;
				chatMessageDetails.GSID = GSID;
				chatMessageDetails.platformID = platformUniqueID;
				chatMessageDetails.platform = platform;
				if (WasKicked)
				{
					chatMessageDetails.Message = ScriptLocalization.Network.PlayerKicked;
					chatMessageDetails.MessageColor = GameSettings.GetInstance().SystemAlertColor;
				}
				else
				{
					chatMessageDetails.Message = ScriptLocalization.Network.Someone_left_lobby;
					chatMessageDetails.MessageColor = GameSettings.GetInstance().SystemColor;
				}
				chatMessageDetails.UserNameColor = chatMessageDetails.MessageColor;
				GameState.ChatSystem.DisplayNewMessage(chatMessageDetails);
			}
		}
		if (EmoteSystem != null)
		{
			Object.Destroy(EmoteSystem.gameObject);
		}
		if (LevelSelectController.lastInstance != null)
		{
			LevelSelectController.lastInstance.OnLobbyPlayerObjectDestroyed(this);
		}
		LobbyManagerManager.Instance.OnLobbyPlayerObjectDestroyed(this);
		if (LocalPlayer != null)
		{
			Debug.Log("Removing local player and controller associated with lobby player");
			PlayerManager.GetInstance().RemovePlayer(LocalPlayer.Number);
			if (LocalPlayer.UseController != null)
			{
				LocalPlayer.UseController.RemovePlayer(LocalPlayer.Number);
				LocalPlayer.UseController.AssociateCharacter(Character.Animals.NONE, LocalPlayer.Number);
				if (LocalPlayer.UseController.GetControllerType() == Controller.ControllerType.KEYBOARD)
				{
					GameState.ChatSystem.keyBoardLobbyPlayer = null;
				}
			}
		}
		if (LobbyManager.instance != null && LobbyManager.instance.PlayerTracker != null)
		{
			LobbyManager.instance.PlayerTracker.RemoveLobbyPlayer(networkNumber);
			if (LobbyManager.instance.IsHost)
			{
				LobbyManager.instance.PlayerTracker.RemovePlayer(networkNumber);
			}
		}
	}

	public void ChangeListener(bool addRemove)
	{
		GameEventManager.ChangeListener<CharacterPickedEvent>(this, addRemove);
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, addRemove);
	}

	private IEnumerator checkQuality()
	{
		while (true)
		{
			if (NetworkManager.singleton.IsClientConnected())
			{
				MsgConnectionQuality msgConnectionQuality = new MsgConnectionQuality();
				msgConnectionQuality.NetworkPlayerNumber = networkNumber;
				msgConnectionQuality.Quality = LobbyManager.instance.GetConnectionQuality();
				NetworkManager.singleton.client.Send(NetMsgTypes.ConnectionQuality, msgConnectionQuality);
			}
			yield return new WaitForSeconds(1f);
		}
	}

	[Command]
	private void CmdSetMainUser(bool isMainUser)
	{
		NetworkMainUser = isMainUser;
	}

	[Command]
	private void CmdSetSteamID(ulong steamID)
	{
		NetworkSteamID = steamID;
	}

	[Command]
	private void CmdSetNodeID(ushort nodeID)
	{
		NetworkplayerNodeID = nodeID;
	}

	[Command]
	private void CmdSetPlayerNumber(int number)
	{
		NetworklocalNumber = number;
	}

	[Command]
	private void CmdSetPlayerInfo(string newPlayerName, double skillMean, double skillStdDev, bool isHost, string GSID, SocialPlatform socialPlatform)
	{
		OnNameChanged(newPlayerName);
		NetworkplayerName = newPlayerName;
		NetworkSkillMean = skillMean;
		NetworkSkillStdDev = skillStdDev;
		NetworkIsHost = isHost;
		Networkplatform = socialPlatform;
		if (GSID != null)
		{
			NetworkGSID = GSID;
		}
	}

	[Command]
	private void CmdSetPlayerHandicap(int newHandicap)
	{
		Networkhandicap = newHandicap;
	}

	public int SetPlayerHandicap(int newHandicap)
	{
		CallCmdSetPlayerHandicap(Mathf.Clamp(newHandicap, 10, 100));
		return Mathf.Clamp(newHandicap, 10, 100);
	}

	[Command]
	private void CmdSetCursorInstance(NetworkInstanceId id)
	{
		if (base.hasAuthority)
		{
			CallRpcSetCursorInstance(id);
		}
	}

	[ClientRpc]
	private void RpcSetCursorInstance(NetworkInstanceId id)
	{
		RunAfterInitialized(delegate
		{
			if (id.IsEmpty())
			{
				cursorInstance = null;
			}
			else
			{
				StartCoroutine(WaitForCursorInstance(id));
			}
		});
	}

	private IEnumerator WaitForCursorInstance(NetworkInstanceId id)
	{
		GameObject cursorObj = null;
		float timeOut = 20f;
		while (cursorObj == null)
		{
			cursorObj = ClientScene.FindLocalObject(id);
			if (cursorObj != null)
			{
				cursorInstance = cursorObj.GetComponent<Cursor>();
				if (cursorInstance != null && IsLocalPlayer && LocalPlayer != null && LocalPlayer.UseController != null)
				{
					cursorInstance.SetLocalController(LocalPlayer.UseController);
				}
			}
			yield return null;
			timeOut -= Time.unscaledDeltaTime;
			if (timeOut < 0f)
			{
				Debug.LogError("RpcSetCursorInstance: Could not find local object with id " + id.Value);
				break;
			}
		}
	}

	[Command]
	private void CmdSetCharacterInstance(NetworkInstanceId id)
	{
		CallRpcSetCharacterInstance(id);
	}

	[ClientRpc]
	private void RpcSetCharacterInstance(NetworkInstanceId id)
	{
		if (base.hasAuthority)
		{
			return;
		}
		if (id.IsEmpty())
		{
			characterInstance = null;
			return;
		}
		characterInstance = ClientScene.FindLocalObject(id).GetComponent<Character>();
		if (characterInstance != null && IsLocalPlayer && LocalPlayer != null && LocalPlayer.UseController != null)
		{
			characterInstance.SetLocalController(LocalPlayer.UseController);
		}
	}

	[Command]
	public void CmdAssignCursor(GameObject cursorObj, int networkNumber, int localNumber)
	{
		if (cursorObj != null)
		{
			Cursor component = cursorObj.GetComponent<Cursor>();
			if (component != null)
			{
				component.NetworknetworkNumber = networkNumber;
				CursorInstance = component;
				cursorObj.GetComponent<NetworkIdentity>().AssignClientAuthority(base.connectionToClient);
				NetworkcursorNetID = cursorObj.GetComponent<NetworkIdentity>().netId;
				component.CallCmdSetLocalPlayerID(localNumber);
				CallRpcAssignCursor(cursorObj, networkNumber, localNumber);
			}
		}
	}

	[ClientRpc]
	private void RpcAssignCursor(GameObject cursorObj, int networkNumber, int localNumber)
	{
		RunAfterInitialized(delegate
		{
			if (networkNumber == this.networkNumber)
			{
				if (cursorObj != null)
				{
					Cursor component = cursorObj.GetComponent<Cursor>();
					component.AssociatedLobbyPlayer = this;
					CursorInstance = component;
					component.transform.parent = base.transform;
					if (IsLocalPlayer && LocalPlayer != null && LocalPlayer.UseController != null)
					{
						component.SetLocalController(LocalPlayer.UseController);
					}
				}
				else
				{
					Debug.LogWarning("Warning: cursorObj is null");
				}
			}
		});
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
	public void CmdAssignCharacter(uint characterNetID, int networkNumber, int localNumber, bool restoreAssignment)
	{
		GameObject gameObject = NetworkServer.FindLocalObject(new NetworkInstanceId(characterNetID));
		if (gameObject != null)
		{
			NetworkcharacterNetID = new NetworkInstanceId(characterNetID);
			Character component = gameObject.GetComponent<Character>();
			if (component != null)
			{
				component.NetworknetworkNumber = networkNumber;
				characterInstance = component;
				if (IsLocalPlayer && LocalPlayer != null && LocalPlayer.UseController != null)
				{
					characterInstance.SetLocalController(LocalPlayer.UseController);
				}
				characterInstance.Networkpicked = true;
				NetworkIdentity component2 = gameObject.GetComponent<NetworkIdentity>();
				if (component2.clientAuthorityOwner != base.connectionToClient)
				{
					if (component2.clientAuthorityOwner != null)
					{
						component2.RemoveClientAuthority(component2.clientAuthorityOwner);
					}
					component2.AssignClientAuthority(base.connectionToClient);
				}
				component.CallCmdSetLocalPlayerID(localNumber);
				CallRpcAssignCharacter(characterNetID, networkNumber, localNumber);
			}
			else
			{
				Debug.LogError("Trying to assign null character");
			}
		}
		if (!(characterInstance == null))
		{
			if (CursorInstance != null)
			{
				CursorInstance.CallCmdDisable(sound: true, showNotebookSprite: false);
			}
			CallRpcSwitchToCharacter(restoreAssignment);
		}
	}

	[ClientRpc]
	private void RpcAssignCharacter(uint characterNetID, int networkNumber, int localNumber)
	{
		if (networkNumber != this.networkNumber)
		{
			return;
		}
		GameObject gameObject = ClientScene.FindLocalObject(new NetworkInstanceId(characterNetID));
		if (gameObject != null)
		{
			Character component = gameObject.GetComponent<Character>();
			component.AssociatedLobbyPlayer = this;
			characterInstance = component;
			characterInstance.SetOutfitsFromArray(characterOutfitsList);
			NetworkPickedAnimal = component.CharacterSprite;
			if (EmoteSystem != null)
			{
				EmoteSystem.characterPortrait.sprite = CharacterSpriteManager.GetInstance().GetCharaterPortrait(PickedAnimal);
			}
			if (IsLocalPlayer && LocalPlayer != null && LocalPlayer.UseController != null)
			{
				characterInstance.SetLocalController(LocalPlayer.UseController);
			}
		}
		else
		{
			Debug.Log("Can't find object with netID " + characterNetID);
		}
		ZoomCamera currentZoomCamera = LobbyManager.instance.GetCurrentZoomCamera();
		if (currentZoomCamera != null)
		{
			currentZoomCamera.AddTarget(characterInstance);
		}
	}

	[Command]
	public void CmdRemoveCharacter()
	{
		if (characterInstance != null)
		{
			characterInstance.NetworknetworkNumber = 0;
			characterInstance.CallCmdSetLocalPlayerID(0);
			characterOutfitsList.Clear();
			NetworkPickedAnimal = Character.Animals.NONE;
			NetworkIdentity component = characterInstance.GetComponent<NetworkIdentity>();
			if (component.clientAuthorityOwner != null)
			{
				component.RemoveClientAuthority(base.connectionToClient);
			}
			characterInstance.CallCmdSetPicked(picked: false);
			CallRpcRemoveCharacter();
			NetworkPickedAnimal = Character.Animals.NONE;
		}
	}

	[ClientRpc]
	private void RpcRemoveCharacter()
	{
		ZoomCamera currentZoomCamera = LobbyManager.instance.GetCurrentZoomCamera();
		if (characterInstance != null)
		{
			if (currentZoomCamera != null)
			{
				currentZoomCamera.RemoveTarget(characterInstance);
				if (cursorInstance != null)
				{
					currentZoomCamera.RemoveTarget(cursorInstance);
				}
			}
			characterInstance.AssociatedGamePlayer = null;
			characterInstance.AssociatedLobbyPlayer = null;
		}
		characterInstance = null;
	}

	[ClientRpc]
	public void RpcSwitchToCharacter(bool restoreAssignment)
	{
		if (!characterInstance.Sitting)
		{
			characterInstance.Enable();
			if (!restoreAssignment)
			{
				CharacterInstance.ForceJump();
			}
		}
		else
		{
			CharacterInstance.Unfreeze();
			CharacterInstance.SitDown();
		}
		ZoomCamera currentZoomCamera = LobbyManager.instance.GetCurrentZoomCamera();
		if (currentZoomCamera != null)
		{
			currentZoomCamera.AddTarget(characterInstance);
			if (cursorInstance != null)
			{
				currentZoomCamera.RemoveTarget(cursorInstance);
			}
		}
	}

	public void SwitchToCursorImmediate()
	{
		if (!(CursorInstance == null))
		{
			ZoomCamera currentZoomCamera = LobbyManager.instance.GetCurrentZoomCamera();
			if (currentZoomCamera != null)
			{
				currentZoomCamera.AddTarget(cursorInstance);
			}
		}
	}

	[Command]
	public void CmdSwitchToCursor()
	{
		CallRpcSwitchToCursor();
	}

	[ClientRpc]
	public void RpcSwitchToCursor()
	{
		SwitchToCursorImmediate();
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e.GetType() == typeof(NetworkMessageReceivedEvent))
		{
			NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.LobbyVoting)
			{
				MsgLobbyVoting msgLobbyVoting = (MsgLobbyVoting)networkMessageReceivedEvent.ReadMessage;
				NetworkLockedForLoad = msgLobbyVoting.VoteStarted;
			}
		}
	}

	[ClientRpc]
	public void RpcClearInstances()
	{
		characterInstance = null;
		cursorInstance = null;
	}

	public void ReceiveEvent(InputEvent e)
	{
		if (playerStatus == Status.INACTIVE || e.Key != InputEvent.InputKey.Suicide || !e.Valueb || !e.Changed || CursorInstance == null)
		{
			return;
		}
		bool flag = playerStatus == Status.CHARACTER && CharacterInstance != null && !CharacterInstance.InMenu;
		bool flag2 = playerStatus == Status.READY && CharacterInstance != null && !CharacterInstance.InMenu;
		if (!characterUnpickRequested && !LockedForLoad && (flag || flag2))
		{
			UnpickCharacter();
			if (CursorInstance.GetComponent<LobbyCursor>().Picked != null)
			{
				e.Consume();
			}
		}
	}

	public void UnpickCharacter()
	{
		LobbyCursor component = CursorInstance.GetComponent<LobbyCursor>();
		if (component.Picked != null)
		{
			characterUnpickRequested = true;
			component.Picked.PlayerColor = Color.white;
			LocalPlayer.PlayerCharacter.Disable(moveAway: false);
			LocalPlayer.PlayerCharacter = null;
			PlayerStatus = Status.CURSOR;
			SwitchToCursorImmediate();
			component.Enable();
			AkSoundEngine.PostEvent("UI_Lobby_Cursor_Creation_Poof", base.gameObject);
			component.MakeMagicSmoke(component.transform, 1f, useCursorColor: true);
			CallCmdSendCharUnpicked(component.Picked.CharacterSprite);
			CallCmdSwitchToCursor();
			CallCmdRemoveCharacter();
			component.Picked = null;
		}
	}

	[Command]
	private void CmdSendCharUnpicked(Character.Animals character)
	{
		LobbyManager.instance.CurrentLevelSelectController.ResetCharacter(CharacterInstance.gameObject, base.gameObject);
	}

	public void OnCharUnpickedConfirmed()
	{
		characterUnpickRequested = false;
	}

	[Command]
	private void CmdSendJoinMessage()
	{
		CallRpcShowJoinMessage(playerName);
	}

	[ClientRpc]
	private void RpcShowJoinMessage(string playerName)
	{
		RunAfterInitialized(delegate
		{
			if (!IsLocalPlayer)
			{
				Debug.Log("[Net] " + playerName + " joined the lobby");
				Color systemColor = GameSettings.GetInstance().SystemColor;
				ChatMessageDetails chatMessageDetails = new ChatMessageDetails(Character.Animals.NONE, playerName, systemColor, ScriptLocalization.Network.SomeoneJoined_lobby, systemColor, EmoteMeanings.CHAT_Text, networkNumber)
				{
					isChatMessage = false,
					platform = platform,
					platformID = platformUniqueID,
					GSID = GSID
				};
				GameState.ChatSystem.DisplayNewMessage(chatMessageDetails);
			}
		});
	}

	[Command]
	public void CmdPlayerPickedCharacter(Character.Animals animal, bool clearOutfit)
	{
		NetworkPickedAnimal = animal;
		LobbyManager.instance.CurrentLevelSelectController.CallRpcPlayerPickedCharacter(networkNumber - 1, animal, PlayerColor, PlayerStatus == Status.COUCH);
		if (clearOutfit)
		{
			characterOutfitsList.Clear();
		}
	}

	public void DoCharacterPickedEvent(Character.Animals chosenCharacter, NetworkInstanceId cursorObjectId, bool clearOutfit)
	{
		CallCmdPlayerPickedCharacter(chosenCharacter, clearOutfit);
		LobbyManager.instance.CurrentLevelSelectController.ShowHotseatMessageForPlayer(this);
		LobbyManager.instance.CurrentLevelSelectController.HideCursor(CursorInstance, sound: false);
		NetworkPickedAnimal = chosenCharacter;
		LocalPlayer.UseController.AssociateCharacter(PickedAnimal, localNumber);
	}

	public bool RequestPickCharacter(Character over)
	{
		if (requestedCharacterInstance == null && !characterUnpickRequested)
		{
			requestedCharacterInstance = over;
			CallCmdRequestPickCharacter(requestedCharacterInstance.netId, over.CharacterSprite);
			return true;
		}
		return false;
	}

	[Command]
	private void CmdRequestPickCharacter(NetworkInstanceId characterInstanceId, Character.Animals animal)
	{
		if (!LobbyManager.instance.CurrentLevelSelectController.IsCharacterTaken(animal))
		{
			CallCmdAssignCharacter(characterInstanceId.Value, networkNumber, localNumber, restoreAssignment: false);
			CallRpcRequestPickResponse(networkNumber, response: true);
		}
		else
		{
			CallRpcRequestPickResponse(networkNumber, response: false);
		}
	}

	[ClientRpc]
	public void RpcRequestPickResponse(int playerNetworkNumber, bool response)
	{
		if (!IsLocalPlayer || networkNumber != playerNetworkNumber)
		{
			return;
		}
		if (requestedCharacterInstance == null)
		{
			Debug.LogError("Server response to pick request but no character requested.");
			return;
		}
		LobbyCursor lobbyCursor = CursorInstance as LobbyCursor;
		if (response)
		{
			lobbyCursor.OnCharacterPickConfirmed(requestedCharacterInstance);
			LocalPlayer.PlayerCharacter = requestedCharacterInstance;
			PlayerStatus = Status.CHARACTER;
			DoCharacterPickedEvent(requestedCharacterInstance.CharacterSprite, base.netId, clearOutfit: true);
		}
		else
		{
			lobbyCursor.OnCharacterPickDenied();
		}
		requestedCharacterInstance = null;
	}

	[Command]
	public void CmdSetOutfitsFromArray(int[] outfitsArray)
	{
		characterOutfitsList.Clear();
		foreach (int item in outfitsArray)
		{
			characterOutfitsList.Add(item);
		}
	}

	[Command]
	public void CmdSetPlayerStatus(Status status)
	{
		NetworkplayerStatus = status;
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
	public void CmdSetGSID(string GSID)
	{
		NetworkGSID = GSID;
	}

	public void LocalSetPlatformUniqueID(string id)
	{
		NetworkplatformUniqueID = id;
		CallCmdSetPlatformUniqueID(id);
	}

	[Command]
	private void CmdSetPlatformUniqueID(string id)
	{
		NetworkplatformUniqueID = id;
	}

	[Command]
	private void CmdSetHasVerifiedSocialAccount(bool value)
	{
		NetworkhasVerifiedSocialAccount = value;
	}

	[Command]
	public void CmdIShouldNotBeHere()
	{
		Debug.LogWarning("Received \"I should not be here\" message from player " + networkNumber);
		LobbyManager.instance.IssueKickMessage(networkNumber, LobbyManager.KickReasons.NONE);
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

	public LobbyPlayer()
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
			((LobbyPlayer)obj).characterOutfitsList.HandleMsg(reader);
		}
	}

	protected static void InvokeCmdCmdSetMainUser(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetMainUser called on client.");
		}
		else
		{
			((LobbyPlayer)obj).CmdSetMainUser(reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdSetSteamID(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetSteamID called on client.");
		}
		else
		{
			((LobbyPlayer)obj).CmdSetSteamID(reader.ReadPackedUInt64());
		}
	}

	protected static void InvokeCmdCmdSetNodeID(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetNodeID called on client.");
		}
		else
		{
			((LobbyPlayer)obj).CmdSetNodeID((ushort)reader.ReadPackedUInt32());
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
			((LobbyPlayer)obj).CmdSetPlayerNumber((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdSetPlayerInfo(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetPlayerInfo called on client.");
		}
		else
		{
			((LobbyPlayer)obj).CmdSetPlayerInfo(reader.ReadString(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadBoolean(), reader.ReadString(), (SocialPlatform)reader.ReadInt32());
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
			((LobbyPlayer)obj).CmdSetPlayerHandicap((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdSetCursorInstance(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetCursorInstance called on client.");
		}
		else
		{
			((LobbyPlayer)obj).CmdSetCursorInstance(reader.ReadNetworkId());
		}
	}

	protected static void InvokeCmdCmdSetCharacterInstance(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetCharacterInstance called on client.");
		}
		else
		{
			((LobbyPlayer)obj).CmdSetCharacterInstance(reader.ReadNetworkId());
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
			((LobbyPlayer)obj).CmdAssignCursor(reader.ReadGameObject(), (int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32());
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
			((LobbyPlayer)obj).CmdRemoveCursor();
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
			((LobbyPlayer)obj).CmdAssignCharacter(reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32(), reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdRemoveCharacter(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRemoveCharacter called on client.");
		}
		else
		{
			((LobbyPlayer)obj).CmdRemoveCharacter();
		}
	}

	protected static void InvokeCmdCmdSwitchToCursor(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSwitchToCursor called on client.");
		}
		else
		{
			((LobbyPlayer)obj).CmdSwitchToCursor();
		}
	}

	protected static void InvokeCmdCmdSendCharUnpicked(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSendCharUnpicked called on client.");
		}
		else
		{
			((LobbyPlayer)obj).CmdSendCharUnpicked((Character.Animals)reader.ReadInt32());
		}
	}

	protected static void InvokeCmdCmdSendJoinMessage(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSendJoinMessage called on client.");
		}
		else
		{
			((LobbyPlayer)obj).CmdSendJoinMessage();
		}
	}

	protected static void InvokeCmdCmdPlayerPickedCharacter(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlayerPickedCharacter called on client.");
		}
		else
		{
			((LobbyPlayer)obj).CmdPlayerPickedCharacter((Character.Animals)reader.ReadInt32(), reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdRequestPickCharacter(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestPickCharacter called on client.");
		}
		else
		{
			((LobbyPlayer)obj).CmdRequestPickCharacter(reader.ReadNetworkId(), (Character.Animals)reader.ReadInt32());
		}
	}

	protected static void InvokeCmdCmdSetOutfitsFromArray(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetOutfitsFromArray called on client.");
		}
		else
		{
			((LobbyPlayer)obj).CmdSetOutfitsFromArray(GeneratedNetworkCode._ReadArrayInt32_None(reader));
		}
	}

	protected static void InvokeCmdCmdSetPlayerStatus(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetPlayerStatus called on client.");
		}
		else
		{
			((LobbyPlayer)obj).CmdSetPlayerStatus((Status)reader.ReadInt32());
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
			((LobbyPlayer)obj).CmdSetInitializedByLocalPlayer();
		}
	}

	protected static void InvokeCmdCmdSetGSID(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetGSID called on client.");
		}
		else
		{
			((LobbyPlayer)obj).CmdSetGSID(reader.ReadString());
		}
	}

	protected static void InvokeCmdCmdSetPlatformUniqueID(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetPlatformUniqueID called on client.");
		}
		else
		{
			((LobbyPlayer)obj).CmdSetPlatformUniqueID(reader.ReadString());
		}
	}

	protected static void InvokeCmdCmdSetHasVerifiedSocialAccount(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetHasVerifiedSocialAccount called on client.");
		}
		else
		{
			((LobbyPlayer)obj).CmdSetHasVerifiedSocialAccount(reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdIShouldNotBeHere(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdIShouldNotBeHere called on client.");
		}
		else
		{
			((LobbyPlayer)obj).CmdIShouldNotBeHere();
		}
	}

	public void CallCmdSetMainUser(bool isMainUser)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetMainUser called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetMainUser(isMainUser);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetMainUser);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(isMainUser);
		SendCommandInternal(networkWriter, 0, "CmdSetMainUser");
	}

	public void CallCmdSetSteamID(ulong steamID)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetSteamID called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetSteamID(steamID);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetSteamID);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt64(steamID);
		SendCommandInternal(networkWriter, 0, "CmdSetSteamID");
	}

	public void CallCmdSetNodeID(ushort nodeID)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetNodeID called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetNodeID(nodeID);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetNodeID);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32(nodeID);
		SendCommandInternal(networkWriter, 0, "CmdSetNodeID");
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

	public void CallCmdSetPlayerInfo(string newPlayerName, double skillMean, double skillStdDev, bool isHost, string GSID, SocialPlatform socialPlatform)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetPlayerInfo called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetPlayerInfo(newPlayerName, skillMean, skillStdDev, isHost, GSID, socialPlatform);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetPlayerInfo);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(newPlayerName);
		networkWriter.Write(skillMean);
		networkWriter.Write(skillStdDev);
		networkWriter.Write(isHost);
		networkWriter.Write(GSID);
		networkWriter.Write((int)socialPlatform);
		SendCommandInternal(networkWriter, 0, "CmdSetPlayerInfo");
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

	public void CallCmdSetCursorInstance(NetworkInstanceId id)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetCursorInstance called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetCursorInstance(id);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetCursorInstance);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(id);
		SendCommandInternal(networkWriter, 0, "CmdSetCursorInstance");
	}

	public void CallCmdSetCharacterInstance(NetworkInstanceId id)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetCharacterInstance called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetCharacterInstance(id);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetCharacterInstance);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(id);
		SendCommandInternal(networkWriter, 0, "CmdSetCharacterInstance");
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

	public void CallCmdAssignCharacter(uint characterNetID, int networkNumber, int localNumber, bool restoreAssignment)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdAssignCharacter called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdAssignCharacter(characterNetID, networkNumber, localNumber, restoreAssignment);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdAssignCharacter);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32(characterNetID);
		networkWriter.WritePackedUInt32((uint)networkNumber);
		networkWriter.WritePackedUInt32((uint)localNumber);
		networkWriter.Write(restoreAssignment);
		SendCommandInternal(networkWriter, 0, "CmdAssignCharacter");
	}

	public void CallCmdRemoveCharacter()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRemoveCharacter called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRemoveCharacter();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRemoveCharacter);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdRemoveCharacter");
	}

	public void CallCmdSwitchToCursor()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSwitchToCursor called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSwitchToCursor();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSwitchToCursor);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdSwitchToCursor");
	}

	public void CallCmdSendCharUnpicked(Character.Animals character)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSendCharUnpicked called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSendCharUnpicked(character);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSendCharUnpicked);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write((int)character);
		SendCommandInternal(networkWriter, 0, "CmdSendCharUnpicked");
	}

	public void CallCmdSendJoinMessage()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSendJoinMessage called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSendJoinMessage();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSendJoinMessage);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdSendJoinMessage");
	}

	public void CallCmdPlayerPickedCharacter(Character.Animals animal, bool clearOutfit)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdPlayerPickedCharacter called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdPlayerPickedCharacter(animal, clearOutfit);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdPlayerPickedCharacter);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write((int)animal);
		networkWriter.Write(clearOutfit);
		SendCommandInternal(networkWriter, 0, "CmdPlayerPickedCharacter");
	}

	public void CallCmdRequestPickCharacter(NetworkInstanceId characterInstanceId, Character.Animals animal)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRequestPickCharacter called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRequestPickCharacter(characterInstanceId, animal);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRequestPickCharacter);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(characterInstanceId);
		networkWriter.Write((int)animal);
		SendCommandInternal(networkWriter, 0, "CmdRequestPickCharacter");
	}

	public void CallCmdSetOutfitsFromArray(int[] outfitsArray)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetOutfitsFromArray called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetOutfitsFromArray(outfitsArray);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetOutfitsFromArray);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WriteArrayInt32_None(networkWriter, outfitsArray);
		SendCommandInternal(networkWriter, 0, "CmdSetOutfitsFromArray");
	}

	public void CallCmdSetPlayerStatus(Status status)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetPlayerStatus called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetPlayerStatus(status);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetPlayerStatus);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write((int)status);
		SendCommandInternal(networkWriter, 0, "CmdSetPlayerStatus");
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

	public void CallCmdSetGSID(string GSID)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetGSID called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetGSID(GSID);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetGSID);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(GSID);
		SendCommandInternal(networkWriter, 0, "CmdSetGSID");
	}

	public void CallCmdSetPlatformUniqueID(string id)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetPlatformUniqueID called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetPlatformUniqueID(id);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetPlatformUniqueID);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(id);
		SendCommandInternal(networkWriter, 0, "CmdSetPlatformUniqueID");
	}

	public void CallCmdSetHasVerifiedSocialAccount(bool value)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetHasVerifiedSocialAccount called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetHasVerifiedSocialAccount(value);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetHasVerifiedSocialAccount);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(value);
		SendCommandInternal(networkWriter, 0, "CmdSetHasVerifiedSocialAccount");
	}

	public void CallCmdIShouldNotBeHere()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdIShouldNotBeHere called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdIShouldNotBeHere();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdIShouldNotBeHere);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdIShouldNotBeHere");
	}

	protected static void InvokeRpcRpcSetCursorInstance(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetCursorInstance called on server.");
		}
		else
		{
			((LobbyPlayer)obj).RpcSetCursorInstance(reader.ReadNetworkId());
		}
	}

	protected static void InvokeRpcRpcSetCharacterInstance(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetCharacterInstance called on server.");
		}
		else
		{
			((LobbyPlayer)obj).RpcSetCharacterInstance(reader.ReadNetworkId());
		}
	}

	protected static void InvokeRpcRpcAssignCursor(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAssignCursor called on server.");
		}
		else
		{
			((LobbyPlayer)obj).RpcAssignCursor(reader.ReadGameObject(), (int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32());
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
			((LobbyPlayer)obj).RpcRemoveCursor((int)reader.ReadPackedUInt32());
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
			((LobbyPlayer)obj).RpcAssignCharacter(reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcRemoveCharacter(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRemoveCharacter called on server.");
		}
		else
		{
			((LobbyPlayer)obj).RpcRemoveCharacter();
		}
	}

	protected static void InvokeRpcRpcSwitchToCharacter(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSwitchToCharacter called on server.");
		}
		else
		{
			((LobbyPlayer)obj).RpcSwitchToCharacter(reader.ReadBoolean());
		}
	}

	protected static void InvokeRpcRpcSwitchToCursor(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSwitchToCursor called on server.");
		}
		else
		{
			((LobbyPlayer)obj).RpcSwitchToCursor();
		}
	}

	protected static void InvokeRpcRpcClearInstances(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcClearInstances called on server.");
		}
		else
		{
			((LobbyPlayer)obj).RpcClearInstances();
		}
	}

	protected static void InvokeRpcRpcShowJoinMessage(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowJoinMessage called on server.");
		}
		else
		{
			((LobbyPlayer)obj).RpcShowJoinMessage(reader.ReadString());
		}
	}

	protected static void InvokeRpcRpcRequestPickResponse(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRequestPickResponse called on server.");
		}
		else
		{
			((LobbyPlayer)obj).RpcRequestPickResponse((int)reader.ReadPackedUInt32(), reader.ReadBoolean());
		}
	}

	public void CallRpcSetCursorInstance(NetworkInstanceId id)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSetCursorInstance called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSetCursorInstance);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(id);
		SendRPCInternal(networkWriter, 0, "RpcSetCursorInstance");
	}

	public void CallRpcSetCharacterInstance(NetworkInstanceId id)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSetCharacterInstance called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSetCharacterInstance);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(id);
		SendRPCInternal(networkWriter, 0, "RpcSetCharacterInstance");
	}

	public void CallRpcAssignCursor(GameObject cursorObj, int networkNumber, int localNumber)
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
		networkWriter.Write(cursorObj);
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

	public void CallRpcAssignCharacter(uint characterNetID, int networkNumber, int localNumber)
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
		networkWriter.WritePackedUInt32(characterNetID);
		networkWriter.WritePackedUInt32((uint)networkNumber);
		networkWriter.WritePackedUInt32((uint)localNumber);
		SendRPCInternal(networkWriter, 0, "RpcAssignCharacter");
	}

	public void CallRpcRemoveCharacter()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcRemoveCharacter called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcRemoveCharacter);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcRemoveCharacter");
	}

	public void CallRpcSwitchToCharacter(bool restoreAssignment)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSwitchToCharacter called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSwitchToCharacter);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(restoreAssignment);
		SendRPCInternal(networkWriter, 0, "RpcSwitchToCharacter");
	}

	public void CallRpcSwitchToCursor()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSwitchToCursor called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSwitchToCursor);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcSwitchToCursor");
	}

	public void CallRpcClearInstances()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcClearInstances called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcClearInstances);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcClearInstances");
	}

	public void CallRpcShowJoinMessage(string playerName)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcShowJoinMessage called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcShowJoinMessage);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(playerName);
		SendRPCInternal(networkWriter, 0, "RpcShowJoinMessage");
	}

	public void CallRpcRequestPickResponse(int playerNetworkNumber, bool response)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcRequestPickResponse called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcRequestPickResponse);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)playerNetworkNumber);
		networkWriter.Write(response);
		SendRPCInternal(networkWriter, 0, "RpcRequestPickResponse");
	}

	static LobbyPlayer()
	{
		kCmdCmdSetMainUser = -279290193;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdSetMainUser, InvokeCmdCmdSetMainUser);
		kCmdCmdSetSteamID = 1283844992;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdSetSteamID, InvokeCmdCmdSetSteamID);
		kCmdCmdSetNodeID = -522018072;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdSetNodeID, InvokeCmdCmdSetNodeID);
		kCmdCmdSetPlayerNumber = 1148564885;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdSetPlayerNumber, InvokeCmdCmdSetPlayerNumber);
		kCmdCmdSetPlayerInfo = -535272966;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdSetPlayerInfo, InvokeCmdCmdSetPlayerInfo);
		kCmdCmdSetPlayerHandicap = 1852678916;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdSetPlayerHandicap, InvokeCmdCmdSetPlayerHandicap);
		kCmdCmdSetCursorInstance = 168574806;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdSetCursorInstance, InvokeCmdCmdSetCursorInstance);
		kCmdCmdSetCharacterInstance = -1732638285;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdSetCharacterInstance, InvokeCmdCmdSetCharacterInstance);
		kCmdCmdAssignCursor = 156291900;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdAssignCursor, InvokeCmdCmdAssignCursor);
		kCmdCmdRemoveCursor = 1637254929;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdRemoveCursor, InvokeCmdCmdRemoveCursor);
		kCmdCmdAssignCharacter = 1216162179;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdAssignCharacter, InvokeCmdCmdAssignCharacter);
		kCmdCmdRemoveCharacter = -1613272690;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdRemoveCharacter, InvokeCmdCmdRemoveCharacter);
		kCmdCmdSwitchToCursor = -1163360932;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdSwitchToCursor, InvokeCmdCmdSwitchToCursor);
		kCmdCmdSendCharUnpicked = -399214578;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdSendCharUnpicked, InvokeCmdCmdSendCharUnpicked);
		kCmdCmdSendJoinMessage = -1049887394;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdSendJoinMessage, InvokeCmdCmdSendJoinMessage);
		kCmdCmdPlayerPickedCharacter = -1006458575;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdPlayerPickedCharacter, InvokeCmdCmdPlayerPickedCharacter);
		kCmdCmdRequestPickCharacter = 1816529104;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdRequestPickCharacter, InvokeCmdCmdRequestPickCharacter);
		kCmdCmdSetOutfitsFromArray = 2022040170;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdSetOutfitsFromArray, InvokeCmdCmdSetOutfitsFromArray);
		kCmdCmdSetPlayerStatus = 1290447422;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdSetPlayerStatus, InvokeCmdCmdSetPlayerStatus);
		kCmdCmdSetInitializedByLocalPlayer = -863993236;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdSetInitializedByLocalPlayer, InvokeCmdCmdSetInitializedByLocalPlayer);
		kCmdCmdSetGSID = 1483017714;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdSetGSID, InvokeCmdCmdSetGSID);
		kCmdCmdSetPlatformUniqueID = 504778954;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdSetPlatformUniqueID, InvokeCmdCmdSetPlatformUniqueID);
		kCmdCmdSetHasVerifiedSocialAccount = 1588520233;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdSetHasVerifiedSocialAccount, InvokeCmdCmdSetHasVerifiedSocialAccount);
		kCmdCmdIShouldNotBeHere = 243596801;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdIShouldNotBeHere, InvokeCmdCmdIShouldNotBeHere);
		kRpcRpcSetCursorInstance = 1562083904;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LobbyPlayer), kRpcRpcSetCursorInstance, InvokeRpcRpcSetCursorInstance);
		kRpcRpcSetCharacterInstance = 1437984393;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LobbyPlayer), kRpcRpcSetCharacterInstance, InvokeRpcRpcSetCharacterInstance);
		kRpcRpcAssignCursor = 239724050;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LobbyPlayer), kRpcRpcAssignCursor, InvokeRpcRpcAssignCursor);
		kRpcRpcRemoveCursor = 1720687079;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LobbyPlayer), kRpcRpcRemoveCursor, InvokeRpcRpcRemoveCursor);
		kRpcRpcAssignCharacter = -42721555;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LobbyPlayer), kRpcRpcAssignCharacter, InvokeRpcRpcAssignCharacter);
		kRpcRpcRemoveCharacter = 1422810872;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LobbyPlayer), kRpcRpcRemoveCharacter, InvokeRpcRpcRemoveCharacter);
		kRpcRpcSwitchToCharacter = 667813453;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LobbyPlayer), kRpcRpcSwitchToCharacter, InvokeRpcRpcSwitchToCharacter);
		kRpcRpcSwitchToCursor = 1705523890;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LobbyPlayer), kRpcRpcSwitchToCursor, InvokeRpcRpcSwitchToCursor);
		kRpcRpcClearInstances = -345523106;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LobbyPlayer), kRpcRpcClearInstances, InvokeRpcRpcClearInstances);
		kRpcRpcShowJoinMessage = -1666278349;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LobbyPlayer), kRpcRpcShowJoinMessage, InvokeRpcRpcShowJoinMessage);
		kRpcRpcRequestPickResponse = -1892473308;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LobbyPlayer), kRpcRpcRequestPickResponse, InvokeRpcRpcRequestPickResponse);
		kListcharacterOutfitsList = -1577332130;
		NetworkBehaviour.RegisterSyncListDelegate(typeof(LobbyPlayer), kListcharacterOutfitsList, InvokeSyncListcharacterOutfitsList);
		NetworkCRC.RegisterBehaviour("LobbyPlayer", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(LockedForLoad);
			writer.Write(characterNetID);
			writer.Write(cursorNetID);
			writer.WritePackedUInt32(playerNodeID);
			writer.Write(IsHost);
			writer.Write(GSID);
			writer.Write(platformUniqueID);
			writer.Write((int)platform);
			writer.Write(hasVerifiedSocialAccount);
			writer.WritePackedUInt32((uint)networkNumber);
			writer.WritePackedUInt32((uint)localNumber);
			writer.Write((int)PickedAnimal);
			writer.Write((int)playerStatus);
			writer.Write(PlayerColor);
			SyncListInt.WriteInstance(writer, characterOutfitsList);
			writer.WritePackedUInt64(SteamID);
			writer.Write((int)ConnectionQuality);
			writer.WritePackedUInt32((uint)handicap);
			writer.Write(playerName);
			writer.Write(MainUser);
			writer.Write(SkillMean);
			writer.Write(SkillStdDev);
			writer.Write(InitializedByLocalPlayer);
			return true;
		}
		bool flag2 = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(LockedForLoad);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(characterNetID);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(cursorNetID);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.WritePackedUInt32(playerNodeID);
		}
		if ((base.syncVarDirtyBits & 0x10) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(IsHost);
		}
		if ((base.syncVarDirtyBits & 0x20) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(GSID);
		}
		if ((base.syncVarDirtyBits & 0x40) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(platformUniqueID);
		}
		if ((base.syncVarDirtyBits & 0x80) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write((int)platform);
		}
		if ((base.syncVarDirtyBits & 0x100) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(hasVerifiedSocialAccount);
		}
		if ((base.syncVarDirtyBits & 0x200) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.WritePackedUInt32((uint)networkNumber);
		}
		if ((base.syncVarDirtyBits & 0x400) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.WritePackedUInt32((uint)localNumber);
		}
		if ((base.syncVarDirtyBits & 0x800) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write((int)PickedAnimal);
		}
		if ((base.syncVarDirtyBits & 0x1000) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write((int)playerStatus);
		}
		if ((base.syncVarDirtyBits & 0x2000) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(PlayerColor);
		}
		if ((base.syncVarDirtyBits & 0x4000) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			SyncListInt.WriteInstance(writer, characterOutfitsList);
		}
		if ((base.syncVarDirtyBits & 0x8000) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.WritePackedUInt64(SteamID);
		}
		if ((base.syncVarDirtyBits & 0x10000) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write((int)ConnectionQuality);
		}
		if ((base.syncVarDirtyBits & 0x20000) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.WritePackedUInt32((uint)handicap);
		}
		if ((base.syncVarDirtyBits & 0x40000) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(playerName);
		}
		if ((base.syncVarDirtyBits & 0x80000) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(MainUser);
		}
		if ((base.syncVarDirtyBits & 0x100000) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(SkillMean);
		}
		if ((base.syncVarDirtyBits & 0x200000) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(SkillStdDev);
		}
		if ((base.syncVarDirtyBits & 0x400000) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(InitializedByLocalPlayer);
		}
		if (!flag2)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
		if (initialState)
		{
			LockedForLoad = reader.ReadBoolean();
			characterNetID = reader.ReadNetworkId();
			cursorNetID = reader.ReadNetworkId();
			playerNodeID = (ushort)reader.ReadPackedUInt32();
			IsHost = reader.ReadBoolean();
			GSID = reader.ReadString();
			platformUniqueID = reader.ReadString();
			platform = (SocialPlatform)reader.ReadInt32();
			hasVerifiedSocialAccount = reader.ReadBoolean();
			networkNumber = (int)reader.ReadPackedUInt32();
			localNumber = (int)reader.ReadPackedUInt32();
			PickedAnimal = (Character.Animals)reader.ReadInt32();
			playerStatus = (Status)reader.ReadInt32();
			PlayerColor = reader.ReadColor();
			SyncListInt.ReadReference(reader, characterOutfitsList);
			SteamID = reader.ReadPackedUInt64();
			ConnectionQuality = (LobbyManager.ConnectionQuality)reader.ReadInt32();
			handicap = (int)reader.ReadPackedUInt32();
			playerName = reader.ReadString();
			MainUser = reader.ReadBoolean();
			SkillMean = reader.ReadDouble();
			SkillStdDev = reader.ReadDouble();
			InitializedByLocalPlayer = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			LockedForLoad = reader.ReadBoolean();
		}
		if ((num & 2) != 0)
		{
			characterNetID = reader.ReadNetworkId();
		}
		if ((num & 4) != 0)
		{
			cursorNetID = reader.ReadNetworkId();
		}
		if ((num & 8) != 0)
		{
			playerNodeID = (ushort)reader.ReadPackedUInt32();
		}
		if ((num & 0x10) != 0)
		{
			IsHost = reader.ReadBoolean();
		}
		if ((num & 0x20) != 0)
		{
			GSID = reader.ReadString();
		}
		if ((num & 0x40) != 0)
		{
			platformUniqueID = reader.ReadString();
		}
		if ((num & 0x80) != 0)
		{
			platform = (SocialPlatform)reader.ReadInt32();
		}
		if ((num & 0x100) != 0)
		{
			hasVerifiedSocialAccount = reader.ReadBoolean();
		}
		if ((num & 0x200) != 0)
		{
			networkNumber = (int)reader.ReadPackedUInt32();
		}
		if ((num & 0x400) != 0)
		{
			localNumber = (int)reader.ReadPackedUInt32();
		}
		if ((num & 0x800) != 0)
		{
			PickedAnimal = (Character.Animals)reader.ReadInt32();
		}
		if ((num & 0x1000) != 0)
		{
			playerStatus = (Status)reader.ReadInt32();
		}
		if ((num & 0x2000) != 0)
		{
			PlayerColor = reader.ReadColor();
		}
		if ((num & 0x4000) != 0)
		{
			SyncListInt.ReadReference(reader, characterOutfitsList);
		}
		if ((num & 0x8000) != 0)
		{
			SteamID = reader.ReadPackedUInt64();
		}
		if ((num & 0x10000) != 0)
		{
			ConnectionQuality = (LobbyManager.ConnectionQuality)reader.ReadInt32();
		}
		if ((num & 0x20000) != 0)
		{
			handicap = (int)reader.ReadPackedUInt32();
		}
		if ((num & 0x40000) != 0)
		{
			OnNameChanged(reader.ReadString());
		}
		if ((num & 0x80000) != 0)
		{
			MainUser = reader.ReadBoolean();
		}
		if ((num & 0x100000) != 0)
		{
			SkillMean = reader.ReadDouble();
		}
		if ((num & 0x200000) != 0)
		{
			SkillStdDev = reader.ReadDouble();
		}
		if ((num & 0x400000) != 0)
		{
			InitializedByLocalPlayer = reader.ReadBoolean();
		}
	}

	public override void PreStartClient()
	{
		base.PreStartClient();
	}
}
