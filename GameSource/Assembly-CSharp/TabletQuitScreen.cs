using UnityEngine;
using UnityEngine.Networking;

public class TabletQuitScreen : TabletScreen
{
	public enum DisplayMode
	{
		MainMenu,
		Treehouse,
		InGameHost,
		InGameClient
	}

	public TabletButton mainMenuQuitButton;

	public TabletButton backToTreehouseButton;

	public TabletButton quitToDesktopButton;

	private bool showingChallengeTimesWarning = true;

	public TabletTextLabel challengeTimesWarning;

	public TabletDisableGroup buttonDisableGroup;

	private bool triggered;

	public void Initialize(DisplayMode displayMode)
	{
		switch (displayMode)
		{
		case DisplayMode.MainMenu:
			mainMenuQuitButton.gameObject.SetActive(value: false);
			backToTreehouseButton.gameObject.SetActive(value: false);
			break;
		case DisplayMode.Treehouse:
			backToTreehouseButton.gameObject.SetActive(value: false);
			break;
		case DisplayMode.InGameClient:
			backToTreehouseButton.gameObject.SetActive(value: false);
			break;
		case DisplayMode.InGameHost:
			break;
		}
	}

	public void OnClickReturnToTreehouse(PickCursor pickCursor)
	{
		if (!triggered)
		{
			OnQuitTriggered();
			ReturnToLobby();
		}
	}

	public void OnClickReturnToMainMenu(PickCursor pickCursor)
	{
		if (LobbyManager.instance != null && LobbyManager.instance.HasPlayersLockedForLoad)
		{
			Debug.LogWarning("Ignored ReturnToMainMenu -- found a player that was locked for load");
		}
		else if (!triggered)
		{
			OnQuitTriggered();
			Matchmaker.ReturnToMainMenu();
		}
	}

	public void OnClickQuitToDesktop(PickCursor pickCursor)
	{
		if (LobbyManager.instance != null && LobbyManager.instance.HasPlayersLockedForLoad)
		{
			Debug.LogWarning("Ignored ExitProgram -- found a player that was locked for load");
		}
		else if (!triggered)
		{
			OnQuitTriggered();
			if (LobbyManager.instance != null && LobbyManager.instance.IsInOnlineGame && LobbyManager.instance.IsHost)
			{
				Matchmaker.Instance.LeaveLobby("QuitGame");
				return;
			}
			Matchmaker.Instance.LeaveLobby();
			QuitGame();
		}
	}

	public void OnClickCancel(PickCursor pickCursor)
	{
		OnClickBurger(pickCursor);
	}

	public static void QuitGame()
	{
		if (LobbyManager.instance != null && LobbyManager.instance.IsHost)
		{
			NetworkServer.SendToAll(NetMsgTypes.HostEndedGame, new MsgHostEndedGame());
		}
		GameState.GetInstance().CleanupBeforeQuit();
		Application.Quit();
	}

	public static void ReturnToLobby()
	{
		NetworkPlayerTracker playerTracker = LobbyManager.instance.PlayerTracker;
		if (playerTracker.WaitingForIDs)
		{
			Debug.LogWarning("Player tracker is still missing NetIDs");
		}
		foreach (uint allGameNetID in playerTracker.GetAllGameNetIDs())
		{
			if (allGameNetID == 0)
			{
				continue;
			}
			GameObject gameObject = ClientScene.FindLocalObject(new NetworkInstanceId(allGameNetID));
			if (!(gameObject == null))
			{
				GamePlayer component = gameObject.GetComponent<GamePlayer>();
				Character characterInstance = component.CharacterInstance;
				component.CharacterInstance = null;
				if (characterInstance != null)
				{
					characterInstance.transform.parent = null;
					Object.Destroy(characterInstance.gameObject);
				}
				Cursor cursorInstance = component.CursorInstance;
				component.CursorInstance = null;
				if (cursorInstance != null)
				{
					cursorInstance.transform.parent = null;
					Object.Destroy(cursorInstance.gameObject);
				}
			}
		}
		if (LobbyManager.instance.CurrentGameController != null)
		{
			LobbyManager.instance.CurrentGameController.EndGame();
		}
	}

	public override void Update()
	{
		base.Update();
		if (ChallengeTimeCache.HasDataLeftToUpload)
		{
			if (!showingChallengeTimesWarning)
			{
				showingChallengeTimesWarning = true;
				challengeTimesWarning.gameObject.SetActive(value: true);
			}
		}
		else if (showingChallengeTimesWarning)
		{
			showingChallengeTimesWarning = false;
			challengeTimesWarning.gameObject.SetActive(value: false);
		}
	}

	public void OnQuitTriggered()
	{
		triggered = true;
		buttonDisableGroup.SetDisabled(disabled: true);
	}
}
