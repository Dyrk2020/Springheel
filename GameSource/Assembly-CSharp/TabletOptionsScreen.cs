using System.Collections;
using System.Collections.Generic;
using GameEvent;
using I2.Loc;
using UnityEngine;
using UnityEngine.Networking;

public class TabletOptionsScreen : TabletScreen, IGameEventListener
{
	public TabletTextLabel resolutionText;

	public TabletTextLabel qualityText;

	public TabletTextLabel windowedText;

	public TabletTextLabel vsyncText;

	public TabletTextLabel showVersionText;

	public TabletTextLabel backgroundAudioText;

	public TabletSliderButton musicSlider;

	public TabletSliderButton soundSlider;

	public TabletSubdialogController subdialogController;

	public RectTransform optionsDialog;

	public RectTransform keyboardConfigDialog;

	public RectTransform onlineSettingsDialog;

	public RectTransform graphicsSettingsDialog;

	public RectTransform languageSettingsDialog;

	private RectTransform lastEnteredSubdialog;

	public TabletTextLabel onlineSettingsCameraFollowsValue;

	public TabletTextLabel onlineSettingsAFKKickerValue;

	public TabletTextLabel onlineSettingsNameVisibilityValue;

	public TabletTextLabel onlineSettingsChatAndEmotesValue;

	public TabletTextLabel onlineSettingsCrossPlatformToggleValue;

	public TabletDisableGroup afkKickerDisableGroup;

	public TabletDisableGroup crossPlatformToggleDisableGroup;

	public TabletButton[] languageButtons;

	public TabletButton languageMenuButton;

	private static List<Resolution> validResolutions;

	private bool changingLanguage;

	private int lastScreenWidth;

	private int lastScreenHeight;

	public static List<Resolution> ValidResolutions
	{
		get
		{
			if (validResolutions == null)
			{
				int refreshRate = Screen.currentResolution.refreshRate;
				validResolutions = new List<Resolution>();
				for (int i = 0; i < Screen.resolutions.Length; i++)
				{
					if (Screen.resolutions[i].refreshRate == refreshRate)
					{
						validResolutions.Add(Screen.resolutions[i]);
					}
				}
			}
			return validResolutions;
		}
	}

