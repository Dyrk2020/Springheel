using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;
using UnityEngine.Events;

public class TabletModalOverlay : MonoBehaviour
{
	public TabletRulesScreen rulesScreen;

	public TabletOptionsScreen optionsScreen;

	public TabletOnlinePlayersScreen playersScreen;

	public TabletLobbyOptionsScreen lobbyOptionsScreen;

	public TabletTwitchOptionsScreen twitchOptionsScreen;

	public CanvasGroup backgroundCanvasGroup;

	public CanvasGroup multipleChoiceCanvasGroup;

	public TabletTextLabel titleText;

	public Transform roundsTimeContainer;

	public TabletToggleButtonGroup roundsTimeToggleGroup;

	public Transform respawnModeContainer;

	public TabletToggleButtonGroup respawnModeToggleGroup;

	public Transform plusMinusContainer;

	public TabletTextLabel plusMinusLabel;

	public TabletButton minusButton;

	public TabletButton plusButton;

	public Transform okButtonContainer;

	public TabletButton okButton;

	public Transform onOffContainer;

	public TabletButton onButton;

	public TabletButton offButton;

	public Transform partyBoxModeContainer;

	public TabletButton[] partyBoxModeButtons;

	public RectTransform doublePartyBoxContainer;

	public TabletButton doublePartyBoxOffButton;

	public TabletButton doublePartyBox2PlayersButton;

	public TabletButton doublePartyBoxAlwaysButton;

	public Transform pointOnOffContainer;

	public TabletButton pointOnButton;

	public TabletButton pointOffButton;

	public TabletButton pointAlwaysAwardButton;

	public TabletDisableGroup pointOnButtonDisableGroup;

	public TabletDisableGroup pointAlwaysAwardButtonDisableGroup;

	public RectTransform simpleMessageContainer;

	public TabletTextLabel simpleMessageText;

	public RectTransform reportReasonsContainer;

	public List<TabletButton> reportReasonButtons;

	public static int[] afkKickerTimes = new int[6] { 30, 60, 90, 120, 0, 15 };

	public RectTransform cameraFollowsContainer;

	public TabletButton cameraFollowsLocalOnlyButton;

	public TabletButton cameraFollowsAllPlayersButton;

	public RectTransform nameVisibilityContainer;

	public TabletButton nameVisibilityAlwaysOnButton;

	public TabletButton nameVisibilityAutoButton;

	public TabletButton nameVisibilityAlwaysOffButton;

	public RectTransform chatAndEmotesContainer;

	public TabletButton chatAndEmotesOnButton;

	public TabletButton chatAndEmotesEmotesOnlyButton;

	public TabletButton chatAndEmotesOffButton;

	public RectTransform lobbyTagContainer;

	public TabletButton lobbyTagFunButton;

	public TabletButton lobbyTagCompetitiveButton;

	public TabletButton lobbyTagBeginnerButton;

	public TabletButton lobbyTagCustomLevelsButton;

	public RectTransform lobbyPrivacyContainer;

	public TabletButton lobbyPrivacyPublicButton;

	public TabletButton lobbyPrivacyFriendsOnlyButton;

	public TabletButton lobbyPrivacyInviteOnlyButton;

	public RectTransform gameModeContainer;

	public TabletButton modePartyButton;

	public TabletButton modeCreativeButton;

	public TabletButton modeFreePlayButton;

	public TabletButton modeChallengeButton;

	public RectTransform modifierGravityContainer;

	public TabletButton gravityNormalButton;

	public TabletButton gravityLowButton;

	public TabletButton gravityHighButton;

	public RectTransform modifierJumpSpeedContainer;

	public TabletButton[] jumpSpeedButtons;

	public RectTransform modifierSprintSpeedContainer;

	public TabletButton[] sprintSpeedButtons;

	public RectTransform modifierGameSpeedContainer;

	public TabletButton[] gameSpeedButtons;

	public RectTransform modifierMultiJumpContainer;

	public TabletButton[] multiJumpButtons;

	public RectTransform modifierProjectilesExplodeContainer;

	public TabletButton[] projectilesExplodeButtons;

	public RectTransform modifierCharacterSizeContainer;

	public TabletButton[] characterSizeButtons;

	public RectTransform modifierPostDeathBehaviorContainer;

	public TabletButton[] postDeathBehaviorButtons;

	public RectTransform modifierMirrorLevelContainer;

	public TabletButton[] mirrorLevelButtons;

	public RectTransform modifierInvisibilityContainer;

	public TabletButton[] invisibilityButtons;

	public TabletRule currentOverlayType;

	public PointBlock.pointBlockType currentPointType;

	private UnityAction OnModalClosed;

	private bool isOpen;

	private bool isOpening;

	private DataModel dataModel;

	private IEnumerator backgroundAnim;

	public bool IsOpen => isOpen;

	public bool IsOpening => isOpening;

	private void Awake()
	{
		((RectTransform)base.transform).anchoredPosition = Vector2.zero;
		base.gameObject.SetActive(value: false);
	}

