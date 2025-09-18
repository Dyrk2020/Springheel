using GameEvent;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class TabletTwitchOptionsScreen : TabletScreen, IGameEventListener
{
	public enum CheckMarkState
	{
		Hidden,
		Success,
		Failure,
		Connecting
	}

	public RectTransform successMark;

	public RectTransform crossMark;

	public RectTransform ellipsisMark;

	public InputField channelNameField;

	public Image channelNameBackground;

	public TabletSimpleAnimator channelNameInputAnimator;

	public TabletTextLabel votingEnabledValue;

	public TabletTextLabel chatDisplayValue;

	public TabletTextLabel successText;

	public TabletDisableGroup channelNameDisableGroup;

	private bool currentChannelConnected;

	private CheckMarkState checkMarkState;

	private void Awake()
	{
		ChangeListener(adding: true);
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public override void OnTransitionInBegin()
	{
		base.OnTransitionInBegin();
		ellipsisMark.gameObject.SetActive(value: false);
		successMark.gameObject.SetActive(value: false);
		crossMark.gameObject.SetActive(value: false);
		if (TwitchChatController.instance != null)
		{
			currentChannelConnected = TwitchChatController.instance.currentChannelConnected;
			channelNameField.text = GameSettings.GetInstance().twitchChannelName;
			UpdateButtonValue(TabletRule.TwitchVotingEnabled);
			UpdateButtonValue(TabletRule.TwitchChatDisplay);
			channelNameDisableGroup.SetDisabled(!GameSettings.GetInstance().enableTwitchVoting);
			RefreshCheckMark();
			RefreshConnectedText();
		}
	}

	public virtual void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<LanguageChangeEvent>(this, adding);
	}

	public virtual void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e is LanguageChangeEvent)
		{
			UpdateAllSettingsButtons();
		}
	}

	private void UpdateAllSettingsButtons()
	{
		UpdateButtonValue(TabletRule.TwitchVotingEnabled);
		UpdateButtonValue(TabletRule.TwitchChatDisplay);
	}

	public void SetTwitchVotingEnabled(bool val)
	{
		GameSettings instance = GameSettings.GetInstance();
		instance.enableTwitchVoting = val;
		if (!val)
		{
			channelNameField.text = "";
			instance.twitchChannelName = "";
		}
		channelNameDisableGroup.SetDisabled(!val);
		UpdateButtonValue(TabletRule.TwitchVotingEnabled);
		RefreshCheckMark();
		RefreshConnectedText();
	}

	public void SetTwitchChatDisplay(bool val)
	{
		GameSettings.GetInstance().showTwitchChat = val;
		UpdateButtonValue(TabletRule.TwitchChatDisplay);
	}

	public void OnClickPartyBoxURL(PickCursor pickCursor)
	{
		OpenURLWrapper.Open("http://www.partybox.horse");
		AnalyticEvent.LinkClickedEvent(AnalyticEvent.SocialLink.Twitch, "http://www.partybox.horse");
	}

	public void OnClickChannelNameInput(PickCursor pickCursor)
	{
		string translation = LocalizationManager.GetTranslation("Network/Twitch Channel Name");
		channelNameField.onValueChanged.RemoveAllListeners();
		channelNameField.onValueChanged.AddListener(delegate
		{
			TwitchChatController.instance.invalidChannelFlag = false;
		});
		Color originalColor = channelNameBackground.color;
		Color buttonBgColor_TransparentHighlight = colorScheme.buttonBgColor_TransparentHighlight;
		channelNameInputAnimator.FadeColor(originalColor, buttonBgColor_TransparentHighlight, 0.25f, Easings.Functions.CubicEaseOut);
		Tablet.ActivateInputField(pickCursor, channelNameField, translation, delegate(string str)
		{
			GameSettings.GetInstance().twitchChannelName = str;
			channelNameField.text = str;
			channelNameInputAnimator.FadeColor(channelNameBackground.color, originalColor, 0.25f, Easings.Functions.CubicEaseOut);
		});
		SteamDeck.OpenVirtualKeyboard(pickCursor);
	}

	private void RefreshCheckMark()
	{
		CheckMarkState checkMarkState = CheckMarkState.Hidden;
		if (TwitchChatController.instance != null)
		{
			checkMarkState = ((!GameSettings.GetInstance().twitchChannelName.NullOrEmpty() || TwitchChatController.instance.invalidChannelFlag) ? (TwitchChatController.instance.currentChannelConnected ? CheckMarkState.Success : ((!TwitchChatController.instance.tryingToConnect) ? CheckMarkState.Failure : CheckMarkState.Connecting)) : CheckMarkState.Hidden);
		}
		if (this.checkMarkState != checkMarkState)
		{
			this.checkMarkState = checkMarkState;
			ellipsisMark.gameObject.SetActive(value: false);
			successMark.gameObject.SetActive(value: false);
			crossMark.gameObject.SetActive(value: false);
		}
	}

	private void RefreshConnectedText()
	{
		if (TwitchChatController.instance != null && !GameSettings.GetInstance().enableTwitchVoting)
		{
			successText.gameObject.SetActive(value: false);
		}
	}

	public override void Update()
	{
		base.Update();
		if (TwitchChatController.instance != null)
		{
			RefreshCheckMark();
			RefreshConnectedText();
		}
	}

	public override void OnModalOverlayClosed()
	{
		base.OnModalOverlayClosed();
		UpdateButtonValue(tablet.modalOverlay.currentOverlayType);
	}

	private void UpdateButtonValue(TabletRule overlayType)
	{
		GameSettings instance = GameSettings.GetInstance();
		switch (overlayType)
		{
		case TabletRule.TwitchVotingEnabled:
			votingEnabledValue.Term = (instance.enableTwitchVoting ? "RuleBook/On" : "RuleBook/Off");
			break;
		case TabletRule.TwitchChatDisplay:
			chatDisplayValue.Term = (instance.showTwitchChat ? "RuleBook/On" : "RuleBook/Off");
			break;
		}
	}

	public override bool OnPressBack(PickCursor pickCursor)
	{
		if (tablet.modalOverlay.IsOpen || tablet.modalOverlay.IsOpening)
		{
			tablet.modalOverlay.OnCancel();
			return true;
		}
		return base.OnPressBack(pickCursor);
	}
}
