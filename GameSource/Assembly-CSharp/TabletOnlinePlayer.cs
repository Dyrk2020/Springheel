using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class TabletOnlinePlayer : MonoBehaviour
{
	public TabletConnectionQualityBars connectionQualityBars;

	public Text playerNameText;

	public Image UCHNetIcon;

	public Image PSNVerifiedIcon;

	public LobbyPlayer lobbyPlayer;

	public int networkNumber;

	public TabletButton reportButton;

	public TabletButton muteButton;

	public TabletTextLabel muteButtonLabel;

	public TabletButton voteKickButton;

	public TabletTextLabel kickButtonLabel;

	public TabletTextLabel voteKickCountText;

	public TabletTextLabel hostIndicatorText;

	public RectTransform secondaryPlayerSpacer;

	public RectTransform mainContainer;

	public RectTransform connectingMessageContainer;

	public TabletButton playerNameButton;

	private string shownName = "";

	public int kickCount;

	public bool waitingForLocalNumber;

	public int sortingRank;

	public static bool PlatformCanShowProfiles => true;

	public void Initialize(LobbyPlayer lobbyPlayer)
	{
		this.lobbyPlayer = lobbyPlayer;
		networkNumber = lobbyPlayer.networkNumber;
		playerNameButton.SetInteractable(PlatformCanShowProfiles && lobbyPlayer.platform == LobbyPlayer.LocalMachinePlatform && !lobbyPlayer.platformUniqueID.NullOrEmpty());
		playerNameText.color = lobbyPlayer.PlayerColor;
		playerNameText.text = "";
		shownName = "";
		kickCount = 0;
		waitingForLocalNumber = false;
		UCHNetIcon.gameObject.SetActive(value: false);
		PSNVerifiedIcon.gameObject.SetActive(value: false);
		reportButton.SetDisabled(disabled: true);
		muteButton.SetDisabled(disabled: true);
		voteKickButton.SetDisabled(disabled: true);
		kickButtonLabel.Term = "Network/Kick";
		voteKickCountText.gameObject.SetActive(value: false);
		hostIndicatorText.gameObject.SetActive(value: false);
		secondaryPlayerSpacer.gameObject.SetActive(value: false);
		mainContainer.gameObject.SetActive(value: false);
		connectingMessageContainer.gameObject.SetActive(value: true);
		lobbyPlayer.RunAfterInitialized(delegate
		{
			playerNameText.text = lobbyPlayer.playerName;
			shownName = lobbyPlayer.playerName;
			NameTag.UpdateIcons(lobbyPlayer, UCHNetIcon, PSNVerifiedIcon, usePlayerColor: true);
			reportButton.SetDisabled(lobbyPlayer.IsLocalPlayer || UserReports.PlayerReportedThisSession(lobbyPlayer));
			muteButton.SetDisabled(lobbyPlayer.IsLocalPlayer);
			hostIndicatorText.gameObject.SetActive(lobbyPlayer.IsHost);
			waitingForLocalNumber = lobbyPlayer.localNumber == 0;
			secondaryPlayerSpacer.gameObject.SetActive(!waitingForLocalNumber && lobbyPlayer.localNumber > 1);
			RefreshKickButton();
			if (!waitingForLocalNumber)
			{
				mainContainer.gameObject.SetActive(value: true);
				connectingMessageContainer.gameObject.SetActive(value: false);
			}
		});
	}

	public void RefreshKickButton()
	{
		bool num = LobbyManager.instance.CurrentLevelSelectController != null;
		bool isHost = LobbyManager.instance.IsHost;
		bool flag = !num || isHost;
		int num2;
		if (!num)
		{
			num2 = ((LobbyManager.instance.PlayerTracker.NumPlayers > 2) ? 1 : 0);
			if (num2 != 0)
			{
				if (lobbyPlayer.VotedToKick)
				{
					kickButtonLabel.Term = "Network/CancelKick";
				}
				else
				{
					kickButtonLabel.Term = "Network/VoteToKick";
				}
				goto IL_007d;
			}
		}
		else
		{
			num2 = 0;
		}
		kickButtonLabel.Term = "Network/Kick";
		goto IL_007d;
		IL_007d:
		if ((num2 == 0 && !isHost) || waitingForLocalNumber || lobbyPlayer.localNumber > 1)
		{
			voteKickButton.gameObject.SetActive(value: false);
			return;
		}
		voteKickButton.gameObject.SetActive(value: true);
		bool flag2 = flag && !lobbyPlayer.EmoteSystem && !lobbyPlayer.IsHost;
		voteKickButton.SetDisabled(!flag2);
	}

	private void Update()
	{
		if (shownName.NullOrEmpty() && lobbyPlayer.playerName != shownName)
		{
			shownName = lobbyPlayer.playerName;
			playerNameText.text = lobbyPlayer.playerName;
		}
		if (!reportButton.Disabled && UserReports.PlayerReportedThisSession(lobbyPlayer))
		{
			reportButton.SetDisabled(disabled: true);
		}
		NameTag.UpdateIcons(lobbyPlayer, UCHNetIcon, PSNVerifiedIcon, usePlayerColor: true);
		if (waitingForLocalNumber && lobbyPlayer.localNumber != 0)
		{
			waitingForLocalNumber = false;
			mainContainer.gameObject.SetActive(value: true);
			connectingMessageContainer.gameObject.SetActive(value: false);
			secondaryPlayerSpacer.gameObject.SetActive(lobbyPlayer.localNumber > 1);
			RefreshKickButton();
			GetComponentInParent<TabletOnlinePlayersScreen>().playerSlotsDirty = true;
		}
	}

	public void SetVoteKickCount(int amount, int numVotesRequired)
	{
		kickCount = amount;
		if (amount == 0)
		{
			voteKickCountText.gameObject.SetActive(value: false);
			return;
		}
		voteKickCountText.gameObject.SetActive(value: true);
		voteKickCountText.text = "(" + amount + "/" + numVotesRequired + ")";
	}

	public void OnClickReport(PickCursor pickCursor)
	{
		TabletOnlinePlayersScreen componentInParent = GetComponentInParent<TabletOnlinePlayersScreen>();
		if (componentInParent == null)
		{
			return;
		}
		if (!UserReports.PlayerReportedThisSession(this.lobbyPlayer))
		{
			LobbyPlayer lobbyPlayer = LobbyManager.instance.GetLobbyPlayer(pickCursor.networkNumber);
			if (lobbyPlayer != null)
			{
				componentInParent.OpenReportDialog(lobbyPlayer, this.lobbyPlayer, playerNameText.text);
			}
		}
		else
		{
			reportButton.SetDisabled(disabled: true);
		}
	}

	public void OnClickMute(PickCursor pickCursor)
	{
		if (!lobbyPlayer.IsLocalPlayer)
		{
			lobbyPlayer.Muted = !lobbyPlayer.Muted;
			muteButtonLabel.Term = (lobbyPlayer.Muted ? "Network/Unmute" : "Network/Mute");
		}
	}

	public void OnClickKick(PickCursor pickCursor)
	{
		TabletOnlinePlayersScreen componentInParent = GetComponentInParent<TabletOnlinePlayersScreen>();
		if (!(componentInParent == null))
		{
			componentInParent.ShowKickPrompt(pickCursor.networkNumber, lobbyPlayer);
		}
	}

	public void OnPlayerReported()
	{
		reportButton.SetDisabled(disabled: true);
	}

	public int CompareTo(TabletOnlinePlayer other)
	{
		return sortingRank.CompareTo(other.sortingRank);
	}

	public void OnClickPlayerName(PickCursor pickCursor)
	{
		if (PlatformCanShowProfiles && lobbyPlayer.platform == LobbyPlayer.LocalMachinePlatform && SteamManager.Initialized && lobbyPlayer.platform == LobbyPlayer.SocialPlatform.Steam)
		{
			if (Application.isEditor)
			{
				Debug.LogWarning("Not opening player profile to avoid Editor glitch");
			}
			else
			{
				SteamFriends.ActivateGameOverlayToUser("steamid", new CSteamID(lobbyPlayer.SteamID));
			}
		}
	}
}