	public void Initialize(TabletRule overlayType, UnityAction OnModalClosed)
	{
		this.OnModalClosed = OnModalClosed;
		currentOverlayType = overlayType;
		ResetDialog();
		dataModel = new DataModel();
		GameSettings instance = GameSettings.GetInstance();
		Modifiers instance2 = Modifiers.GetInstance();
		okButton.clickSound = "UI_UPad_Button_Click_Ok_Soft";
		switch (overlayType)
		{
		case TabletRule.PointsToWin:
			titleText.text = ScriptLocalization.RuleBook.Points_to_Win;
			plusMinusContainer.gameObject.SetActive(value: true);
			okButtonContainer.gameObject.SetActive(value: true);
			dataModel.Set("MaxScore", instance.MaxScore);
			break;
		case TabletRule.LengthLimit:
			titleText.text = ScriptLocalization.RuleBook.Length_Limit;
			roundsTimeContainer.gameObject.SetActive(value: true);
			okButtonContainer.gameObject.SetActive(value: true);
			dataModel.Set("GameLimitType", (int)instance.GameLimitType);
			dataModel.Set("MaxRounds", instance.MaxRounds);
			dataModel.Set("MaxTime", instance.MaxTime);
			break;
		case TabletRule.PlacementTimer:
			titleText.text = ScriptLocalization.RuleBook.Placement_Timer;
			plusMinusContainer.gameObject.SetActive(value: true);
			okButtonContainer.gameObject.SetActive(value: true);
			dataModel.Set("PlaceTime", (int)instance.PlaceTime);
			break;
		case TabletRule.RunTimeLimit:
			titleText.text = ScriptLocalization.RuleBook.RunTimerLimit;
			plusMinusContainer.gameObject.SetActive(value: true);
			okButtonContainer.gameObject.SetActive(value: true);
			dataModel.Set("RunTimerLimit", instance.RunTimerLimit);
			break;
		case TabletRule.DoublePartyBox:
			titleText.text = ScriptLocalization.RuleBook.DoublePartyBoxText;
			doublePartyBoxContainer.gameObject.SetActive(value: true);
			SetButtonStyle(doublePartyBoxOffButton, instance.DoublePartyBox == DoublePartyBox.Off);
			SetButtonStyle(doublePartyBox2PlayersButton, instance.DoublePartyBox == DoublePartyBox.TwoPlayers);
			SetButtonStyle(doublePartyBoxAlwaysButton, instance.DoublePartyBox == DoublePartyBox.Always);
			dataModel.Set("DoublePartyBox", instance.DoublePartyBox);
			break;
		case TabletRule.PiecesPerRound:
			titleText.text = ScriptLocalization.RuleBook.PiecePerRound;
			plusMinusContainer.gameObject.SetActive(value: true);
			okButtonContainer.gameObject.SetActive(value: true);
			dataModel.Set("CreativePiecesPerRound", instance.CreativePiecesPerRound);
			break;
		case TabletRule.PointEnabled:
		{
			titleText.text = ScriptLocalization.RuleBook_Presets.EnablePoint;
			pointOnOffContainer.gameObject.SetActive(value: true);
			switch (currentPointType)
			{
			default:
				pointAlwaysAwardButtonDisableGroup.SetDisabled(disabled: false);
				pointOnButtonDisableGroup.SetDisabled(disabled: false);
				break;
			case PointBlock.pointBlockType.coin:
				pointAlwaysAwardButtonDisableGroup.SetDisabled(disabled: false);
				pointOnButtonDisableGroup.SetDisabled(disabled: true);
				break;
			case PointBlock.pointBlockType.soloWin:
				pointAlwaysAwardButtonDisableGroup.SetDisabled(disabled: true);
				pointOnButtonDisableGroup.SetDisabled(disabled: false);
				break;
			}
			int num4 = 0;
			if (instance.PointTypeEnabled(currentPointType))
			{
				num4++;
				if (instance.AlwaysAwardPointType(currentPointType))
				{
					num4++;
				}
			}
			dataModel.Set("PointEnabled", num4);
			break;
		}
		case TabletRule.SimpleMessage:
			simpleMessageContainer.gameObject.SetActive(value: true);
			okButtonContainer.gameObject.SetActive(value: true);
			okButton.clickSound = "UI_UPad_Button_Click_Generic";
			break;
		case TabletRule.OptionResolution:
			titleText.text = ScriptLocalization.Options_Options.Resolution;
			plusMinusContainer.gameObject.SetActive(value: true);
			okButtonContainer.gameObject.SetActive(value: true);
			dataModel.Set("ResX", Screen.width);
			dataModel.Set("ResY", Screen.height);
			dataModel.Set("RefreshRate", Screen.currentResolution.refreshRate);
			break;
		case TabletRule.OptionQuality:
			titleText.text = ScriptLocalization.Options_Options.Quality;
			plusMinusContainer.gameObject.SetActive(value: true);
			okButtonContainer.gameObject.SetActive(value: true);
			dataModel.Set("Quality", QualitySettings.masterTextureLimit);
			break;
		case TabletRule.OptionVsync:
			titleText.text = ScriptLocalization.Options_Options.vsync;
			onOffContainer.gameObject.SetActive(value: true);
			SetOnOffButtonStyles(QualitySettings.vSyncCount > 0);
			dataModel.Set("VSync", QualitySettings.vSyncCount);
			break;
		case TabletRule.OptionWindowed:
			titleText.text = ScriptLocalization.Options_Options.windowed;
			onOffContainer.gameObject.SetActive(value: true);
			SetOnOffButtonStyles(!Screen.fullScreen);
			dataModel.Set("Windowed", !Screen.fullScreen);
			break;
		case TabletRule.OptionShowVersion:
		{
			titleText.text = ScriptLocalization.Options_Options.ShowVersion;
			onOffContainer.gameObject.SetActive(value: true);
			SaveFileData saveFileDataForMainUser2 = StatTracker.Instance.GetSaveFileDataForMainUser();
			SetOnOffButtonStyles(!saveFileDataForMainUser2.HideVersion);
			dataModel.Set("ShowVersion", !saveFileDataForMainUser2.HideVersion);
			break;
		}
		case TabletRule.OptionBackgroundAudio:
		{
			titleText.text = ScriptLocalization.Options_Options.BackgroundAudio;
			onOffContainer.gameObject.SetActive(value: true);
			SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
			SetOnOffButtonStyles(saveFileDataForMainUser.BackgroundAudio);
			dataModel.Set("BackgroundAudio", saveFileDataForMainUser.BackgroundAudio);
			break;
		}
		case TabletRule.PlayerReportReason:
		{
			titleText.text = ScriptLocalization.Network.ReportPlayerSelectReason;
			reportReasonsContainer.gameObject.SetActive(value: true);
			int num8 = 0;
			for (int num9 = 0; num9 < reportReasonButtons.Count; num9++)
			{
				SetButtonStyle(reportReasonButtons[num9], num9 == num8);
			}
			dataModel.Set("ReportReason", 0);
			break;
		}
		case TabletRule.OnlineSettingsCameraFollows:
			titleText.text = ScriptLocalization.Network.CameraFollows;
			cameraFollowsContainer.gameObject.SetActive(value: true);
			SetButtonStyle(cameraFollowsLocalOnlyButton, ZoomCamera.LocalOnly);
			SetButtonStyle(cameraFollowsAllPlayersButton, !ZoomCamera.LocalOnly);
			dataModel.Set("CameraFollows", ZoomCamera.LocalOnly ? 1 : 0);
			break;
		case TabletRule.OnlineSettingsAFKKickTime:
		{
			titleText.text = ScriptLocalization.Network.AFKKicker;
			plusMinusContainer.gameObject.SetActive(value: true);
			okButtonContainer.gameObject.SetActive(value: true);
			int value = ((LobbyManager.instance != null && !LobbyManager.instance.IsHost) ? instance.CurrentLobbyAFKAutoKickTime : instance.AFKAutoKickTime);
			int num = Array.IndexOf(afkKickerTimes, value);
			if (num == -1)
			{
				num = 0;
			}
			dataModel.Set("AFKKicker", num);
			break;
		}
		case TabletRule.OnlineSettingsNameDisplay:
			titleText.text = ScriptLocalization.Network.Name_Visibility;
			nameVisibilityContainer.gameObject.SetActive(value: true);
			SetButtonStyle(nameVisibilityAlwaysOnButton, instance.OnlinePlayerNames == OnlinePlayerNames.AlwaysOn);
			SetButtonStyle(nameVisibilityAlwaysOffButton, instance.OnlinePlayerNames == OnlinePlayerNames.AlwaysOff);
			SetButtonStyle(nameVisibilityAutoButton, instance.OnlinePlayerNames == OnlinePlayerNames.Auto);
			dataModel.Set("NameVisibility", (int)instance.OnlinePlayerNames);
			break;
		case TabletRule.OnlineSettingsEmotes:
			titleText.text = ScriptLocalization.Network.ChatAndEmotes;
			chatAndEmotesContainer.gameObject.SetActive(value: true);
			SetButtonStyle(chatAndEmotesOnButton, instance.OnlineChatEmotes == OnlineChatEmotes.ChatAndEmotesOn);
			SetButtonStyle(chatAndEmotesOffButton, instance.OnlineChatEmotes == OnlineChatEmotes.ChatAndEmotesOff);
			SetButtonStyle(chatAndEmotesEmotesOnlyButton, instance.OnlineChatEmotes == OnlineChatEmotes.EmotesOnly);
			dataModel.Set("ChatAndEmotes", (int)instance.OnlineChatEmotes);
			break;
		case TabletRule.LobbyOptionsTag:
			titleText.text = ScriptLocalization.Network.LobbyTag;
			lobbyTagContainer.gameObject.SetActive(value: true);
			SetButtonStyle(lobbyTagFunButton, instance.lobbyTag == LobbyTags.Fun);
			SetButtonStyle(lobbyTagBeginnerButton, instance.lobbyTag == LobbyTags.Beginner);
			SetButtonStyle(lobbyTagCompetitiveButton, instance.lobbyTag == LobbyTags.Competitive);
			SetButtonStyle(lobbyTagCustomLevelsButton, instance.lobbyTag == LobbyTags.CustomLevels);
			dataModel.Set("LobbyTag", (int)instance.lobbyTag);
			break;
		case TabletRule.LobbyOptionsPrivacy:
			titleText.text = ScriptLocalization.Network.Privacy;
			lobbyPrivacyContainer.gameObject.SetActive(value: true);
			SetButtonStyle(lobbyPrivacyPublicButton, instance.lobbyPrivacy == MatchmakingLobby.Visibility.PUBLIC);
			SetButtonStyle(lobbyPrivacyInviteOnlyButton, instance.lobbyPrivacy == MatchmakingLobby.Visibility.PRIVATE);
			SetButtonStyle(lobbyPrivacyFriendsOnlyButton, instance.lobbyPrivacy == MatchmakingLobby.Visibility.FRIENDS);
			dataModel.Set("LobbyPrivacy", (int)instance.lobbyPrivacy);
			break;
		case TabletRule.TwitchVotingEnabled:
			titleText.text = ScriptLocalization.Options_Twitch.Enable_Twitch_Voting;
			onOffContainer.gameObject.SetActive(value: true);
			SetOnOffButtonStyles(instance.enableTwitchVoting);
			dataModel.Set("TwitchVoting", instance.enableTwitchVoting);
			break;
		case TabletRule.TwitchChatDisplay:
			titleText.text = ScriptLocalization.Options_Twitch.Show_Twitch_Chat;
			onOffContainer.gameObject.SetActive(value: true);
			SetOnOffButtonStyles(instance.showTwitchChat);
			dataModel.Set("TwitchChatDisplay", instance.showTwitchChat);
			break;
		case TabletRule.GameMode:
			titleText.text = ScriptLocalization.RuleBook.Game_Mode;
			gameModeContainer.gameObject.SetActive(value: true);
			SetButtonStyle(modePartyButton, instance.GameMode == GameState.GameMode.PARTY);
			SetButtonStyle(modeCreativeButton, instance.GameMode == GameState.GameMode.CREATIVE);
			SetButtonStyle(modeFreePlayButton, instance.GameMode == GameState.GameMode.FREEPLAY);
			SetButtonStyle(modeChallengeButton, instance.GameMode == GameState.GameMode.CHALLENGE);
			dataModel.Set("GameMode", (int)instance.GameMode);
			break;
		case TabletRule.CrossPlatformToggle:
			titleText.text = ScriptLocalization.Options.CrossPlatformPlay;
			onOffContainer.gameObject.SetActive(value: true);
			SetOnOffButtonStyles(instance.CrossPlatformToggle);
			dataModel.Set("CrossPlatformToggle", instance.CrossPlatformToggle);
			break;
		case TabletRule.ModifierGravity:
			titleText.text = ScriptLocalization.Modifiers.Gravity;
			modifierGravityContainer.gameObject.SetActive(value: true);
			SetButtonStyle(gravityNormalButton, instance2.GravityMode == 0);
			SetButtonStyle(gravityLowButton, instance2.GravityMode == 1);
			SetButtonStyle(gravityHighButton, instance2.GravityMode == 2);
			dataModel.Set("GravityMode", instance2.GravityMode);
			break;
		case TabletRule.ModifierJumpSpeed:
		{
			titleText.text = ScriptLocalization.Modifiers.Jump_Strength;
			modifierJumpSpeedContainer.gameObject.SetActive(value: true);
			for (int num11 = 0; num11 < jumpSpeedButtons.Length; num11++)
			{
				SetButtonStyle(jumpSpeedButtons[num11], num11 == instance2.JumpSpeedMode);
			}
			dataModel.Set("JumpSpeedMode", instance2.JumpSpeedMode);
			break;
		}
		case TabletRule.ModifierSprintSpeed:
		{
			titleText.text = ScriptLocalization.Modifiers.SprintSpeed;
			modifierSprintSpeedContainer.gameObject.SetActive(value: true);
			for (int num10 = 0; num10 < sprintSpeedButtons.Length; num10++)
			{
				SetButtonStyle(sprintSpeedButtons[num10], num10 == instance2.SprintSpeedMode);
			}
			dataModel.Set("SprintSpeedMode", instance2.SprintSpeedMode);
			break;
		}
		case TabletRule.ModifierWallJumpsDisabled:
			titleText.text = ScriptLocalization.Modifiers.Walljumps;
			onOffContainer.gameObject.SetActive(value: true);
			SetOnOffButtonStyles(!instance2.wallJumpsDisabled);
			dataModel.Set("WallJumps", !instance2.wallJumpsDisabled);
			break;
		case TabletRule.ModifierWallSlidesDisabled:
			titleText.text = ScriptLocalization.Modifiers.Walljumps;
			onOffContainer.gameObject.SetActive(value: true);
			SetOnOffButtonStyles(instance2.wallSlidesDisabled);
			dataModel.Set("WallSlidesDisabled", instance2.wallSlidesDisabled);
			break;
		case TabletRule.ModifierGameSpeed:
		{
			titleText.text = ScriptLocalization.Modifiers.GameSpeed;
			modifierGameSpeedContainer.gameObject.SetActive(value: true);
			for (int num7 = 0; num7 < gameSpeedButtons.Length; num7++)
			{
				SetButtonStyle(gameSpeedButtons[num7], num7 == instance2.GameSpeedMode);
			}
			dataModel.Set("GameSpeedMode", instance2.GameSpeedMode);
			break;
		}
		case TabletRule.ModifierDanceInvincibility:
			titleText.text = ScriptLocalization.Modifiers.DanceInvincibility;
			onOffContainer.gameObject.SetActive(value: true);
			SetOnOffButtonStyles(instance2.danceInvincibility);
			dataModel.Set("DanceInvincibility", instance2.danceInvincibility);
			break;
		case TabletRule.ModifierInvisibility:
		{
			titleText.text = ScriptLocalization.Modifiers.Invisibility;
			modifierInvisibilityContainer.gameObject.SetActive(value: true);
			for (int num6 = 0; num6 < invisibilityButtons.Length; num6++)
			{
				SetButtonStyle(invisibilityButtons[num6], instance2.invisibilityMode == num6);
			}
			dataModel.Set("InvisibilityMode", instance2.invisibilityMode);
			break;
		}
		case TabletRule.ModifierMirrorControls:
			titleText.text = ScriptLocalization.Modifiers.MirrorControls;
			onOffContainer.gameObject.SetActive(value: true);
			SetOnOffButtonStyles(instance2.mirrorControls);
			dataModel.Set("MirrorControls", instance2.mirrorControls);
			break;
		case TabletRule.ModifierPlatformSpeed:
		{
			titleText.text = ScriptLocalization.Modifiers.MoveBlockSpeed;
			modifierGameSpeedContainer.gameObject.SetActive(value: true);
			for (int num5 = 0; num5 < gameSpeedButtons.Length; num5++)
			{
				SetButtonStyle(gameSpeedButtons[num5], num5 == instance2.PlatformSpeedMode);
			}
			dataModel.Set("PlatformSpeedMode", instance2.PlatformSpeedMode);
			break;
		}
		case TabletRule.ModifierRateOfFire:
		{
			titleText.text = ScriptLocalization.Modifiers.ProjectileRateOfFire;
			modifierGameSpeedContainer.gameObject.SetActive(value: true);
			for (int num3 = 0; num3 < gameSpeedButtons.Length; num3++)
			{
				SetButtonStyle(gameSpeedButtons[num3], num3 == instance2.RateOfFireMode);
			}
			dataModel.Set("RateOfFireMode", instance2.RateOfFireMode);
			break;
		}
		case TabletRule.ModifierMultiJump:
		{
			titleText.text = ScriptLocalization.Modifiers.MultiJump;
			modifierMultiJumpContainer.gameObject.SetActive(value: true);
			for (int num2 = 0; num2 < multiJumpButtons.Length; num2++)
			{
				SetButtonStyle(multiJumpButtons[num2], num2 == instance2.MultiJumpMode);
			}
			dataModel.Set("MultiJumpMode", instance2.MultiJumpMode);
			break;
		}
		case TabletRule.ModifierProjectilesExplode:
		{
			titleText.text = ScriptLocalization.Modifiers.ProjectileExplosions;
			modifierProjectilesExplodeContainer.gameObject.SetActive(value: true);
			for (int n = 0; n < projectilesExplodeButtons.Length; n++)
			{
				SetButtonStyle(projectilesExplodeButtons[n], n == instance2.ProjectileExplosionMode);
			}
			dataModel.Set("ProjectileExplosionMode", instance2.ProjectileExplosionMode);
			break;
		}
		case TabletRule.ModifierCharacterSize:
		{
			titleText.text = ScriptLocalization.Modifiers.CharacterSize;
			modifierCharacterSizeContainer.gameObject.SetActive(value: true);
			for (int m = 0; m < characterSizeButtons.Length; m++)
			{
				SetButtonStyle(characterSizeButtons[m], m == instance2.CharacterSizeMode);
			}
			dataModel.Set("CharacterSizeMode", instance2.CharacterSizeMode);
			break;
		}
		case TabletRule.ModifierJetpackMode:
			titleText.text = ScriptLocalization.Modifiers.JetPacksExclamation;
			onOffContainer.gameObject.SetActive(value: true);
			SetOnOffButtonStyles(instance2.jetpackMode);
			dataModel.Set("JetpackMode", instance2.jetpackMode);
			break;
		case TabletRule.ModifierPostDeathBehaviorMode:
		{
			titleText.text = ScriptLocalization.Modifiers.PostDeathBehaviour;
			modifierPostDeathBehaviorContainer.gameObject.SetActive(value: true);
			for (int l = 0; l < postDeathBehaviorButtons.Length; l++)
			{
				SetButtonStyle(postDeathBehaviorButtons[l], l == instance2.PostDeathBehaviorMode);
			}
			dataModel.Set("PostDeathBehaviorMode", instance2.PostDeathBehaviorMode);
			break;
		}
		case TabletRule.ModifierMirrorLevel:
		{
			titleText.text = ScriptLocalization.Modifiers.MirrorLevel;
			modifierMirrorLevelContainer.gameObject.SetActive(value: true);
			for (int k = 0; k < mirrorLevelButtons.Length; k++)
			{
				SetButtonStyle(mirrorLevelButtons[k], k == instance2.CameraFlipMode);
			}
			dataModel.Set("CameraFlipMode", instance2.CameraFlipMode);
			break;
		}
		case TabletRule.ModifierDoomsdayMeteors:
			titleText.text = ScriptLocalization.Modifiers.DoomsdayMeteors;
			plusMinusContainer.gameObject.SetActive(value: true);
			okButtonContainer.gameObject.SetActive(value: true);
			dataModel.Set("DoomsdayMeteorsMode", instance2.DoomsdayMeteorsMode);
			break;
		case TabletRule.ModifierDoomsdayLava:
			titleText.text = ScriptLocalization.Modifiers.DoomsdayLava;
			plusMinusContainer.gameObject.SetActive(value: true);
			okButtonContainer.gameObject.SetActive(value: true);
			dataModel.Set("DoomsdayLavaMode", instance2.DoomsdayLavaMode);
			break;
		case TabletRule.ModifierPlayerPlayerCollisions:
			titleText.text = ScriptLocalization.Modifiers.PlayerPlayerCollisions;
			onOffContainer.gameObject.SetActive(value: true);
			SetOnOffButtonStyles(instance2.playerPlayerCollisions);
			dataModel.Set("PlayerPlayerCollisions", instance2.playerPlayerCollisions);
			break;
		case TabletRule.ModifierProjectileSpeed:
		{
			titleText.text = ScriptLocalization.Modifiers.ProjectileSpeed;
			modifierGameSpeedContainer.gameObject.SetActive(value: true);
			for (int j = 0; j < gameSpeedButtons.Length; j++)
			{
				SetButtonStyle(gameSpeedButtons[j], j == instance2.ProjectileSpeedMode);
			}
			dataModel.Set("ProjectileSpeedMode", instance2.ProjectileSpeedMode);
			break;
		}
		case TabletRule.RespawnMode:
			titleText.text = ScriptLocalization.RuleBook_Presets.Respawn;
			respawnModeContainer.gameObject.SetActive(value: true);
			okButtonContainer.gameObject.SetActive(value: true);
			dataModel.Set("RespawnMode", (int)instance.respawnMode);
			dataModel.Set("NumRespawns", instance.numRespawns);
			break;
		case TabletRule.PartyBoxMode:
		{
			titleText.text = ScriptLocalization.RuleBook_Presets.PartyBoxMode;
			partyBoxModeContainer.gameObject.SetActive(value: true);
			int partyBoxMode = (int)instance.partyBoxMode;
			for (int i = 0; i < partyBoxModeButtons.Length; i++)
			{
				SetButtonStyle(partyBoxModeButtons[i], i == partyBoxMode);
			}
			dataModel.Set("PartyBoxMode", (int)instance.partyBoxMode);
			break;
		}
		case TabletRule.ModifierFrictionless:
			titleText.text = ScriptLocalization.Modifiers.Frictionless;
			onOffContainer.gameObject.SetActive(value: true);
			SetOnOffButtonStyles(instance2.frictionless);
			dataModel.Set("Frictionless", instance2.frictionless);
			break;
		}
		dataModel.dirty = false;
		ApplyDataModel(overlayType, dataModel);
		TransitionDialog(transitioningIn: true);
	}

