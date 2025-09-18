using GameEvent;
using I2.Loc;
using Steamworks;
using UnityEngine;

public class TabletLobbyOptionsScreen : TabletScreen, IGameEventListener
{
	public TabletTextLabel lobbyTagValueText;

	public TabletTextLabel lobbyPrivacyValueText;

	public TabletTextLabel lobbyCodeText;

	public TabletTextLabel hostDisabledCrossplayText;

	public TabletButton inviteFriendsButton;

	public TabletDisableGroup lobbyOptionsGroup;

	public TabletFreezeLobbyGroup freezeLobbyGroup;

	private bool lobbyCodeShown;

	private MatchmakingLobby.Visibility shownVisibility;

	private LobbyTags shownTag;

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
		freezeLobbyGroup.UpdateCheckboxState();
	}

	public virtual void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding);
		GameEventManager.ChangeListener<LanguageChangeEvent>(this, adding);
	}

	public virtual void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e is NetworkMessageReceivedEvent networkMessageReceivedEvent && networkMessageReceivedEvent.Message.msgType == NetMsgTypes.GameRuleSet && LobbyManager.instance != null && !LobbyManager.instance.IsHost)
		{
			MsgGameRuleSet msgGameRuleSet = (MsgGameRuleSet)networkMessageReceivedEvent.ReadMessage;
			switch (msgGameRuleSet.NewRule)
			{
			case TabletRule.LobbyOptionsTag:
				shownTag = (LobbyTags)msgGameRuleSet.Value;
				UpdateButtonValue(TabletRule.LobbyOptionsTag);
				break;
			case TabletRule.LobbyOptionsPrivacy:
				shownVisibility = (MatchmakingLobby.Visibility)msgGameRuleSet.Value;
				UpdateButtonValue(TabletRule.LobbyOptionsPrivacy);
				break;
			}
		}
		if (e is LanguageChangeEvent)
		{
			UpdateAllSettingsButtons();
		}
	}

	private void UpdateAllSettingsButtons()
	{
		UpdateButtonValue(TabletRule.LobbyOptionsPrivacy);
		UpdateButtonValue(TabletRule.LobbyOptionsTag);
		UpdateButtonValue(TabletRule.CrossPlatformToggle);
	}

	public void Start()
	{
		if (LobbyManager.instance != null && !LobbyManager.instance.IsHost)
		{
			lobbyOptionsGroup.SetDisabled(disabled: true);
		}
		UpdateAllSettingsButtons();
	}

	public async void OnClickInviteFriends(PickCursor pickCursor)
	{
		if (LobbyManager.instance.IsInOnlineGame)
		{
			SteamMatchmaker steamMatchmaker = (SteamMatchmaker)Matchmaker.Instance;
			if (steamMatchmaker.CurrentLobby != null && steamMatchmaker.SteamLobby != null)
			{
				Debug.LogError("[Net] Opening Friend Invite Overlay");
				SteamFriends.ActivateGameOverlayInviteDialog(steamMatchmaker.SteamLobby.LobbyID);
			}
		}
	}

	public void OnClickShowToggle(PickCursor pickCursor)
	{
		lobbyCodeShown = !lobbyCodeShown;
		if (lobbyCodeShown)
		{
			AkSoundEngine.PostEvent("UI_UPad_Online_Lobby_Code_Show", base.gameObject);
			lobbyCodeText.text = Matchmaker.CurrentMatchmakingLobby.GetLobbyCode();
		}
		else
		{
			AkSoundEngine.PostEvent("UI_UPad_Online_Lobby_Code_Hide", base.gameObject);
			lobbyCodeText.text = "****";
		}
	}

	public void OnClickCopyLobbyCode(PickCursor pickCursor)
	{
		QuickSaver.CopyStringToClipboard(Matchmaker.CurrentMatchmakingLobby.GetLobbyCode());
		UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ShareableCodeClipboard, 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
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
		case TabletRule.LobbyOptionsTag:
			if (LobbyManager.instance != null)
			{
				switch (LobbyManager.instance.IsHost ? instance.lobbyTag : shownTag)
				{
				case LobbyTags.Fun:
					lobbyTagValueText.Term = "Network/Tag/Fun";
					break;
				case LobbyTags.Beginner:
					lobbyTagValueText.Term = "Network/Tag/Beginner";
					break;
				case LobbyTags.Competitive:
					lobbyTagValueText.Term = "Network/Tag/Competitive";
					break;
				case LobbyTags.CustomLevels:
					lobbyTagValueText.Term = "Network/Tag/CustomLevels";
					break;
				}
			}
			break;
		case TabletRule.LobbyOptionsPrivacy:
			if (LobbyManager.instance != null)
			{
				switch (LobbyManager.instance.IsHost ? instance.lobbyPrivacy : shownVisibility)
				{
				case MatchmakingLobby.Visibility.PUBLIC:
					lobbyPrivacyValueText.Term = "Network/Public";
					break;
				case MatchmakingLobby.Visibility.FRIENDS:
					lobbyPrivacyValueText.Term = "Network/FriendsOnly";
					break;
				case MatchmakingLobby.Visibility.PRIVATE:
					lobbyPrivacyValueText.Term = "Network/InviteOnly";
					break;
				case MatchmakingLobby.Visibility.INVISIBLE:
					lobbyPrivacyValueText.Term = "RuleBook/Off";
					break;
				}
			}
			break;
		}
	}

	public void SetLobbyPrivacy(MatchmakingLobby.Visibility lobbyPrivacy)
	{
		GameSettings.GetInstance().lobbyPrivacy = lobbyPrivacy;
		if (Matchmaker.CurrentMatchmakingLobby != null && Matchmaker.Instance.IsLobbyOwner())
		{
			Matchmaker.CurrentMatchmakingLobby.LobbyVisibility = lobbyPrivacy;
		}
		TabletModalOverlay.BroadcastRuleChange(TabletRule.LobbyOptionsPrivacy, (int)lobbyPrivacy);
		UpdateButtonValue(TabletRule.LobbyOptionsPrivacy);
		shownVisibility = lobbyPrivacy;
	}

	public void SetLobbyTag(LobbyTags lobbyTag)
	{
		GameSettings.GetInstance().lobbyTag = lobbyTag;
		if (Matchmaker.CurrentMatchmakingLobby != null && Matchmaker.Instance.IsLobbyOwner())
		{
			Matchmaker.CurrentMatchmakingLobby.SetLobbyTag(lobbyTag);
		}
		TabletModalOverlay.BroadcastRuleChange(TabletRule.LobbyOptionsTag, (int)lobbyTag);
		UpdateButtonValue(TabletRule.LobbyOptionsTag);
		shownTag = lobbyTag;
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

	public override void Update()
	{
		base.Update();
		if (Matchmaker.CurrentMatchmakingLobby != null)
		{
			hostDisabledCrossplayText.gameObject.SetActive(Matchmaker.CurrentMatchmakingLobby.GetLobbyDisallowCrossplay());
		}
	}
}
