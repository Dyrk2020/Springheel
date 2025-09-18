using System.Collections;
using System.Collections.Generic;
using GameEvent;
using I2.Loc;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TabletRulesScreen : TabletScreen, IGameEventListener
{
	public RectTransform rulesetInfoContainer;

	public TabletTextLabel rulesetNameText;

	public TabletTextLabel rulesetDescriptionText;

	public TabletButton rulesetSaveButton;

	public TabletButton rulesetResetButton;

	public TabletButton rulesetLoadButton;

	public TabletButton rulesetNameButton;

	public TabletDisableGroup[] clientDisableGroups;

	public TabletSubdialogController subdialogController;

	public RectTransform mainRulesPageSubdialog;

	public TabletModalOverlay modalOverlay;

	public TabletPresetSelectOverlay presetOverlay;

	public TabletTextLabel pointsToWinLabel;

	public TabletTextLabel lengthLimitLabel;

	public TabletTextLabel placementTimerLabel;

	public TabletTextLabel runTimeLimitLabel;

	public TabletTextLabel doublePartyBoxLabel;

	public TabletTextLabel piecesPerRoundLabel;

	public TabletTextLabel respawnModeLabel;

	public TabletTextLabel partyBoxModeLabel;

	public Transform pointSettingsSubdialog;

	private Dictionary<PointBlock.pointBlockType, TabletPointWidget> pointWidgets;

	public RectTransform blockSettingsSubdialog;

	public TabletBlockList tabletBlockList;

	public TabletCheckbox showPercentCheckbox;

	public RectTransform modifierSettingsSubdialog;

	public TabletSimpleScroll modifierSettingsScroller;

	public TabletDisableGroup ModifierDisableGroup;

	public TabletTextLabel modifierGravityLabel;

	public TabletTextLabel modifierJumpSpeedLabel;

	public TabletTextLabel modifierSprintSpeedLabel;

	public TabletTextLabel modifierWallJumpsDisabledLabel;

	public TabletTextLabel modifierWallSlidesDisabledLabel;

	public TabletTextLabel modifierGameSpeedLabel;

	public TabletTextLabel modifierDanceInvincibilityLabel;

	public TabletTextLabel modifierMirrorControlsLabel;

	public TabletTextLabel modifierPlatformSpeedLabel;

	public TabletTextLabel modifierRateOfFireLabel;

	public TabletTextLabel modifierMultiJumpLabel;

	public TabletTextLabel modifierProjectilesExplodeLabel;

	public TabletTextLabel modifierCharacterSizeLabel;

	public TabletTextLabel modifierJetpackMode;

	public TabletTextLabel modifierPostDeathBehaviorModeLabel;

	public TabletTextLabel modifierMirrorLevelLabel;

	public TabletTextLabel modifierDoomsdayMeteorsLabel;

	public TabletTextLabel modifierDoomsdayLavaLabel;

	public TabletTextLabel modifierPlayerPlayerCollisionsLabel;

	public TabletTextLabel modifierProjectileSpeedLabel;

	public TabletTextLabel modifierInvisibilityLabel;

	public TabletCheckbox previewModifiersToggle;

	public TabletCheckbox forceLobbyModifiersToggle;

	public TabletCheckbox competiveRandomizerToggle;

	public TabletTextLabel modifierFrictionlessLabel;

	public int regularModiferTextSize = 50;

	public int smallModifierTextSize = 25;

	private RectTransform lastEnteredSubdialog;

	private IEnumerator rulesetNameAnim;

	public bool rulesDirty;

	private bool initialized;

	public TabletBlockList TabletBlockList;

	public Text HostControlMessage;

	public void Initialize()
	{
		if (initialized)
		{
			return;
		}
		initialized = true;
		ChangeListener(adding: true);
		pointWidgets = new Dictionary<PointBlock.pointBlockType, TabletPointWidget>();
		TabletPointWidget[] componentsInChildren = pointSettingsSubdialog.GetComponentsInChildren<TabletPointWidget>(includeInactive: true);
		foreach (TabletPointWidget tabletPointWidget in componentsInChildren)
		{
			pointWidgets[tabletPointWidget.pointType] = tabletPointWidget;
		}
		GameSettings instance = GameSettings.GetInstance();
		UpdateAllRuleButtons();
		UpdateAllPointButtons();
		UpdateAllModifierButtons();
		bool flag = LobbyManager.instance != null && !LobbyManager.instance.IsHost;
		if (flag)
		{
			TabletDisableGroup[] array = clientDisableGroups;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetDisabled(disabled: true);
			}
			HostControlMessage.gameObject.SetActive(value: true);
			rulesetNameButton.SetInteractable(interactable: false);
		}
		else
		{
			UpdateRulesetText(instance.GetCurrentRuleset());
			HostControlMessage.gameObject.SetActive(value: false);
		}
		if (LobbyManager.instance != null && LobbyManager.instance.CurrentGameController != null)
		{
			forceLobbyModifiersToggle.gameObject.SetActive(value: false);
			previewModifiersToggle.gameObject.SetActive(value: false);
		}
		tabletBlockList.Initialize(flag);
	}

	private void Start()
	{
		Initialize();
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public virtual void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding);
		GameEventManager.ChangeListener<SpecialUIEvent>(this, adding);
		GameEventManager.ChangeListener<LanguageChangeEvent>(this, adding);
		GameEventManager.ChangeListener<CheatKonamiEvent>(this, adding);
	}

	public virtual void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e is NetworkMessageReceivedEvent networkMessageReceivedEvent)
		{
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.SetBlockFrequency || networkMessageReceivedEvent.Message.msgType == NetMsgTypes.SetAllBlockFrequencies || networkMessageReceivedEvent.Message.msgType == NetMsgTypes.SendAllBlockFrequencies)
			{
				tabletBlockList.PassRuleEvent(e);
			}
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.RulesetDirty && !LobbyManager.instance.IsHost)
			{
				if ((networkMessageReceivedEvent.ReadMessage as MsgRulesetDirty).dirty)
				{
					MarkRulesDirty(force: true);
				}
				else
				{
					ClearDirtyBit();
				}
			}
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.ApplyRuleset && !LobbyManager.instance.IsHost)
			{
				MsgApplyRuleset rulesetMsg = networkMessageReceivedEvent.ReadMessage as MsgApplyRuleset;
				if (LobbyManager.instance != null && LobbyManager.instance.CurrentLevelSelectController != null)
				{
					LobbyManager.instance.CurrentLevelSelectController.ExecuteOnRuleBookInitialized(delegate
					{
						ApplyRulesetFromMessage(rulesetMsg);
					});
				}
				else
				{
					ApplyRulesetFromMessage(rulesetMsg);
				}
			}
			if (networkMessageReceivedEvent.Message.msgType != NetMsgTypes.GameRuleSet)
			{
				return;
			}
			MsgGameRuleSet msgGameRuleSet = (MsgGameRuleSet)networkMessageReceivedEvent.ReadMessage;
			int buttonIndex = 0;
			if (msgGameRuleSet.NewRule != TabletRule.None && !LobbyManager.instance.IsHost)
			{
				GameSettings instance = GameSettings.GetInstance();
				Modifiers instance2 = Modifiers.GetInstance();
				switch (msgGameRuleSet.NewRule)
				{
				case TabletRule.PointsToWin:
					instance.MaxScore = msgGameRuleSet.Value;
					break;
				case TabletRule.LengthLimit:
					instance.GameLimitType = (GameLimitType)msgGameRuleSet.Value;
					switch (instance.GameLimitType)
					{
					case GameLimitType.ROUNDS:
						instance.MaxRounds = msgGameRuleSet.Value2;
						break;
					case GameLimitType.TIME:
						instance.MaxTime = msgGameRuleSet.Value2;
						break;
					}
					break;
				case TabletRule.PlacementTimer:
					instance.PlaceTime = msgGameRuleSet.Value;
					instance.UsePlaceTimer = instance.PlaceTime > 0f;
					break;
				case TabletRule.RunTimeLimit:
					instance.RunTimerLimit = msgGameRuleSet.Value;
					break;
				case TabletRule.DoublePartyBox:
					instance.DoublePartyBox = (DoublePartyBox)msgGameRuleSet.Value;
					break;
				case TabletRule.PiecesPerRound:
					instance.CreativePiecesPerRound = msgGameRuleSet.Value;
					break;
				case TabletRule.PointValue:
					instance.SetPointTypeValue((PointBlock.pointBlockType)msgGameRuleSet.Value, msgGameRuleSet.Value2);
					buttonIndex = msgGameRuleSet.Value;
					break;
				case TabletRule.PointEnabled:
					instance.SetPointTypeEnabled((PointBlock.pointBlockType)msgGameRuleSet.Value, msgGameRuleSet.Value2 != 0);
					instance.SetAlwaysAwardPointType((PointBlock.pointBlockType)msgGameRuleSet.Value, msgGameRuleSet.Value2 == 2);
					buttonIndex = msgGameRuleSet.Value;
					break;
				case TabletRule.ModifierGravity:
					instance2.GravityMode = msgGameRuleSet.Value;
					buttonIndex = msgGameRuleSet.Value;
					break;
				case TabletRule.ModifierJumpSpeed:
					instance2.JumpSpeedMode = msgGameRuleSet.Value;
					buttonIndex = msgGameRuleSet.Value;
					break;
				case TabletRule.ModifierSprintSpeed:
					instance2.SprintSpeedMode = msgGameRuleSet.Value;
					buttonIndex = msgGameRuleSet.Value;
					break;
				case TabletRule.ModifierWallJumpsDisabled:
					instance2.wallJumpsDisabled = msgGameRuleSet.Valueb;
					break;
				case TabletRule.ModifierWallSlidesDisabled:
					instance2.wallSlidesDisabled = msgGameRuleSet.Valueb;
					break;
				case TabletRule.ModifierGameSpeed:
					instance2.GameSpeedMode = msgGameRuleSet.Value;
					Time.timeScale = instance2.GameSpeed;
					break;
				case TabletRule.ModifierDanceInvincibility:
					instance2.danceInvincibility = msgGameRuleSet.Valueb;
					break;
				case TabletRule.ModifierInvisibility:
					instance2.invisibilityMode = msgGameRuleSet.Value;
					break;
				case TabletRule.ModifierMirrorControls:
					instance2.mirrorControls = msgGameRuleSet.Valueb;
					break;
				case TabletRule.ModifierPlatformSpeed:
					instance2.PlatformSpeedMode = msgGameRuleSet.Value;
					break;
				case TabletRule.ModifierRateOfFire:
					instance2.RateOfFireMode = msgGameRuleSet.Value;
					instance2.OnModifiersDynamicChange();
					break;
				case TabletRule.ModifierMultiJump:
					instance2.MultiJumpMode = msgGameRuleSet.Value;
					break;
				case TabletRule.ModifierProjectilesExplode:
					instance2.ProjectileExplosionMode = msgGameRuleSet.Value;
					break;
				case TabletRule.ModifierCharacterSize:
					instance2.CharacterSizeMode = msgGameRuleSet.Value;
					instance2.OnModifiersDynamicChange();
					break;
				case TabletRule.ModifierJetpackMode:
					instance2.jetpackMode = msgGameRuleSet.Valueb;
					break;
				case TabletRule.ModifierPostDeathBehaviorMode:
					instance2.PostDeathBehaviorMode = msgGameRuleSet.Value;
					break;
				case TabletRule.ModifierMirrorLevel:
					instance2.CameraFlipMode = msgGameRuleSet.Value;
					break;
				case TabletRule.ModifierDoomsdayMeteors:
					instance2.DoomsdayMeteorsMode = msgGameRuleSet.Value;
					break;
				case TabletRule.ModifierDoomsdayLava:
					instance2.DoomsdayLavaMode = msgGameRuleSet.Value;
					break;
				case TabletRule.ModifierPlayerPlayerCollisions:
					instance2.playerPlayerCollisions = msgGameRuleSet.Valueb;
					break;
				case TabletRule.ModifierProjectileSpeed:
					instance2.ProjectileSpeedMode = msgGameRuleSet.Value;
					break;
				case TabletRule.ModifierPreviewModsInTreehouse:
					instance2.modsPreview = msgGameRuleSet.Valueb;
					if (LobbyManager.instance != null && LobbyManager.instance.CurrentLevelSelectController != null)
					{
						instance2.modsApplied = instance2.modsPreview;
					}
					instance2.OnModifiersDynamicChange();
					break;
				case TabletRule.ModifierForceLobbyModifiers:
					instance2.forceLobbyModifiers = msgGameRuleSet.Valueb;
					break;
				case TabletRule.RespawnMode:
				{
					instance.respawnMode = (RespawnMode)msgGameRuleSet.Value;
					RespawnMode respawnMode = instance.respawnMode;
					if ((uint)(respawnMode - 1) <= 2u)
					{
						instance.numRespawns = msgGameRuleSet.Value2;
					}
					break;
				}
				case TabletRule.PartyBoxMode:
					instance.partyBoxMode = (PartyBoxMode)msgGameRuleSet.Value;
					break;
				case TabletRule.OnlineSettingsAFKKickTime:
					instance.CurrentLobbyAFKAutoKickTime = msgGameRuleSet.Value;
					if (LobbyManager.instance != null && LobbyManager.instance.IsHost)
					{
						instance.AFKAutoKickTime = msgGameRuleSet.Value;
					}
					break;
				case TabletRule.ModifierFrictionless:
					instance2.frictionless = msgGameRuleSet.Valueb;
					break;
				case TabletRule.CompetitiveRandomizer:
					instance.competitiveRandomizer = msgGameRuleSet.Valueb;
					break;
				}
				UpdateButtonValue(msgGameRuleSet.NewRule, buttonIndex);
			}
			if (TabletRuleUtility.IsRuleModifierChange(msgGameRuleSet.NewRule))
			{
				GameEventManager.SendEvent(new ModifiersChangedEvent(msgGameRuleSet.NewRule));
			}
		}
		else if (e is SpecialUIEvent specialUIEvent)
		{
			if (specialUIEvent.SpecialUIType == SpecialUIEvent.SpecialUI.NOITEMSELECTED)
			{
				UserMessageManager.Instance.UserMessage(LocalizationManager.GetTranslation("RuleBook/OneBlockMessage"), 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
			}
		}
		else
		{
			if (e is LanguageChangeEvent)
			{
				GameSettings instance3 = GameSettings.GetInstance();
				UpdateRulesetText(instance3.GetCurrentRuleset());
				UpdateAllModifierButtons();
				UpdateAllRuleButtons();
			}
			if (e is CheatKonamiEvent)
			{
				RespawnMode respawnMode2 = GameSettings.GetInstance().respawnMode;
				respawnModeLabel.text = GameSettings.GetRespawnModeValueString((respawnMode2 == RespawnMode.Off) ? RespawnMode.RespawnsPerMatch : respawnMode2, 30);
				SetLineModified(respawnModeLabel, isDefaultValue: false);
			}
		}
	}

	private void ApplyRulesetFromMessage(MsgApplyRuleset rulesetMsg)
	{
		GameSettings instance = GameSettings.GetInstance();
		if (rulesetMsg.premadeIdx == -1)
		{
			GameRulePreset gameRulePreset = ScriptableObject.CreateInstance<GameRulePreset>();
			gameRulePreset.IsPremade = false;
			gameRulePreset.LoadRulesetFromXML(QuickSaver.GetXmlDocFromString(rulesetMsg.rulesetXML));
			bool num = rulesetMsg.applyMods && !gameRulePreset.mods.IsCurrentlyApplied();
			instance.ApplyTemporaryRuleset(gameRulePreset, rulesetMsg.applyRules, rulesetMsg.applyPoints, rulesetMsg.applyBlocks, rulesetMsg.applyMods);
			bool flag = !rulesetMsg.applyRules || !rulesetMsg.applyPoints || !rulesetMsg.applyBlocks || !rulesetMsg.applyMods;
			OnPresetLoaded(flag ? null : gameRulePreset, flag);
			AnimateChangeToRuleset(flag ? null : gameRulePreset);
			if (!rulesetMsg.temporary)
			{
				ShowPresetLoadMessage(gameRulePreset, rulesetMsg.applyRules, rulesetMsg.applyPoints, rulesetMsg.applyBlocks, rulesetMsg.applyMods);
			}
			if (num)
			{
				GameEventManager.SendEvent(new ModifiersChangedEvent(TabletRule.None));
			}
			Object.Destroy(gameRulePreset);
		}
		else
		{
			GameRulePreset rulesetByIndex = instance.GetRulesetByIndex(rulesetMsg.premadeIdx);
			bool num2 = rulesetMsg.applyMods && !rulesetByIndex.mods.IsCurrentlyApplied();
			if (rulesetMsg.applyRules && rulesetMsg.applyPoints && rulesetMsg.applyBlocks && rulesetMsg.applyMods)
			{
				instance.ToPreset(rulesetMsg.premadeIdx);
				OnPresetLoaded(instance.GetCurrentRuleset(), partial: false);
			}
			else
			{
				instance.PartialLoadPreset(rulesetMsg.premadeIdx, rulesetMsg.applyRules, rulesetMsg.applyPoints, rulesetMsg.applyBlocks, rulesetMsg.applyMods);
				OnPresetLoaded(null, partial: true);
			}
			if (!rulesetMsg.temporary)
			{
				ShowPresetLoadMessage(rulesetByIndex, rulesetMsg.applyRules, rulesetMsg.applyPoints, rulesetMsg.applyBlocks, rulesetMsg.applyMods);
			}
			if (num2)
			{
				GameEventManager.SendEvent(new ModifiersChangedEvent(TabletRule.None));
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (rulesetNameAnim != null && !rulesetNameAnim.MoveNext())
		{
			rulesetNameAnim = null;
		}
	}

	public void OnClickNextGameMode()
	{
		if (LobbyManager.instance != null && LobbyManager.instance.HasPlayersLockedForLoad)
		{
			Debug.LogWarning("Ignored button press - we are locked for load!");
			return;
		}
		GameState.GameMode gameMode = GameSettings.GetInstance().GameMode;
		MsgSwitchToMode msgSwitchToMode = new MsgSwitchToMode();
		GameState.GameMode mode = (msgSwitchToMode.toMode = GameState.NextMode(gameMode));
		NetworkManager.singleton.client.Send(NetMsgTypes.SwitchToMode, msgSwitchToMode);
		GameEventManager.SendEvent(new GameModeSetEvent(mode));
	}

	public void OnClickPrevGameMode()
	{
		if (LobbyManager.instance != null && LobbyManager.instance.HasPlayersLockedForLoad)
		{
			Debug.LogWarning("Ignored button press - we are locked for load!");
			return;
		}
		GameState.GameMode gameMode = GameSettings.GetInstance().GameMode;
		MsgSwitchToMode msgSwitchToMode = new MsgSwitchToMode();
		GameState.GameMode mode = (msgSwitchToMode.toMode = GameState.PreviousMode(gameMode));
		NetworkManager.singleton.client.Send(NetMsgTypes.SwitchToMode, msgSwitchToMode);
		GameEventManager.SendEvent(new GameModeSetEvent(mode));
	}

	public void OnSelectGameMode(GameState.GameMode gameMode)
	{
		if (LobbyManager.instance != null && LobbyManager.instance.HasPlayersLockedForLoad)
		{
			Debug.LogWarning("Ignored button press - we are locked for load!");
			return;
		}
		MsgSwitchToMode msgSwitchToMode = new MsgSwitchToMode();
		msgSwitchToMode.toMode = gameMode;
		NetworkManager.singleton.client.Send(NetMsgTypes.SwitchToMode, msgSwitchToMode);
		GameEventManager.SendEvent(new GameModeSetEvent(gameMode));
	}

	public void OnClickPlusPoint(PointBlock.pointBlockType pointType)
	{
		if (!(LobbyManager.instance != null) || LobbyManager.instance.IsHost)
		{
			GameSettings instance = GameSettings.GetInstance();
			int num = instance.PointTypeValue(pointType);
			num = Mathf.Min(num + instance.pointValueIncrement, instance.maxPointValue);
			instance.SetPointTypeValue(pointType, num);
			TabletModalOverlay.BroadcastRuleChange(TabletRule.PointValue, (int)pointType, num);
		}
	}

	public void OnClickMinusPoint(PointBlock.pointBlockType pointType)
	{
		if (!(LobbyManager.instance != null) || LobbyManager.instance.IsHost)
		{
			GameSettings instance = GameSettings.GetInstance();
			int num = instance.PointTypeValue(pointType);
			num = Mathf.Max(num - instance.pointValueIncrement, instance.minPointValue);
			instance.SetPointTypeValue(pointType, num);
			TabletModalOverlay.BroadcastRuleChange(TabletRule.PointValue, (int)pointType, num);
		}
	}

	public override bool OnPressBack(PickCursor pickCursor)
	{
		if (modalOverlay.IsOpen || modalOverlay.IsOpening)
		{
			modalOverlay.OnCancel();
			return true;
		}
		if (presetOverlay.isOpen)
		{
			presetOverlay.OnPressBack(pickCursor);
			return true;
		}
		if (subdialogController.currentSubdialog == blockSettingsSubdialog && GameSettings.GetInstance().AvailableBlocks == 0)
		{
			return true;
		}
		if (!subdialogController.IsOnMainSubdialog)
		{
			subdialogController.PopSubdialog();
			return true;
		}
		return base.OnPressBack(pickCursor);
	}

	public override void OnModalOverlayClosed()
	{
		base.OnModalOverlayClosed();
		UpdateButtonValue(modalOverlay.currentOverlayType);
	}

	public void OnClickPointTypeEnabled(PointBlock.pointBlockType pointType)
	{
		if (!(LobbyManager.instance != null) || LobbyManager.instance.IsHost)
		{
			bool flag = GameSettings.GetInstance().PointTypeEnabled(pointType);
			if (flag)
			{
				AkSoundEngine.PostEvent("UI_UPad_PointsSettings_Disable", base.gameObject);
			}
			else
			{
				AkSoundEngine.PostEvent("UI_UPad_PointsSettings_Enable", base.gameObject);
			}
			SetPointEnabled(pointType, !flag);
		}
	}

	public void OnPointTypeModalClosed()
	{
		UpdateButtonValue(modalOverlay.currentOverlayType, (int)modalOverlay.currentPointType);
	}

	private void SetLineModified(TabletTextLabel valueLabel, bool isDefaultValue)
	{
		TabletTextLabel[] componentsInChildren;
		if ((isDefaultValue && valueLabel.labelType != TabletTextLabel.LabelType.CustomSize) || (!isDefaultValue && valueLabel.labelType != TabletTextLabel.LabelType.CustomSize_Modified))
		{
			componentsInChildren = valueLabel.transform.parent.GetComponentsInChildren<TabletTextLabel>();
			foreach (TabletTextLabel tabletTextLabel in componentsInChildren)
			{
				if (isDefaultValue)
				{
					if (tabletTextLabel.labelType != TabletTextLabel.LabelType.CustomSize)
					{
						tabletTextLabel.labelType = TabletTextLabel.LabelType.CustomSize;
						tabletTextLabel.UpdateTextColorAndSize();
					}
				}
				else if (tabletTextLabel.labelType != TabletTextLabel.LabelType.CustomSize_Modified)
				{
					tabletTextLabel.labelType = TabletTextLabel.LabelType.CustomSize_Modified;
					tabletTextLabel.UpdateTextColorAndSize();
				}
			}
		}
		componentsInChildren = valueLabel.transform.parent.GetComponentsInChildren<TabletTextLabel>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].UpdateDynamicText();
		}
	}

	private bool IsPointWidgetDefault(TabletPointWidget pointWidget)
	{
		GameSettings instance = GameSettings.GetInstance();
		GameRulePreset defaultRuleset = instance.DefaultRuleset;
		bool num = instance.PointTypeEnabled(pointWidget.pointType);
		bool flag = defaultRuleset.PointTypeEnabled(pointWidget.pointType);
		if (num == flag)
		{
			int num2 = instance.PointTypeValue(pointWidget.pointType);
			int num3 = defaultRuleset.PointTypeValue(pointWidget.pointType);
			if (num2 == num3)
			{
				bool num4 = instance.AlwaysAwardPointType(pointWidget.pointType);
				bool flag2 = defaultRuleset.AlwaysAwardPointType(pointWidget.pointType);
				return num4 == flag2;
			}
			return false;
		}
		return false;
	}

	private void UpdateButtonValue(TabletRule overlayType, int buttonIndex = 0, bool textSizeModifier = false)
	{
		GameSettings instance = GameSettings.GetInstance();
		Modifiers instance2 = Modifiers.GetInstance();
		GameRulePreset defaultRuleset = instance.DefaultRuleset;
		switch (overlayType)
		{
		case TabletRule.PointsToWin:
			pointsToWinLabel.text = GameSettings.GetPointLimitValueString(instance.MaxScore);
			SetLineModified(pointsToWinLabel, instance.MaxScore == defaultRuleset.MaxScore);
			break;
		case TabletRule.LengthLimit:
			lengthLimitLabel.text = GameSettings.GetLengthLimitValueString(instance.GameLimitType, instance.MaxRounds, instance.MaxTime);
			SetLineModified(lengthLimitLabel, instance.GameLimitType == defaultRuleset.GameLimitType && instance.MaxRounds == defaultRuleset.MaxRounds);
			break;
		case TabletRule.PlacementTimer:
			placementTimerLabel.text = GameSettings.GetPlacementTimerValueString(instance.UsePlaceTimer, instance.PlaceTime);
			SetLineModified(placementTimerLabel, instance.UsePlaceTimer == defaultRuleset.UsePlaceTimer && instance.PlaceTime == defaultRuleset.PlaceTime);
			break;
		case TabletRule.RunTimeLimit:
			runTimeLimitLabel.text = GameSettings.GetRunTimerValueString(instance.RunTimerLimit);
			SetLineModified(runTimeLimitLabel, instance.RunTimerLimit == defaultRuleset.RunTimerLimit);
			break;
		case TabletRule.DoublePartyBox:
			doublePartyBoxLabel.text = GameSettings.GetDoublePartyBoxValueString(instance.DoublePartyBox);
			SetLineModified(doublePartyBoxLabel, instance.DoublePartyBox == defaultRuleset.DoublePartyBox);
			break;
		case TabletRule.PiecesPerRound:
			piecesPerRoundLabel.text = instance.CreativePiecesPerRound.ToString();
			SetLineModified(piecesPerRoundLabel, instance.CreativePiecesPerRound == defaultRuleset.CreativePiecesPerRound);
			break;
		case TabletRule.PointValue:
		{
			if (pointWidgets.TryGetValue((PointBlock.pointBlockType)buttonIndex, out var value2))
			{
				int pointValue = (instance.PointTypeEnabled((PointBlock.pointBlockType)buttonIndex) ? instance.PointTypeValue((PointBlock.pointBlockType)buttonIndex) : 0);
				value2.SetPointValue(pointValue);
				SetLineModified(value2.blockName, IsPointWidgetDefault(value2));
			}
			break;
		}
		case TabletRule.PointEnabled:
		{
			if (!pointWidgets.TryGetValue((PointBlock.pointBlockType)buttonIndex, out var value))
			{
				break;
			}
			bool num = instance.PointTypeEnabled((PointBlock.pointBlockType)buttonIndex);
			bool flag = instance.AlwaysAwardPointType((PointBlock.pointBlockType)buttonIndex);
			int num2 = 0;
			if (num)
			{
				num2++;
				if (flag)
				{
					num2++;
				}
			}
			value.SetPointEnabled(num2);
			SetLineModified(value.blockName, IsPointWidgetDefault(value));
			break;
		}
		case TabletRule.ModifierGravity:
			modifierGravityLabel.text = Modifiers.GetGravityValueString(instance2.GravityMode);
			SetLineModified(modifierGravityLabel, instance2.GravityMode == defaultRuleset.mods.GravityMode);
			break;
		case TabletRule.ModifierJumpSpeed:
			modifierJumpSpeedLabel.text = Modifiers.GetJumpSpeedValueString(instance2.JumpSpeedMode);
			SetLineModified(modifierJumpSpeedLabel, instance2.JumpSpeedMode == defaultRuleset.mods.JumpSpeedMode);
			break;
		case TabletRule.ModifierSprintSpeed:
			modifierSprintSpeedLabel.text = Modifiers.GetSprintSpeedValueString(instance2.SprintSpeedMode);
			SetLineModified(modifierSprintSpeedLabel, instance2.SprintSpeedMode == defaultRuleset.mods.SprintSpeedMode);
			break;
		case TabletRule.ModifierWallJumpsDisabled:
			modifierWallJumpsDisabledLabel.text = Modifiers.GetOnOffValueString(!instance2.wallJumpsDisabled);
			SetLineModified(modifierWallJumpsDisabledLabel, instance2.wallJumpsDisabled == defaultRuleset.mods.WallJumpsDisabled);
			break;
		case TabletRule.ModifierWallSlidesDisabled:
			modifierWallSlidesDisabledLabel.text = Modifiers.GetOnOffValueString(instance2.wallSlidesDisabled);
			SetLineModified(modifierWallSlidesDisabledLabel, instance2.wallSlidesDisabled == defaultRuleset.mods.WallSlidesDisabled);
			break;
		case TabletRule.ModifierGameSpeed:
			modifierGameSpeedLabel.text = Modifiers.GetSprintSpeedValueString(instance2.GameSpeedMode);
			SetLineModified(modifierGameSpeedLabel, instance2.GameSpeedMode == defaultRuleset.mods.GameSpeedMode);
			break;
		case TabletRule.ModifierDanceInvincibility:
			modifierDanceInvincibilityLabel.text = Modifiers.GetOnOffValueString(instance2.danceInvincibility);
			SetLineModified(modifierDanceInvincibilityLabel, instance2.danceInvincibility == defaultRuleset.mods.DanceInvincibility);
			break;
		case TabletRule.ModifierInvisibility:
			modifierInvisibilityLabel.text = Modifiers.GetInvisibilityModeValueString(instance2.invisibilityMode);
			SetLineModified(modifierInvisibilityLabel, instance2.invisibilityMode == defaultRuleset.mods.InvisibilityMode);
			break;
		case TabletRule.ModifierMirrorControls:
			modifierMirrorControlsLabel.text = Modifiers.GetOnOffValueString(instance2.mirrorControls);
			SetLineModified(modifierMirrorControlsLabel, instance2.mirrorControls == defaultRuleset.mods.MirrorControls);
			break;
		case TabletRule.ModifierPlatformSpeed:
			modifierPlatformSpeedLabel.text = Modifiers.GetSprintSpeedValueString(instance2.PlatformSpeedMode);
			SetLineModified(modifierPlatformSpeedLabel, instance2.PlatformSpeedMode == defaultRuleset.mods.PlatformSpeedMode);
			break;
		case TabletRule.ModifierRateOfFire:
			modifierRateOfFireLabel.text = Modifiers.GetSprintSpeedValueString(instance2.RateOfFireMode);
			SetLineModified(modifierRateOfFireLabel, instance2.RateOfFireMode == defaultRuleset.mods.RateOfFireMode);
			break;
		case TabletRule.ModifierMultiJump:
			modifierMultiJumpLabel.text = Modifiers.GetMultiJumpValueString(instance2.MultiJumpMode);
			SetLineModified(modifierMultiJumpLabel, instance2.MultiJumpMode == defaultRuleset.mods.MultiJumpMode);
			break;
		case TabletRule.ModifierProjectilesExplode:
			modifierProjectilesExplodeLabel.text = Modifiers.GetProjectileExplosionValueString(instance2.ProjectileExplosionMode);
			SetLineModified(modifierProjectilesExplodeLabel, instance2.ProjectileExplosionMode == defaultRuleset.mods.ProjectileExplosionMode);
			break;
		case TabletRule.ModifierCharacterSize:
			modifierCharacterSizeLabel.text = Modifiers.GetCharacterSizeValueString(instance2.CharacterSizeMode);
			SetLineModified(modifierCharacterSizeLabel, instance2.CharacterSizeMode == defaultRuleset.mods.CharacterSizeMode);
			break;
		case TabletRule.ModifierJetpackMode:
			modifierJetpackMode.text = Modifiers.GetOnOffValueString(instance2.jetpackMode);
			SetLineModified(modifierJetpackMode, instance2.jetpackMode == defaultRuleset.mods.JetpackMode);
			break;
		case TabletRule.ModifierPostDeathBehaviorMode:
			modifierPostDeathBehaviorModeLabel.text = Modifiers.GetPostDeathBehaviorValueString(instance2.PostDeathBehaviorMode);
			SetLineModified(modifierPostDeathBehaviorModeLabel, instance2.PostDeathBehaviorMode == defaultRuleset.mods.PostDeathBehaviorMode);
			break;
		case TabletRule.ModifierMirrorLevel:
			modifierMirrorLevelLabel.text = Modifiers.GetCameraFlipValueString(instance2.CameraFlipMode);
			SetLineModified(modifierMirrorLevelLabel, instance2.CameraFlipMode == defaultRuleset.mods.CameraFlipMode);
			break;
		case TabletRule.ModifierDoomsdayMeteors:
			modifierDoomsdayMeteorsLabel.text = Modifiers.GetDoomsdayMeteorsValueString(instance2.DoomsdayMeteorsMode, instance2.DoomsdayMeteorsDelay);
			SetLineModified(modifierDoomsdayMeteorsLabel, instance2.DoomsdayMeteorsMode == defaultRuleset.mods.DoomsdayMeteorsMode);
			break;
		case TabletRule.ModifierDoomsdayLava:
			modifierDoomsdayLavaLabel.text = Modifiers.GetDoomsdayLavaValueString(instance2.DoomsdayLavaMode, instance2.DoomsdayLavaDelay);
			SetLineModified(modifierDoomsdayLavaLabel, instance2.DoomsdayLavaMode == defaultRuleset.mods.DoomsdayLavaMode);
			break;
		case TabletRule.ModifierPlayerPlayerCollisions:
			modifierPlayerPlayerCollisionsLabel.text = Modifiers.GetOnOffValueString(instance2.playerPlayerCollisions);
			SetLineModified(modifierPlayerPlayerCollisionsLabel, instance2.playerPlayerCollisions == defaultRuleset.mods.PlayerPlayerCollisions);
			break;
		case TabletRule.ModifierProjectileSpeed:
			modifierProjectileSpeedLabel.text = Modifiers.GetSprintSpeedValueString(instance2.ProjectileSpeedMode);
			SetLineModified(modifierProjectileSpeedLabel, instance2.ProjectileSpeedMode == defaultRuleset.mods.ProjectileSpeedMode);
			break;
		case TabletRule.ModifierPreviewModsInTreehouse:
			previewModifiersToggle.SetValue(instance2.modsPreview, triggerCallback: false);
			break;
		case TabletRule.ModifierForceLobbyModifiers:
			forceLobbyModifiersToggle.SetValue(instance2.forceLobbyModifiers, triggerCallback: false);
			break;
		case TabletRule.RespawnMode:
			respawnModeLabel.text = GameSettings.GetRespawnModeValueString(instance.respawnMode, instance.numRespawns);
			SetLineModified(respawnModeLabel, instance.respawnMode == defaultRuleset.respawnMode);
			break;
		case TabletRule.PartyBoxMode:
			partyBoxModeLabel.text = GameSettings.GetPartyBoxModeValueString(instance.partyBoxMode);
			SetLineModified(partyBoxModeLabel, instance.partyBoxMode == defaultRuleset.partyBoxMode);
			break;
		case TabletRule.ModifierFrictionless:
			modifierFrictionlessLabel.text = Modifiers.GetOnOffValueString(instance2.frictionless);
			SetLineModified(modifierFrictionlessLabel, instance2.frictionless == defaultRuleset.mods.Frictionless);
			break;
		case TabletRule.CompetitiveRandomizer:
			competiveRandomizerToggle.SetValue(instance2.competitiveRandomizer, triggerCallback: false);
			break;
		}
	}

	public void OnClickSaveRuleset(PickCursor pickCursor)
	{
		if (!(LobbyManager.instance != null) || LobbyManager.instance.IsHost)
		{
			presetOverlay.Initialize(TabletPresetSelectOverlay.Mode.Save, presetOverlay.SwitchToFileDialog);
		}
	}

	public void OnClickLoadRuleset(PickCursor pickCursor)
	{
		if (LobbyManager.instance != null && !LobbyManager.instance.IsHost)
		{
			return;
		}
		presetOverlay.Initialize(TabletPresetSelectOverlay.Mode.Open, delegate
		{
			if (rulesDirty)
			{
				presetOverlay.ShowPrompt(TabletPresetSelectOverlay.PromptMode.AreYouSureOpen, delegate
				{
					presetOverlay.SwitchToFileDialog();
				}, delegate
				{
					presetOverlay.Close();
				});
			}
			else
			{
				presetOverlay.SwitchToFileDialog();
			}
		});
	}

	public void OnClickResetRuleset(PickCursor pickCursor)
	{
		if (LobbyManager.instance != null && !LobbyManager.instance.IsHost)
		{
			return;
		}
		presetOverlay.Initialize(TabletPresetSelectOverlay.Mode.PromptOnly, delegate
		{
			presetOverlay.ShowPrompt(TabletPresetSelectOverlay.PromptMode.ResetAll, delegate
			{
				GameSettings instance = GameSettings.GetInstance();
				instance.ToDefaultPreset();
				OnPresetLoaded(instance.DefaultRuleset, partial: false);
				NotifyPresetLoad(0, loadRules: true, loadPoints: true, loadBlocks: true, loadMods: true);
				presetOverlay.TransitionOut(TabletSubdialogController.TransitionDirection.Left, presetOverlay.Close);
			}, delegate
			{
				presetOverlay.Close();
			});
		});
	}

	public void LoadPreset(int presetIdx, bool loadRules, bool loadPoints, bool loadBlocks, bool loadMods)
	{
		GameSettings instance = GameSettings.GetInstance();
		_ = instance.rulePresetList[presetIdx];
		if (loadRules && loadPoints && loadBlocks && loadMods)
		{
			instance.ToPreset(presetIdx);
			OnPresetLoaded(instance.GetCurrentRuleset(), partial: false);
		}
		else
		{
			instance.PartialLoadPreset(presetIdx, loadRules, loadPoints, loadBlocks, loadMods);
			OnPresetLoaded(null, partial: true);
		}
		NotifyPresetLoad(presetIdx, loadRules, loadPoints, loadBlocks, loadMods);
	}

	public void NotifyPresetLoad(int presetIdx, bool loadRules, bool loadPoints, bool loadBlocks, bool loadMods)
	{
		GameRulePreset gameRulePreset = GameSettings.GetInstance().rulePresetList[presetIdx];
		MsgApplyRuleset msgApplyRuleset = new MsgApplyRuleset();
		msgApplyRuleset.premadeIdx = (gameRulePreset.IsPremade ? presetIdx : (-1));
		if (!gameRulePreset.IsPremade)
		{
			msgApplyRuleset.rulesetXML = gameRulePreset.GetRulesetXmlString();
		}
		msgApplyRuleset.applyRules = loadRules;
		msgApplyRuleset.applyPoints = loadPoints;
		msgApplyRuleset.applyBlocks = loadBlocks;
		msgApplyRuleset.applyMods = loadMods;
		LobbyManager.instance.client.Send(NetMsgTypes.ApplyRuleset, msgApplyRuleset);
		ShowPresetLoadMessage(gameRulePreset, loadRules, loadPoints, loadBlocks, loadMods);
		if (loadRules && loadPoints && loadBlocks && loadMods)
		{
			ClearDirtyBit();
		}
	}

	private void OnPresetLoaded(GameRulePreset preset, bool partial)
	{
		if (partial)
		{
			AnimateChangeToRuleset(null);
			MarkRulesDirty();
		}
		else
		{
			AnimateChangeToRuleset(preset);
			ClearDirtyBit();
		}
		UpdateAllRuleButtons();
		UpdateAllPointButtons();
		UpdateAllBlockSettings();
		UpdateAllModifierButtons();
		Modifiers.GetInstance().OnModifiersDynamicChange();
	}

	public void UpdateAllRuleButtons()
	{
		UpdateButtonValue(TabletRule.PointsToWin);
		UpdateButtonValue(TabletRule.LengthLimit);
		UpdateButtonValue(TabletRule.PlacementTimer);
		UpdateButtonValue(TabletRule.RunTimeLimit);
		UpdateButtonValue(TabletRule.DoublePartyBox);
		UpdateButtonValue(TabletRule.PiecesPerRound);
		UpdateButtonValue(TabletRule.RespawnMode);
		UpdateButtonValue(TabletRule.PartyBoxMode);
	}

	public void UpdateAllPointButtons()
	{
		for (int i = 0; i <= 10; i++)
		{
			UpdateButtonValue(TabletRule.PointValue, i);
			UpdateButtonValue(TabletRule.PointEnabled, i);
		}
	}

	public void UpdateAllBlockSettings()
	{
		tabletBlockList.OnItemFilterRefreshed();
		competiveRandomizerToggle.SetValue(GameSettings.GetInstance().competitiveRandomizer, triggerCallback: false);
	}

	public void UpdateAllModifierButtons()
	{
		UpdateButtonValue(TabletRule.ModifierGravity);
		UpdateButtonValue(TabletRule.ModifierJumpSpeed);
		UpdateButtonValue(TabletRule.ModifierSprintSpeed);
		UpdateButtonValue(TabletRule.ModifierWallJumpsDisabled);
		UpdateButtonValue(TabletRule.ModifierWallSlidesDisabled);
		UpdateButtonValue(TabletRule.ModifierGameSpeed);
		UpdateButtonValue(TabletRule.ModifierDanceInvincibility);
		UpdateButtonValue(TabletRule.ModifierInvisibility);
		UpdateButtonValue(TabletRule.ModifierMirrorControls);
		UpdateButtonValue(TabletRule.ModifierPlatformSpeed);
		UpdateButtonValue(TabletRule.ModifierRateOfFire);
		UpdateButtonValue(TabletRule.ModifierMultiJump);
		UpdateButtonValue(TabletRule.ModifierProjectilesExplode);
		UpdateButtonValue(TabletRule.ModifierCharacterSize);
		UpdateButtonValue(TabletRule.ModifierJetpackMode);
		UpdateButtonValue(TabletRule.ModifierPostDeathBehaviorMode);
		UpdateButtonValue(TabletRule.ModifierMirrorLevel);
		UpdateButtonValue(TabletRule.ModifierDoomsdayMeteors);
		UpdateButtonValue(TabletRule.ModifierDoomsdayLava);
		UpdateButtonValue(TabletRule.ModifierPlayerPlayerCollisions);
		UpdateButtonValue(TabletRule.ModifierProjectileSpeed);
		UpdateButtonValue(TabletRule.ModifierPreviewModsInTreehouse);
		UpdateButtonValue(TabletRule.ModifierForceLobbyModifiers);
		UpdateButtonValue(TabletRule.ModifierFrictionless);
	}

	private void UpdateRulesetText(GameRulePreset preset)
	{
		if (preset != null)
		{
			UpdateRulesetText(preset.GetNameString(), preset.GetDescriptionString(), !preset.IsPremade);
		}
		else
		{
			UpdateRulesetText(LocalizationManager.GetTranslation("RuleBook/Presets/Custom"), LocalizationManager.GetTranslation("RuleBook/Presets/CustomDescription"), enableFilter: false);
		}
	}

	private void UpdateRulesetText(string rulesetName, string rulesetDescription, bool enableFilter)
	{
		rulesetNameText.EnableWordFilter = enableFilter;
		rulesetDescriptionText.EnableWordFilter = enableFilter;
		rulesetNameText.text = rulesetName;
		if (!rulesetDescription.NullOrEmpty())
		{
			if (rulesetDescription.Length > 80)
			{
				rulesetDescriptionText.GetComponent<Text>().fontSize = 36;
			}
			else
			{
				rulesetDescriptionText.GetComponent<Text>().fontSize = 50;
			}
			rulesetDescriptionText.text = rulesetDescription;
		}
		else
		{
			rulesetDescriptionText.GetComponent<Text>().fontSize = 50;
			rulesetDescriptionText.text = LocalizationManager.GetTranslation("RuleBook/Presets/NoDescription");
		}
	}

	public void AnimateChangeToRuleset(GameRulePreset preset)
	{
		if (preset != null)
		{
			string nameString = preset.GetNameString();
			if (!nameString.NullOrEmpty())
			{
				rulesetNameAnim = AnimateRulesetNameChange(nameString, preset.GetDescriptionString(), !preset.IsPremade);
				return;
			}
		}
		rulesetNameAnim = AnimateRulesetNameChange(LocalizationManager.GetTranslation("RuleBook/Presets/Custom"), LocalizationManager.GetTranslation("RuleBook/Presets/CustomDescription"), enableFilter: false);
	}

	private IEnumerator AnimateRulesetNameChange(string rulesetName, string rulesetDescription, bool enableFilter)
	{
		Vector3 one = Vector3.one;
		Vector3 vector = new Vector3(0f, 1f, 1f);
		return (new SequenceTweener() + new WaitForConditionTweener(() => subdialogController.IsOnMainSubdialog) + new LocalScaleTweener(rulesetInfoContainer, one, vector, 0.2f, Easings.Functions.BackEaseIn).SetOnFinish(delegate
		{
			UpdateRulesetText(rulesetName, rulesetDescription, enableFilter);
		}) + new LocalScaleTweener(rulesetInfoContainer, vector, one, 0.2f, Easings.Functions.ExponentialEaseOut)).Animate();
	}

	public void ClearDirtyBit()
	{
		if (rulesDirty)
		{
			rulesDirty = false;
		}
	}

	public void MarkRulesDirty(bool force = false)
	{
		GameSettings.GetInstance().OnRulesDirty();
		if (force || !rulesDirty)
		{
			rulesDirty = true;
			AnimateChangeToRuleset(null);
			ChatMessageDetails chatMessageDetails = new ChatMessageDetails(Character.Animals.NONE, null, GameSettings.GetInstance().SystemColor, null, EmoteMeanings.CHAT_Text, 0);
			chatMessageDetails.Message = LocalizationManager.GetTranslation("RuleBook/Presets/HostCustomizingRules");
			chatMessageDetails.isChatMessage = false;
			GameState.ChatSystem.DisplayNewMessage(chatMessageDetails);
			if (LobbyManager.instance.IsHost)
			{
				MsgRulesetDirty msgRulesetDirty = new MsgRulesetDirty();
				msgRulesetDirty.dirty = true;
				LobbyManager.instance.client.Send(NetMsgTypes.RulesetDirty, msgRulesetDirty);
			}
		}
	}

	private void ShowPresetLoadMessage(GameRulePreset ruleset, bool applyRules, bool applyPoints, bool applyBlocks, bool applyMods)
	{
		GameSettings instance = GameSettings.GetInstance();
		bool flag = ruleset == instance.DefaultRuleset;
		if (!(applyRules && applyPoints && applyBlocks))
		{
			List<string> list = new List<string>();
			if (applyRules)
			{
				list.Add(LocalizationManager.GetTranslation("RuleBook/Game Rules"));
			}
			if (applyPoints)
			{
				list.Add(LocalizationManager.GetTranslation("RuleBook/Presets/PointSettings"));
			}
			if (applyBlocks)
			{
				list.Add(LocalizationManager.GetTranslation("RuleBook/Presets/BlockSettings"));
			}
			if (applyMods)
			{
				list.Add(ScriptLocalization.Modifiers.ModifiersTitle);
			}
			string text = null;
			switch (list.Count)
			{
			case 1:
				if (flag)
				{
					text = LocalizationManager.GetTranslation("RuleBook/Presets/PageResetToDefault");
					text = string.Format(text, list[0]);
				}
				else
				{
					text = LocalizationManager.GetTranslation("RuleBook/Presets/HostLoadedOnePart");
					text = string.Format(text, list[0], ruleset.GetNameString());
				}
				break;
			case 2:
				text = LocalizationManager.GetTranslation("RuleBook/Presets/HostLoadedTwoParts");
				text = string.Format(text, list[0], list[1], ruleset.GetNameString());
				break;
			case 3:
				text = ScriptLocalization.RuleBook_Presets.HostLoadedThreeParts;
				text = string.Format(text, list[0], list[1], list[2], ruleset.GetNameString());
				break;
			default:
				text = LocalizationManager.GetTranslation("RuleBook/Presets/HostCustomizingRules");
				break;
			}
			ChatMessageDetails chatMessageDetails = new ChatMessageDetails(Character.Animals.NONE, null, GameSettings.GetInstance().SystemColor, null, EmoteMeanings.CHAT_Text, 0);
			chatMessageDetails.Message = text;
			chatMessageDetails.isChatMessage = true;
			GameState.ChatSystem.DisplayNewMessage(chatMessageDetails);
		}
		else
		{
			ChatMessageDetails chatMessageDetails2 = new ChatMessageDetails(Character.Animals.NONE, null, GameSettings.GetInstance().SystemColor, null, EmoteMeanings.CHAT_Text, 0);
			string text2 = null;
			text2 = ((!ruleset.IsPremade) ? LocalizationManager.GetTranslation("RuleBook/Presets/HostLoadedCustomPreset") : ((!flag) ? LocalizationManager.GetTranslation("RuleBook/Presets/HostLoadedBuiltinPreset") : LocalizationManager.GetTranslation("RuleBook/Presets/HostResetAllRules")));
			chatMessageDetails2.Message = string.Format(text2, ruleset.GetNameString());
			chatMessageDetails2.isChatMessage = true;
			GameState.ChatSystem.DisplayNewMessage(chatMessageDetails2);
		}
	}

	public void OnClickResetGameRulesPage(PickCursor pickCursor)
	{
		if ((!(LobbyManager.instance != null) || LobbyManager.instance.IsHost) && !GameSettings.GetInstance().DefaultRuleset.IsCurrentlyApplied(checkRules: true, checkPoints: false, checkBlocks: false, checkMods: false))
		{
			LoadPreset(0, loadRules: true, loadPoints: false, loadBlocks: false, loadMods: false);
		}
	}

	public void OnClickResetPointsPage(PickCursor pickCursor)
	{
		if ((LobbyManager.instance != null && !LobbyManager.instance.IsHost) || GameSettings.GetInstance().DefaultRuleset.IsCurrentlyApplied(checkRules: false, checkPoints: true, checkBlocks: false, checkMods: false))
		{
			return;
		}
		LoadPreset(0, loadRules: false, loadPoints: true, loadBlocks: false, loadMods: false);
		foreach (KeyValuePair<PointBlock.pointBlockType, TabletPointWidget> pointWidget in pointWidgets)
		{
			pointWidget.Value.Wobble();
		}
	}

	public void OnClickResetModifiersPage()
	{
		if ((!(LobbyManager.instance != null) || LobbyManager.instance.IsHost) && !GameSettings.GetInstance().DefaultRuleset.IsCurrentlyApplied(checkRules: false, checkPoints: false, checkBlocks: false, checkMods: true))
		{
			LoadPreset(0, loadRules: false, loadPoints: false, loadBlocks: false, loadMods: true);
			GameEventManager.SendEvent(new ModifiersChangedEvent(TabletRule.None));
		}
	}

	public void SetPointEnabled(PointBlock.pointBlockType pointType, bool value)
	{
		if (!(LobbyManager.instance != null) || LobbyManager.instance.IsHost)
		{
			GameSettings instance = GameSettings.GetInstance();
			if (instance.PointTypeEnabled(pointType) != value)
			{
				instance.SetPointTypeEnabled(pointType, value);
				int num = (value ? 1 : 0);
				num += (instance.AlwaysAwardPointType(pointType) ? 1 : 0);
				UpdateButtonValue(TabletRule.PointEnabled, (int)pointType);
				TabletModalOverlay.BroadcastRuleChange(TabletRule.PointEnabled, (int)pointType, num);
				MarkRulesDirty();
			}
		}
	}

	public void SetPointAlwaysAward(PointBlock.pointBlockType pointType, bool value)
	{
		if (!(LobbyManager.instance != null) || LobbyManager.instance.IsHost)
		{
			GameSettings instance = GameSettings.GetInstance();
			if (instance.AlwaysAwardPointType(pointType) != value)
			{
				instance.SetAlwaysAwardPointType(pointType, value);
				int num = (value ? 1 : 0);
				num += (instance.PointTypeEnabled(pointType) ? 1 : 0);
				UpdateButtonValue(TabletRule.PointEnabled, (int)pointType);
				TabletModalOverlay.BroadcastRuleChange(TabletRule.PointEnabled, (int)pointType, num);
				MarkRulesDirty();
			}
		}
	}

	public void SetPointValue(PointBlock.pointBlockType pointType, int value)
	{
		if (!(LobbyManager.instance != null) || LobbyManager.instance.IsHost)
		{
			GameSettings instance = GameSettings.GetInstance();
			if (instance.PointTypeValue(pointType) != value)
			{
				instance.SetPointTypeValue(pointType, value);
				UpdateButtonValue(TabletRule.PointValue, (int)pointType);
				TabletModalOverlay.BroadcastRuleChange(TabletRule.PointValue, (int)pointType, value);
				MarkRulesDirty();
			}
		}
	}

	public void OnClickAlwaysAwardHint(PickCursor pickCursor)
	{
		string translation = LocalizationManager.GetTranslation("RuleBook/AlwaysAward");
		string translation2 = LocalizationManager.GetTranslation("RuleBook/Presets/AlwaysAwardHelpMessage");
		modalOverlay.ShowSimpleMessage(translation, translation2, base.OnModalOverlayClosed);
	}

	public void OnClickLobbyModifiersHint(PickCursor pickCursor)
	{
		string doNotLoadLevelSpecificModifiers = ScriptLocalization.Modifiers.DoNotLoadLevelSpecificModifiers;
		string doNotLoadModifiersDescription = ScriptLocalization.Modifiers.DoNotLoadModifiersDescription;
		modalOverlay.ShowSimpleMessage(doNotLoadLevelSpecificModifiers, doNotLoadModifiersDescription, base.OnModalOverlayClosed);
	}

	public void OnClickCompetitiveRandomizer(PickCursor pickCursor)
	{
		string translation = LocalizationManager.GetTranslation("Modifiers/CompetitiveRandomizerModifiers");
		string translation2 = LocalizationManager.GetTranslation("Modifiers/CompetitiveRandomizerModifiersDescription");
		modalOverlay.ShowSimpleMessage(translation, translation2, base.OnModalOverlayClosed);
	}

	public void OnClickToggleAdvancedProbabilities()
	{
		tabletBlockList.ShowAdvancedProbabilities(!showPercentCheckbox.Value);
	}

	public void RefreshAdvancedProbabilities()
	{
		tabletBlockList.RefreshAdvancedProbabilities();
	}

	public override void OnCursorScroll(Vector2 scrollAmount)
	{
		if (subdialogController.currentSubdialog == modifierSettingsSubdialog)
		{
			modifierSettingsScroller.ApplyScrolling(scrollAmount.y);
		}
	}

	public override bool OnRotateRight(PickCursor pickCursor)
	{
		if (presetOverlay.isOpen && presetOverlay.OnRotateRight(pickCursor))
		{
			return true;
		}
		if (subdialogController.currentSubdialog == blockSettingsSubdialog && !pickCursor.lastRotateWasMouseWheel && tabletBlockList.CurrentPage < tabletBlockList.NumPages - 1)
		{
			tabletBlockList.nextButton.OnAccept(pickCursor);
			return true;
		}
		if (subdialogController.currentSubdialog == modifierSettingsSubdialog && pickCursor.lastRotateWasMouseWheel)
		{
			if (Modifiers.GetInstance().CameraFlippedOnX)
			{
				modifierSettingsScroller.OnClickScrollMinus(pickCursor);
			}
			else
			{
				modifierSettingsScroller.OnClickScrollPlus(pickCursor);
			}
			return true;
		}
		if (subdialogController.currentSubdialog == mainRulesPageSubdialog && !pickCursor.lastRotateWasMouseWheel && lastEnteredSubdialog != null)
		{
			subdialogController.TransitionLeftTo(lastEnteredSubdialog);
			return true;
		}
		return false;
	}

	public override bool OnRotateLeft(PickCursor pickCursor)
	{
		if (presetOverlay.isOpen && presetOverlay.OnRotateLeft(pickCursor))
		{
			return true;
		}
		if (subdialogController.currentSubdialog == modifierSettingsSubdialog && pickCursor.lastRotateWasMouseWheel)
		{
			if (Modifiers.GetInstance().CameraFlippedOnX)
			{
				modifierSettingsScroller.OnClickScrollPlus(pickCursor);
			}
			else
			{
				modifierSettingsScroller.OnClickScrollMinus(pickCursor);
			}
			return true;
		}
		if (subdialogController.currentSubdialog == blockSettingsSubdialog && !pickCursor.lastRotateWasMouseWheel && tabletBlockList.CurrentPage > 0)
		{
			tabletBlockList.prevButton.OnAccept(pickCursor);
			return true;
		}
		return false;
	}

	public void EnterSubdialog(RectTransform target)
	{
		if (!subdialogController.IsAnimating)
		{
			lastEnteredSubdialog = target;
			subdialogController.TransitionLeftTo(target);
		}
	}

	public void OnTogglePreviewModsInTreehouse()
	{
		if (LobbyManager.instance != null && LobbyManager.instance.CurrentLevelSelectController != null && LobbyManager.instance.IsHost)
		{
			Modifiers instance = Modifiers.GetInstance();
			instance.modsPreview = previewModifiersToggle.Value;
			instance.modsApplied = instance.modsPreview;
			MarkRulesDirty();
			instance.OnModifiersDynamicChange();
			TabletModalOverlay.BroadcastRuleChange(TabletRule.ModifierPreviewModsInTreehouse, 0, 0, instance.modsPreview);
		}
	}

	public void OnToggleForceLobbyModifiers()
	{
		if (LobbyManager.instance != null && LobbyManager.instance.CurrentLevelSelectController != null && LobbyManager.instance.IsHost)
		{
			Modifiers instance = Modifiers.GetInstance();
			instance.forceLobbyModifiers = forceLobbyModifiersToggle.Value;
			MarkRulesDirty();
			TabletModalOverlay.BroadcastRuleChange(TabletRule.ModifierForceLobbyModifiers, 0, 0, instance.forceLobbyModifiers);
		}
	}

	public void OnToggleForceTrueRandomBlocks()
	{
		if (!(LobbyManager.instance != null) || LobbyManager.instance.IsHost)
		{
			GameSettings instance = GameSettings.GetInstance();
			instance.competitiveRandomizer = competiveRandomizerToggle.Value;
			bool competitiveRandomizer = instance.competitiveRandomizer;
			MarkRulesDirty();
			TabletModalOverlay.BroadcastRuleChange(TabletRule.CompetitiveRandomizer, 0, 0, competitiveRandomizer);
		}
	}

	public void RandomizeModifiers(GameRulePreset ruleset, int num)
	{
		if (num == 0)
		{
			return;
		}
		List<TabletRule> list = new List<TabletRule>
		{
			TabletRule.ModifierGravity,
			TabletRule.ModifierJumpSpeed,
			TabletRule.ModifierSprintSpeed,
			TabletRule.ModifierWallJumpsDisabled,
			TabletRule.ModifierGameSpeed,
			TabletRule.ModifierDanceInvincibility,
			TabletRule.ModifierPlatformSpeed,
			TabletRule.ModifierRateOfFire,
			TabletRule.ModifierMultiJump,
			TabletRule.ModifierProjectilesExplode,
			TabletRule.ModifierCharacterSize,
			TabletRule.ModifierJetpackMode,
			TabletRule.ModifierPostDeathBehaviorMode,
			TabletRule.ModifierMirrorLevel,
			TabletRule.ModifierDoomsdayMeteors,
			TabletRule.ModifierDoomsdayLava,
			TabletRule.ModifierPlayerPlayerCollisions,
			TabletRule.ModifierProjectileSpeed,
			TabletRule.ModifierInvisibility,
			TabletRule.ModifierFrictionless
		};
		HashSet<TabletRule> hashSet = new HashSet<TabletRule>();
		int index = Random.Range(0, list.Count);
		TabletRule modifier = list[index];
		hashSet.Add(list[index]);
		list.RemoveAt(index);
		for (int i = 1; i < num; i++)
		{
			float num2 = Random.value;
			int j;
			for (j = 0; j < list.Count - 1; j++)
			{
				num2 -= ModifierPairMatrix.GetValue(modifier, list[j], normalize: true);
				if (num2 <= 0f)
				{
					break;
				}
			}
			hashSet.Add(list[j]);
			list.RemoveAt(j);
		}
		foreach (TabletRule item in hashSet)
		{
			DoRandomizeModifier(ruleset, item);
		}
	}

	public void DoRandomizeModifier(GameRulePreset ruleset, TabletRule modifier)
	{
		Debug.LogWarning("Randomizing modifier: " + modifier);
		Modifiers instance = Modifiers.GetInstance();
		switch (modifier)
		{
		case TabletRule.ModifierGravity:
			ruleset.mods.GravityMode = Random.Range(1, instance.GravityValues.Length);
			break;
		case TabletRule.ModifierJumpSpeed:
			ruleset.mods.JumpSpeedMode = Random.Range(1, instance.JumpSpeedValues.Length);
			break;
		case TabletRule.ModifierSprintSpeed:
			ruleset.mods.SprintSpeedMode = Random.Range(1, instance.SprintModifiers.Length);
			break;
		case TabletRule.ModifierWallJumpsDisabled:
			ruleset.mods.WallJumpsDisabled = true;
			break;
		case TabletRule.ModifierGameSpeed:
			ruleset.mods.GameSpeedMode = Random.Range(1, instance.GameSpeedValues.Length);
			break;
		case TabletRule.ModifierDanceInvincibility:
			ruleset.mods.DanceInvincibility = true;
			break;
		case TabletRule.ModifierPlatformSpeed:
			ruleset.mods.PlatformSpeedMode = Random.Range(1, instance.PlatformMoveSpeedValues.Length);
			break;
		case TabletRule.ModifierRateOfFire:
			ruleset.mods.RateOfFireMode = Random.Range(1, instance.RateOfFireValues.Length);
			break;
		case TabletRule.ModifierMultiJump:
			ruleset.mods.MultiJumpMode = Random.Range(1, instance.MultiJumpValues.Length);
			break;
		case TabletRule.ModifierProjectilesExplode:
			ruleset.mods.ProjectileExplosionMode = Random.Range(1, instance.ProjectileExplosionScales.Length - 1);
			break;
		case TabletRule.ModifierCharacterSize:
			ruleset.mods.CharacterSizeMode = Random.Range(1, instance.CharacterScales.Length);
			break;
		case TabletRule.ModifierJetpackMode:
			ruleset.mods.JetpackMode = true;
			break;
		case TabletRule.ModifierPostDeathBehaviorMode:
			ruleset.mods.PostDeathBehaviorMode = Random.Range(1, 4);
			break;
		case TabletRule.ModifierMirrorLevel:
			ruleset.mods.CameraFlipMode = Random.Range(1, 4);
			break;
		case TabletRule.ModifierDoomsdayMeteors:
			ruleset.mods.DoomsdayMeteorsMode = Random.Range(1, instance.DoomsdayModifierTimes.Length);
			break;
		case TabletRule.ModifierDoomsdayLava:
			ruleset.mods.DoomsdayLavaMode = Random.Range(1, instance.DoomsdayModifierTimes.Length);
			break;
		case TabletRule.ModifierPlayerPlayerCollisions:
			ruleset.mods.PlayerPlayerCollisions = true;
			break;
		case TabletRule.ModifierProjectileSpeed:
			ruleset.mods.ProjectileSpeedMode = Random.Range(1, instance.ProjectileSpeedValues.Length);
			break;
		case TabletRule.ModifierInvisibility:
			ruleset.mods.InvisibilityMode = Random.Range(1, 4);
			break;
		case TabletRule.ModifierFrictionless:
			ruleset.mods.Frictionless = true;
			break;
		case TabletRule.ModifierWallSlidesDisabled:
		case TabletRule.ModifierInvisibleWhenMoving_UNUSED:
		case TabletRule.ModifierInvisibleWhenStationary_UNUSED:
		case TabletRule.ModifierMirrorControls:
		case TabletRule.ModifierPreviewModsInTreehouse:
		case TabletRule.ModifierForceLobbyModifiers:
			break;
		}
	}

	public void OnClickRandomizeModifiers()
	{
		GameSettings instance = GameSettings.GetInstance();
		instance.DefaultRuleset.mods.WriteToModSettings(includeTreehouseSettings: false);
		GameRulePreset gameRulePreset = ScriptableObject.CreateInstance<GameRulePreset>();
		gameRulePreset.IsPremade = false;
		gameRulePreset.LoadRulesFromSettings();
		int num = ((Random.value < 0.05f) ? 10 : Random.Range(3, 6));
		RandomizeModifiers(gameRulePreset, num);
		instance.ApplyTemporaryRuleset(gameRulePreset, loadRules: false, loadPoints: false, loadBlocks: false, loadModifiers: true);
		OnPresetLoaded(null, partial: true);
		AnimateChangeToRuleset(null);
		MsgApplyRuleset msgApplyRuleset = new MsgApplyRuleset();
		msgApplyRuleset.rulesetXML = gameRulePreset.GetRulesetXmlString();
		msgApplyRuleset.premadeIdx = -1;
		msgApplyRuleset.applyRules = false;
		msgApplyRuleset.applyPoints = false;
		msgApplyRuleset.applyBlocks = false;
		msgApplyRuleset.applyMods = true;
		msgApplyRuleset.temporary = true;
		LobbyManager.instance.client.Send(NetMsgTypes.ApplyRuleset, msgApplyRuleset);
		GameEventManager.SendEvent(new ModifiersChangedEvent(TabletRule.None));
		Object.Destroy(gameRulePreset);
		if (ModifierDisableGroup != null)
		{
			StartCoroutine(randomModifierBuffer(0.5f));
		}
	}

	private IEnumerator randomModifierBuffer(float time)
	{
		float timer = 0f;
		ModifierDisableGroup.SetDisabled(disabled: true);
		while (timer < time)
		{
			timer += Time.unscaledDeltaTime;
			yield return null;
		}
		ModifierDisableGroup.SetDisabled(disabled: false);
	}
}
