using System.Collections;
using GameEvent;
using UnityEngine;
using UnityEngine.Networking;

public class LobbyCursor : Cursor
{
	public Character Picked;

	public Character over;

	public Collider2D SelectionBox;

	public Transform sizeScaler;

	public float sizeScaleMod = 1f;

	public float LeaveTime;

	protected float leaveTimer;

	public HoldBToGiveUp holdBToIndicatorPrefab;

	protected HoldBToGiveUp holdBIndicatorInstance;

	private bool backJustPressed;

	public bool InGame;

	private Modifiers.CameraFlipModes currentCameraFlipMode;

	private static int kCmdCmdDoDisappearEffect;

	private static int kRpcRpcDoDisappearEffect;

	public override void Awake()
	{
		base.Awake();
		holdBIndicatorInstance = Object.Instantiate(holdBToIndicatorPrefab, base.transform.position, Quaternion.identity);
		holdBIndicatorInstance.transform.parent = sizeScaler;
		holdBIndicatorInstance.transform.localPosition = new Vector3(-1f, -0.8f, 0f);
		holdBIndicatorInstance.transform.localScale = Vector3.one * 0.4f;
		holdBIndicatorInstance.InstantHide();
		holdBIndicatorInstance.multiControllerButton.preferMouseButtons = true;
	}

	public override void Start()
	{
		base.Start();
		if (!base.netId.IsEmpty())
		{
			GameEventManager.SendEvent(new LobbyCursorCreatedEvent(base.gameObject));
		}
	}

	public override void ChangeListener(bool adding)
	{
		base.ChangeListener(adding);
		GameEventManager.ChangeListener<PlayerInGameRuleEvent>(this, adding);
	}

	public override bool hoverState()
	{
		return over != null;
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (ZoomCamera.CurrentZoomCamera != null)
		{
			sizeScaler.localScale = Vector3.one * ZoomCamera.CurrentZoomCamera.fieldOfView * sizeScaleMod;
		}
	}

	protected override void Update()
	{
		base.Update();
		Modifiers instance = Modifiers.GetInstance();
		if (instance.CameraFlipping != currentCameraFlipMode)
		{
			currentCameraFlipMode = instance.CameraFlipping;
			bool flag = base.transform.localScale.x < 0f;
			bool flag2 = base.transform.localScale.y < 0f;
			Vector3 localScale = base.transform.localScale;
			switch (currentCameraFlipMode)
			{
			case Modifiers.CameraFlipModes.None:
				if (flag)
				{
					localScale.x = 0f - localScale.x;
				}
				if (flag2)
				{
					localScale.y = 0f - localScale.y;
				}
				break;
			case Modifiers.CameraFlipModes.FlipX:
				if (!flag)
				{
					localScale.x = 0f - localScale.x;
				}
				if (flag2)
				{
					localScale.y = 0f - localScale.y;
				}
				break;
			case Modifiers.CameraFlipModes.FlipY:
				if (flag)
				{
					localScale.x = 0f - localScale.x;
				}
				if (!flag2)
				{
					localScale.y = 0f - localScale.y;
				}
				break;
			case Modifiers.CameraFlipModes.FlipXY:
				if (!flag)
				{
					localScale.x = 0f - localScale.x;
				}
				if (!flag2)
				{
					localScale.y = 0f - localScale.y;
				}
				break;
			}
			base.transform.localScale = localScale;
		}
		if (!(holdBIndicatorInstance != null))
		{
			return;
		}
		if (back && !disabled && !waitForEnableToBeDone)
		{
			if (backJustPressed || leaveTimer != 0f)
			{
				leaveTimer += Time.unscaledDeltaTime;
			}
			if (leaveTimer == 0f)
			{
				return;
			}
			if (leaveTimer >= LeaveTime)
			{
				holdBIndicatorInstance.Hide();
				leave();
				return;
			}
			if (!holdBIndicatorInstance.Visible)
			{
				holdBIndicatorInstance.Show();
			}
			holdBIndicatorInstance.SetFillAmount(leaveTimer / LeaveTime);
		}
		else
		{
			leaveTimer = 0f;
			holdBIndicatorInstance.Hide();
		}
	}

