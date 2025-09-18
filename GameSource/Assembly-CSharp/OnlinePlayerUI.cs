using GameEvent;
using UnityEngine;
using UnityEngine.UI;

public class OnlinePlayerUI : MonoBehaviour, IGameEventListener
{
	public LobbyPlayer lobbyPlayer;

	public OnlinePlayerUISystem systemController;

	public PickableOnlineSettingButton MuteButton;

	public PickableOnlineSettingButton KickButton;

	public PickableOnlineSettingButton PlayerName;

	public PickableOnlineSettingButton ReportButton;

	public ConnectionQualityBars ConnectionBars;

	public Text KickCountText;

	private int kickCount;

	private int votesRequired;

	public Image UCHNetIcon;

	public Image PSNVerifiedIcon;

	public void Setup(LobbyPlayer newLobbyPlayer, OnlinePlayerUISystem systemController)
	{
		if (newLobbyPlayer == null)
		{
			Debug.LogError("Warning! Lobby Player is null!");
		}
		this.systemController = systemController;
		MuteButton.relatedOnlinePlayerUI = this;
		ReportButton.relatedOnlinePlayerUI = this;
		ReportButton.relatedOnlinePlayerUISystem = systemController;
		lobbyPlayer = newLobbyPlayer;
		systemController.inventoryPage.AddPickable(MuteButton);
		systemController.inventoryPage.AddPickable(KickButton);
		systemController.inventoryPage.AddPickable(PlayerName);
		systemController.inventoryPage.AddPickable(ReportButton);
		systemController.inventoryPage.imagesOnPage.Add(UCHNetIcon);
		systemController.inventoryPage.imagesOnPage.Add(PSNVerifiedIcon);
		MuteButton.Enable();
		KickButton.Enable();
		PlayerName.Enable();
		ReportButton.Enable();
		KickCountText.enabled = false;
	}

	private void Start()
	{
		ChangeListener(addRemove: true);
	}

	public void ChangeListener(bool addRemove)
	{
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, addRemove);
	}

	internal void KickPlayer()
	{
		systemController.GetReadyToConfirmKick(lobbyPlayer);
	}

	internal void ToggleMutePlayer()
	{
		lobbyPlayer.Muted = !lobbyPlayer.Muted;
	}

	private void LateUpdate()
	{
		if (lobbyPlayer != null)
		{
			NameTag.UpdateIcons(lobbyPlayer, UCHNetIcon, PSNVerifiedIcon, usePlayerColor: false);
			if (PlayerName.PickColliders.Length != 0 && lobbyPlayer.Initialized && lobbyPlayer.platform != LobbyPlayer.LocalMachinePlatform)
			{
				PlayerName.PickColliders = new Collider2D[0];
			}
		}
	}

	private void OnDestroy()
	{
		systemController.inventoryPage.RemovePickable(MuteButton);
		systemController.inventoryPage.RemovePickable(KickButton);
		systemController.inventoryPage.RemovePickable(PlayerName);
		systemController.inventoryPage.RemovePickable(ReportButton);
		systemController.inventoryPage.imagesOnPage.Remove(UCHNetIcon);
		systemController.inventoryPage.imagesOnPage.Remove(PSNVerifiedIcon);
		ChangeListener(addRemove: false);
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (!(e.GetType() == typeof(NetworkMessageReceivedEvent)))
		{
			return;
		}
		NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
		if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.ConnectionQuality)
		{
			MsgConnectionQuality msgConnectionQuality = networkMessageReceivedEvent.ReadMessage as MsgConnectionQuality;
			if (msgConnectionQuality.NetworkPlayerNumber == lobbyPlayer.networkNumber)
			{
				ConnectionBars.Quality = msgConnectionQuality.Quality;
			}
		}
	}

	public void SetVotesRequired(int numVotesRequired)
	{
		votesRequired = numVotesRequired;
		UpdateKickCountText();
	}

	public void SetVoteKickCount(int amount, int numVotesRequired)
	{
		kickCount = amount;
		votesRequired = numVotesRequired;
		UpdateKickCountText();
	}

	private void UpdateKickCountText()
	{
		if (votesRequired > 1 && kickCount > 0)
		{
			KickCountText.text = "(" + kickCount + "/" + votesRequired + ")";
			KickCountText.enabled = true;
		}
		else
		{
			KickCountText.enabled = false;
		}
	}

	public void ShowReportDialogForPlayer(LobbyPlayer reportingPlayer)
	{
		systemController.DisplayReportDialog(reportingPlayer, lobbyPlayer);
	}
}
