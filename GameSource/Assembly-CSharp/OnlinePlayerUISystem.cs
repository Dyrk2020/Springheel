using System;
using System.Collections.Generic;
using GameEvent;
using I2.Loc;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class OnlinePlayerUISystem : MonoBehaviour, IGameEventListener
{
	public GameObject OnlinePlayerUIPrefab;

	public Transform[] SlotPositions;

	private List<OnlinePlayerUI> OnlinePlayerUis = new List<OnlinePlayerUI>(PlayerManager.maxPlayers);

	public PickableOnlineSettingButton AreYouSureMessage;

	public PickableOnlineSettingButton AreYouSureYes;

	public PickableOnlineSettingButton AreYouSureNo;

	public PickableOnlineSettingButton AreYouSureSlash;

	public LobbyPlayer lobbyPlayerUpForKick;

	public InventoryPage inventoryPage;

	public RectTransform playerListContainer;

	public ReportUserDialog reportUserDialog;

	private void Awake()
	{
		OnlinePlayerUis.Clear();
		for (int i = 0; i < PlayerManager.maxPlayers; i++)
		{
			OnlinePlayerUis.Add(null);
		}
	}

	private void Start()
	{
		AreYouSureMessage.relatedOnlinePlayerUISystem = this;
		AreYouSureYes.relatedOnlinePlayerUISystem = this;
		AreYouSureNo.relatedOnlinePlayerUISystem = this;
		AreYouSureSlash.relatedOnlinePlayerUISystem = this;
		ChangeListener(addRemove: true);
		NetworkLobbyPlayer[] lobbySlots = LobbyManager.instance.lobbySlots;
		for (int i = 0; i < lobbySlots.Length; i++)
		{
			LobbyPlayer lobbyPlayer = (LobbyPlayer)lobbySlots[i];
			if (lobbyPlayer != null)
			{
				AddNewPlayer(lobbyPlayer);
			}
		}
		reportUserDialog.gameObject.SetActive(value: false);
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
		GameEventManager.ChangeListener<NoteBookDisplayEvent>(this, addRemove);
		GameEventManager.ChangeListener<InventoryPageDisplayEvent>(this, addRemove);
	}

	internal void GetReadyToConfirmKick(LobbyPlayer lobbyPlayer)
	{
		lobbyPlayerUpForKick = lobbyPlayer;
		bool flag = SceneManager.GetActiveScene().name == "TreeHouseLobby";
		if (lobbyPlayer.VotedToKick)
		{
			AreYouSureMessage.buttonText.text = string.Format(ScriptLocalization.Network.AreYouSureCancelKick, lobbyPlayer.playerName);
		}
		else if (flag || LobbyManager.instance.PlayerTracker.NumPlayers <= 2)
		{
			AreYouSureMessage.buttonText.text = string.Format(ScriptLocalization.Network.AreyousureKick, lobbyPlayer.playerName);
		}
		else
		{
			AreYouSureMessage.buttonText.text = string.Format(ScriptLocalization.Network.AreYouSureVoteToKick, lobbyPlayer.playerName);
		}
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
		if (type == typeof(NetworkMessageReceivedEvent))
		{
			NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.UpdateVoteKickCounts)
			{
				MsgUpdateVoteKickCounts msgUpdateVoteKickCounts = (MsgUpdateVoteKickCounts)networkMessageReceivedEvent.ReadMessage;
				foreach (OnlinePlayerUI onlinePlayerUi in OnlinePlayerUis)
				{
					if (onlinePlayerUi != null && onlinePlayerUi.lobbyPlayer != null && onlinePlayerUi.lobbyPlayer.networkNumber == msgUpdateVoteKickCounts.networkNumber)
					{
						onlinePlayerUi.SetVoteKickCount(msgUpdateVoteKickCounts.votes, OnlinePlayerUis.Count - 1);
					}
				}
			}
		}
		if (type == typeof(NoteBookDisplayEvent) && !(e as NoteBookDisplayEvent).Opened)
		{
			HideReportDialog();
		}
		if (type == typeof(InventoryPageDisplayEvent) && (e as InventoryPageDisplayEvent).pageNumber != inventoryPage.pageNumber)
		{
			HideReportDialog();
		}
	}

	public void AddNewPlayer(LobbyPlayer lobbyPlayer)
	{
		foreach (OnlinePlayerUI onlinePlayerUi in OnlinePlayerUis)
		{
			if (onlinePlayerUi != null && onlinePlayerUi.lobbyPlayer == lobbyPlayer)
			{
				return;
			}
		}
		int numOnlinePlayerUIs = GetNumOnlinePlayerUIs();
		GameObject obj = UnityEngine.Object.Instantiate(OnlinePlayerUIPrefab);
		obj.transform.SetParent(SlotPositions[numOnlinePlayerUIs].transform, worldPositionStays: false);
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localScale = Vector3.one;
		OnlinePlayerUI component = obj.GetComponent<OnlinePlayerUI>();
		OnlinePlayerUis[lobbyPlayer.networkNumber - 1] = component;
		component.Setup(lobbyPlayer, this);
		UpdateVotesRequired();
	}

	private int GetNumOnlinePlayerUIs()
	{
		int num = 0;
		for (int i = 0; i < OnlinePlayerUis.Count; i++)
		{
			if (OnlinePlayerUis[i] != null)
			{
				num++;
			}
		}
		return num;
	}

	private void UpdateVotesRequired()
	{
		int numOnlinePlayerUIs = GetNumOnlinePlayerUIs();
		foreach (OnlinePlayerUI onlinePlayerUi in OnlinePlayerUis)
		{
			if (onlinePlayerUi != null)
			{
				onlinePlayerUi.SetVotesRequired(numOnlinePlayerUIs - 1);
			}
		}
	}

	public void RemovePlayer(int networkNumber)
	{
		if (networkNumber > 0 && networkNumber <= OnlinePlayerUis.Count)
		{
			if (OnlinePlayerUis[networkNumber - 1] != null)
			{
				UnityEngine.Object.Destroy(OnlinePlayerUis[networkNumber - 1].gameObject);
				OnlinePlayerUis[networkNumber - 1] = null;
			}
			int num = 0;
			foreach (OnlinePlayerUI onlinePlayerUi in OnlinePlayerUis)
			{
				if (onlinePlayerUi != null)
				{
					onlinePlayerUi.transform.SetParent(SlotPositions[num].transform, worldPositionStays: false);
					num++;
				}
			}
		}
		else
		{
			Debug.LogError("Invalid network number!");
		}
		UpdateVotesRequired();
	}

	internal void VoteToKickPlayer()
	{
		bool flag = SceneManager.GetActiveScene().name == "TreeHouseLobby";
		if (LobbyManager.instance.IsHost && flag)
		{
			LobbyManager.instance.IssueKickMessage(lobbyPlayerUpForKick.networkNumber, LobbyManager.KickReasons.HOST);
		}
		else
		{
			lobbyPlayerUpForKick.VotedToKick = !lobbyPlayerUpForKick.VotedToKick;
			foreach (Player item in PlayerManager.GetInstance())
			{
				if (item != null)
				{
					MsgVoteToKick msgVoteToKick = new MsgVoteToKick();
					msgVoteToKick.NetworkPlayerToKick = lobbyPlayerUpForKick.networkNumber;
					msgVoteToKick.NetworkPlayerVoting = item.AssociatedLobbyPlayer.networkNumber;
					msgVoteToKick.VoteToKick = lobbyPlayerUpForKick.VotedToKick;
					LobbyManager.instance.client.Send(NetMsgTypes.VoteToKick, msgVoteToKick);
					if (lobbyPlayerUpForKick.VotedToKick)
					{
						break;
					}
				}
			}
		}
		lobbyPlayerUpForKick = null;
	}

	internal void DonotKickPlayer()
	{
		lobbyPlayerUpForKick = null;
	}

	public void DisplayReportDialog(LobbyPlayer reporter, LobbyPlayer reportedUser)
	{
		playerListContainer.localPosition = new Vector3(0f, -3000f, 0f);
		reportUserDialog.gameObject.SetActive(value: true);
		reportUserDialog.Initialize(reporter, reportedUser);
		reportUserDialog.SetInventoryPage(inventoryPage);
	}

	public void HideReportDialog()
	{
		playerListContainer.localPosition = new Vector3(0f, 0f, 0f);
		reportUserDialog.gameObject.SetActive(value: false);
		reportUserDialog.OnDialogClosed();
	}
}