	private void TransitionDialog(bool transitioningIn)
	{
		if (transitioningIn)
		{
			isOpening = true;
			base.gameObject.SetActive(value: true);
			backgroundCanvasGroup.alpha = 0f;
			multipleChoiceCanvasGroup.alpha = 0f;
		}
		SequenceTweener sequenceTweener = new SequenceTweener();
		if (transitioningIn)
		{
			sequenceTweener.Add(new CanvasGroupAlphaTweener(backgroundCanvasGroup, 0f, 1f, 0.2f, Easings.Functions.QuadraticEaseOut));
			sequenceTweener.Add(new DelayTweener(0.1f, new CanvasGroupAlphaTweener(multipleChoiceCanvasGroup, 0f, 1f, 0.1f, Easings.Functions.QuadraticEaseOut)));
		}
		else
		{
			sequenceTweener.Add(new CanvasGroupAlphaTweener(multipleChoiceCanvasGroup, multipleChoiceCanvasGroup.alpha, 0f, 0.1f, Easings.Functions.QuadraticEaseOut));
			sequenceTweener.Add(new CanvasGroupAlphaTweener(backgroundCanvasGroup, backgroundCanvasGroup.alpha, 0f, 0.2f, Easings.Functions.QuadraticEaseOut));
		}
		sequenceTweener.SetOnFinish(delegate
		{
			isOpening = false;
			if (!transitioningIn)
			{
				base.gameObject.SetActive(value: false);
			}
			else
			{
				isOpen = true;
			}
		});
		backgroundAnim = sequenceTweener.PrimeAndAnimate();
		if (transitioningIn)
		{
			AkSoundEngine.PostEvent("UI_UPad_Modal_Open", base.gameObject);
		}
		else
		{
			AkSoundEngine.PostEvent("UI_UPad_Modal_Close", base.gameObject);
		}
	}