	public override void ReceiveEvent(InputEvent e)
	{
		if (!Controller.FullScreenComputerIsActive || e.Key == InputEvent.InputKey.Back)
		{
			base.ReceiveEvent(e);
			if (e.Key == InputEvent.InputKey.Back && e.Valueb)
			{
				backJustPressed = e.Changed;
			}
		}
	}

	protected override void OnAccept()
	{
		base.OnAccept();
		bool num = over != null && over.Enabled && !over.Picked;
		bool flag = Picked == null;
		bool flag2 = !LocalPlayer.UseController.IsKeyboard || !Controller.InputFieldIsActive;
		bool flag3 = AssociatedLobbyPlayer == null || !AssociatedLobbyPlayer.characterUnpickRequested;
		bool flag4 = !Controller.FullScreenComputerIsActive;
		if (num && flag && flag2 && flag3 && flag4 && LocalPlayer.AssociatedLobbyPlayer.RequestPickCharacter(over))
		{
			CallCmdDoDisappearEffect();
			AkSoundEngine.PostEvent("UI_Lobby_Cursor_Creation_Poof", base.gameObject);
			MakeMagicSmoke(base.transform, 1f, useCursorColor: true);
			Disable();
			over = null;
		}
	}

	public void MakeMagicSmoke(Transform positionTransform, float uniformScale, bool useCursorColor)
	{
		StartCoroutine(MakeMagicSmokeAfterAFrame(positionTransform, uniformScale, useCursorColor));
	}

	private IEnumerator MakeMagicSmokeAfterAFrame(Transform positionTransform, float uniformScale, bool useCursorColor)
	{
		yield return null;
		SmokePool.Instance.SpawnSmoke(SmokePool.SmokeType.POOF, positionTransform.position, uniformScale, useCursorColor ? cursorColor : Color.white, base.gameObject.layer);
	}

	[Command]
	private void CmdDoDisappearEffect()
	{
		CallRpcDoDisappearEffect();
	}

	[ClientRpc]
	private void RpcDoDisappearEffect()
	{
		if (!base.hasAuthority)
		{
			MakeMagicSmoke(base.transform, 1f, useCursorColor: true);
		}
	}

	public void OnCharacterPickConfirmed(Character requestedCharacter)
	{
		MakeMagicSmoke(requestedCharacter.transform, 0.6f, useCursorColor: false);
		requestedCharacter.PlayerColor = cursorColor;
		requestedCharacter.SetLobbyCollider(enable: true);
		requestedCharacter.Active = true;
		Picked = requestedCharacter;
		requestedCharacter.HoveredCursors.Clear();
	}

	public void OnCharacterPickDenied()
	{
		MakeMagicSmoke(base.transform, 1f, useCursorColor: true);
		Enable();
		LevelSelectController currentLevelSelectController = LobbyManager.instance.CurrentLevelSelectController;
		if (currentLevelSelectController != null)
		{
			currentLevelSelectController.MainCamera.AddTarget(this);
		}
	}

	protected void leave()
	{
		base.OnBack();
		if (over != null && Picked == null)
		{
			over.HoveredCursors.Remove(this);
		}
		if (!(AssociatedLobbyPlayer != null) || AssociatedLobbyPlayer.PlayerStatus != LobbyPlayer.Status.CURSOR)
		{
			return;
		}
		AssociatedLobbyPlayer.PlayerStatus = LobbyPlayer.Status.INACTIVE;
		if (PlayerManager.GetInstance().NumPlayers > 1)
		{
			AssociatedLobbyPlayer.RemovePlayer();
		}
		else
		{
			LevelSelectController currentLevelSelectController = LobbyManager.instance.CurrentLevelSelectController;
			if (currentLevelSelectController != null)
			{
				if (LobbyManager.instance.IsHost)
				{
					NetworkServer.SendToAll(NetMsgTypes.HostEndedGame, new MsgHostEndedGame());
				}
				currentLevelSelectController.TransitionToMainMenu();
			}
		}
		Object.Destroy(base.gameObject);
	}