	private void Awake()
	{
		ChangeListener(adding: true);
		lastScreenWidth = Screen.width;
		lastScreenHeight = Screen.height;
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public virtual void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding);
		GameEventManager.ChangeListener<LanguageChangeEvent>(this, adding);
	}

	public virtual void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e is NetworkMessageReceivedEvent networkMessageReceivedEvent && networkMessageReceivedEvent.Message.msgType == NetMsgTypes.AFKTimerChanged)
		{
			MsgAFKTimerChanged msgAFKTimerChanged = (MsgAFKTimerChanged)networkMessageReceivedEvent.ReadMessage;
			SetAFKKickTime(msgAFKTimerChanged.Time, asHost: false);
		}
		if (e is LanguageChangeEvent)
		{
			UpdateAllSettingsButtons();
		}
	}

	public void OnApplyResolution(int width, int height)
	{
		Screen.SetResolution(width, height, Screen.fullScreen);
		UnityEngine.Cursor.lockState = CursorLockMode.None;
		StartCoroutine(resetCursorLock());
	}

	private IEnumerator resetCursorLock()
	{
		yield return null;
		yield return new WaitForEndOfFrame();
		UnityEngine.Cursor.lockState = CursorLockMode.Confined;
	}

	public static int GetResIndex(int currentW, int currentH, int currentR)
	{
		for (int i = 0; i < ValidResolutions.Count; i++)
		{
			Resolution resolution = ValidResolutions[i];
			if (resolution.width == currentW && resolution.height == currentH && resolution.refreshRate == currentR)
			{
				return i;
			}
		}
		return -1;
	}

	private void Start()
	{
		UpdateAllSettingsButtons();
		StartCoroutine(waitTwoFramesForLanguage());
	}

	private IEnumerator waitTwoFramesForLanguage()
	{
		yield return null;
		yield return null;
		RefreshLanguageButtons();
	}

	public override void OnModalOverlayClosed()
	{
		base.OnModalOverlayClosed();
		UpdateButtonValue(tablet.modalOverlay.currentOverlayType);
		if (ControllerMonitor.Instance.IsMainControllerSet)
		{
			StatTracker.Instance.SaveGameForAllUsers();
		}
	}

	private void UpdateAllSettingsButtons()
	{
		UpdateButtonValue(TabletRule.OptionResolution);
		UpdateButtonValue(TabletRule.OptionQuality);
		UpdateButtonValue(TabletRule.OptionVsync);
		UpdateButtonValue(TabletRule.OptionWindowed);
		UpdateButtonValue(TabletRule.OptionShowVersion);
		UpdateButtonValue(TabletRule.OptionBackgroundAudio);
		UpdateButtonValue(TabletRule.OptionMusicVolume);
		UpdateButtonValue(TabletRule.OptionSoundVolume);
		UpdateButtonValue(TabletRule.OnlineSettingsCameraFollows);
		UpdateButtonValue(TabletRule.OnlineSettingsAFKKickTime);
		UpdateButtonValue(TabletRule.OnlineSettingsEmotes);
		UpdateButtonValue(TabletRule.OnlineSettingsNameDisplay);
		UpdateButtonValue(TabletRule.CrossPlatformToggle);
	}

	private void UpdateButtonValue(TabletRule overlayType)
	{
		switch (overlayType)
		{
		case TabletRule.OptionResolution:
			resolutionText.text = GetFormattedResolutionString(Screen.width, Screen.height, Screen.currentResolution.refreshRate);
			break;
		case TabletRule.OptionQuality:
			qualityText.text = GetQualityString(QualitySettings.masterTextureLimit);
			break;
		case TabletRule.OptionWindowed:
			StartCoroutine(RefreshWindowedLabel());
			break;
		case TabletRule.OptionVsync:
			vsyncText.text = ((QualitySettings.vSyncCount > 0) ? ScriptLocalization.RuleBook.On : ScriptLocalization.RuleBook.Off);
			break;
		case TabletRule.OptionShowVersion:
			if (ControllerMonitor.Instance.IsMainControllerSet)
			{
				SaveFileData saveFileDataForMainUser4 = StatTracker.Instance.GetSaveFileDataForMainUser();
				showVersionText.text = ((!saveFileDataForMainUser4.HideVersion) ? ScriptLocalization.RuleBook.On : ScriptLocalization.RuleBook.Off);
			}
			else
			{
				showVersionText.text = "???";
			}
			break;
		case TabletRule.OptionBackgroundAudio:
			if (ControllerMonitor.Instance.IsMainControllerSet)
			{
				SaveFileData saveFileDataForMainUser3 = StatTracker.Instance.GetSaveFileDataForMainUser();
				backgroundAudioText.text = (saveFileDataForMainUser3.BackgroundAudio ? ScriptLocalization.RuleBook.On : ScriptLocalization.RuleBook.Off);
			}
			else
			{
				backgroundAudioText.text = "???";
			}
			break;
		case TabletRule.OptionMusicVolume:
			if (ControllerMonitor.Instance.IsMainControllerSet)
			{
				SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
				musicSlider.SetValue(saveFileDataForMainUser.MusicVolume, sendEvent: false);
			}
			break;
		case TabletRule.OptionSoundVolume:
			if (ControllerMonitor.Instance.IsMainControllerSet)
			{
				SaveFileData saveFileDataForMainUser2 = StatTracker.Instance.GetSaveFileDataForMainUser();
				soundSlider.SetValue(saveFileDataForMainUser2.SoundVolume, sendEvent: false);
			}
			break;
		case TabletRule.OnlineSettingsCameraFollows:
			if (StatTracker.Instance.GetSaveFileDataForMainUser().CameraLocalOnly)
			{
				onlineSettingsCameraFollowsValue.Term = "Network/LocalPlayers";
			}
			else
			{
				onlineSettingsCameraFollowsValue.Term = "Network/AllPlayers";
			}
			break;
		case TabletRule.OnlineSettingsAFKKickTime:
		{
			int currentLobbyAFKAutoKickTime = GameSettings.GetInstance().CurrentLobbyAFKAutoKickTime;
			if (currentLobbyAFKAutoKickTime > 0)
			{
				onlineSettingsAFKKickerValue.text = currentLobbyAFKAutoKickTime + " " + ScriptLocalization.RuleBook.secondsAbbreviation;
			}
			else
			{
				onlineSettingsAFKKickerValue.text = ScriptLocalization.RuleBook.NoLimit;
			}
			bool flag = LobbyManager.instance == null || LobbyManager.instance.IsHost;
			afkKickerDisableGroup.SetDisabled(!flag);
			break;
		}
		case TabletRule.OnlineSettingsNameDisplay:
			switch (GameSettings.GetInstance().OnlinePlayerNames)
			{
			case OnlinePlayerNames.AlwaysOn:
				onlineSettingsNameVisibilityValue.Term = "Network/Always On";
				break;
			case OnlinePlayerNames.Auto:
				onlineSettingsNameVisibilityValue.Term = "Network/NameAuto";
				break;
			case OnlinePlayerNames.AlwaysOff:
				onlineSettingsNameVisibilityValue.Term = "Network/Always Off";
				break;
			}
			break;
		case TabletRule.OnlineSettingsEmotes:
			switch (GameSettings.GetInstance().OnlineChatEmotes)
			{
			case OnlineChatEmotes.ChatAndEmotesOn:
				onlineSettingsChatAndEmotesValue.Term = "Network/ChatOn";
				break;
			case OnlineChatEmotes.EmotesOnly:
				onlineSettingsChatAndEmotesValue.Term = "Network/ChatEmotesOnly";
				break;
			case OnlineChatEmotes.ChatAndEmotesOff:
				onlineSettingsChatAndEmotesValue.Term = "Network/ChatOff";
				break;
			}
			break;
		case TabletRule.CrossPlatformToggle:
		{
			GameSettings instance = GameSettings.GetInstance();
			onlineSettingsCrossPlatformToggleValue.text = (instance.CrossPlatformToggle ? ScriptLocalization.RuleBook.On : ScriptLocalization.RuleBook.Off);
			crossPlatformToggleDisableGroup.SetDisabled(!SceneManagerWrapper.IsInMainMenu);
			break;
		}
		case TabletRule.PlayerReportReason:
		case TabletRule.LobbyOptionsTag:
		case TabletRule.LobbyOptionsPrivacy:
		case TabletRule.TwitchVotingEnabled:
		case TabletRule.TwitchChatDisplay:
		case TabletRule.GameMode:
		case TabletRule.RespawnMode:
		case TabletRule.PartyBoxMode:
			break;
		}
	}

	public static string GetFormattedResolutionString(int w, int h, int r)
	{
		return string.Format("{0} x {1} @{2}" + ScriptLocalization.Options.hertz, w, h, r);
	}

	public static string GetQualityString(int val)
	{
		return val switch
		{
			0 => ScriptLocalization.Options_Options.QualityHi, 
			1 => ScriptLocalization.Options_Options.QualityMed, 
			2 => ScriptLocalization.Options_Options.QualityLo, 
			_ => null, 
		};
	}

	public void OnMusicVolumeValueChange()
	{
		float value = musicSlider.Value;
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		GameSettings.Music = value;
		if (!WwiseSuspender.Muted)
		{
			AkSoundEngine.SetRTPCValue("MUS_volume", value * 100f);
		}
		saveFileDataForMainUser.MusicVolume = value;
	}

	public void OnSoundVolumeValueChange()
	{
		float value = soundSlider.Value;
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		GameSettings.Sound = value;
		if (!WwiseSuspender.Muted)
		{
			AkSoundEngine.SetRTPCValue("SFX_volume", value * 100f);
		}
		saveFileDataForMainUser.SoundVolume = value;
	}

	public void OnClickKeyboardSettings(PickCursor pickCursor)
	{
		if (!subdialogController.IsAnimating)
		{
			lastEnteredSubdialog = keyboardConfigDialog;
			subdialogController.TransitionLeftTo(keyboardConfigDialog);
		}
	}

	public void OnClickGraphicsSettings(PickCursor pickCursor)
	{
		if (!subdialogController.IsAnimating)
		{
			lastEnteredSubdialog = graphicsSettingsDialog;
			subdialogController.TransitionLeftTo(graphicsSettingsDialog);
		}
	}

	public void OnClickOnlineSettings(PickCursor pickCursor)
	{
		if (!subdialogController.IsAnimating)
		{
			lastEnteredSubdialog = onlineSettingsDialog;
			subdialogController.TransitionLeftTo(onlineSettingsDialog);
		}
	}

	public void OnClickLanguageSettings(PickCursor pickCursor)
	{
		if (!subdialogController.IsAnimating)
		{
			lastEnteredSubdialog = languageSettingsDialog;
			subdialogController.TransitionLeftTo(languageSettingsDialog);
		}
	}

	public void OnClickScoreBalancer(PickCursor pickCursor)
	{
		if (!subdialogController.IsAnimating)
		{
			tablet.GotoHelpPage(8);
		}
	}

	public override bool OnPressBack(PickCursor pickCursor)
	{
		if (tablet.modalOverlay.IsOpen || tablet.modalOverlay.IsOpening)
		{
			tablet.modalOverlay.OnCancel();
			return true;
		}
		if (!subdialogController.IsOnMainSubdialog)
		{
			subdialogController.PopSubdialog();
			return true;
		}
		return base.OnPressBack(pickCursor);
	}

	public void OnClickSwitchSetupControllers(PickCursor pickCursor)
	{
	}

	public void RefreshLanguageButtons()
	{
		TabletButton[] array = languageButtons;
		foreach (TabletButton tabletButton in array)
		{
			if (changingLanguage)
			{
				tabletButton.SetDisabled(disabled: true);
				continue;
			}
			tabletButton.SetDisabled(disabled: false);
			if (tabletButton.name == LocalizationManager.CurrentLanguage)
			{
				if (tabletButton.buttonType != TabletButton.ButtonType.Simple)
				{
					tabletButton.buttonType = TabletButton.ButtonType.Simple;
					tabletButton.SetDisabled(disabled: false);
				}
			}
			else if (tabletButton.buttonType != TabletButton.ButtonType.Transparent)
			{
				tabletButton.buttonType = TabletButton.ButtonType.Transparent;
				tabletButton.SetDisabled(disabled: false);
			}
		}
	}

	public void OnClickLanguage(string languageName)
	{
		if (languageName != LocalizationManager.CurrentLanguage && !changingLanguage)
		{
			LocalizationManager.CurrentLanguage = languageName;
			GameEventManager.SendEvent(new LanguageChangeEvent(LocalizationManager.CurrentLanguage));
			changingLanguage = true;
			StartCoroutine(waitForLanguageChange());
			RefreshLanguageButtons();
		}
	}

	private IEnumerator waitForLanguageChange()
	{
		int i = 0;
		while (i != 5)
		{
			yield return null;
			int num = i + 1;
			i = num;
		}
		float timeout = Time.unscaledTime;
		while (Time.unscaledDeltaTime > 0.05f && Time.unscaledTime - timeout > 3f)
		{
			yield return null;
		}
		changingLanguage = false;
		RefreshLanguageButtons();
	}

	private IEnumerator RefreshWindowedLabel()
	{
		yield return null;
		bool flag = !Screen.fullScreen;
		windowedText.text = (flag ? ScriptLocalization.RuleBook.On : ScriptLocalization.RuleBook.Off);
	}

	public void SetCameraLocalOnly(bool val)
	{
		ZoomCamera.LocalOnly = val;
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		if (saveFileDataForMainUser != null)
		{
			saveFileDataForMainUser.CameraLocalOnly = val;
		}
		UpdateButtonValue(TabletRule.OnlineSettingsCameraFollows);
	}

	public void SetAFKKickTime(int timeInSeconds, bool asHost)
	{
		GameSettings instance = GameSettings.GetInstance();
		instance.CurrentLobbyAFKAutoKickTime = timeInSeconds;
		if (asHost)
		{
			instance.AFKAutoKickTime = timeInSeconds;
			SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
			if (saveFileDataForMainUser != null)
			{
				saveFileDataForMainUser.AFKAutoKickTime = timeInSeconds;
			}
			MsgAFKTimerChanged msgAFKTimerChanged = new MsgAFKTimerChanged();
			msgAFKTimerChanged.Time = timeInSeconds;
			NetworkServer.SendToAll(NetMsgTypes.AFKTimerChanged, msgAFKTimerChanged);
		}
		UpdateButtonValue(TabletRule.OnlineSettingsAFKKickTime);
	}

	public void SetChatAndEmotes(int v)
	{
		GameSettings.GetInstance().OnlineChatEmotes = (OnlineChatEmotes)v;
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		if (saveFileDataForMainUser != null)
		{
			saveFileDataForMainUser.OnlineChatEmotes = (OnlineChatEmotes)v;
		}
		UpdateButtonValue(TabletRule.OnlineSettingsEmotes);
	}

	public void SetNameVisibility(int v)
	{
		GameSettings.GetInstance().OnlinePlayerNames = (OnlinePlayerNames)v;
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		if (saveFileDataForMainUser != null)
		{
			saveFileDataForMainUser.OnlinePlayerNames = (OnlinePlayerNames)v;
		}
		UpdateButtonValue(TabletRule.OnlineSettingsNameDisplay);
	}

	public void SetCrossPlatformToggle(bool v)
	{
		GameSettings instance = GameSettings.GetInstance();
		if (GameSettings.PlatformCanDisableCrossPlay)
		{
			instance.CrossPlatformToggle = v;
		}
		else
		{
			instance.CrossPlatformToggle = true;
		}
		StatTracker.Instance.GetSaveFileDataForMainUser().CrossPlatformToggle = instance.CrossPlatformToggle;
		UpdateButtonValue(TabletRule.CrossPlatformToggle);
	}

	public override bool OnRotateRight(PickCursor pickCursor)
	{
		if (!pickCursor.lastRotateWasMouseWheel && subdialogController.currentSubdialog == optionsDialog && lastEnteredSubdialog != null)
		{
			subdialogController.TransitionLeftTo(lastEnteredSubdialog);
			return true;
		}
		return false;
	}

	public override void Update()
	{
		base.Update();
		if (lastScreenHeight != Screen.height || lastScreenWidth != Screen.width)
		{
			lastScreenHeight = Screen.height;
			lastScreenWidth = Screen.width;
			UpdateButtonValue(TabletRule.OptionResolution);
		}
	}
}