	private void SetOnOffButtonStyles(bool buttonOn)
	{
		onButton.buttonType = ((!buttonOn) ? TabletButton.ButtonType.Transparent : TabletButton.ButtonType.Simple);
		offButton.buttonType = (buttonOn ? TabletButton.ButtonType.Transparent : TabletButton.ButtonType.Simple);
	}

	private void SetButtonStyle(TabletButton button, bool buttonOn)
	{
		button.buttonType = ((!buttonOn) ? TabletButton.ButtonType.Transparent : TabletButton.ButtonType.Simple);
	}

	public void ShowSimpleMessage(string title, string message, UnityAction OnModalClosed)
	{
		Initialize(TabletRule.SimpleMessage, OnModalClosed);
		titleText.text = title;
		simpleMessageText.text = message;
	}

	private void ResetDialog()
	{
		roundsTimeContainer.gameObject.SetActive(value: false);
		plusMinusContainer.gameObject.SetActive(value: false);
		okButtonContainer.gameObject.SetActive(value: false);
		onOffContainer.gameObject.SetActive(value: false);
		pointOnOffContainer.gameObject.SetActive(value: false);
		simpleMessageContainer.gameObject.SetActive(value: false);
		reportReasonsContainer.gameObject.SetActive(value: false);
		cameraFollowsContainer.gameObject.SetActive(value: false);
		nameVisibilityContainer.gameObject.SetActive(value: false);
		doublePartyBoxContainer.gameObject.SetActive(value: false);
		chatAndEmotesContainer.gameObject.SetActive(value: false);
		lobbyTagContainer.gameObject.SetActive(value: false);
		lobbyPrivacyContainer.gameObject.SetActive(value: false);
		gameModeContainer.gameObject.SetActive(value: false);
		modifierGravityContainer.gameObject.SetActive(value: false);
		modifierJumpSpeedContainer.gameObject.SetActive(value: false);
		modifierSprintSpeedContainer.gameObject.SetActive(value: false);
		modifierGameSpeedContainer.gameObject.SetActive(value: false);
		modifierMultiJumpContainer.gameObject.SetActive(value: false);
		modifierProjectilesExplodeContainer.gameObject.SetActive(value: false);
		modifierCharacterSizeContainer.gameObject.SetActive(value: false);
		modifierPostDeathBehaviorContainer.gameObject.SetActive(value: false);
		modifierMirrorLevelContainer.gameObject.SetActive(value: false);
		modifierInvisibilityContainer.gameObject.SetActive(value: false);
		respawnModeContainer.gameObject.SetActive(value: false);
		partyBoxModeContainer.gameObject.SetActive(value: false);
	}