	public override void Enable()
	{
		base.Enable();
		BoundingBox.enabled = true;
		if (over != null)
		{
			over.HoveredCursors.Remove(this);
			over = null;
		}
		leaveTimer = 0f;
	}

	public override void Disable(bool sound = true, bool showNotebookSprite = false)
	{
		base.Disable(sound, showNotebookSprite);
		BoundingBox.enabled = false;
		if (over != null)
		{
			over.HoveredCursors.Remove(this);
			over = null;
		}
		if (holdBIndicatorInstance != null && holdBIndicatorInstance.Visible)
		{
			holdBIndicatorInstance.Hide();
		}
		leaveTimer = 0f;
	}

	public override void SetColor(Color c)
	{
		base.SetColor(c);
		GetComponentInChildren<SpriteRenderer>().color = c;
	}

	private void OnTriggerExit2D(Collider2D c)
	{
		if (over != null)
		{
			Character componentInChildren = c.gameObject.GetComponentInChildren<Character>();
			if (componentInChildren != null && componentInChildren == over)
			{
				over.HoveredCursors.Remove(this);
				over = null;
			}
		}
	}

	private void OnTriggerStay2D(Collider2D c)
	{
		Character componentInChildren = c.gameObject.GetComponentInChildren<Character>();
		if (componentInChildren != null && !componentInChildren.Picked)
		{
			if (over != null && componentInChildren != over)
			{
				over.HoveredCursors.Remove(this);
			}
			over = componentInChildren;
			if (!over.HoveredCursors.Contains(this))
			{
				over.HoveredCursors.Add(this);
			}
		}
		if (over == null && componentInChildren != null && !componentInChildren.Picked)
		{
			over = componentInChildren;
			if (!over.HoveredCursors.Contains(this))
			{
				over.HoveredCursors.Add(this);
			}
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (over != null)
		{
			over.HoveredCursors.Remove(this);
		}
	}

	public override void handleEvent(global::GameEvent.GameEvent e)
	{
		if (InGame)
		{
			return;
		}
		base.handleEvent(e);
		if (!(e.GetType() == typeof(PlayerInGameRuleEvent)))
		{
			return;
		}
		PlayerInGameRuleEvent playerInGameRuleEvent = e as PlayerInGameRuleEvent;
		if (playerInGameRuleEvent.PlayerNumber == networkNumber && AssociatedLobbyPlayer.PlayerStatus == LobbyPlayer.Status.CURSOR)
		{
			if (playerInGameRuleEvent.Entered)
			{
				Disable(sound: true, showNotebookSprite: true);
			}
			else
			{
				Enable();
			}
		}
	}

	protected override void InitControllerButtons(Controller usedController)
	{
		base.InitControllerButtons(usedController);
		holdBIndicatorInstance.SetLocalController(usedController);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdDoDisappearEffect(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDoDisappearEffect called on client.");
		}
		else
		{
			((LobbyCursor)obj).CmdDoDisappearEffect();
		}
	}

	public void CallCmdDoDisappearEffect()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdDoDisappearEffect called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdDoDisappearEffect();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdDoDisappearEffect);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdDoDisappearEffect");
	}

	protected static void InvokeRpcRpcDoDisappearEffect(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDoDisappearEffect called on server.");
		}
		else
		{
			((LobbyCursor)obj).RpcDoDisappearEffect();
		}
	}

	public void CallRpcDoDisappearEffect()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcDoDisappearEffect called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcDoDisappearEffect);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcDoDisappearEffect");
	}

	static LobbyCursor()
	{
		kCmdCmdDoDisappearEffect = -1440207747;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyCursor), kCmdCmdDoDisappearEffect, InvokeCmdCmdDoDisappearEffect);
		kRpcRpcDoDisappearEffect = -46698649;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LobbyCursor), kRpcRpcDoDisappearEffect, InvokeRpcRpcDoDisappearEffect);
		NetworkCRC.RegisterBehaviour("LobbyCursor", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}

	public override void PreStartClient()
	{
		base.PreStartClient();
	}
}
