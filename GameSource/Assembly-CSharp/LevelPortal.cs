using System.Collections.Generic;
using System.Runtime.InteropServices;
using GameEvent;
using UnityEngine;
using UnityEngine.Networking;

public class LevelPortal : NetworkBehaviour, IGameEventListener
{
	public GameState.PortalID PortalID;

	private List<VoteArrow> arrows = new List<VoteArrow>();

	public Dictionary<Character, VoteArrow> Votes = new Dictionary<Character, VoteArrow>();

	public GameState.LevelName TargetLevel;

	public GameObject voteButton;

	public float slideDistance;

	public float pushForce;

	protected float startY;

	protected Rigidbody2D rbButton;

	protected bool buttonPress;

	protected bool buttonWasPressed;

	public Animator Countdown;

	public Animator HighLightCountDown;

	public bool DimOnLock;

	public LevelSelectController levelSelectController;

	public GameObject ExclamationPoint;

	public GameObject NewSign;

	public bool neverNew;

	[SyncVar]
	public bool levelHasUnlock;

	public UnLockInfo LevelUnlockInside;

	public string SoundFXHint;

	public string LevelMusicString;

	private string[] AllLevelSoundFXHints = new string[16]
	{
		"UI_Hover_Level_Farm_Ambiance", "UI_Hover_Level_Lobby_Ambiance", "UI_Hover_Level_Rooftops", "UI_Hover_Level_Waterfall", "UI_Hover_Level_OldHouse", "UI_Hover_Level_Iceberg", "UI_Hover_Level_Pyramid", "UI_Hover_Level_DanceParty", "UI_Hover_Level_Metal_Plant", "UI_Hover_Level_Windmill",
		"UI_Hover_Level_Pier", "UI_Hover_Level_JungleTemple", "UI_Hover_Level_Volcano", "UI_Hover_Light_Bridge", "UI_Hover_Nuclear_Plant", "UI_Hover_Crumbling_Bridge"
	};

	public string snapshotXml;

	public Animator LockedOutAnimator;

	private bool locked;

	private bool shouldPlayClickSound;

	private bool shouldPlayAmbientSound;

	private bool paused;

	private static int kRpcRpcStartCountDown;

	private static int kRpcRpcExitCountDown;

	private static int kRpcRpcHighLightPortal;

	private static int kRpcRpcNeutralLightPortal;

	private static int kRpcRpcLowlightPortal;

	public bool LevelIsNew => StatTracker.Instance.GetSaveFileDataForMainUser().GetStat<StatCountArray>("TotalLevelRounds").values[(int)TargetLevel] == 0;

	public bool Locked
	{
		get
		{
			return locked;
		}
		set
		{
			if (locked != value)
			{
				LockedOutAnimator.SetBool("LockedOut", value);
				if (value)
				{
					if (DimOnLock && HighLightCountDown != null)
					{
						HighLightCountDown.SetBool("Dimmed", value: true);
					}
				}
				else if (DimOnLock && HighLightCountDown != null)
				{
					HighLightCountDown.SetBool("Dimmed", value: false);
				}
			}
			locked = value;
			if (locked)
			{
				resetArrows();
			}
		}
	}