	public void OnClickPlus()
	{
		GameSettings instance = GameSettings.GetInstance();
		Modifiers instance2 = Modifiers.GetInstance();
		switch (currentOverlayType)
		{
		case TabletRule.PointsToWin:
			dataModel.IncrementIntClamped("MaxScore", 50, instance.minMaxScore, instance.maxMaxScore);
			break;
		case TabletRule.LengthLimit:
			switch ((GameLimitType)dataModel.GetInt("GameLimitType"))
			{
			case GameLimitType.ROUNDS:
				dataModel.IncrementIntClamped("MaxRounds", 1, instance.minMaxRounds, instance.maxMaxRounds);
				break;
			case GameLimitType.TIME:
				dataModel.IncrementIntClamped("MaxTime", 60, instance.minMaxTime, instance.maxMaxTime);
				break;
			}
			break;
		case TabletRule.PlacementTimer:
			dataModel.IncrementIntClamped("PlaceTime", 5, 0, instance.MaxPlaceTime);
			break;
		case TabletRule.RunTimeLimit:
			if (dataModel.GetInt("RunTimerLimit") >= 60)
			{
				dataModel.IncrementIntClamped("RunTimerLimit", 30, instance.minRunTimer, instance.maxRunTimer);
			}
			else
			{
				dataModel.IncrementIntClamped("RunTimerLimit", 15, instance.minRunTimer, instance.maxRunTimer);
			}
			break;
		case TabletRule.PiecesPerRound:
			dataModel.IncrementIntClamped("CreativePiecesPerRound", 1, 1, instance.MaxCreativePieces);
			break;
		case TabletRule.OptionResolution:
		{
			int currentW = dataModel.GetInt("ResX");
			int currentH = dataModel.GetInt("ResY");
			int currentR = dataModel.GetInt("RefreshRate");
			int resIndex = TabletOptionsScreen.GetResIndex(currentW, currentH, currentR);
			resIndex = ((resIndex != -1) ? ((resIndex + 1) % TabletOptionsScreen.ValidResolutions.Count) : 0);
			Resolution resolution = TabletOptionsScreen.ValidResolutions[resIndex];
			dataModel.Set("ResX", resolution.width);
			dataModel.Set("ResY", resolution.height);
			dataModel.Set("RefreshRate", resolution.refreshRate);
			break;
		}
		case TabletRule.OptionQuality:
		{
			int num4 = dataModel.GetInt("Quality");
			dataModel.Set("Quality", (num4 + 2) % 3);
			break;
		}
		case TabletRule.OnlineSettingsAFKKickTime:
		{
			int num3 = dataModel.GetInt("AFKKicker");
			num3 = (num3 + 1) % afkKickerTimes.Length;
			dataModel.Set("AFKKicker", num3);
			break;
		}
		case TabletRule.ModifierDoomsdayMeteors:
		{
			int num2 = dataModel.GetInt("DoomsdayMeteorsMode");
			num2 = (num2 + 1) % instance2.DoomsdayModifierTimes.Length;
			dataModel.Set("DoomsdayMeteorsMode", num2);
			break;
		}
		case TabletRule.ModifierDoomsdayLava:
		{
			int num = dataModel.GetInt("DoomsdayLavaMode");
			num = (num + 1) % instance2.DoomsdayModifierTimes.Length;
			dataModel.Set("DoomsdayLavaMode", num);
			break;
		}
		case TabletRule.RespawnMode:
		{
			RespawnMode respawnMode = (RespawnMode)dataModel.GetInt("RespawnMode");
			if ((uint)(respawnMode - 1) <= 2u)
			{
				dataModel.IncrementIntClamped("NumRespawns", 1, instance.minRespawns, instance.maxRespawns);
			}
			break;
		}
		default:
			return;
		}
		ApplyDataModel(currentOverlayType, dataModel);
	}

	public void OnClickMinus()
	{
		GameSettings instance = GameSettings.GetInstance();
		Modifiers instance2 = Modifiers.GetInstance();
		switch (currentOverlayType)
		{
		case TabletRule.PointsToWin:
			dataModel.IncrementIntClamped("MaxScore", -50, instance.minMaxScore, instance.maxMaxScore);
			break;
		case TabletRule.LengthLimit:
			switch ((GameLimitType)dataModel.GetInt("GameLimitType"))
			{
			case GameLimitType.ROUNDS:
				dataModel.IncrementIntClamped("MaxRounds", -1, instance.minMaxRounds, instance.maxMaxRounds);
				break;
			case GameLimitType.TIME:
				dataModel.IncrementIntClamped("MaxTime", -60, instance.minMaxTime, instance.maxMaxTime);
				break;
			}
			break;
		case TabletRule.PlacementTimer:
			dataModel.IncrementIntClamped("PlaceTime", -5, 0, instance.MaxPlaceTime);
			break;
		case TabletRule.RunTimeLimit:
			if (dataModel.GetInt("RunTimerLimit") <= 60)
			{
				dataModel.IncrementIntClamped("RunTimerLimit", -15, instance.minRunTimer, instance.maxRunTimer);
			}
			else
			{
				dataModel.IncrementIntClamped("RunTimerLimit", -30, instance.minRunTimer, instance.maxRunTimer);
			}
			break;
		case TabletRule.PiecesPerRound:
			dataModel.IncrementIntClamped("CreativePiecesPerRound", -1, 1, instance.MaxCreativePieces);
			break;
		case TabletRule.OptionResolution:
		{
			int currentW = dataModel.GetInt("ResX");
			int currentH = dataModel.GetInt("ResY");
			int currentR = dataModel.GetInt("RefreshRate");
			int resIndex = TabletOptionsScreen.GetResIndex(currentW, currentH, currentR);
			resIndex = ((resIndex != -1) ? ((resIndex - 1 + TabletOptionsScreen.ValidResolutions.Count) % TabletOptionsScreen.ValidResolutions.Count) : 0);
			Resolution resolution = TabletOptionsScreen.ValidResolutions[resIndex];
			dataModel.Set("ResX", resolution.width);
			dataModel.Set("ResY", resolution.height);
			dataModel.Set("RefreshRate", resolution.refreshRate);
			break;
		}
		case TabletRule.OptionQuality:
		{
			int num4 = dataModel.GetInt("Quality");
			dataModel.Set("Quality", (num4 + 1) % 3);
			break;
		}
		case TabletRule.OnlineSettingsAFKKickTime:
		{
			int num3 = dataModel.GetInt("AFKKicker");
			num3 = (num3 - 1 + afkKickerTimes.Length) % afkKickerTimes.Length;
			dataModel.Set("AFKKicker", num3);
			break;
		}
		case TabletRule.ModifierDoomsdayMeteors:
		{
			int num2 = dataModel.GetInt("DoomsdayMeteorsMode");
			num2 = (num2 - 1 + instance2.DoomsdayModifierTimes.Length) % instance2.DoomsdayModifierTimes.Length;
			dataModel.Set("DoomsdayMeteorsMode", num2);
			break;
		}
		case TabletRule.ModifierDoomsdayLava:
		{
			int num = dataModel.GetInt("DoomsdayLavaMode");
			num = (num - 1 + instance2.DoomsdayModifierTimes.Length) % instance2.DoomsdayModifierTimes.Length;
			dataModel.Set("DoomsdayLavaMode", num);
			break;
		}
		case TabletRule.RespawnMode:
		{
			RespawnMode respawnMode = (RespawnMode)dataModel.GetInt("RespawnMode");
			if ((uint)(respawnMode - 1) <= 2u)
			{
				dataModel.IncrementIntClamped("NumRespawns", -1, instance.minRespawns, instance.maxRespawns);
			}
			break;
		}
		default:
			return;
		}
		ApplyDataModel(currentOverlayType, dataModel);
	}

