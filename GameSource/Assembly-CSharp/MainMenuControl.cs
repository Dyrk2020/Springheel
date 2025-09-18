using System;
using GameEvent;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuControl : MonoBehaviour, InputReceiver, IGameEventListener
{
	protected bool Started;

	public InventoryBook inventoryBook;

	public Text startText;

	public Text startTextShadow;

	public Text player1Text;

	public Text player1TextShadow;

	public Text specialInviteMessageText;

	public Image loadingSpinner;

	public Animator CameraAnimator;

	public CanvasSplash blackFade;

	public CanvasSplash Splash;

	public CanvasSplash AudioSplash;

	protected bool skipUsed;

	private PickCursor menuCursor;

	public bool Starting;

	private bool processingInviteEvent;

	private const float disableDelay = 1f;

	private static float timeSinceLastDisabled = 999f;

	public MultiControllerButton JoinButton;

	protected virtual void Start()
	{
		Time.timeScale = 1f;
		StatTracker.Instance.Initialize();
		JoinButton.gameObject.SetActive(value: false);
		RichPresenceManager.Instance.SetDefaultPresenceString();
		if (StatTracker.PlatformKnowsUserAtStart)
		{
			StatTracker.Instance.LoadGameForMainUser();
		}
		Controller.AddGlobalReceiver(this);
		ChangeListeners(adding: true);
		skipUsed = false;
		if (!GameState.GetInstance().PreservePlayers)
		{
			foreach (Player item in PlayerManager.GetInstance())
			{
				if (item != null)
				{
					item.UseController = null;
				}
			}
			for (int i = 0; i != 4; i++)
			{
				PlayerManager.GetInstance().RemovePlayer(i + 1);
			}
			ControllerMonitor.Instance.ClearAllJoinedControllers();
			Controller.ClearPlayersForAllControllers();
			ControllerDisconnect.ClearAllPrompts();
		}
		else if (GameState.GetInstance().UsingHotSeat)
		{
			foreach (Player item2 in PlayerManager.GetInstance())
			{
				if (item2 != null)
				{
					if (!item2.UseController.ControlsPlayer(item2.Number))
					{
						item2.UseController = null;
						PlayerManager.GetInstance().RemovePlayer(item2.Number);
						continue;
					}
					Controller useController = item2.UseController;
					item2.UseController = null;
					useController.ClearPlayers();
					item2.UseController = useController;
				}
			}
		}
		GameState.GetInstance().UsingHotSeat = false;
		LobbyPlayer[] array = UnityEngine.Object.FindObjectsOfType<LobbyPlayer>();
		foreach (LobbyPlayer lobbyPl in array)
		{
			LobbyManagerManager.Instance.MarkLobbyPlayerToRemove(lobbyPl);
		}
		PickableNetworkButton.networkMenuCurrentState = PickableNetworkButton.NetworkPageStates.MainNetworkMenu;
		GameEventManager.SendEvent(new ClearChatEvent());
		specialInviteMessageText.enabled = false;
		loadingSpinner.enabled = false;
		GameState.GetInstance().lastLevelPlayed = "";
		GameState.GetInstance().guaranteedUnlocks.Clear();
		GameSettings.GetInstance().ResetModsToDefaults();
		LevelSelectController.ClearLastLobbyRulesetCopy();
		SaveSystemProtector.UnProtect();
	}

	public void OnDestroy()
	{
		ChangeListeners(adding: false);
	}

	public void ChangeListeners(bool adding)
	{
		GameEventManager.ChangeListener<PlayerInGameRuleEvent>(this, adding);
		GameEventManager.ChangeListener<DrivingPlayerRemovedEvent>(this, adding);
		GameEventManager.ChangeListener<ControllerConnectionEvent>(this, adding);
	}

	private void Update()
	{
		timeSinceLastDisabled += Time.unscaledDeltaTime;
		if (!skipUsed && GameSettings.GetInstance().SplashScreenOnce && !skipUsed)
		{
			blackFade.Skip();
			Splash.Skip();
			AudioSplash.Skip();
			skipUsed = true;
		}
		LobbyManagerManager.OnMainMenuUpdate();
		if (!processingInviteEvent && (GameState.GetInstance().autoStartHosting || GameState.GetInstance().autoJoinInvitedGame))
		{
			startText.enabled = false;
			startTextShadow.enabled = false;
			player1Text.enabled = false;
			player1TextShadow.enabled = false;
			GameSettings.GetInstance().SplashScreenOnce = true;
			processingInviteEvent = true;
			if (GameState.GetInstance().autoStartHosting)
			{
				specialInviteMessageText.text = LocalizationManager.GetTranslation("Network/HostingWithDefaultSettings");
			}
			else if (GameState.GetInstance().autoJoinInvitedGame)
			{
				specialInviteMessageText.text = LocalizationManager.GetTranslation("Network/JoiningGameDotDotDot");
			}
			specialInviteMessageText.enabled = true;
			loadingSpinner.enabled = true;
			Matchmaker.Instance.OnReadyToAutoJoin(this);
		}
		else if (Matchmaker.Instance.WaitingForController)
		{
			startText.text = LocalizationManager.GetTranslation("Network/Join Game");
			startTextShadow.text = LocalizationManager.GetTranslation("Network/Join Game");
			JoinButton.gameObject.SetActive(value: true);
		}
		else
		{
			startText.text = LocalizationManager.GetTranslation("InGameText/Start");
			startTextShadow.text = LocalizationManager.GetTranslation("InGameText/Start");
			JoinButton.gameObject.SetActive(value: false);
		}
		if (inventoryBook.ScreenMode && inventoryBook.CurrentScreenpage == inventoryBook.ScreenPage)
		{
			_ = ControllerMonitor.Instance.IsMainControllerSet;
		}
	}

	private void CloseNetworkComputerScreen()
	{
		if (inventoryBook.ScreenMode && inventoryBook.CurrentScreenpage == inventoryBook.ScreenPage)
		{
			inventoryBook.GotoPage(fakeVariable: false, InventoryPage.PageTypes.TabletInterface);
		}
	}

	public void ReceiveEvent(InputEvent e)
	{
		if (blackFade.State != UISplashScreen.STATE.HIDE || Splash.State != UISplashScreen.STATE.HIDE || AudioSplash.State != UISplashScreen.STATE.HIDE)
		{
			if ((e.Key == InputEvent.InputKey.Accept || e.Key == InputEvent.InputKey.Start) && e.Valueb && e.Changed && !skipUsed)
			{
				blackFade.Skip();
				Splash.Skip();
				AudioSplash.Skip();
				skipUsed = true;
			}
		}
		else if (!Started && !processingInviteEvent && (e.Key == InputEvent.InputKey.Accept || e.Key == InputEvent.InputKey.Start) && e.Valueb && e.Changed && timeSinceLastDisabled > 1f)
		{
			JoinControllerToMainMenu(e.Sender, popupMainMenuBook: true);
		}
	}

	public void JoinControllerToMainMenu(Controller controller, bool popupMainMenuBook)
	{
		PickableButton.ResetMasks();
		timeSinceLastDisabled = 0f;
		ControllerMonitor.Instance.SetMainMenuController(controller);
		GameSparksManager.Instance.WakeUp();
		StatTracker.Instance.LoadGameForMainUser();
		if (!PlayerManager.GetInstance().FirstUserLoggedIn && SteamManager.Initialized)
		{
			PlayerManager.GetInstance().SetFirstLogin();
		}
		if (Matchmaker.Instance.WaitingForController)
		{
			bool flag = false;
			foreach (Player item in PlayerManager.GetInstance())
			{
				if (item != null && item.UseController == controller)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				PlayerManager.GetInstance().AddPlayer(controller);
			}
			return;
		}
		if (popupMainMenuBook)
		{
			menuCursor = inventoryBook.AddMainMenuPlayer(1, 1, controller, Character.Animals.NONE);
		}
		GameState.GetInstance().SendStartAnalytics();
		if (menuCursor != null && popupMainMenuBook)
		{
			inventoryBook.Show(OpenSound: false);
			inventoryBook.GotoPage(fakeVariable: true, InventoryPage.PageTypes.TabletInterface);
			inventoryBook.backPage = 1;
			inventoryBook.backEnabled = true;
			inventoryBook.TabletPage.GetComponentInChildren<TabletMainMenuHome>().Initialize(this);
			GameEventManager.SendEvent(new PlayerInGameRuleEvent(entered: true, 1, BookSoundEffect: false));
		}
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(PlayerInGameRuleEvent))
		{
			if ((e as PlayerInGameRuleEvent).Entered)
			{
				Started = true;
				GameSettings.GetInstance().SplashScreenOnce = true;
				startText.enabled = false;
				startTextShadow.enabled = false;
				player1Text.enabled = false;
				player1TextShadow.enabled = false;
				specialInviteMessageText.enabled = false;
				loadingSpinner.enabled = false;
			}
			else if (!Starting)
			{
				timeSinceLastDisabled = 0f;
				Started = false;
				startText.enabled = true;
				startTextShadow.enabled = true;
				player1Text.enabled = true;
				player1TextShadow.enabled = true;
				specialInviteMessageText.enabled = false;
				loadingSpinner.enabled = false;
				if (menuCursor != null && menuCursor.LocalPlayer != null)
				{
					if (menuCursor.LocalPlayer.UseController != null)
					{
						menuCursor.LocalPlayer.UseController.RemovePlayer(1);
					}
					menuCursor.LocalPlayer.UseController = null;
				}
				StatTracker.Instance.SaveGameForAllUsers();
				PlayerManager.GetInstance().ClearAllPlayers();
				if (PlayerManager.GetInstance().FirstUserLoggedIn)
				{
					PlayerManager.GetInstance().ClearFirstLogin(sendDrivingPlayerRemovedEvent: false);
				}
				if (menuCursor != null)
				{
					UnityEngine.Object.Destroy(menuCursor.gameObject);
				}
				ControllerMonitor.Instance.ClearAllJoinedControllers();
			}
			CameraAnimator.SetBool("Start", Started);
		}
		if (type == typeof(DrivingPlayerRemovedEvent))
		{
			GameEventManager.SendEvent(new PlayerInGameRuleEvent(entered: false, 0));
		}
		if (type == typeof(ControllerConnectionEvent))
		{
			ControllerConnectionEvent controllerConnectionEvent = e as ControllerConnectionEvent;
			if (!controllerConnectionEvent.Connected && controllerConnectionEvent.Player != null && menuCursor != null && controllerConnectionEvent.Player == menuCursor.LocalPlayer)
			{
				GameEventManager.SendEvent(new PlayerInGameRuleEvent(entered: false, 0));
			}
		}
	}

	public void OnAutoJoinFailed()
	{
		GameState.GetInstance().autoJoinInvitedGame = false;
		GameState.GetInstance().autoStartHosting = false;
		processingInviteEvent = false;
		startText.enabled = true;
		startTextShadow.enabled = true;
		player1Text.enabled = true;
		player1TextShadow.enabled = true;
		specialInviteMessageText.enabled = false;
		loadingSpinner.enabled = false;
		PlayerManager.GetInstance().ClearAllPlayers();
		if (PlayerManager.GetInstance().FirstUserLoggedIn)
		{
			PlayerManager.GetInstance().ClearFirstLogin(sendDrivingPlayerRemovedEvent: false);
		}
		ControllerMonitor.Instance.ClearAllJoinedControllers();
	}

	public void OnLeaveLobbyCalled()
	{
		if (processingInviteEvent)
		{
			OnAutoJoinFailed();
		}
	}
}
