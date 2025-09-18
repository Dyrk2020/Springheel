using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameEvent;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InventoryBook : MonoBehaviour, IGameEventListener, InputReceiver
{
	public int currentPage;

	public int lastCurrentPage;

	public bool backEnabled;

	public int startNumberingAt;

	public bool MainMenuBook;

	public int backPage;

	public bool DynamicOptionPageNumber;

	public int OptionPageNumber;

	public bool PauseMode;

	public int PrepausePage;

	public float horizontalPosition;

	[HideInInspector]
	public InventoryPage[] InventoryPages;

	public GameObject Rings;

	public Transform pageHolder;

	public Transform[] cursorSpawnLocation;

	public Animator bookAnimator;

	public Bounds BoundingBox;

	public Bounds BoundingBoxComputerScreen;

	public Bounds currentResolutionBoundingBox;

	public Bounds currentResolutionBoundingBoxComputerScreen;

	public InventoryPage ScreenPage;

	public InventoryPage SecondScreenPage;

	public InventoryPage TabletPage;

	public InventoryPage CurrentScreenpage;

	public int AssociatedPlayer;

	public Cursor AssociatedCursor;

	public Camera UiCamera;

	public List<PickCursor> cursors = new List<PickCursor>();

	private int cursorCounter;

	public bool ShowingOnHost;

	public bool ScreenMode;

	public MainMenuAnnouncement Annoucement;

	public PickCursor pickCursorPrefab;

	protected bool active;

	public bool Visible;

	protected bool ClearBackPage;

	protected bool GoToPageTurning;

	protected Vector2 lastScreenSize;

	public bool FrozenOnPage;

	public bool FrozenWhileOpening;

	public List<Text> disabledTextElements = new List<Text>();

	public int ActiveCursors => cursors.Count;

	public bool inInventory
	{
		get
		{
			foreach (PickCursor cursor in cursors)
			{
				if (cursor.Picking)
				{
					return true;
				}
			}
			return false;
		}
	}

	public void DisableAllTextElements()
	{
		Text[] componentsInChildren = GetComponentsInChildren<Text>(includeInactive: true);
		foreach (Text text in componentsInChildren)
		{
			if (text.enabled)
			{
				text.enabled = false;
				disabledTextElements.Add(text);
			}
		}
	}

	public bool EnableTextElements(int num)
	{
		if (disabledTextElements.Count < num)
		{
			num = disabledTextElements.Count;
		}
		for (int i = 0; i < num; i++)
		{
			disabledTextElements[i].enabled = true;
		}
		disabledTextElements.RemoveRange(0, num);
		return disabledTextElements.Count == 0;
	}

	private void Awake()
	{
		List<Transform> list = new List<Transform>();
		foreach (Transform item in pageHolder)
		{
			list.Add(item);
		}
		foreach (Transform item2 in list)
		{
			InventoryPage component = item2.GetComponent<InventoryPage>();
			if (component != null && item2.gameObject.activeSelf)
			{
				component.ReplaceSelfWithPrefab();
			}
		}
		List<InventoryPage> list2 = new List<InventoryPage>();
		foreach (Transform item3 in pageHolder)
		{
			InventoryPage component2 = item3.GetComponent<InventoryPage>();
			if (component2 != null && item3.gameObject.activeSelf)
			{
				list2.Add(component2);
			}
		}
		InventoryPages = list2.ToArray();
		UiCamera = GetComponentInParent<Camera>();
		SceneManager.activeSceneChanged += MyOnLevelWasLoaded;
	}

	private void Start()
	{
		RecalcBoundingBoxes();
		if (UiCamera == null)
		{
			if (base.transform.parent == null)
			{
				Debug.LogWarning("Rule book is missing its parent.");
			}
			else
			{
				UiCamera = GetComponentInParent<Camera>();
			}
			if (UiCamera == null)
			{
				Debug.LogError("Still missing camera in parent for rule book. ");
			}
		}
		bool flag = LobbyManager.instance != null && !LobbyManager.instance.IsInOnlineGame;
		bool flag2 = GameSettings.GetInstance().GameMode != GameState.GameMode.FREEPLAY;
		bool flag3 = GameSettings.GetInstance().GameMode == GameState.GameMode.CHALLENGE;
		HashSet<InventoryPage> hashSet = new HashSet<InventoryPage>();
		bool flag4 = false;
		InventoryPage[] inventoryPages = InventoryPages;
		foreach (InventoryPage inventoryPage in inventoryPages)
		{
			if (inventoryPage.pageType == InventoryPage.PageTypes.TitlePage)
			{
				flag4 = true;
			}
			if ((flag && inventoryPage.OnlineOnly) || !inventoryPage.isActiveAndEnabled)
			{
				hashSet.Add(inventoryPage);
				continue;
			}
			if (flag2 && inventoryPage.FreeplayOnly)
			{
				hashSet.Add(inventoryPage);
				continue;
			}
			if (inventoryPage.BlankLevelOnly && SceneManager.GetActiveScene().name != "BlankLevel")
			{
				hashSet.Add(inventoryPage);
				continue;
			}
			if (flag3 && inventoryPage.HideInChallengeMode)
			{
				hashSet.Add(inventoryPage);
				continue;
			}
			if (inventoryPage.Deactivated)
			{
				hashSet.Add(inventoryPage);
				continue;
			}
			AllowedOnPlatform component = inventoryPage.GetComponent<AllowedOnPlatform>();
			if (component != null && !component.GetAllowed)
			{
				hashSet.Add(inventoryPage);
			}
		}
		if (DynamicOptionPageNumber)
		{
			OptionPageNumber = 0;
			inventoryPages = InventoryPages;
			foreach (InventoryPage inventoryPage2 in inventoryPages)
			{
				if ((inventoryPage2.pageType == InventoryPage.PageTypes.InventoryPage1 || inventoryPage2.pageType == InventoryPage.PageTypes.InventoryPage2 || inventoryPage2.pageType == InventoryPage.PageTypes.InventoryPage3 || inventoryPage2.pageType == InventoryPage.PageTypes.InventoryPage4 || inventoryPage2.pageType == InventoryPage.PageTypes.InventoryPage5 || inventoryPage2.pageType == InventoryPage.PageTypes.InventoryPage6 || inventoryPage2.pageType == InventoryPage.PageTypes.InventoryPage7 || inventoryPage2.pageType == InventoryPage.PageTypes.InventoryPage8 || inventoryPage2.pageType == InventoryPage.PageTypes.InventoryPage9) && !hashSet.Contains(inventoryPage2))
				{
					OptionPageNumber++;
				}
			}
		}
		List<InventoryPage> list = InventoryPages.ToList();
		foreach (InventoryPage item in hashSet)
		{
			item.hideContent();
			item.gameObject.SetActive(value: false);
			list.Remove(item);
		}
		InventoryPages = list.ToArray();
		for (int j = 0; j < InventoryPages.Length; j++)
		{
			InventoryPages[j].setPageLayer(j * -10);
			InventoryPages[j].transform.position = pageHolder.transform.position;
			int num = ((j < OptionPageNumber) ? j : (j - OptionPageNumber));
			int num2 = InventoryPages.Length;
			if (flag4)
			{
				num2--;
			}
			if (OptionPageNumber < InventoryPages.Length)
			{
				num2 = ((j < OptionPageNumber) ? OptionPageNumber : (InventoryPages.Length - OptionPageNumber));
			}
			num += startNumberingAt;
			InventoryPages[j].SetPageNumber(j, num, num2);
			InventoryPages[j].inventoryBook = this;
			InventoryPages[j].textCanvas.worldCamera = UiCamera;
		}
		Rings.transform.position = pageHolder.transform.position;
		UpdatePages(0);
		if (ScreenPage != null)
		{
			ScreenPage.SetPageNumber(-1, -1);
		}
		if (SecondScreenPage != null)
		{
			SecondScreenPage.SetPageNumber(-2, -2);
		}
		if (TabletPage != null)
		{
			TabletPage.SetPageNumber(-3, -3);
		}
		ChangeListeners(adding: true);
		TurnOffScreens();
		if (Annoucement != null)
		{
			Annoucement.Hide();
		}
	}

	public void OnDestroy()
	{
		ChangeListeners(adding: false);
		SceneManager.activeSceneChanged -= MyOnLevelWasLoaded;
	}

	public void ChangeListeners(bool adding)
	{
		GameEventManager.ChangeListener<PlayerInventoryEvent>(this, adding);
		GameEventManager.ChangeListener<PickBlockEvent>(this, adding);
		GameEventManager.ChangeListener<PlayerInGameRuleEvent>(this, adding);
		GameEventManager.ChangeListener<PauseEvent>(this, adding);
		GameEventManager.ChangeListener<SpecialUIEvent>(this, adding);
		GameEventManager.ChangeListener<SoftPauseEvent>(this, adding);
		GameEventManager.ChangeListener<ControllerConnectionEvent>(this, adding);
	}

	private void Update()
	{
		if (UiCamera != null && ((float)Screen.width != lastScreenSize.x || (float)Screen.height != lastScreenSize.y))
		{
			Vector3 vector = UiCamera.ScreenToWorldPoint(new Vector3(0f, UiCamera.pixelHeight / 2, 250f));
			base.transform.position = new Vector3(horizontalPosition + vector.x, base.transform.position.y, base.transform.position.z);
			RecalcBoundingBoxes();
		}
		lastScreenSize.x = Screen.width;
		lastScreenSize.y = Screen.height;
		bool flag = LobbyManager.instance != null && LobbyManager.instance.CurrentLevelSelectController != null && LobbyManager.instance.CurrentLevelSelectController.CountDownStart.CurrentNumber <= 1;
		if (LobbyManager.instance != null && LobbyManager.instance.HasPlayersLockedForLoad && flag && Visible)
		{
			ForceClose();
		}
	}

	private Bounds GetBoundsForCurrentAspect(Bounds originalBoundingBox)
	{
		float num = base.transform.position.z - UiCamera.transform.position.z;
		float num2 = 2f * num * Mathf.Tan(UiCamera.fieldOfView * 0.5f * (MathF.PI / 180f));
		float num3 = num2 * 1.7777778f;
		float num4 = num2 * ((float)Screen.width / (float)Screen.height) - num3;
		Vector3 center = originalBoundingBox.center;
		center.x += num4 * 0.5f;
		Vector3 size = originalBoundingBox.size;
		size.x += num4;
		return new Bounds(center, size);
	}

	private void RecalcBoundingBoxes()
	{
		currentResolutionBoundingBox = GetBoundsForCurrentAspect(BoundingBox);
		currentResolutionBoundingBoxComputerScreen = GetBoundsForCurrentAspect(BoundingBoxComputerScreen);
		foreach (PickCursor cursor in cursors)
		{
			if (cursor != null)
			{
				cursor.SetBounds(new Bounds(currentResolutionBoundingBox.center / base.transform.localScale.x, currentResolutionBoundingBox.size / base.transform.localScale.x));
			}
		}
	}

	private void UpdatePages(int newPage)
	{
		for (int i = 0; i < InventoryPages.Length; i++)
		{
			if (i < newPage)
			{
				InventoryPages[i].setPageState(1);
			}
			if (i == newPage)
			{
				InventoryPages[i].setPageState(0);
			}
			if (i > newPage)
			{
				InventoryPages[i].setPageState(-1);
			}
		}
		GameEventManager.SendEvent(new InventoryPageDisplayEvent(newPage));
	}

	public void NextPage(int pageDifference = 1, bool allowClearBackPage = false, bool pageTurnSound = false)
	{
		if (FrozenOnPage)
		{
			return;
		}
		if (Controller.InputFieldIsActive)
		{
			Controller.UnlockInputField();
		}
		if (ScreenMode)
		{
			return;
		}
		if (!GoToPageTurning)
		{
			if (PauseMode)
			{
				if (currentPage + pageDifference < OptionPageNumber)
				{
					return;
				}
			}
			else if (currentPage + pageDifference >= OptionPageNumber)
			{
				return;
			}
		}
		currentPage += pageDifference;
		if (pageTurnSound && currentPage >= 0 && currentPage < InventoryPages.Length)
		{
			AkSoundEngine.PostEvent("UI_InGame_Inventory_Page_Turn", base.gameObject);
		}
		currentPage = Mathf.Clamp(currentPage, 0, InventoryPages.Length - 1);
		UpdatePages(currentPage);
		if (allowClearBackPage && ClearBackPage)
		{
			backEnabled = false;
			ClearBackPage = false;
		}
	}

	public void PreviousPage(bool allowClearBackPage = false, bool pageTurnSounds = false)
	{
		NextPage(-1, allowClearBackPage, pageTurnSounds);
	}

	public void GotoPage(int targetPageNumber, bool enableBack = false, bool pageTurnSounds = false)
	{
		if (ScreenMode)
		{
			TurnOffScreens(ShowBook: true);
			StartCoroutine(GotoPageCoRoutine(targetPageNumber, pageTurnSounds));
		}
		else
		{
			backEnabled = enableBack;
			backPage = currentPage;
			StartCoroutine(GotoPageCoRoutine(targetPageNumber, pageTurnSounds));
		}
	}

	public void GotoPage(bool fakeVariable, InventoryPage.PageTypes targetPage, bool enableBack = false, bool pageTurnSounds = false)
	{
		switch (targetPage)
		{
		case InventoryPage.PageTypes.ComputerTerminal:
			TurnScreenOn(ScreenPage);
			return;
		case InventoryPage.PageTypes.SecondComputerTerminal:
			TurnScreenOn(SecondScreenPage);
			return;
		case InventoryPage.PageTypes.TabletInterface:
			TurnScreenOn(TabletPage);
			GetComponentInChildren<Tablet>().OnShowTablet();
			return;
		case InventoryPage.PageTypes.TabletInterfaceMods:
		{
			TurnScreenOn(TabletPage);
			Tablet componentInChildren = GetComponentInChildren<Tablet>();
			componentInChildren.OnShowTablet();
			componentInChildren.GotoScreen(componentInChildren.rulesScreen);
			componentInChildren.rulesScreen.EnterSubdialog(componentInChildren.rulesScreen.modifierSettingsSubdialog);
			return;
		}
		}
		InventoryPage[] inventoryPages = InventoryPages;
		foreach (InventoryPage inventoryPage in inventoryPages)
		{
			if (inventoryPage.pageType == targetPage)
			{
				GotoPage(inventoryPage.pageNumber, enableBack, pageTurnSounds);
				return;
			}
		}
		GotoPage(0, enableBack);
		Debug.Log("Page " + targetPage.ToString() + " not found");
	}

	private IEnumerator GotoPageCoRoutine(int targetPageNumber, bool pageTurnSounds = false)
	{
		if (Controller.InputFieldIsActive)
		{
			Controller.UnlockInputField();
		}
		while (GoToPageTurning)
		{
			yield return null;
		}
		GoToPageTurning = true;
		int numberOfTurns = targetPageNumber - currentPage;
		for (int i = 0; i < Math.Abs(numberOfTurns); i++)
		{
			if (numberOfTurns > 0)
			{
				NextPage(1, allowClearBackPage: false, pageTurnSounds);
			}
			else
			{
				PreviousPage(allowClearBackPage: false, pageTurnSounds);
			}
			yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(0.1f));
		}
		if (backEnabled)
		{
			ClearBackPage = true;
		}
		GoToPageTurning = false;
	}

	public void ReceiveEvent(InputEvent e)
	{
	}

	public void Show(bool OpenSound = true)
	{
		if (bookAnimator.isActiveAndEnabled)
		{
			bookAnimator.SetBool("On", value: true);
		}
		bool visible = Visible;
		Visible = true;
		if (!visible)
		{
			GotoPage(lastCurrentPage);
		}
		if (OpenSound)
		{
			AkSoundEngine.PostEvent("UI_InGame_Open_Inventory", base.gameObject);
		}
		GameEventManager.SendEvent(new NoteBookDisplayEvent(opened: true));
		Vector3 vector = UiCamera.ScreenToWorldPoint(new Vector3(0f, UiCamera.pixelHeight / 2, 250f));
		base.transform.position = new Vector3(horizontalPosition + vector.x, base.transform.position.y, base.transform.position.z);
		RecalcBoundingBoxes();
		if (Annoucement != null)
		{
			Annoucement.Show();
		}
	}

	public void Hide()
	{
		if (FrozenOnPage)
		{
			return;
		}
		if (bookAnimator.isActiveAndEnabled)
		{
			bookAnimator.SetBool("On", value: false);
		}
		Visible = false;
		lastCurrentPage = currentPage;
		if (!ScreenMode)
		{
			AkSoundEngine.PostEvent("UI_InGame_Close_Inventory", base.gameObject);
			GotoPage(0);
		}
		else
		{
			if (CurrentScreenpage == TabletPage)
			{
				GetComponentInChildren<Tablet>().OnHideTablet();
			}
			TurnOffScreens();
		}
		cursorCounter = 0;
		GameEventManager.SendEvent(new NoteBookDisplayEvent(opened: false));
		if (ControllerMonitor.Instance.IsMainControllerSet)
		{
			StatTracker.Instance.SaveGameForAllUsers();
		}
		if (Annoucement != null)
		{
			Annoucement.Hide();
		}
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(StartPhaseEvent))
		{
			StartPhaseEvent obj = e as StartPhaseEvent;
			if (obj.Phase == GameControl.GamePhase.PLACE)
			{
				active = true;
			}
			if (obj.Phase == GameControl.GamePhase.PLAY)
			{
				active = false;
			}
		}
		if (type == typeof(PlayerInventoryEvent))
		{
			PlayerInventoryEvent playerInventoryEvent = e as PlayerInventoryEvent;
			if (playerInventoryEvent.Entered)
			{
				if (!Visible)
				{
					Show();
				}
				if (HasCursor(playerInventoryEvent.PlayerNumber) && !GetCursor(playerInventoryEvent.PlayerNumber).Picking)
				{
					cursorCounter++;
				}
				ShowCursor(playerInventoryEvent.PlayerNumber);
			}
			else
			{
				cursorCounter--;
				if (cursorCounter <= 0)
				{
					Hide();
					if (playerInventoryEvent.LeavingPauseMenu)
					{
						lastCurrentPage = PrepausePage;
					}
				}
			}
		}
		if (type == typeof(PlayerInGameRuleEvent))
		{
			PlayerInGameRuleEvent playerInGameRuleEvent = e as PlayerInGameRuleEvent;
			if (playerInGameRuleEvent.Entered)
			{
				if (!Visible)
				{
					lastCurrentPage = 0;
					Show(playerInGameRuleEvent.SoundEffect);
				}
				LobbyPlayer lobbyPlayer = null;
				if (LobbyManager.instance != null)
				{
					lobbyPlayer = LobbyManager.instance.GetLobbyPlayer(playerInGameRuleEvent.PlayerNumber);
				}
				if (!HasCursor(playerInGameRuleEvent.PlayerNumber) && lobbyPlayer != null)
				{
					AddPlayer(lobbyPlayer.localNumber, lobbyPlayer.networkNumber, lobbyPlayer.LocalPlayer.UseController, lobbyPlayer.PickedAnimal);
				}
				PickCursor cursor = GetCursor(playerInGameRuleEvent.PlayerNumber);
				if (!cursor.Picking)
				{
					cursorCounter++;
				}
				if (lobbyPlayer != null)
				{
					cursor.SetSprites(lobbyPlayer.PickedAnimal);
					if (lobbyPlayer.PickedAnimal == Character.Animals.NONE)
					{
						cursor.SetColor(lobbyPlayer.PlayerColor);
					}
					else
					{
						cursor.SetColor(Color.white);
					}
				}
				ShowCursor(playerInGameRuleEvent.PlayerNumber);
			}
			else
			{
				cursorCounter--;
				if (cursorCounter <= 0)
				{
					Hide();
				}
			}
		}
		if (type == typeof(PauseEvent))
		{
			PauseEvent pauseEvent = e as PauseEvent;
			ShowingOnHost = true;
			onPauseEvent(pauseEvent.Paused, pauseEvent.PlayerNumber);
		}
		if (type == typeof(SoftPauseEvent))
		{
			SoftPauseEvent softPauseEvent = e as SoftPauseEvent;
			onPauseEvent(softPauseEvent.SoftPaused, softPauseEvent.PlayerNumber);
			ShowingOnHost = softPauseEvent.HostPausing;
		}
		if (!(type == typeof(ControllerConnectionEvent)))
		{
			return;
		}
		ControllerConnectionEvent controllerConnectionEvent = e as ControllerConnectionEvent;
		if (!controllerConnectionEvent.Connected)
		{
			return;
		}
		foreach (PickCursor cursor2 in cursors)
		{
			if (cursor2.LocalPlayer == controllerConnectionEvent.Player && controllerConnectionEvent.Player.UseController != null)
			{
				controllerConnectionEvent.Player.UseController.AddReceiver(cursor2);
			}
		}
	}

	private void onPauseEvent(bool pause, int playerNumber)
	{
		if (pause)
		{
			PauseMode = true;
			if (!Visible)
			{
				Show(OpenSound: false);
			}
			if (HasCursor(playerNumber))
			{
				PickCursor cursor = GetCursor(playerNumber);
				if (!cursor.Picking)
				{
					if (!cursor.PauseMode)
					{
						cursorCounter++;
						PrepausePage = currentPage;
						cursor.PauseMode = true;
						ShowCursor(playerNumber);
					}
				}
				else if (!cursor.PauseMode)
				{
					PrepausePage = currentPage;
				}
			}
			GotoPage(fakeVariable: true, InventoryPage.PageTypes.TabletInterface);
			backPage = OptionPageNumber;
			FreezeUntilNextFrame();
			{
				foreach (PickCursor cursor2 in cursors)
				{
					if (cursor2 != null && playerNumber == cursor2.networkNumber)
					{
						cursor2.PauseMode = true;
						cursor2.Enable();
					}
				}
				return;
			}
		}
		PauseMode = false;
		if (inInventory)
		{
			GotoPage(PrepausePage);
			backEnabled = false;
			backPage = 0;
		}
	}

	public void ShowCursor(int PlayerNumber)
	{
		foreach (PickCursor cursor in cursors)
		{
			if (cursor != null && PlayerNumber == cursor.networkNumber)
			{
				if (!cursor.Enabled)
				{
					cursor.Enable();
				}
				if (ScreenMode)
				{
					Bounds bounds = currentResolutionBoundingBoxComputerScreen;
					cursor.SetBounds(new Bounds(bounds.center / base.transform.localScale.x, bounds.size / base.transform.localScale.x));
				}
			}
		}
	}

	public bool HasCursor(int PlayerNumber)
	{
		if (PlayerNumber < 1 || PlayerNumber > 4)
		{
			return false;
		}
		foreach (PickCursor cursor in cursors)
		{
			if (cursor != null && PlayerNumber == cursor.networkNumber)
			{
				return true;
			}
		}
		return false;
	}

	public PickCursor GetCursor(int PlayerNumber)
	{
		if (PlayerNumber < 1 || PlayerNumber > 4)
		{
			return null;
		}
		foreach (PickCursor cursor in cursors)
		{
			if (cursor != null && PlayerNumber == cursor.networkNumber)
			{
				return cursor;
			}
		}
		return null;
	}

	public PickCursor AddPlayer(int localPlayerNumber, int networkPlayerNumber, Controller input, Character.Animals animal)
	{
		RecalcBoundingBoxes();
		if (localPlayerNumber > 4 || localPlayerNumber < 1)
		{
			return null;
		}
		PickCursor pickCursor = null;
		foreach (PickCursor cursor in cursors)
		{
			if (cursor != null && cursor.networkNumber == networkPlayerNumber)
			{
				pickCursor = cursor;
			}
		}
		if (pickCursor == null)
		{
			pickCursor = UnityEngine.Object.Instantiate(pickCursorPrefab);
		}
		AkSoundEngine.SetSwitch("Character", animal.ToString(), base.gameObject);
		pickCursor.transform.parent = base.transform;
		pickCursor.transform.localPosition = cursorSpawnLocation[localPlayerNumber - 1].localPosition;
		pickCursor.InventoryBookMenu = this;
		pickCursor.SetBounds(new Bounds(currentResolutionBoundingBox.center / base.transform.localScale.x, currentResolutionBoundingBox.size / base.transform.localScale.x));
		pickCursor.NetworknetworkNumber = networkPlayerNumber;
		pickCursor.NetworklocalNumber = localPlayerNumber;
		pickCursor.LocalPlayer = PlayerManager.GetInstance().GetPlayer(pickCursor.localNumber);
		pickCursor.SetSprites(animal);
		pickCursor.UseCamera = UiCamera.GetComponent<Camera>();
		pickCursor.Disable();
		Transform[] componentsInChildren = pickCursor.GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = 5;
		}
		pickCursor.SetLayer(5, isServer: true);
		input.AddReceiver(pickCursor);
		cursors.Add(pickCursor);
		return pickCursor;
	}

	public void RemovePlayer(int networkPlayerNumber, Controller input)
	{
		PickCursor pickCursor = null;
		foreach (PickCursor cursor in cursors)
		{
			if (cursor != null && cursor.networkNumber == networkPlayerNumber)
			{
				pickCursor = cursor;
			}
		}
		if (pickCursor != null)
		{
			cursors.Remove(pickCursor);
			input.RemoveReceiver(pickCursor);
			UnityEngine.Object.Destroy(pickCursor.gameObject);
		}
	}

	public void ForceClose()
	{
		foreach (PickCursor cursor in cursors)
		{
			if (cursor != null && cursor.LocalPlayer != null)
			{
				if (cursor.LocalPlayer.UseController != null)
				{
					cursor.LocalPlayer.UseController.RemoveReceiver(cursor);
				}
				cursor.Freeze();
				cursor.Disable();
				GameEventManager.SendEvent(new PlayerInGameRuleEvent(entered: false, cursor.networkNumber));
			}
		}
		cursors.Clear();
	}

	public PickCursor AddMainMenuPlayer(int playerNumber, int networkNumber, Controller input, Character.Animals animal)
	{
		RecalcBoundingBoxes();
		PickCursor pickCursor = null;
		foreach (PickCursor cursor in cursors)
		{
			if (cursor != null && cursor.networkNumber == playerNumber)
			{
				pickCursor = cursor;
			}
		}
		if (pickCursor == null)
		{
			pickCursor = UnityEngine.Object.Instantiate(pickCursorPrefab);
		}
		AkSoundEngine.SetSwitch("Character", animal.ToString(), base.gameObject);
		pickCursor.transform.parent = base.transform;
		pickCursor.transform.localPosition = cursorSpawnLocation[0].localPosition;
		pickCursor.InventoryBookMenu = this;
		pickCursor.SetBounds(new Bounds(currentResolutionBoundingBox.center / base.transform.localScale.x, currentResolutionBoundingBox.size / base.transform.localScale.x));
		pickCursor.NetworknetworkNumber = networkNumber;
		pickCursor.NetworklocalNumber = playerNumber;
		if (animal != Character.Animals.NONE)
		{
			pickCursor.SetSprites(animal);
		}
		pickCursor.UseCamera = UiCamera.GetComponent<Camera>();
		pickCursor.LocalPlayer = PlayerManager.GetInstance().AddPlayer(input);
		input.AddPlayer(1);
		input.AssumeUser(assume: true);
		Transform[] componentsInChildren = pickCursor.GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = 5;
		}
		pickCursor.Disable(sound: false);
		input.AddReceiver(pickCursor);
		cursors.Add(pickCursor);
		return pickCursor;
	}

	public void TurnScreenOn(InventoryPage thisScreen)
	{
		if (ScreenMode)
		{
			if (ScreenPage != null)
			{
				ScreenPage.animator.SetBool("Up", value: false);
			}
			if (SecondScreenPage != null)
			{
				SecondScreenPage.animator.SetBool("Up", value: false);
			}
			if (TabletPage != null)
			{
				TabletPage.animator.SetBool("Up", value: false);
			}
		}
		CurrentScreenpage = thisScreen;
		if (bookAnimator.isActiveAndEnabled)
		{
			bookAnimator.SetBool("On", value: false);
		}
		if (thisScreen.animator.isActiveAndEnabled)
		{
			thisScreen.animator.SetBool("Up", value: true);
		}
		ScreenMode = true;
		Bounds bounds = currentResolutionBoundingBoxComputerScreen;
		currentPage = thisScreen.pageNumber;
		if (Annoucement != null)
		{
			if (thisScreen.pageType == InventoryPage.PageTypes.TabletInterface)
			{
				Annoucement.Show();
			}
			else
			{
				Annoucement.Hide();
			}
		}
		if (thisScreen != TabletPage)
		{
			foreach (PickCursor cursor in cursors)
			{
				if (cursor != null)
				{
					cursor.SetBounds(new Bounds(bounds.center / base.transform.localScale.x, bounds.size / base.transform.localScale.x));
				}
			}
		}
		thisScreen.showContent();
	}

	public void TurnOffScreens(bool ShowBook = false)
	{
		Controller.FullScreenComputerIsActive = false;
		if (bookAnimator.isActiveAndEnabled)
		{
			bookAnimator.SetBool("On", ShowBook);
		}
		if (ScreenPage != null)
		{
			ScreenPage.animator.SetBool("Up", value: false);
		}
		if (SecondScreenPage != null)
		{
			SecondScreenPage.animator.SetBool("Up", value: false);
		}
		if (TabletPage != null)
		{
			TabletPage.animator.SetBool("Up", value: false);
		}
		ScreenMode = false;
		CurrentScreenpage = null;
		Bounds bounds = currentResolutionBoundingBox;
		currentPage = 1;
		if (Annoucement != null)
		{
			Annoucement.Show();
		}
		foreach (PickCursor cursor in cursors)
		{
			if (cursor != null)
			{
				cursor.SetBounds(new Bounds(bounds.center / base.transform.localScale.x, bounds.size / base.transform.localScale.x));
			}
		}
	}

	private void MyOnLevelWasLoaded(Scene previousScene, Scene newScene)
	{
		UiCamera = GetComponentInParent<Camera>();
	}

	private void FreezeUntilNextFrame()
	{
		FrozenWhileOpening = true;
		StartCoroutine(unfreezeOpeningNextFrame());
	}

	private IEnumerator unfreezeOpeningNextFrame()
	{
		yield return new WaitForFixedUpdate();
		yield return new WaitForEndOfFrame();
		FrozenWhileOpening = false;
	}
}