	public void OnSelectChoice(int idx)
	{
		switch (currentOverlayType)
		{
		case TabletRule.DoublePartyBox:
			dataModel.Set("DoublePartyBox", idx);
			OnAccept();
			break;
		case TabletRule.PointEnabled:
			dataModel.Set("PointEnabled", idx);
			OnAccept();
			break;
		case TabletRule.OptionVsync:
			dataModel.Set("VSync", idx);
			OnAccept();
			break;
		case TabletRule.OptionWindowed:
			dataModel.Set("Windowed", idx != 0);
			OnAccept();
			break;
		case TabletRule.OptionShowVersion:
			dataModel.Set("ShowVersion", idx != 0);
			OnAccept();
			break;
		case TabletRule.OptionBackgroundAudio:
			dataModel.Set("BackgroundAudio", idx != 0);
			OnAccept();
			break;
		case TabletRule.PlayerReportReason:
			dataModel.Set("ReportReason", idx);
			OnAccept();
			break;
		case TabletRule.OnlineSettingsCameraFollows:
			dataModel.Set("CameraFollows", idx);
			OnAccept();
			break;
		case TabletRule.OnlineSettingsEmotes:
			dataModel.Set("ChatAndEmotes", idx);
			OnAccept();
			break;
		case TabletRule.OnlineSettingsNameDisplay:
			dataModel.Set("NameVisibility", idx);
			OnAccept();
			break;
		case TabletRule.LobbyOptionsTag:
			dataModel.Set("LobbyTag", idx);
			OnAccept();
			break;
		case TabletRule.LobbyOptionsPrivacy:
			dataModel.Set("LobbyPrivacy", idx);
			OnAccept();
			break;
		case TabletRule.TwitchVotingEnabled:
			dataModel.Set("TwitchVoting", idx != 0);
			OnAccept();
			break;
		case TabletRule.TwitchChatDisplay:
			dataModel.Set("TwitchChatDisplay", idx != 0);
			OnAccept();
			break;
		case TabletRule.GameMode:
			dataModel.Set("GameMode", idx);
			OnAccept();
			break;
		case TabletRule.CrossPlatformToggle:
			dataModel.Set("CrossPlatformToggle", idx != 0);
			OnAccept();
			break;
		case TabletRule.ModifierGravity:
			dataModel.Set("GravityMode", idx);
			OnAccept();
			break;
		case TabletRule.ModifierJumpSpeed:
			dataModel.Set("JumpSpeedMode", idx);
			OnAccept();
			break;
		case TabletRule.ModifierSprintSpeed:
			dataModel.Set("SprintSpeedMode", idx);
			OnAccept();
			break;
		case TabletRule.ModifierWallJumpsDisabled:
			dataModel.Set("WallJumps", idx != 0);
			OnAccept();
			break;
		case TabletRule.ModifierWallSlidesDisabled:
			dataModel.Set("WallSlidesDisabled", idx != 0);
			OnAccept();
			break;
		case TabletRule.ModifierGameSpeed:
			dataModel.Set("GameSpeedMode", idx);
			OnAccept();
			break;
		case TabletRule.ModifierDanceInvincibility:
			dataModel.Set("DanceInvincibility", idx != 0);
			OnAccept();
			break;
		case TabletRule.ModifierInvisibility:
			dataModel.Set("InvisibilityMode", idx);
			OnAccept();
			break;
		case TabletRule.ModifierMirrorControls:
			dataModel.Set("MirrorControls", idx != 0);
			OnAccept();
			break;
		case TabletRule.ModifierPlatformSpeed:
			dataModel.Set("PlatformSpeedMode", idx);
			OnAccept();
			break;
		case TabletRule.ModifierRateOfFire:
			dataModel.Set("RateOfFireMode", idx);
			OnAccept();
			break;
		case TabletRule.ModifierMultiJump:
			dataModel.Set("MultiJumpMode", idx);
			OnAccept();
			break;
		case TabletRule.ModifierProjectilesExplode:
			dataModel.Set("ProjectileExplosionMode", idx);
			OnAccept();
			break;
		case TabletRule.ModifierCharacterSize:
			dataModel.Set("CharacterSizeMode", idx);
			OnAccept();
			break;
		case TabletRule.ModifierJetpackMode:
			dataModel.Set("JetpackMode", idx != 0);
			OnAccept();
			break;
		case TabletRule.ModifierPostDeathBehaviorMode:
			dataModel.Set("PostDeathBehaviorMode", idx);
			OnAccept();
			break;
		case TabletRule.ModifierMirrorLevel:
			dataModel.Set("CameraFlipMode", idx);
			OnAccept();
			break;
		case TabletRule.ModifierPlayerPlayerCollisions:
			dataModel.Set("PlayerPlayerCollisions", idx != 0);
			OnAccept();
			break;
		case TabletRule.ModifierProjectileSpeed:
			dataModel.Set("ProjectileSpeedMode", idx);
			OnAccept();
			break;
		case TabletRule.PartyBoxMode:
			dataModel.Set("PartyBoxMode", idx);
			OnAccept();
			break;
		case TabletRule.ModifierFrictionless:
			dataModel.Set("Frictionless", idx != 0);
			OnAccept();
			break;
		}
	}

