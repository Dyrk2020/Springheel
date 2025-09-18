using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Tablet : MonoBehaviour
{
	public class CursorInfo
	{
		public GameObject lastHoverTarget;
	}

	public List<TabletScreen> tabletScreens = new List<TabletScreen>();

	public TabletScreen loadingScreen;

	public TabletScreen emptyScreen;

	public TabletScreen currentScreen;

	public Transform transitionTopLayer;

	public Transform transitionBottomLayer;

	public TabletColorScheme defaultScheme;

	public RectMask2D contentMask;

	public RectMask2D sidebarMask;

	public TabletSidebar tabletSidebar;

	public TabletClickableLayer mainLayer;

	public TabletClickableLayer sidebarLayer;

	public TabletClickableLayer modalLayer;

	public TabletClickableLayer presetLayer;

	public TabletRulesScreen rulesScreen;

	public TabletQuitScreen quitScreen;

	public TabletModalOverlay modalOverlay;

	public TabletPresetSelectOverlay presetOverlay;

	public RectTransform PickableBlockClampRect;

	public Material PickableBlockSpriteMaterial;

	public Material UpblowerParticleMaterial;

	public RectTransform PickableBlockButtonsOverlay;

	public TabletScreen lastBackedScreen;

	public RectTransform modifiersContainer;

	public RectTransform inGameModifiersContainer;

	private Canvas canvas;

	private InventoryBook book;

	private static bool introShown = false;

	private Dictionary<PickCursor, CursorInfo> trackedCursors = new Dictionary<PickCursor, CursorInfo>();

	private Dictionary<GameObject, int> hoverCounts = new Dictionary<GameObject, int>();

	private Dictionary<PickCursor, Vector2> scrollImpulses = new Dictionary<PickCursor, Vector2>();

	public static Tablet clickEventReceiver;

	public List<RectTransform> hiddenOfflineElements;

	public List<TabletStyledObject> DisableOfflineElements;

	private List<PickCursor> untrackedCursors = new List<PickCursor>(4);

	private HashSet<GameObject> objectsToDelete = new HashSet<GameObject>();

	private static List<RaycastResult> raycastResultCache = new List<RaycastResult>(128);

	private EventSystem eventSystem;

	public bool CurrentlyShown
	{
		get
		{
			InventoryBook componentInParent = GetComponentInParent<InventoryBook>();
			if (componentInParent != null)
			{
				if (componentInParent.ScreenMode)
				{
					return componentInParent.CurrentScreenpage == componentInParent.TabletPage;
				}
				return false;
			}
			return false;
		}
	}

	private void Awake()
	{
		clickEventReceiver = this;
		contentMask.enabled = true;
		sidebarMask.enabled = true;
		eventSystem = EventSystem.current;
	}

	private void OnDestroy()
	{
		clickEventReceiver = null;
	}

	public void Start()
	{
		modalOverlay.gameObject.SetActive(value: false);
		foreach (TabletScreen tabletScreen in tabletScreens)
		{
			tabletScreen.tablet = this;
			tabletScreen.gameObject.SetActive(value: false);
		}
		canvas = GetComponentInChildren<Canvas>();
		canvas.worldCamera = GetComponentInParent<Camera>();
		book = GetComponentInParent<InventoryBook>();
		rulesScreen.Initialize();
		Localize[] componentsInChildren = GetComponentsInChildren<Localize>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].OnLocalize(Force: true);
		}
		if (LobbyManager.instance != null && !LobbyManager.instance.IsInOnlineGame)
		{
			foreach (RectTransform hiddenOfflineElement in hiddenOfflineElements)
			{
				hiddenOfflineElement.gameObject.SetActive(value: false);
			}
			foreach (TabletStyledObject disableOfflineElement in DisableOfflineElements)
			{
				disableOfflineElement.SetDisabled(disabled: true);
			}
		}
		if (LobbyManager.instance == null && SceneManager.GetActiveScene().name == "MainMenu")
		{
			GetComponent<InventoryPage>().ScreenBackButtonTarget = InventoryPage.PageTypes.nonePage;
		}
		if (LobbyManager.instance != null && LobbyManager.instance.CurrentGameController != null)
		{
			modifiersContainer.SetParent(inGameModifiersContainer, worldPositionStays: false);
		}
	}

	public void OnShowTablet()
	{
		AkSoundEngine.PostEvent("UI_UPad_Open", base.gameObject);
		if (!introShown)
		{
			introShown = true;
			loadingScreen.gameObject.SetActive(value: true);
			TransitionTo(loadingScreen);
		}
		else if (currentScreen == null)
		{
			TransitionTo(emptyScreen);
			OpenBurgerMenu(null);
		}
		if ((bool)GameState.ChatSystem)
		{
			GameState.ChatSystem.ShiftPosition();
		}
		PickableBlockButtonsOverlay.gameObject.SetActive(value: true);
	}

	public void OnHideTablet()
	{
		AkSoundEngine.PostEvent("UI_UPad_Close", base.gameObject);
		if (GameState.ChatSystem != null)
		{
			GameState.ChatSystem.UnshiftPosition();
		}
		ClearHoverEffects();
		StartCoroutine(resetToMain());
	}

	private IEnumerator resetToMain()
	{
		while (OnPressBack(null))
		{
			yield return null;
		}
	}

	public void TransitionTo(TabletScreen tabletScreen)
	{
		if (currentScreen != tabletScreen)
		{
			if (currentScreen != null)
			{
				currentScreen.TransitionOut();
			}
			TabletScreen tabletScreen2 = currentScreen;
			currentScreen = tabletScreen;
			if (tabletScreen2 != null)
			{
				tabletScreen2.transform.SetParent(transitionTopLayer);
			}
			currentScreen.transform.SetParent(transitionBottomLayer);
			currentScreen.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
			currentScreen.TransitionIn(tabletScreen2);
		}
	}

	private void Update()
	{
		if (eventSystem == null)
		{
			eventSystem = EventSystem.current;
		}
		UpdateTrackedCursors();
		if (book.Visible)
		{
			UpdateHoverEffects();
		}
	}

	private void LateUpdate()
	{
		ApplyCursorScroll();
		RefreshPickableBlockClipRect();
	}

	public void RefreshPickableBlockClipRect()
	{
		Vector4 screenSpaceRect = GetScreenSpaceRect(contentMask.rectTransform);
		Vector4 screenSpaceRect2 = GetScreenSpaceRect(PickableBlockClampRect);
		Vector4 value = IntersectRects(screenSpaceRect, screenSpaceRect2);
		PickableBlockSpriteMaterial.SetVector("_ClipRect", value);
		UpblowerParticleMaterial.SetVector("_ClipRect", value);
	}

	private Vector4 IntersectRects(Vector4 rect0, Vector4 rect1)
	{
		float x = Mathf.Max(rect0.x, rect1.x);
		float y = Mathf.Min(rect0.y, rect1.y);
		float z = Mathf.Min(rect0.z, rect1.z);
		float w = Mathf.Max(rect0.w, rect1.w);
		return new Vector4(x, y, z, w);
	}

	public static Vector4 GetScreenSpaceRect(RectTransform transform)
	{
		Vector2 vector = Vector2.Scale(transform.rect.size, transform.lossyScale);
		float num = transform.position.x - vector.x * (1f - transform.pivot.x);
		float num2 = transform.position.y - vector.y * (1f - transform.pivot.y);
		return new Vector4(num, num2, num + vector.x, num2 + vector.y);
	}

	public GameObject GetTopPointedObject(PickCursor cursor)
	{
		PointerEventData pointerEventData = new PointerEventData(eventSystem);
		pointerEventData.position = canvas.worldCamera.WorldToScreenPoint(cursor.cursorPoint.position);
		List<RaycastResult> list = raycastResultCache;
		list.Clear();
		eventSystem.RaycastAll(pointerEventData, list);
		if (list.Count > 0)
		{
			TabletClickableLayer componentInParent = list[0].gameObject.GetComponentInParent<TabletClickableLayer>();
			if (componentInParent != null)
			{
				if (componentInParent == presetLayer)
				{
					if (presetOverlay.isOpen)
					{
						return list[0].gameObject;
					}
					return null;
				}
				if (componentInParent == modalLayer)
				{
					if (modalOverlay.IsOpen)
					{
						return list[0].gameObject;
					}
					return null;
				}
				if (tabletSidebar.IsOpen)
				{
					if (componentInParent == sidebarLayer)
					{
						return list[0].gameObject;
					}
					return null;
				}
				if (componentInParent == mainLayer)
				{
					return list[0].gameObject;
				}
			}
		}
		return null;
	}

	private void UpdateHoverEffects()
	{
		foreach (KeyValuePair<PickCursor, CursorInfo> trackedCursor in trackedCursors)
		{
			if (!trackedCursor.Key.Enabled)
			{
				continue;
			}
			GameObject lastHoverTarget = trackedCursor.Value.lastHoverTarget;
			GameObject topPointedObject = GetTopPointedObject(trackedCursor.Key);
			if (!(lastHoverTarget != topPointedObject))
			{
				continue;
			}
			if (topPointedObject != null)
			{
				TabletClickable componentInParent = topPointedObject.GetComponentInParent<TabletClickable>();
				componentInParent?.AddTrackedCursor(trackedCursor.Key);
				if (!hoverCounts.ContainsKey(topPointedObject))
				{
					hoverCounts[topPointedObject] = 1;
					componentInParent?.OnCursorOver();
				}
				else
				{
					hoverCounts[topPointedObject]++;
				}
			}
			if (lastHoverTarget != null)
			{
				lastHoverTarget.GetComponentInParent<TabletClickable>()?.RemoveTrackedCursor(trackedCursor.Key);
				if (hoverCounts.ContainsKey(lastHoverTarget))
				{
					hoverCounts[lastHoverTarget]--;
				}
			}
			trackedCursor.Value.lastHoverTarget = topPointedObject;
		}
		objectsToDelete.Clear();
		foreach (KeyValuePair<GameObject, int> hoverCount in hoverCounts)
		{
			if (hoverCount.Value <= 0)
			{
				if (objectsToDelete == null)
				{
					objectsToDelete = new HashSet<GameObject>();
				}
				objectsToDelete.Add(hoverCount.Key);
				hoverCount.Key.GetComponentInParent<TabletClickable>()?.OnCursorOut();
			}
		}
		if (objectsToDelete == null)
		{
			return;
		}
		foreach (GameObject item in objectsToDelete)
		{
			hoverCounts.Remove(item);
		}
	}

	private void ClearHoverEffects()
	{
		foreach (KeyValuePair<GameObject, int> hoverCount in hoverCounts)
		{
			hoverCount.Key.GetComponentInParent<TabletClickable>()?.OnCursorOut();
		}
		hoverCounts.Clear();
		trackedCursors.Clear();
	}

	private void UpdateTrackedCursors()
	{
		foreach (PickCursor cursor in book.cursors)
		{
			if (cursor != null && !trackedCursors.ContainsKey(cursor))
			{
				TrackCursor(cursor);
			}
		}
		untrackedCursors.Clear();
		foreach (KeyValuePair<PickCursor, CursorInfo> trackedCursor in trackedCursors)
		{
			if (!book.cursors.Contains(trackedCursor.Key))
			{
				untrackedCursors.Add(trackedCursor.Key);
			}
		}
		if (untrackedCursors.Count <= 0)
		{
			return;
		}
		foreach (PickCursor untrackedCursor in untrackedCursors)
		{
			UntrackCursor(untrackedCursor);
		}
	}

	private void TrackCursor(PickCursor cursor)
	{
		trackedCursors.Add(cursor, new CursorInfo());
	}

	private void UntrackCursor(PickCursor cursor)
	{
		trackedCursors.Remove(cursor);
	}

	public void ResetStyles()
	{
		foreach (TabletScreen tabletScreen in tabletScreens)
		{
			if (tabletScreen.colorScheme == null)
			{
				tabletScreen.colorScheme = defaultScheme;
			}
			tabletScreen.ResetStyles();
		}
		tabletSidebar.ResetStyles();
	}

	public void OpenBurgerMenu(PickCursor cursor)
	{
		tabletSidebar.Open();
	}

	public void OnBackMenu(PickCursor cursor)
	{
		OnPressBack(cursor);
	}

	public void GotoScreen(TabletScreen screen)
	{
		if (currentScreen != screen)
		{
			AkSoundEngine.PostEvent("UI_UPad_To_Broadmenu", base.gameObject);
			TransitionTo(screen);
		}
		tabletSidebar.Close();
	}

	public static bool PropagateClick(PickCursor cursor)
	{
		if (clickEventReceiver != null)
		{
			return clickEventReceiver.OnClick(cursor);
		}
		return false;
	}

	public bool OnClick(PickCursor cursor)
	{
		GameObject topPointedObject = GetTopPointedObject(cursor);
		if (Controller.InputFieldIsActive)
		{
			if (topPointedObject != null)
			{
				InputField component = topPointedObject.GetComponent<InputField>();
				if (component != null && Controller.lockedInputField == component)
				{
					return true;
				}
			}
			else
			{
				Controller.UnlockInputField();
			}
		}
		if (topPointedObject != null)
		{
			TabletClickable componentInParent = topPointedObject.GetComponentInParent<TabletClickable>();
			if (componentInParent != null)
			{
				if (componentInParent.Disabled)
				{
					return false;
				}
				if (!componentInParent.Interactable)
				{
					return false;
				}
				componentInParent.OnAccept(cursor);
				return true;
			}
		}
		return false;
	}

	public bool OnPressBack(PickCursor cursor)
	{
		if (modalOverlay.IsOpen || modalOverlay.IsOpening)
		{
			modalOverlay.OnCancel();
			return true;
		}
		if (currentScreen.OnPressBack(cursor))
		{
			return true;
		}
		if (!tabletSidebar.IsOpen)
		{
			if (currentScreen == emptyScreen)
			{
				lastBackedScreen = null;
			}
			else
			{
				lastBackedScreen = currentScreen;
			}
			AkSoundEngine.PostEvent("UI_UPad_Back_Broadmenu", base.gameObject);
			TransitionTo(emptyScreen);
			OpenBurgerMenu(null);
			return true;
		}
		ClearHoverEffects();
		return false;
	}

	public void OpenLegacyRuleScreen(PickCursor pickCursor)
	{
		LevelSelectController.lastInstance.OpenLegacyRulebook(gotoRules: true);
	}

	public static void ActivateInputField(PickCursor pickCursor, InputField inputField, string imePromptText, UnityAction<string> OnEndEdit)
	{
		Controller.LockInputField(inputField, OnEndEdit);
		SteamDeck.OpenVirtualKeyboard(pickCursor);
	}

	public bool OnRotateLeft(PickCursor pickCursor)
	{
		if (currentScreen.OnRotateLeft(pickCursor))
		{
			return true;
		}
		if (!pickCursor.lastRotateWasMouseWheel)
		{
			return OnPressBack(pickCursor);
		}
		return false;
	}

	public bool OnRotateRight(PickCursor pickCursor)
	{
		if (currentScreen.OnRotateRight(pickCursor))
		{
			return true;
		}
		if (!pickCursor.lastRotateWasMouseWheel && tabletSidebar.IsOpen && lastBackedScreen != null)
		{
			GotoScreen(lastBackedScreen);
			return true;
		}
		return false;
	}

	public void OpenQuitMenu(PickCursor pickCursor)
	{
		GotoScreen(quitScreen);
		if (SceneManager.GetActiveScene().name == "MainMenu")
		{
			quitScreen.Initialize(TabletQuitScreen.DisplayMode.MainMenu);
		}
		else if (LobbyManager.instance.CurrentLevelSelectController != null)
		{
			quitScreen.Initialize(TabletQuitScreen.DisplayMode.Treehouse);
		}
		else if (LobbyManager.instance.IsHost)
		{
			quitScreen.Initialize(TabletQuitScreen.DisplayMode.InGameHost);
		}
		else
		{
			quitScreen.Initialize(TabletQuitScreen.DisplayMode.InGameClient);
		}
	}

	public void AccumulateCursorScroll(PickCursor pickCursor, float xScroll, float yScroll)
	{
		Vector2 value = Vector2.zero;
		scrollImpulses.TryGetValue(pickCursor, out value);
		value.x += xScroll * Time.deltaTime;
		value.y += yScroll * Time.deltaTime;
		scrollImpulses[pickCursor] = value;
	}

	public void ApplyCursorScroll()
	{
		Vector2 zero = Vector2.zero;
		foreach (KeyValuePair<PickCursor, Vector2> scrollImpulse in scrollImpulses)
		{
			zero += scrollImpulse.Value;
		}
		scrollImpulses.Clear();
		if (zero.x != 0f && zero.y != 0f)
		{
			currentScreen.OnCursorScroll(zero);
		}
	}

	public void GotoHelpPage(int pageIndex)
	{
		foreach (TabletScreen tabletScreen in tabletScreens)
		{
			if (tabletScreen is TabletHelpScreen tabletHelpScreen)
			{
				tabletHelpScreen.JumpToHelpPage(pageIndex);
				if (currentScreen != tabletHelpScreen)
				{
					AkSoundEngine.PostEvent("UI_UPad_To_Broadmenu", base.gameObject);
					TransitionTo(tabletHelpScreen);
				}
				tabletSidebar.Close();
				break;
			}
		}
	}
}
