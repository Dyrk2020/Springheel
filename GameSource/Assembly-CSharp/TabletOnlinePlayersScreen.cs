using System;
using System.Collections.Generic;
using GameEvent;
using I2.Loc;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TabletOnlinePlayersScreen : TabletScreen, IGameEventListener
{
	public TabletOnlinePlayer[] onlinePlayerSlots;

	private List<TabletOnlinePlayer> activePlayerSlots = new List<TabletOnlinePlayer>(8);

	public RectTransform playerList;

	public TabletSubdialogController subdialogController;

	public RectTransform mainDialog;

	public RectTransform reportDialog;

	public RectTransform kickConfirmDialog;

	public TabletTextLabel promptTextLabel;

	public TabletTextLabel promptConfirmButtonLabel;

	public InputField reportDescriptionInputField;

	public Image reportDescriptionBackground;

	public TabletSimpleAnimator reportDescInputAnimator;

	public TabletTextLabel reportReasonValueText;

	public Text reportedPlayerName;

	public Image UCHNetIcon;

	public Image PSNVerifiedIcon;

	private UserReports.ReportInformation reportInformation;

	private int reportedNetworkNumber = -1;

	private int kickingPlayerNetworkNumber = -1;

	private LobbyPlayer kickedLobbyPlayer;

	public bool playerSlotsDirty;

	private void Awake()
	{
		TabletOnlinePlayer[] array = onlinePlayerSlots;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: false);
		}
		ChangeListener(addRemove: true);
	}

	private void Start()
	{
		if (!(LobbyManager.instance != null))
		{
			return;
		}
		NetworkLobbyPlayer[] lobbySlots = LobbyManager.instance.lobbySlots;
		for (int i = 0; i < lobbySlots.Length; i++)
		{
			LobbyPlayer lobbyPlayer = (LobbyPlayer)lobbySlots[i];
			if (lobbyPlayer != null)
			{
				AddNewPlayer(lobbyPlayer);
			}
		}
	}

	private void OnDestroy()
	{
		ChangeListener(addRemove: false);
	}

	public void ChangeListener(bool addRemove)
	{
		GameEventManager.ChangeListener<LobbyPlayerCreatedEvent>(this, addRemove);
		GameEventManager.ChangeListener<LobbyPlayerRemovedEvent>(this, addRemove);
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, addRemove);
	}

	private bool LobbyPlayerInList(LobbyPlayer lobbyPlayer)
	{
		foreach (TabletOnlinePlayer activePlayerSlot in activePlayerSlots)
		{
			if (activePlayerSlot.lobbyPlayer == lobbyPlayer)
			{
				return true;
			}
		}
		return false;
	}

	private void AddNewPlayer(LobbyPlayer lobbyPlayer)
	{
		if (LobbyPlayerInList(lobbyPlayer))
		{
			return;
		}
		for (int i = 0; i < onlinePlayerSlots.Length; i++)
		{
			if (!activePlayerSlots.Contains(onlinePlayerSlots[i]))
			{
				TabletOnlinePlayer tabletOnlinePlayer = onlinePlayerSlots[i];
				tabletOnlinePlayer.transform.SetParent(null);
				tabletOnlinePlayer.transform.SetParent(playerList);
				tabletOnlinePlayer.gameObject.SetActive(value: true);
				tabletOnlinePlayer.Initialize(lobbyPlayer);
				activePlayerSlots.Add(tabletOnlinePlayer);
				playerSlotsDirty = true;
				return;
			}
		}
		Debug.LogError("Not enough online player slots...");
	}

	private void RemovePlayer(int networkNumber)
	{
		for (int i = 0; i < activePlayerSlots.Count; i++)
		{
			TabletOnlinePlayer tabletOnlinePlayer = activePlayerSlots[i];
			if (tabletOnlinePlayer.networkNumber == networkNumber)
			{
				activePlayerSlots.RemoveAt(i);
				tabletOnlinePlayer.gameObject.SetActive(value: false);
				playerSlotsDirty = true;
				return;
			}
		}
		Debug.LogError("Could not find player slot with network number " + networkNumber);
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(LobbyPlayerCreatedEvent))
		{
			LobbyPlayerCreatedEvent lobbyPlayerCreatedEvent = e as LobbyPlayerCreatedEvent;
			AddNewPlayer(lobbyPlayerCreatedEvent.LobbyPlayerObj.GetComponent<LobbyPlayer>());
		}
		if (type == typeof(LobbyPlayerRemovedEvent))
		{
			LobbyPlayerRemovedEvent lobbyPlayerRemovedEvent = e as LobbyPlayerRemovedEvent;
			RemovePlayer(lobbyPlayerRemovedEvent.PlayerNumber);
		}
		if (!(type == typeof(NetworkMessageReceivedEvent)))
		{
			return;
		}
		NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
		if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.UpdateVoteKickCounts)
		{
			MsgUpdateVoteKickCounts msgUpdateVoteKickCounts = (MsgUpdateVoteKickCounts)networkMessageReceivedEvent.ReadMessage;
			foreach (TabletOnlinePlayer activePlayerSlot in activePlayerSlots)
			{
				if (activePlayerSlot.networkNumber == msgUpdateVoteKickCounts.networkNumber)
				{
					activePlayerSlot.SetVoteKickCount(msgUpdateVoteKickCounts.votes, activePlayerSlots.Count - 1);
					break;
				}
			}
		}
		if (networkMessageReceivedEvent.Message.msgType != NetMsgTypes.ConnectionQuality)
		{
			return;
		}
		MsgConnectionQuality msgConnectionQuality = networkMessageReceivedEvent.ReadMessage as MsgConnectionQuality;
		foreach (TabletOnlinePlayer activePlayerSlot2 in activePlayerSlots)
		{
			if (activePlayerSlot2.networkNumber == msgConnectionQuality.NetworkPlayerNumber)
			{
				activePlayerSlot2.connectionQualityBars.Quality = msgConnectionQuality.Quality;
				break;
			}
		}
	}

	public void OpenReportDialog(LobbyPlayer reporter, LobbyPlayer reportedUser, string reportedUserNameFiltered)
	{
		if (!UserReports.PlayerReportedThisSession(reportedUser))
		{
			subdialogController.TransitionLeftTo(reportDialog);
			NameTag.UpdateIcons(reportedUser, UCHNetIcon, PSNVerifiedIcon, usePlayerColor: false);
			reportedPlayerName.text = reportedUserNameFiltered;
			reportDescriptionInputField.text = "";
			reportedNetworkNumber = reportedUser.networkNumber;
			reportInformation = new UserReports.ReportInformation
			{
				reporterUsername = reporter.playerName,
				reporterGSID = reporter.GSID,
				reporterPlatform = reporter.platform,
				reporterPlatformID = reporter.platformUniqueID,
				reportedUsername = reportedUser.playerName,
				reportedGSID = reportedUser.GSID,
				reportedPlatform = reportedUser.platform,
				reportedPlatformID = reportedUser.platformUniqueID,
				reportChatlog = ChatDisplay.GetChatLogAsString(),
				reportLevelCode = GameState.GetInstance().currentSnapshotInfo.snapshotCode
			};
			UpdateReportReasonValueText();
		}
	}

	public void OnClickReportReason(PickCursor pickCursor)
	{
		Color originalColor = reportDescriptionBackground.color;
		Color buttonBgColor_TransparentHighlight = colorScheme.buttonBgColor_TransparentHighlight;
		reportDescInputAnimator.FadeColor(reportDescriptionBackground.color, buttonBgColor_TransparentHighlight, 0.25f, Easings.Functions.CubicEaseOut);
		Tablet.ActivateInputField(pickCursor, reportDescriptionInputField, LocalizationManager.GetTranslation("Network/ReportPlayerCommentsTitle"), delegate(string str)
		{
			reportDescriptionInputField.text = str;
			reportDescInputAnimator.FadeColor(reportDescriptionBackground.color, originalColor, 0.25f, Easings.Functions.CubicEaseOut);
		});
	}

	private void UpdateReportReasonValueText()
	{
		switch (reportInformation.reportReason)
		{
		case UserReports.ReportReason.OffensiveMessage:
			reportReasonValueText.Term = "Network/ReportUserReasons/OffensiveMessage";
			break;
		case UserReports.ReportReason.OffensiveUsername:
			reportReasonValueText.Term = "Network/ReportUserReasons/OffensiveUsername";
			break;
		case UserReports.ReportReason.DisruptiveBehavior:
			reportReasonValueText.Term = "Network/ReportUserReasons/DisruptiveBehaviour";
			break;
		case UserReports.ReportReason.Cheating:
			reportReasonValueText.Term = "Network/ReportUserReasons/Cheating";
			break;
		case UserReports.ReportReason.Other:
			reportReasonValueText.Term = "Network/ReportUserReasons/Other";
			break;
		}
	}

	public void SetReportReason(UserReports.ReportReason reason)
	{
		reportInformation.reportReason = reason;
		UpdateReportReasonValueText();
	}

	public void OnClickCancelReport(PickCursor pickCursor)
	{
		subdialogController.TransitionRightTo(mainDialog);
		reportInformation = null;
		reportedNetworkNumber = -1;
	}

	public void OnClickConfirmReport(PickCursor pickCursor)
	{
		reportInformation.reportComments = reportDescriptionInputField.text;
		GameSparksManager.Instance.CreateQuery().SubmitUserReport(reportInformation);
		if (reportedNetworkNumber != -1)
		{
			foreach (TabletOnlinePlayer activePlayerSlot in activePlayerSlots)
			{
				if (activePlayerSlot.networkNumber == reportedNetworkNumber)
				{
					activePlayerSlot.OnPlayerReported();
					break;
				}
			}
		}
		UserReports.NotifyReportedUser(reportInformation.reportedGSID);
		subdialogController.TransitionRightTo(mainDialog);
		reportedNetworkNumber = -1;
		reportInformation = null;
		UserMessageManager.Instance.UserMessage(LocalizationManager.GetTranslation("Network/ReportSubmitted"), 3f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
	}

	public void ShowKickPrompt(int kickingPlayerNetworkNumber, LobbyPlayer lobbyPlayer)
	{
		subdialogController.TransitionLeftTo(kickConfirmDialog, TransitionSound.Modal);
		this.kickingPlayerNetworkNumber = kickingPlayerNetworkNumber;
		kickedLobbyPlayer = lobbyPlayer;
		if (LobbyManager.instance.IsHost && LobbyManager.instance.CurrentLevelSelectController != null)
		{
			promptTextLabel.text = string.Format(ScriptLocalization.Network.AreyousureKick, lobbyPlayer.playerName);
			promptConfirmButtonLabel.Term = "Network/Kick";
		}
		else if (lobbyPlayer.VotedToKick)
		{
			promptTextLabel.text = string.Format(ScriptLocalization.Network.AreYouSureCancelKick, lobbyPlayer.playerName);
			promptConfirmButtonLabel.Term = "Network/CancelKick";
		}
		else
		{
			promptTextLabel.text = string.Format(ScriptLocalization.Network.AreYouSureVoteToKick, lobbyPlayer.playerName);
			promptConfirmButtonLabel.Term = "Network/VoteToKick";
		}
	}

	public void OnKickPromptConfirm(PickCursor pickCursor)
	{
		subdialogController.TransitionRightTo(mainDialog, TransitionSound.None);
		if (LobbyManager.instance.IsHost && LobbyManager.instance.CurrentLevelSelectController != null)
		{
			LobbyManager.instance.IssueKickMessage(kickedLobbyPlayer.networkNumber, LobbyManager.KickReasons.HOST);
		}
		else
		{
			kickedLobbyPlayer.VotedToKick = !kickedLobbyPlayer.VotedToKick;
			MsgVoteToKick msgVoteToKick = new MsgVoteToKick();
			msgVoteToKick.NetworkPlayerToKick = kickedLobbyPlayer.networkNumber;
			msgVoteToKick.NetworkPlayerVoting = kickingPlayerNetworkNumber;
			msgVoteToKick.VoteToKick = kickedLobbyPlayer.VotedToKick;
			LobbyManager.instance.client.Send(NetMsgTypes.VoteToKick, msgVoteToKick);
			foreach (TabletOnlinePlayer activePlayerSlot in activePlayerSlots)
			{
				if (activePlayerSlot.networkNumber == kickedLobbyPlayer.networkNumber)
				{
					activePlayerSlot.RefreshKickButton();
				}
			}
		}
		kickingPlayerNetworkNumber = -1;
		kickedLobbyPlayer = null;
	}

	public void OnKickPromptCancel(PickCursor pickCursor)
	{
		subdialogController.TransitionRightTo(mainDialog);
		kickingPlayerNetworkNumber = -1;
		kickedLobbyPlayer = null;
	}

	public void RefreshAllKickCounts()
	{
		foreach (TabletOnlinePlayer activePlayerSlot in activePlayerSlots)
		{
			activePlayerSlot.SetVoteKickCount(activePlayerSlot.kickCount, activePlayerSlots.Count - 1);
		}
	}

	public void SortActivePlayers()
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (TabletOnlinePlayer activePlayerSlot in activePlayerSlots)
		{
			int playerNodeID = activePlayerSlot.lobbyPlayer.playerNodeID;
			int value = 0;
			if (!dictionary.TryGetValue(playerNodeID, out value))
			{
				dictionary.Add(playerNodeID, activePlayerSlot.networkNumber);
			}
			else if (activePlayerSlot.networkNumber < value)
			{
				dictionary[playerNodeID] = activePlayerSlot.networkNumber;
			}
		}
		foreach (TabletOnlinePlayer activePlayerSlot2 in activePlayerSlots)
		{
			if (activePlayerSlot2.waitingForLocalNumber)
			{
				activePlayerSlot2.sortingRank = 9999;
			}
			else
			{
				activePlayerSlot2.sortingRank = dictionary[activePlayerSlot2.lobbyPlayer.playerNodeID] * 100 + activePlayerSlot2.lobbyPlayer.localNumber;
			}
		}
		activePlayerSlots.Sort((TabletOnlinePlayer a, TabletOnlinePlayer b) => a.CompareTo(b));
		foreach (TabletOnlinePlayer activePlayerSlot3 in activePlayerSlots)
		{
			activePlayerSlot3.transform.SetParent(null);
		}
		foreach (TabletOnlinePlayer activePlayerSlot4 in activePlayerSlots)
		{
			activePlayerSlot4.transform.SetParent(playerList);
		}
	}

	public override void Update()
	{
		base.Update();
		if (playerSlotsDirty)
		{
			playerSlotsDirty = false;
			SortActivePlayers();
			RefreshAllKickCounts();
		}
	}

	public override bool OnPressBack(PickCursor pickCursor)
	{
		if (tablet.modalOverlay.IsOpen || tablet.modalOverlay.IsOpening)
		{
			tablet.modalOverlay.OnCancel();
			return true;
		}
		if (subdialogController.currentSubdialog == kickConfirmDialog)
		{
			OnKickPromptCancel(pickCursor);
			return true;
		}
		if (subdialogController.currentSubdialog == reportDialog)
		{
			OnClickCancelReport(pickCursor);
			return true;
		}
		return base.OnPressBack(pickCursor);
	}
}