	public void OnAccept()
	{
		GameSettings instance = GameSettings.GetInstance();
		Modifiers instance2 = Modifiers.GetInstance();
		switch (currentOverlayType)
		{
		case TabletRule.PointsToWin:
			if (dataModel.dirty)
			{
				instance.MaxScore = dataModel.GetInt("MaxScore");
				BroadcastRuleChange(currentOverlayType, instance.MaxScore);
				rulesScreen.MarkRulesDirty();
			}
			break;
		case TabletRule.LengthLimit:
			if (dataModel.dirty)
			{
				instance.GameLimitType = (GameLimitType)dataModel.GetInt("GameLimitType");
				int value2 = 0;
				switch (instance.GameLimitType)
				{
				case GameLimitType.ROUNDS:
					instance.MaxRounds = dataModel.GetInt("MaxRounds");
					value2 = instance.MaxRounds;
					break;
				case GameLimitType.TIME:
					instance.MaxTime = dataModel.GetInt("MaxTime");
					value2 = instance.MaxTime;
					break;
				}
				BroadcastRuleChange(currentOverlayType, (int)instance.GameLimitType, value2);
				rulesScreen.MarkRulesDirty();
			}
			break;
		case TabletRule.PlacementTimer:
			if (dataModel.dirty)
			{
				instance.PlaceTime = dataModel.GetInt("PlaceTime");
				instance.UsePlaceTimer = instance.PlaceTime > 0f;
				BroadcastRuleChange(currentOverlayType, (int)instance.PlaceTime);
				rulesScreen.MarkRulesDirty();
			}
			break;
		case TabletRule.RunTimeLimit:
			if (dataModel.dirty)
			{
				instance.RunTimerLimit = dataModel.GetInt("RunTimerLimit");
				BroadcastRuleChange(currentOverlayType, instance.RunTimerLimit);
				rulesScreen.MarkRulesDirty();
			}
			break;
		case TabletRule.DoublePartyBox:
			if (dataModel.dirty)
			{
				int doublePartyBox = dataModel.GetInt("DoublePartyBox");
				instance.DoublePartyBox = (DoublePartyBox)doublePartyBox;
				BroadcastRuleChange(currentOverlayType, (int)instance.DoublePartyBox);
				rulesScreen.MarkRulesDirty();
			}
			break;
		case TabletRule.PiecesPerRound:
			if (dataModel.dirty)
			{
				instance.CreativePiecesPerRound = dataModel.GetInt("CreativePiecesPerRound");
				BroadcastRuleChange(currentOverlayType, instance.CreativePiecesPerRound);
				rulesScreen.MarkRulesDirty();
			}
			break;
		case TabletRule.PointEnabled:
			if (dataModel.dirty)
			{
				int num2 = dataModel.GetInt("PointEnabled");
				if (num2 > 0)
				{
					instance.SetPointTypeEnabled(currentPointType, enabled: true);
					instance.SetAlwaysAwardPointType(currentPointType, num2 == 2);
				}
				else
				{
					instance.SetPointTypeEnabled(currentPointType, enabled: false);
					instance.SetAlwaysAwardPointType(currentPointType, alwaysAward: false);
				}
				BroadcastRuleChange(currentOverlayType, (int)currentPointType, num2);
				rulesScreen.MarkRulesDirty();
			}
			break;
		case TabletRule.OptionResolution:
			if (dataModel.dirty)
			{
				optionsScreen.OnApplyResolution(dataModel.GetInt("ResX"), dataModel.GetInt("ResY"));
			}
			break;
		case TabletRule.OptionQuality:
			if (dataModel.dirty)
			{
				QualitySettings.masterTextureLimit = dataModel.GetInt("Quality");
			}
			break;
		case TabletRule.OptionVsync:
			if (dataModel.dirty)
			{
				int num = dataModel.GetInt("VSync");
				StatTracker.Instance.GetSaveFileDataForMainUser().VSync = num != 0;
				QualitySettings.vSyncCount = num;
			}
			break;
		case TabletRule.OptionWindowed:
			if (dataModel.dirty)
			{
				Screen.fullScreen = !dataModel.GetBool("Windowed");
			}
			break;
		case TabletRule.OptionShowVersion:
			if (dataModel.dirty)
			{
				StatTracker.Instance.GetSaveFileDataForMainUser().HideVersion = !dataModel.GetBool("ShowVersion");
			}
			break;
		case TabletRule.OptionBackgroundAudio:
			if (dataModel.dirty)
			{
				StatTracker.Instance.GetSaveFileDataForMainUser().BackgroundAudio = dataModel.GetBool("BackgroundAudio");
			}
			break;
		case TabletRule.PlayerReportReason:
			if (dataModel.dirty)
			{
				playersScreen.SetReportReason((UserReports.ReportReason)dataModel.GetInt("ReportReason"));
			}
			break;
		case TabletRule.OnlineSettingsCameraFollows:
			if (dataModel.dirty)
			{
				optionsScreen.SetCameraLocalOnly(dataModel.GetInt("CameraFollows") == 1);
			}
			break;
		case TabletRule.OnlineSettingsAFKKickTime:
			if (dataModel.dirty)
			{
				int timeInSeconds = afkKickerTimes[dataModel.GetInt("AFKKicker")];
				optionsScreen.SetAFKKickTime(timeInSeconds, asHost: true);
			}
			break;
		case TabletRule.OnlineSettingsEmotes:
			if (dataModel.dirty)
			{
				optionsScreen.SetChatAndEmotes(dataModel.GetInt("ChatAndEmotes"));
			}
			break;
		case TabletRule.OnlineSettingsNameDisplay:
			if (dataModel.dirty)
			{
				optionsScreen.SetNameVisibility(dataModel.GetInt("NameVisibility"));
			}
			break;
		case TabletRule.LobbyOptionsTag:
			if (dataModel.dirty)
			{
				lobbyOptionsScreen.SetLobbyTag((LobbyTags)dataModel.GetInt("LobbyTag"));
			}
			break;
		case TabletRule.LobbyOptionsPrivacy:
			if (dataModel.dirty)
			{
				lobbyOptionsScreen.SetLobbyPrivacy((MatchmakingLobby.Visibility)dataModel.GetInt("LobbyPrivacy"));
			}
			break;
		case TabletRule.TwitchVotingEnabled:
			if (dataModel.dirty)
			{
				twitchOptionsScreen.SetTwitchVotingEnabled(dataModel.GetBool("TwitchVoting"));
			}
			break;
		case TabletRule.TwitchChatDisplay:
			if (dataModel.dirty)
			{
				twitchOptionsScreen.SetTwitchChatDisplay(dataModel.GetBool("TwitchChatDisplay"));
			}
			break;
		case TabletRule.GameMode:
			if (dataModel.dirty)
			{
				rulesScreen.OnSelectGameMode((GameState.GameMode)dataModel.GetInt("GameMode"));
			}
			break;
		case TabletRule.CrossPlatformToggle:
			if (dataModel.dirty)
			{
				optionsScreen.SetCrossPlatformToggle(dataModel.GetBool("CrossPlatformToggle"));
			}
			break;
		case TabletRule.ModifierGravity:
			if (dataModel.dirty)
			{
				instance2.GravityMode = dataModel.GetInt("GravityMode");
				BroadcastRuleChange(currentOverlayType, instance2.GravityMode);
				rulesScreen.MarkRulesDirty();
			}
			break;
		case TabletRule.ModifierJumpSpeed:
			if (dataModel.dirty)
			{
				instance2.JumpSpeedMode = dataModel.GetInt("JumpSpeedMode");
				BroadcastRuleChange(currentOverlayType, instance2.JumpSpeedMode);
				rulesScreen.MarkRulesDirty();
			}
			break;
		case TabletRule.ModifierSprintSpeed:
			if (dataModel.dirty)
			{
				instance2.SprintSpeedMode = dataModel.GetInt("SprintSpeedMode");
				BroadcastRuleChange(currentOverlayType, instance2.SprintSpeedMode);
				rulesScreen.MarkRulesDirty();
			}
			break;
		case TabletRule.ModifierWallJumpsDisabled:
			if (dataModel.dirty)
			{
				instance2.wallJumpsDisabled = !dataModel.GetBool("WallJumps");
				BroadcastRuleChange(currentOverlayType, 0, 0, instance2.wallJumpsDisabled);
				rulesScreen.MarkRulesDirty();
			}
			break;
		case TabletRule.ModifierWallSlidesDisabled:
			if (dataModel.dirty)
			{
				instance2.wallSlidesDisabled = dataModel.GetBool("WallSlidesDisabled");
				BroadcastRuleChange(currentOverlayType, 0, 0, instance2.wallSlidesDisabled);
				rulesScreen.MarkRulesDirty();
			}
			break;
		case TabletRule.ModifierGameSpeed:
			if (dataModel.dirty)
			{
				instance2.GameSpeedMode = dataModel.GetInt("GameSpeedMode");
				Time.timeScale = instance2.GameSpeed;
				BroadcastRuleChange(currentOverlayType, instance2.GameSpeedMode);
				rulesScreen.MarkRulesDirty();
			}
			break;
		case TabletRule.ModifierDanceInvincibility:
			if (dataModel.dirty)
			{
				instance2.danceInvincibility = dataModel.GetBool("DanceInvincibility");
				BroadcastRuleChange(currentOverlayType, 0, 0, instance2.danceInvincibility);
				rulesScreen.MarkRulesDirty();
			}
			break;
		case TabletRule.ModifierInvisibility:
			if (dataModel.dirty)
			{
				instance2.invisibilityMode = dataModel.GetInt("InvisibilityMode");
				BroadcastRuleChange(currentOverlayType, instance2.invisibilityMode);
				rulesScreen.MarkRulesDirty();
			}
			break;
		case TabletRule.ModifierMirrorControls:
			if (dataModel.dirty)
			{
				instance2.mirrorControls = dataModel.GetBool("MirrorControls");
				BroadcastRuleChange(currentOverlayType, 0, 0, instance2.mirrorControls);
				rulesScreen.MarkRulesDirty();
			}
			break;
		case TabletRule.ModifierPlatformSpeed:
			if (dataModel.dirty)
			{
				instance2.PlatformSpeedMode = dataModel.GetInt("PlatformSpeedMode");
				BroadcastRuleChange(currentOverlayType, instance2.PlatformSpeedMode);
				rulesScreen.MarkRulesDirty();
			}
			break;
		case TabletRule.ModifierRateOfFire:
			if (dataModel.dirty)
			{
				instance2.RateOfFireMode = dataModel.GetInt("RateOfFireMode");
				BroadcastRuleChange(currentOverlayType, instance2.RateOfFireMode);
				rulesScreen.MarkRulesDirty();
				instance2.OnModifiersDynamicChange();
			}
			break;
		case TabletRule.ModifierMultiJump:
			if (dataModel.dirty)
			{
				instance2.MultiJumpMode = dataModel.GetInt("MultiJumpMode");
				BroadcastRuleChange(currentOverlayType, instance2.MultiJumpMode);
				rulesScreen.MarkRulesDirty();
			}
			break;
		case TabletRule.ModifierProjectilesExplode:
			if (dataModel.dirty)
			{
				instance2.ProjectileExplosionMode = dataModel.GetInt("ProjectileExplosionMode");
				BroadcastRuleChange(currentOverlayType, instance2.ProjectileExplosionMode);
				rulesScreen.MarkRulesDirty();
			}
			break;
		case TabletRule.ModifierCharacterSize:
			if (dataModel.dirty)
			{
				instance2.CharacterSizeMode = dataModel.GetInt("CharacterSizeMode");
				BroadcastRuleChange(currentOverlayType, instance2.CharacterSizeMode);
				rulesScreen.MarkRulesDirty();
				instance2.OnModifiersDynamicChange();
			}
			break;
		case TabletRule.ModifierJetpackMode:
			if (dataModel.dirty)
			{
				instance2.jetpackMode = dataModel.GetBool("JetpackMode");
				BroadcastRuleChange(currentOverlayType, 0, 0, instance2.jetpackMode);
				rulesScreen.MarkRulesDirty();
			}
			break;
		case TabletRule.ModifierPostDeathBehaviorMode:
			if (dataModel.dirty)
			{
				instance2.PostDeathBehaviorMode = dataModel.GetInt("PostDeathBehaviorMode");
				BroadcastRuleChange(currentOverlayType, instance2.PostDeathBehaviorMode);
				rulesScreen.MarkRulesDirty();
			}
			break;
		case TabletRule.ModifierMirrorLevel:
			if (dataModel.dirty)
			{
				instance2.CameraFlipMode = dataModel.GetInt("CameraFlipMode");
				BroadcastRuleChange(currentOverlayType, instance2.CameraFlipMode);
				rulesScreen.MarkRulesDirty();
			}
			break;
		case TabletRule.ModifierDoomsdayMeteors:
			if (dataModel.dirty)
			{
				instance2.DoomsdayMeteorsMode = dataModel.GetInt("DoomsdayMeteorsMode");
				BroadcastRuleChange(currentOverlayType, instance2.DoomsdayMeteorsMode);
				rulesScreen.MarkRulesDirty();
			}
			break;
		case TabletRule.ModifierDoomsdayLava:
			if (dataModel.dirty)
			{
				instance2.DoomsdayLavaMode = dataModel.GetInt("DoomsdayLavaMode");
				BroadcastRuleChange(currentOverlayType, instance2.DoomsdayLavaMode);
				rulesScreen.MarkRulesDirty();
			}
			break;
		case TabletRule.ModifierPlayerPlayerCollisions:
			if (dataModel.dirty)
			{
				instance2.playerPlayerCollisions = dataModel.GetBool("PlayerPlayerCollisions");
				BroadcastRuleChange(currentOverlayType, 0, 0, instance2.playerPlayerCollisions);
				rulesScreen.MarkRulesDirty();
			}
			break;
		case TabletRule.ModifierProjectileSpeed:
			if (dataModel.dirty)
			{
				instance2.ProjectileSpeedMode = dataModel.GetInt("ProjectileSpeedMode");
				BroadcastRuleChange(currentOverlayType, instance2.ProjectileSpeedMode);
				rulesScreen.MarkRulesDirty();
			}
			break;
		case TabletRule.RespawnMode:
			if (dataModel.dirty)
			{
				instance.respawnMode = (RespawnMode)dataModel.GetInt("RespawnMode");
				int value = 0;
				RespawnMode respawnMode = instance.respawnMode;
				if ((uint)(respawnMode - 1) <= 2u)
				{
					instance.numRespawns = dataModel.GetInt("NumRespawns");
					value = instance.numRespawns;
				}
				BroadcastRuleChange(currentOverlayType, (int)instance.respawnMode, value);
				rulesScreen.MarkRulesDirty();
			}
			break;
		case TabletRule.PartyBoxMode:
			if (dataModel.dirty)
			{
				instance.partyBoxMode = (PartyBoxMode)dataModel.GetInt("PartyBoxMode");
				BroadcastRuleChange(currentOverlayType, (int)instance.partyBoxMode);
				rulesScreen.MarkRulesDirty();
			}
			break;
		case TabletRule.ModifierFrictionless:
			if (dataModel.dirty)
			{
				instance2.frictionless = dataModel.GetBool("Frictionless");
				BroadcastRuleChange(currentOverlayType, 0, 0, instance2.frictionless);
				rulesScreen.MarkRulesDirty();
			}
			break;
		}
		Close();
	}