	public bool NetworklevelHasUnlock
	{
		get
		{
			return levelHasUnlock;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref levelHasUnlock, 1u);
		}
	}

	protected virtual void Awake()
	{
	}

	protected virtual void Start()
	{
		startY = voteButton.transform.localPosition.y;
		VoteArrow[] componentsInChildren = GetComponentsInChildren<VoteArrow>();
		foreach (VoteArrow voteArrow in componentsInChildren)
		{
			arrows.Add(voteArrow);
			voteArrow.levelPortal = this;
		}
		buttonWasPressed = false;
		if (TargetLevel < GameState.LevelName.RANDOM && LevelIsNew && !neverNew)
		{
			NewSign.SetActive(value: true);
		}
		else
		{
			NewSign.SetActive(value: false);
		}
		ChangeListener(adding: true);
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public virtual void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<PauseEvent>(this, adding);
	}

	public virtual void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e.GetType() == typeof(PauseEvent))
		{
			PauseEvent pauseEvent = e as PauseEvent;
			paused = pauseEvent.Paused;
		}
	}

	protected virtual void Update()
	{
		if (levelHasUnlock)
		{
			if (!ExclamationPoint.activeSelf)
			{
				ExclamationPoint.SetActive(value: true);
			}
		}
		else if (ExclamationPoint.activeSelf)
		{
			ExclamationPoint.SetActive(value: false);
		}
		if (HighLightCountDown != null)
		{
			HighLightCountDown.SetBool("ButtonPressed", Votes.Count > 0);
		}
	}

	private void FixedUpdate()
	{
		if (paused)
		{
			return;
		}
		foreach (VoteArrow arrow in arrows)
		{
			if (arrow.ChrPresent)
			{
				arrow.ButtonPressed = Votes.Count > 0;
			}
		}
		if (Votes.Count <= 0)
		{
			return;
		}
		if (shouldPlayAmbientSound)
		{
			shouldPlayAmbientSound = false;
			if (TargetLevel == GameState.LevelName.RANDOM)
			{
				AkSoundEngine.PostEvent(AllLevelSoundFXHints[Random.Range(0, AllLevelSoundFXHints.Length)], base.gameObject);
			}
			else if (!SoundFXHint.NullOrEmpty())
			{
				AkSoundEngine.PostEvent(SoundFXHint, base.gameObject);
			}
		}
		if (shouldPlayClickSound)
		{
			shouldPlayClickSound = false;
			AkSoundEngine.PostEvent("UI_Lobby_Level_VoteButtonPressed", base.gameObject);
		}
	}

	public bool IsCharacterVoting(Character c)
	{
		if (Votes.ContainsKey(c))
		{
			return c.OnGround;
		}
		return false;
	}

	protected void OnTriggerEnter2D(Collider2D c)
	{
		OnTriggerStay2D(c);
	}

	protected virtual void OnTriggerStay2D(Collider2D c)
	{
		if (Locked)
		{
			return;
		}
		Character component = c.gameObject.GetComponent<Character>();
		if (component != null && component.Picked && component.Feet.transform.position.y > base.transform.position.y && !Votes.ContainsKey(component))
		{
			if (Votes.Count == 0)
			{
				shouldPlayAmbientSound = true;
			}
			VoteArrow unusedArrow = getUnusedArrow(component);
			Votes.Add(component, unusedArrow);
			unusedArrow.characterPresent();
			unusedArrow.lastCharacterSelected = component;
			Color playerColor = component.PlayerColor;
			unusedArrow.setColor(playerColor, playerColor - new Color(0.2f, 0.2f, 0.2f, 0f));
			component.suggestDance = true;
			shouldPlayClickSound = true;
			LobbyPlayer associatedLobbyPlayer = component.AssociatedLobbyPlayer;
			if (associatedLobbyPlayer != null)
			{
				GameEventManager.SendEvent(new CharacterVoteEvent(isVoting: true, associatedLobbyPlayer.netId, component.netId));
			}
		}
	}

	protected virtual void OnTriggerExit2D(Collider2D c)
	{
		Character component = c.gameObject.GetComponent<Character>();
		if (component != null && Votes.ContainsKey(component))
		{
			VoteArrow voteArrow = Votes[component];
			voteArrow.characterLeft();
			voteArrow.ButtonPressed = false;
			voteArrow.lastCharacterSelected = component;
			Votes.Remove(component);
			component.suggestDance = false;
			LobbyPlayer associatedLobbyPlayer = component.AssociatedLobbyPlayer;
			if (associatedLobbyPlayer != null)
			{
				GameEventManager.SendEvent(new CharacterVoteEvent(isVoting: false, associatedLobbyPlayer.netId, component.netId));
			}
		}
	}

	private void resetArrows()
	{
		foreach (KeyValuePair<Character, VoteArrow> vote in Votes)
		{
			Character key = vote.Key;
			VoteArrow value = vote.Value;
			value.characterLeft();
			value.ButtonPressed = false;
			value.lastCharacterSelected = key;
			key.suggestDance = false;
			LobbyPlayer associatedLobbyPlayer = key.AssociatedLobbyPlayer;
			if (associatedLobbyPlayer != null)
			{
				GameEventManager.SendEvent(new CharacterVoteEvent(isVoting: false, associatedLobbyPlayer.netId, key.netId));
			}
		}
		Votes.Clear();
	}

	private VoteArrow getUnusedArrow(Character ch)
	{
		VoteArrow voteArrow = null;
		VoteArrow voteArrow2 = null;
		foreach (VoteArrow arrow in arrows)
		{
			if (!arrow.ChrPresent)
			{
				if (arrow.lastCharacterSelected == ch)
				{
					return arrow;
				}
				if (voteArrow2 == null && arrow.lastCharacterSelected == null)
				{
					voteArrow2 = arrow;
				}
				if (voteArrow == null)
				{
					voteArrow = arrow;
				}
			}
		}
		if (!(voteArrow2 != null))
		{
			return voteArrow;
		}
		return voteArrow2;
	}

	public void StartCountDown()
	{
		HighLightCountDown.SetInteger("LightValue", 1);
		CallRpcStartCountDown();
	}

	[ClientRpc]
	private void RpcStartCountDown()
	{
		if (base.hasAuthority)
		{
			return;
		}
		if (HighLightCountDown != null)
		{
			HighLightCountDown.SetInteger("LightValue", 1);
		}
		foreach (VoteArrow value in Votes.Values)
		{
			value.lightState = VoteArrow.LightState.FLASHING;
		}
	}

	public void ExitCountDown()
	{
		if (HighLightCountDown != null && HighLightCountDown.isInitialized)
		{
			HighLightCountDown.SetInteger("LightValue", 0);
		}
		foreach (VoteArrow value in Votes.Values)
		{
			value.lightState = VoteArrow.LightState.OFF;
		}
		if (base.hasAuthority)
		{
			CallRpcExitCountDown();
		}
	}

	[ClientRpc]
	private void RpcExitCountDown()
	{
		if (base.hasAuthority)
		{
			return;
		}
		if (HighLightCountDown != null && HighLightCountDown.isInitialized)
		{
			HighLightCountDown.SetInteger("LightValue", 0);
		}
		foreach (VoteArrow value in Votes.Values)
		{
			value.lightState = VoteArrow.LightState.OFF;
		}
	}

	public void highLightPortal()
	{
		if (HighLightCountDown != null)
		{
			HighLightCountDown.SetInteger("LightValue", 1);
			HighLightCountDown.SetTrigger("HighLightTrigger");
			CallRpcHighLightPortal();
		}
	}

	[ClientRpc(channel = 1)]
	private void RpcHighLightPortal()
	{
		if (!base.hasAuthority && HighLightCountDown != null)
		{
			HighLightCountDown.SetInteger("LightValue", 1);
			HighLightCountDown.SetTrigger("HighLightTrigger");
		}
	}

	public void neutralLightPortal()
	{
		if (HighLightCountDown != null)
		{
			HighLightCountDown.SetInteger("LightValue", 0);
			CallRpcNeutralLightPortal();
		}
	}

	[ClientRpc]
	private void RpcNeutralLightPortal()
	{
		if (!base.hasAuthority && HighLightCountDown != null)
		{
			HighLightCountDown.SetInteger("LightValue", 0);
		}
	}

	internal void LowlightPortal()
	{
		if (HighLightCountDown != null)
		{
			HighLightCountDown.SetInteger("LightValue", -1);
			CallRpcLowlightPortal();
		}
	}

	[ClientRpc(channel = 1)]
	private void RpcLowlightPortal()
	{
		if (!base.hasAuthority && HighLightCountDown != null)
		{
			HighLightCountDown.SetInteger("LightValue", -1);
		}
	}

	public void turnOffAnArrow()
	{
		foreach (VoteArrow value in Votes.Values)
		{
			if (!value.TempDisabled)
			{
				value.TempDisabled = true;
				value.VoteLocked = false;
				value.lightState = VoteArrow.LightState.OFF;
				break;
			}
		}
	}

	public void LockVotes()
	{
		foreach (VoteArrow value in Votes.Values)
		{
			value.VoteLocked = true;
		}
	}

	public void ClearTempDisabled()
	{
		foreach (VoteArrow value in Votes.Values)
		{
			value.TempDisabled = false;
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcStartCountDown(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStartCountDown called on server.");
		}
		else
		{
			((LevelPortal)obj).RpcStartCountDown();
		}
	}

	protected static void InvokeRpcRpcExitCountDown(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcExitCountDown called on server.");
		}
		else
		{
			((LevelPortal)obj).RpcExitCountDown();
		}
	}

	protected static void InvokeRpcRpcHighLightPortal(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcHighLightPortal called on server.");
		}
		else
		{
			((LevelPortal)obj).RpcHighLightPortal();
		}
	}

	protected static void InvokeRpcRpcNeutralLightPortal(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcNeutralLightPortal called on server.");
		}
		else
		{
			((LevelPortal)obj).RpcNeutralLightPortal();
		}
	}

	protected static void InvokeRpcRpcLowlightPortal(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcLowlightPortal called on server.");
		}
		else
		{
			((LevelPortal)obj).RpcLowlightPortal();
		}
	}

	public void CallRpcStartCountDown()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcStartCountDown called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcStartCountDown);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcStartCountDown");
	}

	public void CallRpcExitCountDown()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcExitCountDown called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcExitCountDown);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcExitCountDown");
	}

	public void CallRpcHighLightPortal()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcHighLightPortal called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcHighLightPortal);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 1, "RpcHighLightPortal");
	}

	public void CallRpcNeutralLightPortal()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcNeutralLightPortal called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcNeutralLightPortal);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcNeutralLightPortal");
	}

	public void CallRpcLowlightPortal()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcLowlightPortal called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcLowlightPortal);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 1, "RpcLowlightPortal");
	}

	static LevelPortal()
	{
		kRpcRpcStartCountDown = 1001157557;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LevelPortal), kRpcRpcStartCountDown, InvokeRpcRpcStartCountDown);
		kRpcRpcExitCountDown = 797345389;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LevelPortal), kRpcRpcExitCountDown, InvokeRpcRpcExitCountDown);
		kRpcRpcHighLightPortal = 201115450;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LevelPortal), kRpcRpcHighLightPortal, InvokeRpcRpcHighLightPortal);
		kRpcRpcNeutralLightPortal = -77515391;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LevelPortal), kRpcRpcNeutralLightPortal, InvokeRpcRpcNeutralLightPortal);
		kRpcRpcLowlightPortal = -1392819980;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LevelPortal), kRpcRpcLowlightPortal, InvokeRpcRpcLowlightPortal);
		NetworkCRC.RegisterBehaviour("LevelPortal", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(levelHasUnlock);
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
			writer.Write(levelHasUnlock);
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
			levelHasUnlock = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			levelHasUnlock = reader.ReadBoolean();
		}
	}

	public override void PreStartClient()
	{
	}
}
