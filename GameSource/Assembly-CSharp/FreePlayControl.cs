using System;
using System.Collections;
using System.Collections.Generic;
using GameEvent;
using UnityEngine;
using UnityEngine.Networking;

public class FreePlayControl : GameControl
{
	private List<Placeable> placedThisPhase = new List<Placeable>();

	private GamePhase tentativePhase;

	public override void ChangeListener(bool adding)
	{
		base.ChangeListener(adding);
		GameEventManager.ChangeListener<FreePlayPlayerSwitchEvent>(this, adding);
		GameEventManager.ChangeListener<GamePlayerRemovedEvent>(this, adding);
	}

	protected override void Update()
	{
		if (deadSession || CleanUpStarted)
		{
			return;
		}
		foreach (GamePlayer item in PlayerQueue)
		{
			if (item != null && item.IsLocalPlayer && item.CharacterInstance != null && item.CharacterInstance.Enabled && item.CharacterInstance.Dead && item.CharacterInstance.LocallyDead && !item.CharacterInstance.isGhost && (!item.CharacterInstance.isZombie || item.CharacterInstance.zombieLocallyDead))
			{
				if (PlayerQueue.Count == 1)
				{
					GameEventManager.SendEvent(new LevelResetEvent());
				}
				if (AllowRespawn)
				{
					resetPlayerCharacter(item.CharacterInstance, sendEvent: true);
				}
			}
		}
		if (base.hasAuthority && (tentativePhase == GamePhase.PLAY || tentativePhase == GamePhase.PLACE) && tentativePhase != base.Phase)
		{
			GamePhase gamePhase = GamePhase.NONE;
			switch (tentativePhase)
			{
			case GamePhase.PLACE:
				gamePhase = GamePhase.PLAY;
				break;
			case GamePhase.PLAY:
				gamePhase = GamePhase.PLACE;
				break;
			}
			bool flag = true;
			foreach (GamePlayer item2 in PlayerQueue)
			{
				if (item2.currentFreePlayPhase != gamePhase)
				{
					flag = false;
					break;
				}
			}
			if (tentativePhase == GamePhase.PLAY)
			{
				foreach (Placeable item3 in placedThisPhase)
				{
					if (item3 != null && !item3.Disabled && !item3.MarkedForDestruction && !item3.Placed)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				GameEventManager.SendEvent(new EndPhaseEvent(base.Phase));
				nextPhase = tentativePhase;
			}
		}
		base.Update();
	}

	private void resetPlayerCharacter(Character c, bool sendEvent)
	{
		if (!(c == null))
		{
			StartCoroutine(resetPlayerCharacterPart2(c, sendEvent));
		}
	}

	private IEnumerator resetPlayerCharacterPart2(Character c, bool sendEvent)
	{
		c.Disable();
		yield return null;
		LevelLayout.SpawnCharacter(c, 0f);
		c.Enable();
		c.Waiting = true;
		yield return null;
		c.Waiting = false;
		c.StartInvincibleTimer(1f);
		SmokePool.Instance.SpawnSmoke(SmokePool.SmokeType.POOF, c.transform.position, 0.5f);
		if (sendEvent)
		{
			GameEventManager.SendEvent(new FreePlayCharacterRespawnEvent(c));
		}
	}

	protected override void SetupStart(GameState.GameMode mode)
	{
		base.SetupStart(mode);
		Debug.Log("Setting up Free Play mode");
		invBookInstance = UnityEngine.Object.Instantiate(InventoryBookPrefab);
		invBookInstance.transform.parent = UICamera.transform;
		invBookInstance.transform.localPosition = new Vector3(0f, 0f, 0f);
		invBookInstance.UiCamera = UICamera;
		invBookInstance.Hide();
		foreach (GamePlayer item in PlayerQueue)
		{
			if (item.IsLocalPlayer)
			{
				((PiecePlacementCursor)item.CursorInstance).InventoryBookMenu = invBookInstance;
				invBookInstance.AddPlayer(item.localNumber, item.networkNumber, item.LocalPlayer.UseController, item.CharacterInstance.CharacterSprite).Disable();
				item.LocalPlayer.FreeplayPhase = StartPhase;
			}
		}
		Vector2 vector = default(Vector2);
		vector.y = UICamera.orthographicSize;
		vector.x = vector.y * UICamera.aspect;
		float num = (float)(360 / PlayerQueue.Count) * (MathF.PI / 180f);
		MainCamera.ForceShowAllPlayer(showAll: true);
		foreach (GamePlayer item2 in PlayerQueue)
		{
			item2.CursorInstance.transform.position = new Vector3(Mathf.Cos(num * (float)item2.TurnOrder) * LevelLayout.CursorSpawnRadius, Mathf.Sin(num * (float)item2.TurnOrder) * LevelLayout.CursorSpawnRadius, 0f) + LevelLayout.CursorSpawnPoint.position;
			item2.CursorInstance.GetComponent<PiecePlacementCursor>().MultiplePlacement = true;
			item2.CursorInstance.GetComponent<PiecePlacementCursor>().KeepPiece = true;
			item2.CursorInstance.UseCamera = MainCamera.GetComponent<Camera>();
			if (StartPhase == GamePhase.PLACE)
			{
				MainCamera.AddTarget(item2.CursorInstance);
				item2.CharacterInstance.Disable();
				item2.CursorInstance.Enable();
			}
			else if (StartPhase == GamePhase.PLAY)
			{
				MainCamera.AddTarget(item2.CharacterInstance);
				resetPlayerCharacter(item2.CharacterInstance, sendEvent: false);
				item2.CursorInstance.Disable();
			}
		}
		StartCoroutine(WaitForPostSetupStart(delegate
		{
			MainCamera.ForceShowAllPlayer(showAll: false);
		}));
		NotifySetupStartDone();
	}

	protected override void DoStart()
	{
		base.DoStart();
		startDelayTimer += Time.unscaledDeltaTime;
		if (startDelayTimer >= StartDelay)
		{
			AkSoundEngine.PostEvent("UI_InGame_Level_Start_ZoomIn", base.gameObject);
			GameEventManager.SendEvent(new EndPhaseEvent(GamePhase.START));
			if (Modifiers.GetInstance().AppliedAndNonDefault)
			{
				GameEventManager.SendEvent(new ModifiersChangedEvent(TabletRule.None));
			}
			if (base.hasAuthority)
			{
				nextPhase = StartPhase;
				tentativePhase = StartPhase;
			}
			else
			{
				nextPhase = GamePhase.WAIT;
			}
			GameControl.LogCurrentModAndRuleInfo();
		}
	}

	protected override void ToPlaceMode()
	{
		if (base.Phase != GamePhase.PLACE)
		{
			AkSoundEngine.PostEvent("Construction_Phase_Freeplay", base.gameObject);
		}
		base.ToPlaceMode();
		StartCoroutine(reEnableAllBlocks());
	}

	private IEnumerator resetAllBlocks()
	{
		yield return new WaitForEndOfFrame();
		foreach (ActiveBlock activeBlock in activeBlocks)
		{
			if (activeBlock != null)
			{
				activeBlock.Reset();
				activeBlock.Active = false;
			}
		}
		yield return new WaitForFixedUpdate();
		foreach (Placeable placedBlock in placedBlocks)
		{
			if (placedBlock != null)
			{
				placedBlock.Enable();
			}
		}
		foreach (ActiveBlock activeBlock2 in activeBlocks)
		{
			if (activeBlock2 != null)
			{
				activeBlock2.Active = true;
			}
		}
	}

	private IEnumerator reEnableAllBlocks()
	{
		yield return new WaitForFixedUpdate();
		foreach (Placeable placedBlock in placedBlocks)
		{
			if (placedBlock != null)
			{
				placedBlock.EnablePlaced();
			}
		}
	}

	protected override void ToPlayMode()
	{
		if (base.Phase != GamePhase.PLAY)
		{
			AkSoundEngine.PostEvent("Plateform_Phase_Freeplay", base.gameObject);
		}
		base.ToPlayMode();
		AkSoundEngine.PostEvent("UI_InGame_Go", base.gameObject);
		placedThisPhase.Clear();
	}

	protected override void AfterAFixedUpdate()
	{
		if (base.Phase == GamePhase.PLAY)
		{
			foreach (ActiveBlock activeBlock in activeBlocks)
			{
				if (!(activeBlock == null) && !activeBlock.Active)
				{
					activeBlock.Active = true;
				}
			}
		}
		base.AfterAFixedUpdate();
	}

	protected override void sendEndAnalytics()
	{
		base.sendEndAnalytics();
		if (base.hasAuthority)
		{
			AnalyticEvent.MatchEndHostEvent(base.MatchGuid, 0, kicks, quits - kicks, Time.timeSinceLevelLoad, roundNumber, complete: false);
		}
		AnalyticEvent.MatchEndClientEvent(base.MatchGuid, ZoomCamera.GlobalCameraTime, ZoomCamera.LocalCameraTime);
	}

	public override void handleEvent(global::GameEvent.GameEvent e)
	{
		base.handleEvent(e);
		Type type = e.GetType();
		if (type == typeof(PiecePlacedEvent))
		{
			PiecePlacedEvent piecePlacedEvent = e as PiecePlacedEvent;
			MultipieceBlock multipieceBlock = piecePlacedEvent.PlacedBlock as MultipieceBlock;
			if (multipieceBlock == null || !multipieceBlock.Separable)
			{
				piecePlacedEvent.PlacedBlock.EnablePlaced();
				destroyMarkedPieces();
			}
			if (base.Phase != GamePhase.PLAY)
			{
				AfterOneFixedUpdate = true;
			}
		}
		if (type == typeof(FreePlayPlayerSwitchEvent))
		{
			FreePlayPlayerSwitchEvent freePlayPlayerSwitchEvent = e as FreePlayPlayerSwitchEvent;
			bool flag = false;
			foreach (GamePlayer item in PlayerQueue)
			{
				if (item.networkNumber == freePlayPlayerSwitchEvent.NetworkNumber)
				{
					if (freePlayPlayerSwitchEvent.Phase == GamePhase.PLACE)
					{
						MainCamera.RemoveTarget(item.CharacterInstance);
						MainCamera.AddTarget(item.CursorInstance);
						if (item.IsLocalPlayer)
						{
							item.CursorInstance.Enable();
							item.CharacterInstance.Disable();
							item.LocalPlayer.FreeplayPhase = GamePhase.PLACE;
						}
						if (base.hasAuthority)
						{
							tentativePhase = GamePhase.PLACE;
						}
						flag = true;
						SmokePool.Instance.SpawnSmoke(SmokePool.SmokeType.POOF, item.CursorInstance.transform.position + new Vector3(-2.5f, -1f, 0f));
					}
					else if (freePlayPlayerSwitchEvent.Phase == GamePhase.PLAY)
					{
						MainCamera.RemoveTarget(item.CursorInstance);
						MainCamera.AddTarget(item.CharacterInstance);
						if (item.IsLocalPlayer)
						{
							item.LocalPlayer.FreeplayPhase = GamePhase.PLAY;
							item.CursorInstance.Disable();
							resetPlayerCharacter(item.CharacterInstance, sendEvent: false);
							SmokePool.Instance.SpawnSmoke(SmokePool.SmokeType.POOF, item.CursorInstance.transform.position + new Vector3(-2.5f, -1f, 0f));
						}
					}
				}
				if (!flag && (item.CursorInstance.Enabled || item.CursorInstance.WaitingForInventory))
				{
					flag = true;
				}
			}
			if (base.hasAuthority && !flag)
			{
				tentativePhase = GamePhase.PLAY;
			}
		}
		if (type == typeof(GamePlayerRemovedEvent))
		{
			GamePlayerRemovedEvent gamePlayerRemovedEvent = e as GamePlayerRemovedEvent;
			Debug.Log("Player removed from game");
			for (int i = 0; i < PlayerQueue.Count; i++)
			{
				GamePlayer gamePlayer = PlayerQueue.Dequeue();
				if (gamePlayer.networkNumber == gamePlayerRemovedEvent.PlayerNetworkNumber)
				{
					if (gamePlayer.CharacterInstance != null)
					{
						if (gamePlayer.CharacterInstance.Enabled)
						{
							SmokePool.Instance.SpawnSmoke(SmokePool.SmokeType.POOF, gamePlayer.CharacterInstance.transform.position);
						}
						MainCamera.RemoveTarget(gamePlayer.CharacterInstance);
						UnityEngine.Object.Destroy(gamePlayer.CharacterInstance.gameObject);
					}
					if (!(gamePlayer.CursorInstance != null))
					{
						continue;
					}
					if (gamePlayer.CursorInstance.Enabled)
					{
						SmokePool.Instance.SpawnSmoke(SmokePool.SmokeType.POOF, gamePlayer.CursorInstance.transform.position);
					}
					if (gamePlayer.CursorInstance is PiecePlacementCursor)
					{
						PiecePlacementCursor piecePlacementCursor = (PiecePlacementCursor)gamePlayer.CursorInstance;
						if (piecePlacementCursor.Piece != null)
						{
							UnityEngine.Object.Destroy(piecePlacementCursor.Piece.gameObject);
						}
					}
					MainCamera.RemoveTarget(gamePlayer.CursorInstance);
					UnityEngine.Object.Destroy(gamePlayer.CursorInstance.gameObject);
				}
				else
				{
					PlayerQueue.Enqueue(gamePlayer);
				}
			}
		}
		if (type == typeof(NetworkClientDisconnectEvent))
		{
			Debug.Log("Client removed from game");
			NetworkClientDisconnectEvent obj = e as NetworkClientDisconnectEvent;
			fadeToLobby();
			GameEventManager.SendEvent(new NetworkClientCleanedUpEvent(obj.ConnectionToClient));
		}
		if (type == typeof(NetworkMessageReceivedEvent))
		{
			NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.CharacterSuccess)
			{
				MsgCharacterSuccess msgCharacterSuccess = (MsgCharacterSuccess)networkMessageReceivedEvent.ReadMessage;
				StartCoroutine(waitForDance(msgCharacterSuccess.NetworkPlayerNumber));
			}
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.PiecePlaced)
			{
				MsgPiecePlaced msgPiecePlaced = (MsgPiecePlaced)networkMessageReceivedEvent.ReadMessage;
				foreach (Placeable allPlaceable in Placeable.AllPlaceables)
				{
					if (allPlaceable != null && allPlaceable.ID == msgPiecePlaced.PieceID)
					{
						placedThisPhase.Add(allPlaceable);
						break;
					}
				}
			}
		}
		if (!(type == typeof(StartPhaseEvent)))
		{
			return;
		}
		StartPhaseEvent startPhaseEvent = e as StartPhaseEvent;
		foreach (GamePlayer item2 in PlayerQueue)
		{
			if (item2.IsLocalPlayer)
			{
				item2.CallCmdSetCurrentFreePlayPhase(startPhaseEvent.Phase);
			}
		}
	}

	private IEnumerator waitForDance(int networkNumber)
	{
		GamePlayer player = null;
		foreach (GamePlayer item in PlayerQueue)
		{
			if (item != null && item.networkNumber == networkNumber)
			{
				player = item;
				break;
			}
		}
		if (player != null)
		{
			player.CharacterInstance.lockSuicide = true;
			float danceTimer = 0f;
			while (danceTimer < DanceTime)
			{
				danceTimer += Time.unscaledDeltaTime;
				yield return null;
			}
			resetPlayerCharacter(player.CharacterInstance, sendEvent: true);
			player.CharacterInstance.lockSuicide = false;
			if (PlayerQueue.Count == 1)
			{
				GameEventManager.SendEvent(new LevelResetEvent());
			}
		}
	}

	private void UNetVersion()
	{
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