	public void OnCancel()
	{
		Close();
	}

	public static void BroadcastRuleChange(TabletRule rule, int value = 0, int value2 = 0, bool valueb = false)
	{
		MsgGameRuleSet msgGameRuleSet = new MsgGameRuleSet();
		msgGameRuleSet.NewRule = rule;
		msgGameRuleSet.Value = value;
		msgGameRuleSet.Value2 = value2;
		msgGameRuleSet.Valueb = valueb;
		LobbyManager.instance.client.Send(NetMsgTypes.GameRuleSet, msgGameRuleSet);
	}

	public void Close()
	{
		if (OnModalClosed != null)
		{
			OnModalClosed();
		}
		dataModel = null;
		OnModalClosed = null;
		currentOverlayType = TabletRule.None;
		isOpen = false;
		isOpening = false;
		TransitionDialog(transitioningIn: false);
	}

	public void OnLengthLimitTypeValueChange()
	{
		dataModel.Set("GameLimitType", roundsTimeToggleGroup.Value);
		ApplyDataModel(currentOverlayType, dataModel);
	}

	public void OnRespawnModeChange()
	{
		dataModel.Set("RespawnMode", respawnModeToggleGroup.Value);
		ApplyDataModel(currentOverlayType, dataModel);
	}

	private void ApplyDataModel(TabletRule overlayType, DataModel dataModel)
	{
		Modifiers instance = Modifiers.GetInstance();
		switch (overlayType)
		{
		case TabletRule.PointsToWin:
			plusMinusLabel.text = (dataModel.GetInt("MaxScore") / 50).ToString();
			break;
		case TabletRule.LengthLimit:
		{
			int num6 = dataModel.GetInt("GameLimitType");
			roundsTimeToggleGroup.SelectByValue(num6, fireEvent: false);
			switch ((GameLimitType)num6)
			{
			case GameLimitType.NONE:
				plusMinusContainer.gameObject.SetActive(value: false);
				break;
			case GameLimitType.ROUNDS:
			{
				plusMinusContainer.gameObject.SetActive(value: true);
				int num8 = dataModel.GetInt("MaxRounds");
				string translation2 = LocalizationManager.GetTranslation("RuleBook/Round" + ((num8 == 1) ? "Singular" : "Plural"));
				plusMinusLabel.text = string.Format(translation2, num8);
				break;
			}
			case GameLimitType.TIME:
			{
				plusMinusContainer.gameObject.SetActive(value: true);
				int num7 = dataModel.GetInt("MaxTime") / 60;
				string translation = LocalizationManager.GetTranslation("RuleBook/Minute" + ((num7 == 1) ? "Singular" : "Plural"));
				plusMinusLabel.text = string.Format(translation, num7);
				break;
			}
			}
			break;
		}
		case TabletRule.PlacementTimer:
		{
			int num10 = dataModel.GetInt("PlaceTime");
			if (num10 > 0)
			{
				plusMinusLabel.text = num10 + " " + ScriptLocalization.RuleBook.secondsAbbreviation;
			}
			else
			{
				plusMinusLabel.text = ScriptLocalization.RuleBook.NoLimit;
			}
			break;
		}
		case TabletRule.RunTimeLimit:
		{
			int num5 = dataModel.GetInt("RunTimerLimit");
			if (num5 > 0)
			{
				plusMinusLabel.text = num5 + " " + ScriptLocalization.RuleBook.secondsAbbreviation;
			}
			else
			{
				plusMinusLabel.text = ScriptLocalization.RuleBook.Off;
			}
			break;
		}
		case TabletRule.PiecesPerRound:
			plusMinusLabel.text = dataModel.GetInt("CreativePiecesPerRound").ToString();
			break;
		case TabletRule.OptionResolution:
		{
			int w = dataModel.GetInt("ResX");
			int h = dataModel.GetInt("ResY");
			int r = dataModel.GetInt("RefreshRate");
			plusMinusLabel.text = TabletOptionsScreen.GetFormattedResolutionString(w, h, r);
			break;
		}
		case TabletRule.OptionQuality:
			plusMinusLabel.text = TabletOptionsScreen.GetQualityString(dataModel.GetInt("Quality"));
			break;
		case TabletRule.OnlineSettingsAFKKickTime:
		{
			int num9 = afkKickerTimes[dataModel.GetInt("AFKKicker")];
			if (num9 > 0)
			{
				plusMinusLabel.text = num9 + " " + ScriptLocalization.RuleBook.secondsAbbreviation;
			}
			else
			{
				plusMinusLabel.text = ScriptLocalization.RuleBook.NoLimit;
			}
			break;
		}
		case TabletRule.ModifierDoomsdayMeteors:
		{
			int num4 = dataModel.GetInt("DoomsdayMeteorsMode");
			if (num4 == 0)
			{
				plusMinusLabel.text = ScriptLocalization.RuleBook.Off;
				break;
			}
			plusMinusLabel.text = instance.DoomsdayModifierTimes[num4] + " " + ScriptLocalization.RuleBook.secondsAbbreviation + " " + ScriptLocalization.RuleBook.Delay;
			break;
		}
		case TabletRule.ModifierDoomsdayLava:
		{
			int num3 = dataModel.GetInt("DoomsdayLavaMode");
			if (num3 == 0)
			{
				plusMinusLabel.text = ScriptLocalization.RuleBook.Off;
				break;
			}
			plusMinusLabel.text = instance.DoomsdayModifierTimes[num3] + " " + ScriptLocalization.RuleBook.secondsAbbreviation + " " + ScriptLocalization.RuleBook.Delay;
			break;
		}
		case TabletRule.RespawnMode:
		{
			int num = dataModel.GetInt("RespawnMode");
			respawnModeToggleGroup.SelectByValue(num, fireEvent: false);
			switch ((RespawnMode)num)
			{
			case RespawnMode.Off:
				plusMinusContainer.gameObject.SetActive(value: false);
				break;
			case RespawnMode.LivesPerRound:
			case RespawnMode.RespawnsPerRound:
			case RespawnMode.RespawnsPerMatch:
			{
				plusMinusContainer.gameObject.SetActive(value: true);
				int num2 = dataModel.GetInt("NumRespawns");
				string format = "{0} " + ((num2 == 1) ? ScriptLocalization.RuleBook_Presets.RespawnSingular : ScriptLocalization.RuleBook_Presets.RespawnPlural);
				plusMinusLabel.text = string.Format(format, num2);
				break;
			}
			}
			break;
		}
		}
		if (plusMinusLabel != null)
		{
			plusMinusLabel.UpdateDynamicText();
		}
	}

	private void Update()
	{
		if (backgroundAnim != null && !backgroundAnim.MoveNext())
		{
			backgroundAnim = null;
		}
	}
}
