using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using GameEvent;
using GameSparks.Core;
using I2.Loc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UndergroundComputer : MonoBehaviour, IGameEventListener, InputReceiver
{
	[Serializable]
	public class LevelSignSprite
	{
		public GameState.LevelName level;

		public Sprite sprite;
	}

	public class FeaturedLevelData
	{
		public string name;

		public string authorName;

		public string authorId;

		public string authorId_old;

		public Dictionary<string, string> authorPlatformIds;

		public string code;

		public int playCount;

		public int getCount;

		public long timestamp;

		public int myVote;

		public int rating;

		public bool isPublished;

		public FeaturedQuickFilter.LevelTypes levelType;

		public int attemptedPlayers;

		public int successfulPlayers;

		public float completionRate;

		public int attempts;

		public int successes;

		public bool archived;

		public bool hasMods;

		public bool isLocal;

		public int numReports;

		public int approvalStatus;

		public bool hasNewReports;

		public bool ignoreReports;

		public string DifficultyString
		{
			get
			{
				if (attempts == 0)
				{
					return "-";
				}
				if (completionRate < GameSettings.GetInstance().hardCompletionRate)
				{
					return LocalizationManager.GetTranslation("UndergroundComputer/Difficulty/Hard");
				}
				if (completionRate < GameSettings.GetInstance().mediumCompletionRate)
				{
					return LocalizationManager.GetTranslation("UndergroundComputer/Difficulty/Medium");
				}
				return LocalizationManager.GetTranslation("UndergroundComputer/Difficulty/Easy");
			}
		}

		public int CompletionPercentage => Mathf.CeilToInt(completionRate * 100f);

		public float SuccessFailureRate => (float)successes / (float)attempts;

		public int SuccessFailurePercentage => Mathf.CeilToInt(SuccessFailureRate * 100f);

		public FeaturedLevelData(GSData record = null)
		{
			if (record != null)
			{
				FillFromGSDataRecord(record);
			}
		}

		public void FillFromGSDataRecord(GSData record)
		{
			name = record.GetString("name");
			authorName = record.GetString("authorDisplayName");
			authorId = record.GetString("author");
			authorId_old = record.GetString("gsPlayerId");
			GSData gSData = record.GetGSData("authorPlatformIds");
			if (gSData != null)
			{
				authorPlatformIds = new Dictionary<string, string>();
				foreach (string key in gSData.BaseData.Keys)
				{
					authorPlatformIds.Add(key, gSData.GetString(key));
				}
			}
			code = record.GetString("code");
			playCount = record.GetInt("playCount") ?? 0;
			getCount = record.GetInt("getCount") ?? 0;
			timestamp = record.GetLong("date") ?? 0;
			myVote = record.GetInt("myVote") ?? 0;
			rating = record.GetInt("rating") ?? 1;
			isPublished = (record.GetInt("published") ?? 0) == 1;
			archived = record.GetBoolean("archived") ?? false;
			hasMods = (record.GetInt("hasMods") ?? 0) == 1;
			if (GameSparksManager.Instance.MainUserIsAdmin)
			{
				numReports = record.GetInt("reports") ?? 0;
				approvalStatus = record.GetInt("approvalStatus") ?? 0;
				hasNewReports = record.GetBoolean("hasNewReports") == true;
				ignoreReports = record.GetBoolean("ignoreReports") == true;
			}
			string text = record.GetString("levelType");
			if (text == "Versus")
			{
				levelType = FeaturedQuickFilter.LevelTypes.Versus;
			}
			else if (text == "Challenge")
			{
				levelType = FeaturedQuickFilter.LevelTypes.Challenge;
			}
			else
			{
				levelType = FeaturedQuickFilter.LevelTypes.Any;
			}
			attemptedPlayers = record.GetInt("attemptedPlayers") ?? 0;
			successfulPlayers = record.GetInt("successfulPlayers") ?? 0;
			completionRate = record.GetFloat("successRate") ?? 0f;
			successes = record.GetInt("successes") ?? 0;
			attempts = record.GetInt("attempts") ?? 0;
		}
	}

	[Serializable]
	public class DisplaySlotPageDef
	{
		public PickableBuildButton.BuildScreenStates buildPage;

		public DisplaySlotDef listViewSlotDef;

		public DisplaySlotDef gridViewSlotDef;

		public Transform[] GetCurrentEntrySlots()
		{
			return currentViewMode switch
			{
				ViewModes.Grid => gridViewSlotDef.slots, 
				ViewModes.List => listViewSlotDef.slots, 
				_ => null, 
			};
		}

		public UnityEngine.Object GetCurrentEntryPrefab()
		{
			return currentViewMode switch
			{
				ViewModes.Grid => gridViewSlotDef.entryPrefab, 
				ViewModes.List => listViewSlotDef.entryPrefab, 
				_ => null, 
			};
		}
	}

	[Serializable]
	public class DisplaySlotDef
	{
		public UnityEngine.Object entryPrefab;

		public Transform slotContainer;

		public Transform[] slots;
	}

	public enum ViewModes
	{
		Grid,
		List
	}

	public class Breadcrumbs
	{
		public PickableBuildButton.BuildScreenStates selectedTab;

		public int mainDropdownIndex;

		public int dateCutoffDropdownIndex;

		public int difficultyDropdownIndex;

		public int levelTypeDropdownIndex;

		public int pageNumber;

		public bool showingInfoPane;

		public string levelCode;

		public string localFilename;

		public int leaderboardNumPlayers = -1;

		public UserInfoPopup.UserInfo playerInfo;

		public int showMods;

		public string GetDebugInfo()
		{
			return "Selected Tab: " + selectedTab.ToString() + ", Main Dropdown: " + mainDropdownIndex + ", Date Cutoff: " + dateCutoffDropdownIndex + ", Difficulty: " + difficultyDropdownIndex + ", Level Type: " + levelTypeDropdownIndex + ", Page: " + pageNumber + ", Info Pane: " + (showingInfoPane ? "Y" : "N") + ", Level Code: " + levelCode + ", Local Filename: " + localFilename + ", Leader Board Players: " + leaderboardNumPlayers + ", Player info: " + ((playerInfo != null) ? "Y" : "N");
		}
	}

	public enum FeaturedLevelTopPanelModes
	{
		QuickFilters,
		AdvancedSearch,
		PlayerLevels,
		MyLevels
	}

	public LevelSignSprite[] levelSignSprites;

	public DisplaySlotPageDef[] displaySlotDefs;

	private Dictionary<PickableBuildButton.BuildScreenStates, DisplaySlotPageDef> perPageDisplaySlotDefs;

	public static ViewModes currentViewMode;

	public static Breadcrumbs lastComputerState;

	private List<FeaturedLevelData> featuredLevelEntries = new List<FeaturedLevelData>();

	private int totalFeaturedEntries;

	private int firstFeaturedEntryIndex;

	private int autoSelectIndexOnRefresh = -1;

	private string autoSelectCodeOnRefresh;

	private string autoSelectLevelNameOnRefresh;

	private UserInfoPopup.UserInfo lastDisplayedUserInfo;

	private List<string> localSaveFilenames = new List<string>();

	private Dictionary<PickableBuildButton.BuildScreenStates, int> currentPageNumbers;

	private Dictionary<PickableBuildButton.BuildScreenStates, int> waitingForFileOperation;

	private bool refreshingLocalSavesList;

	public PickableBuildButton codeEntryField;

	public PickableBuildButton codeStatusText;

	public PickableBuildButton codeStatusCheckmark;

	public PickableBuildButton codeArchivedText;

	public PickableBuildButton codeSaveLocalCopy;

	[HideInInspector]
	public bool currentCodeValidated;

	[HideInInspector]
	public bool currentCodeArchived;

	private string currentCodeAssociatedXml;

	private string currentCodeAssociatedName;

	private CustomLevelPortal.AuthorInfo currentCodeAuthorInfo;

	private FeaturedQuickFilter.LevelTypes currentCodeLevelType = FeaturedQuickFilter.LevelTypes.Any;

	private GameSparksQuery currentQuery;

	public PickableBuildButton nextButton;

	public PickableBuildButton prevButton;

	public Text pageIndicator;

	public CustomLevelPortal[] slotPortals;

	public Transform ComputerSlotsContainer;

	public PickableBuildButton[] ComputerSlots;

	public Transform tabContainer;

	private PickableBuildButton[] tabs;

	public SpriteRenderer spinnyLoadingThing;

	private int loadingOperations;

	private bool computerDisabled;

	public PlaceableMetadataList metadataList;

	public FeaturedLevelTopPanelModes topPanelMode;

	public Transform mainRowContainer;

	public Transform mainFilterContainer;

	public Transform playerLevelsContainer;

	public FeaturedQuickInfoPane quickInfoPane;

	public bool featuredQuickInfoEnabled;

	public FeaturedQuickFilter.LevelTypes currentLevelType = FeaturedQuickFilter.LevelTypes.Challenge;

	public FeaturedQuickFilter myLevelsQuickFilter;

	public FeaturedQuickFilter.SortingFilter currentFilter;

	private FeaturedQuickFilter.SortingFilter previousFilter;

	private int previousPage = -1;

	private FeaturedLevelTopPanelModes previousPanelMode;

	public UGCNameTag playerLevelsNameTag;

	public PickableBuildButton.BuildScreenStates previousTab;

	public PickableBuildButton previousSelectedEntry;

	public ShareDialog shareDialog;

	public ReportDialog reportDialog;

	public ViewReportsDialog viewReportsDialog;

	public AdminPanelDialog adminPanelDialog;

	public GenericButton adminPanelButton;

	public GameObject adminPanelContainer;

	public UnityEngine.Object adminPanelPrefab;

	public Transform adminFlaggedLevelControls;

	public DropdownMenu adminApprovalStatusDropdown;

	public GenericButton adminHideAcknowledgedButton;

	public Image adminHideAcknowledgedCheckmark;

	public bool adminHideAcknowledgedReports;

	public int cutoffDays;

	public float lowerDifficultyBound;

	public float upperDifficultyBound = 1f;

	public bool allowUnpublished;

	public Image allowUnpublishedAdminText;

	public int showMods;

	public PickableBuildButton showModsButton;

	public Text showModsTipText;

	public Image showModsOverlayYesImage;

	public Image showModsOverlayNoImage;

	public Image showPublishedYesImage;

	public DropdownMenu mainFilterDropdown;

	public DropdownMenu levelTypeDropdown;

	public DropdownMenu dateCutoffDropdown;

	public DropdownMenu difficultyDropdown;

	public List<string> featuredLevelCodes;

	public PickableBuildButton advancedSearchBox;

	public PickableBuildButton advancedSearchButton;

	private string lastFilteredSearchQuery;

	private string lastFilteredSearchQueryResult;

	private bool neverOpened = true;

	private Dictionary<Controller, float[]> rightStickValues = new Dictionary<Controller, float[]>();

	public Transform featuredContentRect;

	public Transform featuredRestrictedMessageRect;

	public Transform featuredAccessRestrictedMessage;

	public Transform featuredNotConnectedMessage;

	public Transform featuredNotConnectedRetryButton;

	public Transform featuredAccessRestrictedAllUGCMessage;

	public bool hiding_online_content;

	public UnityEngine.Object userInfoPopupPrefab;

	private UserInfoPopup userInfoPopup;

	public DeleteDialog deleteDialogue;

	public static long lastRefreshTimestamp;

	public static GameState.GameMode quickPlayMode;

	public bool CurrentlyLoading => loadingOperations > 0;

	public bool WaitingForFileOperationOnCurrentPage => waitingForFileOperation[PickableBuildButton.buildMenuCurrentState] > 0;

	public bool ShowingEmptyPage
	{
		get
		{
			if (currentFilter != null)
			{
				switch (currentFilter.filterType)
				{
				case FeaturedQuickFilter.FilterTypes.Local:
					if (!WaitingForFileOperationOnCurrentPage)
					{
						return localSaveFilenames.Count == 0;
					}
					return false;
				case FeaturedQuickFilter.FilterTypes.Recent:
					return StatTracker.Instance.GetSaveFileDataForMainUser().recentSnapshotEntries.Count == 0;
				case FeaturedQuickFilter.FilterTypes.Favourites:
					return StatTracker.Instance.GetSaveFileDataForMainUser().favoriteSnapshots.Count == 0;
				case FeaturedQuickFilter.FilterTypes.Featured:
				case FeaturedQuickFilter.FilterTypes.Trending:
				case FeaturedQuickFilter.FilterTypes.Sorted:
					if (!WaitingForFileOperationOnCurrentPage)
					{
						return featuredLevelEntries.Count == 0;
					}
					return false;
				}
			}
			return false;
		}
	}

	public int NumPages
	{
		get
		{
			if (currentFilter != null)
			{
				DisplaySlotPageDef currentSlotPageDef = GetCurrentSlotPageDef();
				if (currentSlotPageDef == null)
				{
					return 0;
				}
				Transform[] currentEntrySlots = currentSlotPageDef.GetCurrentEntrySlots();
				switch (currentFilter.filterType)
				{
				case FeaturedQuickFilter.FilterTypes.Local:
					if (localSaveFilenames.Count == 0)
					{
						return 0;
					}
					return Mathf.CeilToInt((float)localSaveFilenames.Count / (float)currentEntrySlots.Length);
				case FeaturedQuickFilter.FilterTypes.Recent:
					if (StatTracker.Instance.GetSaveFileDataForMainUser().recentSnapshotEntries.Count == 0)
					{
						return 0;
					}
					return Mathf.CeilToInt((float)StatTracker.Instance.GetSaveFileDataForMainUser().recentSnapshotEntries.Count / (float)currentEntrySlots.Length);
				case FeaturedQuickFilter.FilterTypes.Favourites:
					if (StatTracker.Instance.GetSaveFileDataForMainUser().favoriteSnapshots.Count == 0)
					{
						return 0;
					}
					return Mathf.CeilToInt((float)StatTracker.Instance.GetSaveFileDataForMainUser().favoriteSnapshots.Count / (float)currentEntrySlots.Length);
				case FeaturedQuickFilter.FilterTypes.Featured:
				case FeaturedQuickFilter.FilterTypes.Trending:
				case FeaturedQuickFilter.FilterTypes.Sorted:
					if (totalFeaturedEntries == 0)
					{
						return 0;
					}
					return Mathf.CeilToInt((float)totalFeaturedEntries / (float)currentEntrySlots.Length);
				default:
					return 0;
				}
			}
			return 0;
		}
	}

	public bool ShouldShowPrev
	{
		get
		{
			PickableBuildButton.BuildScreenStates buildMenuCurrentState = PickableBuildButton.buildMenuCurrentState;
			if ((uint)(buildMenuCurrentState - 6) <= 2u || buildMenuCurrentState == PickableBuildButton.BuildScreenStates.DeleteDialog)
			{
				return false;
			}
			if (PlatformFeatureRestrictions.MustHideAllUGC)
			{
				return false;
			}
			if ((currentFilter == null || currentFilter.filterType != FeaturedQuickFilter.FilterTypes.Local) && PlatformFeatureRestrictions.HideOnlineContent)
			{
				return false;
			}
			if (WaitingForFileOperationOnCurrentPage)
			{
				return false;
			}
			if (featuredQuickInfoEnabled)
			{
				if (PickableBuildButton.SelectedEntry != null)
				{
					SnapshotEntry component = PickableBuildButton.SelectedEntry.GetComponent<SnapshotEntry>();
					if (component != null && firstFeaturedEntryIndex + component.indexOnCurrentPage > 0)
					{
						return true;
					}
				}
				return false;
			}
			if (NumPages > 0)
			{
				return CurrentPage != 0;
			}
			return false;
		}
	}

	public bool ShouldShowNext
	{
		get
		{
			PickableBuildButton.BuildScreenStates buildMenuCurrentState = PickableBuildButton.buildMenuCurrentState;
			if ((uint)(buildMenuCurrentState - 6) <= 2u || buildMenuCurrentState == PickableBuildButton.BuildScreenStates.DeleteDialog)
			{
				return false;
			}
			if (PlatformFeatureRestrictions.MustHideAllUGC)
			{
				return false;
			}
			if ((currentFilter == null || currentFilter.filterType != FeaturedQuickFilter.FilterTypes.Local) && PlatformFeatureRestrictions.HideOnlineContent)
			{
				return false;
			}
			if (WaitingForFileOperationOnCurrentPage)
			{
				return false;
			}
			if (featuredQuickInfoEnabled)
			{
				if (PickableBuildButton.SelectedEntry != null)
				{
					SnapshotEntry component = PickableBuildButton.SelectedEntry.GetComponent<SnapshotEntry>();
					if (component != null && component.indexOnCurrentPage != -1 && firstFeaturedEntryIndex + component.indexOnCurrentPage < totalFeaturedEntries - 1)
					{
						return true;
					}
				}
				return false;
			}
			int numPages = NumPages;
			if (numPages > 0)
			{
				return CurrentPage != numPages - 1;
			}
			return false;
		}
	}

	public bool ShouldShowPageNumber
	{
		get
		{
			PickableBuildButton.BuildScreenStates buildMenuCurrentState = PickableBuildButton.buildMenuCurrentState;
			if ((uint)(buildMenuCurrentState - 6) <= 2u)
			{
				return false;
			}
			if (PlatformFeatureRestrictions.MustHideAllUGC)
			{
				return false;
			}
			if ((currentFilter == null || currentFilter.filterType != FeaturedQuickFilter.FilterTypes.Local) && PlatformFeatureRestrictions.HideOnlineContent)
			{
				return false;
			}
			if (WaitingForFileOperationOnCurrentPage)
			{
				return false;
			}
			if (featuredQuickInfoEnabled)
			{
				if (PickableBuildButton.SelectedEntry != null)
				{
					SnapshotEntry component = PickableBuildButton.SelectedEntry.GetComponent<SnapshotEntry>();
					if (component != null)
					{
						return component.indexOnCurrentPage != -1;
					}
					return false;
				}
				return false;
			}
			return NumPages > 1;
		}
	}

	public int CurrentPage
	{
		get
		{
			return currentPageNumbers[PickableBuildButton.buildMenuCurrentState];
		}
		set
		{
			currentPageNumbers[PickableBuildButton.buildMenuCurrentState] = value;
		}
	}

	public bool ShouldShowPublishedMark
	{
		get
		{
			bool mainUserIsAdmin = GameSparksManager.Instance.MainUserIsAdmin;
			bool num = !currentFilter.restrictToUserId.NullOrEmpty() && GameSparksManager.Instance.MainUserGSID == currentFilter.restrictToUserId;
			bool flag = currentFilter.filterType == FeaturedQuickFilter.FilterTypes.Favourites || currentFilter.filterType == FeaturedQuickFilter.FilterTypes.Recent;
			bool flag2 = currentFilter.allowUnpublished;
			if (!(num || flag))
			{
				return mainUserIsAdmin && flag2;
			}
			return true;
		}
	}

	public bool IsInSubmenu
	{
		get
		{
			if (!featuredQuickInfoEnabled && lastDisplayedUserInfo == null && PickableBuildButton.buildMenuCurrentState != PickableBuildButton.BuildScreenStates.ShareDialog && PickableBuildButton.buildMenuCurrentState != PickableBuildButton.BuildScreenStates.ReportDialog && PickableBuildButton.buildMenuCurrentState != PickableBuildButton.BuildScreenStates.ViewReportsDialog)
			{
				return PickableBuildButton.buildMenuCurrentState == PickableBuildButton.BuildScreenStates.AdminPanelDialog;
			}
			return true;
		}
	}

	private void Awake()
	{
		Controller.AddGlobalReceiver(this);
		ChangeListener(adding: true);
	}

	public void Initialize()
	{
		PickableBuildButton.undergroundComputer = this;
		if (!ControllerMonitor.Instance.IsMainControllerSet || !GameSparksManager.Instance.MainUserIsAdmin)
		{
			adminPanelButton.gameObject.SetActive(value: false);
			adminPanelButton.OnClick.RemoveAllListeners();
			adminPanelButton.OnClickWithCursor.RemoveAllListeners();
			adminPanelButton = null;
			adminPanelDialog = null;
			adminFlaggedLevelControls.gameObject.SetActive(value: false);
		}
		else
		{
			adminPanelDialog = adminPanelContainer.AddPrefabAsChild<AdminPanelDialog>(adminPanelPrefab);
			GetComponent<InventoryPage>().AddInstantiatedElements(adminPanelDialog.transform);
			adminPanelDialog.ResetDialog();
		}
		tabs = new PickableBuildButton[tabContainer.childCount];
		int num = 0;
		foreach (Transform item in tabContainer)
		{
			tabs[num++] = item.GetComponent<PickableBuildButton>();
		}
		waitingForFileOperation = new Dictionary<PickableBuildButton.BuildScreenStates, int>();
		currentPageNumbers = new Dictionary<PickableBuildButton.BuildScreenStates, int>();
		foreach (PickableBuildButton.BuildScreenStates value in Enum.GetValues(typeof(PickableBuildButton.BuildScreenStates)))
		{
			waitingForFileOperation.Add(value, 0);
			currentPageNumbers.Add(value, 0);
		}
		for (int i = 0; i < slotPortals.Length; i++)
		{
			CustomLevelPortal obj = slotPortals[i];
			obj.SetPortalIndex(i);
			obj.ClearContents();
		}
		for (int j = 0; j < ComputerSlots.Length; j++)
		{
			ComputerSlots[j].ClearComputerSlotContents();
		}
		perPageDisplaySlotDefs = new Dictionary<PickableBuildButton.BuildScreenStates, DisplaySlotPageDef>();
		DisplaySlotPageDef[] array = displaySlotDefs;
		foreach (DisplaySlotPageDef displaySlotPageDef in array)
		{
			perPageDisplaySlotDefs[displaySlotPageDef.buildPage] = displaySlotPageDef;
			if (displaySlotPageDef.listViewSlotDef.slotContainer != null)
			{
				AutoPopulateSlots(displaySlotPageDef.listViewSlotDef.slotContainer, ref displaySlotPageDef.listViewSlotDef.slots);
			}
			if (displaySlotPageDef.gridViewSlotDef.slotContainer != null)
			{
				AutoPopulateSlots(displaySlotPageDef.gridViewSlotDef.slotContainer, ref displaySlotPageDef.gridViewSlotDef.slots);
			}
		}
		ResetCodeEntryStatus();
		spinnyLoadingThing.enabled = false;
		playerLevelsNameTag.UpdateIcons();
		if (difficultyDropdown.TryGetEntryByIndex(1, out var entry))
		{
			FeaturedDifficultyDropdownData component = entry.GetComponent<FeaturedDifficultyDropdownData>();
			if (component != null)
			{
				component.lowerLimit = GameSettings.GetInstance().mediumCompletionRate;
				component.upperLimit = 1f;
			}
		}
		if (difficultyDropdown.TryGetEntryByIndex(2, out entry))
		{
			FeaturedDifficultyDropdownData component2 = entry.GetComponent<FeaturedDifficultyDropdownData>();
			if (component2 != null)
			{
				component2.lowerLimit = GameSettings.GetInstance().hardCompletionRate;
				component2.upperLimit = GameSettings.GetInstance().mediumCompletionRate;
			}
		}
		if (difficultyDropdown.TryGetEntryByIndex(3, out entry))
		{
			FeaturedDifficultyDropdownData component3 = entry.GetComponent<FeaturedDifficultyDropdownData>();
			if (component3 != null)
			{
				component3.lowerLimit = 0f;
				component3.upperLimit = GameSettings.GetInstance().hardCompletionRate;
			}
		}
	}

	private void OnFirstOpened()
	{
		if (!LobbyManager.instance.IsHost)
		{
			return;
		}
		if (GameSparksManager.Instance.MainUserIsAdmin)
		{
			quickInfoPane.RefreshAdminBatchList();
		}
		foreach (DropdownEntry dropdownEntry in mainFilterDropdown.dropdownEntries)
		{
			FeaturedAdminFilter component = dropdownEntry.GetComponent<FeaturedAdminFilter>();
			if (component != null && component.minimumPermissionLevel > GameSparksManager.Instance.MainUserPermissionLevel)
			{
				dropdownEntry.gameObject.SetActive(value: false);
			}
		}
		if (adminFlaggedLevelControls != null)
		{
			adminFlaggedLevelControls.gameObject.SetActive(value: false);
		}
		SetTopPanelMode(FeaturedLevelTopPanelModes.QuickFilters);
		if (lastComputerState == null)
		{
			if (PlatformFeatureRestrictions.IsNotConnected || PlatformFeatureRestrictions.MustHideAllUGC || PlatformFeatureRestrictions.IsUGCRestricted)
			{
				OpenSpecialFilterPage(FeaturedSpecialFilter.SpecialFilterType.Local);
			}
			else
			{
				OnSelectFeaturedLevelTab(refreshCurrentPage: true, resetFilters: true);
			}
		}
		else
		{
			Breadcrumbs breadcrumbs = lastComputerState;
			lastComputerState = null;
			switch (breadcrumbs.selectedTab)
			{
			case PickableBuildButton.BuildScreenStates.FeaturedLevelTab:
				difficultyDropdown.SelectEntryByIndex(breadcrumbs.difficultyDropdownIndex, triggerOnChangeEvent: false);
				UpdateDifficultyFromDropdownValues();
				dateCutoffDropdown.SelectEntryByIndex(breadcrumbs.dateCutoffDropdownIndex, triggerOnChangeEvent: false);
				UpdateDateCutoffFromDropdownValues();
				levelTypeDropdown.SelectEntryByIndex(breadcrumbs.levelTypeDropdownIndex, triggerOnChangeEvent: false);
				UpdateLevelTypeFromDropdownValues(refreshPage: false);
				mainFilterDropdown.SelectEntryByIndex(breadcrumbs.mainDropdownIndex, triggerOnChangeEvent: false);
				autoSelectCodeOnRefresh = breadcrumbs.levelCode;
				autoSelectLevelNameOnRefresh = breadcrumbs.localFilename;
				showMods = breadcrumbs.showMods;
				if (breadcrumbs.playerInfo != null)
				{
					UserInfoPopup.ShowLevelsForUser(this, breadcrumbs.playerInfo, breadcrumbs.pageNumber);
					previousFilter = null;
				}
				else
				{
					RefreshMainFilter(breadcrumbs.pageNumber);
				}
				break;
			case PickableBuildButton.BuildScreenStates.LevelCodesTab:
				OnSelectLevelCodesTab();
				if (!breadcrumbs.levelCode.NullOrEmpty())
				{
					OnCodeEntryFieldSubmitted(breadcrumbs.levelCode);
				}
				break;
			}
		}
		currentLevelType = levelTypeDropdown.selectedDropdownEntry.GetComponent<FeaturedLevelTypeDropdownData>().levelType;
		FeaturedDateCutoffDropdownData component2 = dateCutoffDropdown.selectedDropdownEntry.GetComponent<FeaturedDateCutoffDropdownData>();
		cutoffDays = ((component2 != null) ? component2.days : 0);
		SetFeaturedLevelMode(currentLevelType, refreshSearch: false);
		SetFeaturedViewMode(ViewModes.Grid, refreshSearch: false);
		SetAllowUnpublishedToggle(GameSparksManager.Instance.MainUserIsAdmin, refreshSearch: false);
		SetShowMods(showMods, refreshSearch: false);
	}

	private void OnReopened()
	{
		mainFilterDropdown.OnRefreshVisibility();
		levelTypeDropdown.OnRefreshVisibility();
		dateCutoffDropdown.OnRefreshVisibility();
		difficultyDropdown.OnRefreshVisibility();
	}

	private void AutoPopulateSlots(Transform slotContainer, ref Transform[] slotTransformArray)
	{
		List<Transform> list = new List<Transform>();
		foreach (Transform item in slotContainer)
		{
			list.Add(item);
		}
		slotTransformArray = list.ToArray();
	}

	private void Start()
	{
		computerDisabled = !LobbyManager.instance.IsHost;
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
		Controller.RemoveGlobalReceiver(this);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<PickCursorClickedBackgroundEvent>(this, adding);
		GameEventManager.ChangeListener<UndergroundComputerOpenedEvent>(this, adding);
		GameEventManager.ChangeListener<PlayerInGameRuleEvent>(this, adding);
	}

	private void UpdateVisibility()
	{
		bool isHost = LobbyManager.instance.IsHost;
		foreach (KeyValuePair<PickableBuildButton.BuildScreenStates, DisplaySlotPageDef> perPageDisplaySlotDef in perPageDisplaySlotDefs)
		{
			PickableBuildButton.BuildScreenStates key = perPageDisplaySlotDef.Key;
			DisplaySlotPageDef value = perPageDisplaySlotDef.Value;
			if (value.gridViewSlotDef.slotContainer != null)
			{
				value.gridViewSlotDef.slotContainer.gameObject.SetActive(!featuredQuickInfoEnabled && isHost && key == PickableBuildButton.buildMenuCurrentState && currentViewMode == ViewModes.Grid);
			}
			if (value.listViewSlotDef.slotContainer != null)
			{
				value.listViewSlotDef.slotContainer.gameObject.SetActive(!featuredQuickInfoEnabled && isHost && key == PickableBuildButton.buildMenuCurrentState && currentViewMode == ViewModes.List);
			}
		}
		if (isHost && PickableBuildButton.buildMenuCurrentState == PickableBuildButton.BuildScreenStates.FeaturedLevelTab)
		{
			mainRowContainer.gameObject.SetActive(value: true);
			mainFilterContainer.gameObject.SetActive(value: true);
			quickInfoPane.Show(featuredQuickInfoEnabled);
		}
		else
		{
			mainRowContainer.gameObject.SetActive(value: false);
			mainFilterContainer.gameObject.SetActive(value: false);
			quickInfoPane.Show(onOff: false);
		}
		if ((PickableBuildButton.buildMenuCurrentState == PickableBuildButton.BuildScreenStates.LevelCodesTab || currentFilter == null || currentFilter.filterType != FeaturedQuickFilter.FilterTypes.Local) && PlatformFeatureRestrictions.HideOnlineContent)
		{
			featuredContentRect.gameObject.SetActive(value: false);
			featuredRestrictedMessageRect.gameObject.SetActive(value: true);
			if (PlatformFeatureRestrictions.MustHideAllUGC)
			{
				featuredAccessRestrictedAllUGCMessage.gameObject.SetActive(value: true);
				featuredAccessRestrictedMessage.gameObject.SetActive(value: false);
				featuredNotConnectedMessage.gameObject.SetActive(value: false);
				featuredNotConnectedRetryButton.gameObject.SetActive(value: false);
			}
			else if (PlatformFeatureRestrictions.IsNotConnected)
			{
				featuredAccessRestrictedAllUGCMessage.gameObject.SetActive(value: false);
				featuredAccessRestrictedMessage.gameObject.SetActive(value: false);
				featuredNotConnectedMessage.gameObject.SetActive(value: true);
				featuredNotConnectedRetryButton.gameObject.SetActive(value: true);
				featuredNotConnectedRetryButton.GetComponent<PickableButton>().Enable();
			}
			else if (PlatformFeatureRestrictions.IsUGCRestricted)
			{
				featuredAccessRestrictedAllUGCMessage.gameObject.SetActive(value: false);
				featuredAccessRestrictedMessage.gameObject.SetActive(value: true);
				featuredNotConnectedMessage.gameObject.SetActive(value: false);
				featuredNotConnectedRetryButton.gameObject.SetActive(value: false);
			}
		}
		else if (PlatformFeatureRestrictions.MustHideAllUGC)
		{
			featuredContentRect.gameObject.SetActive(value: false);
			featuredRestrictedMessageRect.gameObject.SetActive(value: true);
			featuredAccessRestrictedAllUGCMessage.gameObject.SetActive(value: true);
			featuredAccessRestrictedMessage.gameObject.SetActive(value: false);
			featuredNotConnectedMessage.gameObject.SetActive(value: false);
			featuredNotConnectedRetryButton.gameObject.SetActive(value: false);
		}
		else
		{
			featuredRestrictedMessageRect.gameObject.SetActive(value: false);
			featuredContentRect.gameObject.SetActive(value: true);
		}
		prevButton.Enable(ShouldShowPrev);
		nextButton.Enable(ShouldShowNext);
		UpdatePageIndicator();
	}

	private void RefreshLocalSavesList(bool keepPage, UnityAction onFinish)
	{
		if (refreshingLocalSavesList)
		{
			return;
		}
		QuickSaver.CheckSaveFolders();
		_ = QuickSaver.LocalSavesFolder;
		PickableButton[] buttons = tabs;
		PickableButton.AllowOnlyButtons(buttons);
		AddWaitingForFileOperation(PickableBuildButton.buildMenuCurrentState);
		AddLoadingOperation();
		refreshingLocalSavesList = true;
		ClearFeaturedLevelPage();
		string extensionFilter = ".snapshot";
		switch (currentFilter.levelType)
		{
		case FeaturedQuickFilter.LevelTypes.Challenge:
			extensionFilter = ".c.snapshot";
			break;
		case FeaturedQuickFilter.LevelTypes.Versus:
			extensionFilter = ".v.snapshot";
			break;
		}
		QuickSaver.GetLocalSaveFilenamesWithoutExtensions(extensionFilter, delegate(IEnumerable<string> filenamesList)
		{
			PickableButton.ResetMasks();
			RemoveWaitingForFileOperation(PickableBuildButton.buildMenuCurrentState);
			RemoveLoadingOperation();
			refreshingLocalSavesList = false;
			if (filenamesList != null)
			{
				localSaveFilenames = (List<string>)filenamesList;
			}
			else
			{
				localSaveFilenames = new List<string>();
			}
			if (!keepPage)
			{
				CurrentPage = 0;
			}
			ClampCurrentPageNumber();
			int startIndex = CurrentPage * GetCurrentEntrySlots().Length;
			PopulateFeaturedLevelEntriesFromLocalSaves(startIndex);
			if (onFinish != null)
			{
				onFinish();
			}
		});
	}

	public static void ClearMissingLocalSnapshotCodeEntries(SaveFileData saveFileData)
	{
		string saveFolder = QuickSaver.LocalSavesFolder;
		HashSet<string> usedFilenames = new HashSet<string>();
		Action OnFilenamesReturned = delegate
		{
			HashSet<string> hashSet = new HashSet<string>();
			foreach (KeyValuePair<string, string> localSnapshotCode in saveFileData.localSnapshotCodes)
			{
				if (!usedFilenames.Contains(localSnapshotCode.Key))
				{
					hashSet.Add(localSnapshotCode.Key);
				}
			}
			foreach (string item in hashSet)
			{
				saveFileData.RemoveLocalSnapshotCodeAssociation(item);
			}
		};
		if (RamFS.PlatformUsesRamFS)
		{
			RamFS.AddGetExistingFilenamesOperation(saveFolder + "/", ".snapshot", ordered: false, delegate(IEnumerable<string> filenames)
			{
				if (filenames != null)
				{
					foreach (string filename in filenames)
					{
						usedFilenames.Add(Path.GetFileNameWithoutExtension(filename));
					}
					OnFilenamesReturned();
				}
				else
				{
					Debug.LogError("ClearMissingLocalSnapshotCodeEntries: Error while getting snapshot filenames...");
				}
			});
			return;
		}
		WorkerThreadManager.Instance.AddFileOpJob(delegate
		{
			foreach (FileInfo item2 in from f in new DirectoryInfo(saveFolder).GetFiles("*.snapshot")
				orderby f.CreationTime descending
				select f)
			{
				usedFilenames.Add(Path.GetFileNameWithoutExtension(item2.Name));
			}
		}, OnFilenamesReturned);
	}

	private void PopulateFeaturedLevelEntriesFromRecent(int startIdx, UnityAction onFinish)
	{
		Transform[] currentEntrySlots = GetCurrentEntrySlots();
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		featuredLevelEntries = new List<FeaturedLevelData>();
		firstFeaturedEntryIndex = startIdx;
		totalFeaturedEntries = saveFileDataForMainUser.recentSnapshotEntries.Count;
		ClearFeaturedLevelPage();
		List<string> list = new List<string>();
		for (int i = 0; i < currentEntrySlots.Length; i++)
		{
			int num = startIdx + i;
			if (num < saveFileDataForMainUser.recentSnapshotEntries.Count)
			{
				list.Add(GameSparksQuery.SanitizeSnapshotCode(saveFileDataForMainUser.recentSnapshotEntries[num].code));
			}
		}
		AddLoadingOperation();
		MaskAllButTabs();
		GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
		query.GetLevelInfo(list);
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			PickableButton.ResetMasks();
			RemoveLoadingOperation();
			if (!query.HasError)
			{
				List<FeaturedLevelData> list2 = query.ResultData["records"] as List<FeaturedLevelData>;
				featuredLevelEntries = list2;
				if (onFinish != null)
				{
					onFinish();
				}
			}
			else
			{
				Debug.LogError("Error: " + query.Error);
			}
		});
	}

	private void PopulateFeaturedLevelEntriesFromLocalSaves(int startIndex)
	{
		featuredLevelEntries = new List<FeaturedLevelData>();
		firstFeaturedEntryIndex = startIndex;
		totalFeaturedEntries = localSaveFilenames.Count;
		Transform[] currentEntrySlots = GetCurrentEntrySlots();
		int num = Mathf.Min(totalFeaturedEntries - startIndex, currentEntrySlots.Length);
		for (int i = 0; i < num; i++)
		{
			featuredLevelEntries.Add(new FeaturedLevelData());
		}
		for (int j = 0; j < currentEntrySlots.Length; j++)
		{
			if (localSaveFilenames.Count > startIndex + j)
			{
				SetLocalLevelEntry(j, localSaveFilenames[startIndex + j]);
			}
		}
	}

	private void SetLocalLevelEntry(int entryIdx, string filenameWithoutExt)
	{
		FeaturedLevelData featuredLevelData = featuredLevelEntries[entryIdx];
		featuredLevelData.name = QuickSaver.GetSnapshotNameWithoutSuffix(filenameWithoutExt);
		featuredLevelData.levelType = QuickSaver.InferLevelTypeFromFilename(filenameWithoutExt);
		featuredLevelData.isLocal = true;
	}

	private void PopulateFeaturedLevelEntriesFromFavourites(int startIndex, UnityAction onFinish)
	{
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		featuredLevelEntries = new List<FeaturedLevelData>();
		firstFeaturedEntryIndex = startIndex;
		totalFeaturedEntries = saveFileDataForMainUser.favoriteSnapshots.Count;
		ClearFeaturedLevelPage();
		Transform[] currentEntrySlots = GetCurrentEntrySlots();
		int num = Mathf.Min(totalFeaturedEntries - startIndex, currentEntrySlots.Length);
		for (int i = 0; i < num; i++)
		{
			featuredLevelEntries.Add(new FeaturedLevelData());
		}
		Dictionary<string, int> codeToIndexMap = new Dictionary<string, int>();
		for (int j = 0; j < currentEntrySlots.Length; j++)
		{
			if (totalFeaturedEntries <= startIndex + j)
			{
				continue;
			}
			SaveFileData.FavoriteSnapshotEntry favoriteSnapshotEntry = saveFileDataForMainUser.favoriteSnapshots[startIndex + j];
			if (favoriteSnapshotEntry.code.NullOrEmpty())
			{
				SetLocalLevelEntry(j, favoriteSnapshotEntry.name);
				continue;
			}
			string key = GameSparksQuery.SanitizeSnapshotCode(favoriteSnapshotEntry.code);
			if (!codeToIndexMap.ContainsKey(key))
			{
				codeToIndexMap.Add(key, j);
			}
			else
			{
				featuredLevelEntries[j] = null;
			}
		}
		if (codeToIndexMap.Count == 0)
		{
			if (onFinish != null)
			{
				onFinish();
			}
			return;
		}
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, int> item in codeToIndexMap)
		{
			list.Add(item.Key);
		}
		AddLoadingOperation();
		MaskAllButTabs();
		GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
		query.GetLevelInfo(list);
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			PickableButton.ResetMasks();
			RemoveLoadingOperation();
			if (!query.HasError)
			{
				foreach (FeaturedLevelData item2 in query.ResultData["records"] as List<FeaturedLevelData>)
				{
					if (codeToIndexMap.TryGetValue(item2.code, out var value))
					{
						featuredLevelEntries[value] = item2;
						codeToIndexMap.Remove(item2.code);
					}
				}
				if (codeToIndexMap.Count > 0)
				{
					foreach (KeyValuePair<string, int> item3 in codeToIndexMap)
					{
						featuredLevelEntries[item3.Value] = null;
					}
				}
				if (onFinish != null)
				{
					onFinish();
				}
			}
			else
			{
				Debug.LogError("Error: " + query.Error);
			}
		});
	}

	private void UpdatePageIndicator()
	{
		if (featuredQuickInfoEnabled)
		{
			if (PickableBuildButton.SelectedEntry != null)
			{
				SnapshotEntry component = PickableBuildButton.SelectedEntry.GetComponent<SnapshotEntry>();
				if (component != null && component.indexOnCurrentPage != -1)
				{
					int num = firstFeaturedEntryIndex + component.indexOnCurrentPage + 1;
					pageIndicator.text = num + "/" + totalFeaturedEntries;
				}
				else
				{
					pageIndicator.text = "";
				}
			}
			else
			{
				pageIndicator.text = "";
			}
		}
		else
		{
			int numPages = NumPages;
			if (numPages > 0)
			{
				pageIndicator.text = CurrentPage + 1 + "/" + numPages;
			}
			else
			{
				pageIndicator.text = "";
			}
		}
	}

	public void RefreshCurrentPage()
	{
		ClampCurrentPageNumber();
		prevButton.Enable(ShouldShowPrev);
		nextButton.Enable(ShouldShowNext);
		UpdatePageIndicator();
		bool shouldShowPublishedMark = ShouldShowPublishedMark;
		DisplaySlotPageDef currentSlotPageDef = GetCurrentSlotPageDef();
		Transform[] currentEntrySlots = currentSlotPageDef.GetCurrentEntrySlots();
		UnityEngine.Object currentEntryPrefab = currentSlotPageDef.GetCurrentEntryPrefab();
		Dictionary<string, SnapshotEntry> filenameButtonMap = new Dictionary<string, SnapshotEntry>();
		for (int i = 0; i < currentEntrySlots.Length; i++)
		{
			if (featuredLevelEntries.Count <= i)
			{
				continue;
			}
			FeaturedLevelData featuredLevelData = featuredLevelEntries[i];
			if (featuredLevelData != null)
			{
				currentEntrySlots[i].gameObject.SetActive(value: true);
				SnapshotEntry snapshotEntry = currentEntrySlots[i].GetComponentInChildren<SnapshotEntry>();
				if (snapshotEntry == null)
				{
					snapshotEntry = currentEntrySlots[i].gameObject.AddPrefabAsChild<SnapshotEntry>(currentEntryPrefab);
				}
				PickableBuildButton component = snapshotEntry.GetComponent<PickableBuildButton>();
				component.Enable();
				component.onShowTip = snapshotEntry.OnShowTip;
				snapshotEntry.indexOnCurrentPage = i;
				snapshotEntry.Initialize(featuredLevelData, shouldShowPublishedMark && featuredLevelData.isPublished);
				if (snapshotEntry.Code.NullOrEmpty() && !filenameButtonMap.ContainsKey(snapshotEntry.SnapshotName))
				{
					filenameButtonMap.Add(snapshotEntry.SnapshotName, snapshotEntry);
				}
			}
		}
		HashSet<string> missingFiles = new HashSet<string>();
		UnityAction highlightMissingFiles = delegate
		{
			foreach (string item in missingFiles)
			{
				filenameButtonMap[item].MarkFileMissing();
			}
		};
		if (RamFS.PlatformUsesRamFS)
		{
			RamFS.AddGetExistingFilenamesOperation("/snapshots/", ".snapshot", ordered: false, delegate(IEnumerable<string> list)
			{
				foreach (KeyValuePair<string, SnapshotEntry> item2 in filenameButtonMap)
				{
					string key = item2.Key;
					string text = QuickSaver.LocalSavesFolder + "/" + key;
					if (!list.Contains(text + ".snapshot") && !list.Contains(text + ".c.snapshot") && !list.Contains(text + ".v.snapshot"))
					{
						missingFiles.Add(item2.Key);
					}
				}
				highlightMissingFiles();
			});
		}
		else
		{
			string localSavesFolder = QuickSaver.LocalSavesFolder;
			WorkerThreadManager.Instance.AddFileOpJob(delegate
			{
				foreach (KeyValuePair<string, SnapshotEntry> item3 in filenameButtonMap)
				{
					if (!QuickSaver.CheckLocalSaveExistsThreadSafe(localSavesFolder, item3.Key))
					{
						missingFiles.Add(item3.Key);
					}
				}
			}, delegate
			{
				highlightMissingFiles();
			});
		}
		if (autoSelectIndexOnRefresh != -1 && autoSelectIndexOnRefresh < currentEntrySlots.Length)
		{
			StartCoroutine(AutoSelectInOneFrame(autoSelectIndexOnRefresh));
		}
		if (!autoSelectCodeOnRefresh.NullOrEmpty() || !autoSelectLevelNameOnRefresh.NullOrEmpty())
		{
			StartCoroutine(AutoSelectInOneFrame(autoSelectCodeOnRefresh, autoSelectLevelNameOnRefresh));
		}
		autoSelectCodeOnRefresh = null;
		autoSelectLevelNameOnRefresh = null;
		autoSelectIndexOnRefresh = -1;
	}

	private IEnumerator AutoSelectInOneFrame(int idx)
	{
		yield return null;
		Transform transform = GetCurrentSlotPageDef().GetCurrentEntrySlots()[idx];
		if (transform != null)
		{
			PickableBuildButton componentInChildren = transform.GetComponentInChildren<PickableBuildButton>();
			if (componentInChildren != null)
			{
				componentInChildren.Select(allowDeselect: true);
			}
		}
	}

	private IEnumerator AutoSelectInOneFrame(string code, string levelName)
	{
		yield return null;
		Transform[] currentEntrySlots = GetCurrentSlotPageDef().GetCurrentEntrySlots();
		for (int i = 0; i < currentEntrySlots.Length; i++)
		{
			PickableBuildButton componentInChildren = currentEntrySlots[i].GetComponentInChildren<PickableBuildButton>();
			if (!(componentInChildren != null))
			{
				continue;
			}
			SnapshotEntry component = componentInChildren.GetComponent<SnapshotEntry>();
			if (!code.NullOrEmpty())
			{
				if (GameSparksQuery.SanitizeSnapshotCode(component.Code) == code)
				{
					componentInChildren.Select(allowDeselect: false);
					break;
				}
			}
			else if (component.featuredLevelData.name == levelName)
			{
				componentInChildren.Select(allowDeselect: false);
				break;
			}
		}
	}

	public void OnSelectLevelCodesTab()
	{
		UpdateVisibility();
	}

	public void OnSelectFeaturedLevelTab(bool refreshCurrentPage, bool resetFilters)
	{
		PickableBuildButton.buildMenuCurrentState = PickableBuildButton.BuildScreenStates.FeaturedLevelTab;
		if (refreshCurrentPage)
		{
			featuredLevelEntries = new List<FeaturedLevelData>();
			firstFeaturedEntryIndex = 0;
			totalFeaturedEntries = 0;
		}
		bool flag = false;
		if (currentFilter == null || resetFilters)
		{
			if (resetFilters)
			{
				if (ScriptLocalization.DefaultLevelNetFilter.Equals("1"))
				{
					mainFilterDropdown.SelectEntryByIndex(1, triggerOnChangeEvent: false);
				}
				else
				{
					mainFilterDropdown.SelectEntryByIndex(0, triggerOnChangeEvent: false);
				}
				levelTypeDropdown.SelectEntryByIndex(0, triggerOnChangeEvent: false);
				currentLevelType = FeaturedQuickFilter.LevelTypes.Any;
				dateCutoffDropdown.SelectEntryByIndex(0, triggerOnChangeEvent: false);
				cutoffDays = 0;
				difficultyDropdown.SelectEntryByIndex(0, triggerOnChangeEvent: false);
				lowerDifficultyBound = 0f;
				upperDifficultyBound = 1f;
				advancedSearchBox.inputField.text = "";
			}
			FeaturedQuickFilter component = mainFilterDropdown.selectedDropdownEntry.GetComponent<FeaturedQuickFilter>();
			FeaturedQuickFilter.SortingFilter sortingFilter = null;
			sortingFilter = ((!(component != null) || component.sortingFilter == null) ? new FeaturedQuickFilter.SortingFilter() : component.sortingFilter.Clone());
			sortingFilter.allowUnpublished = allowUnpublished;
			sortingFilter.cutoffDays = cutoffDays;
			sortingFilter.levelType = currentLevelType;
			OnMainFilterDropdownValueChange();
			flag = true;
			mainFilterDropdown.SetClickDisabled(disabled: false);
			levelTypeDropdown.SetClickDisabled(disabled: false);
			dateCutoffDropdown.SetClickDisabled(disabled: false);
			difficultyDropdown.SetClickDisabled(disabled: false);
		}
		UpdateVisibility();
		if (refreshCurrentPage && !flag)
		{
			RefreshCurrentPage();
		}
	}

	public void OpenSpecialFilterPage(FeaturedSpecialFilter.SpecialFilterType filterType)
	{
		featuredLevelEntries = new List<FeaturedLevelData>();
		firstFeaturedEntryIndex = 0;
		totalFeaturedEntries = 0;
		mainFilterDropdown.OnClickDropdownEntry(mainFilterDropdown.FindFirstDropdownEntryWithCriteria(delegate(DropdownEntry entry)
		{
			FeaturedSpecialFilter component = entry.GetComponent<FeaturedSpecialFilter>();
			return component != null && component.type == filterType;
		}), triggerOnChangeEvent: false);
		PickableBuildButton.buildMenuCurrentState = PickableBuildButton.BuildScreenStates.FeaturedLevelTab;
		PickableBuildButton.DeselectLastSelected();
		OnMainFilterDropdownValueChange();
		OnSelectFeaturedLevelTab(refreshCurrentPage: false, resetFilters: false);
	}

	public void OnClickPrev()
	{
		if (featuredQuickInfoEnabled)
		{
			if (!(PickableBuildButton.SelectedEntry != null))
			{
				return;
			}
			SnapshotEntry component = PickableBuildButton.SelectedEntry.GetComponent<SnapshotEntry>();
			if (!(component != null) || component.indexOnCurrentPage == -1 || firstFeaturedEntryIndex + component.indexOnCurrentPage <= 0)
			{
				return;
			}
			Transform[] currentEntrySlots = GetCurrentEntrySlots();
			_ = currentEntrySlots.Length;
			if (component.indexOnCurrentPage == 0)
			{
				if (CurrentPage > 0)
				{
					CurrentPage--;
					int startIndex = firstFeaturedEntryIndex - currentEntrySlots.Length;
					autoSelectIndexOnRefresh = currentEntrySlots.Length - 1;
					UpdateFeaturedLevelView(startIndex);
				}
			}
			else
			{
				Transform transform = currentEntrySlots[component.indexOnCurrentPage - 1];
				if (transform != null)
				{
					transform.GetComponentInChildren<PickableBuildButton>().Select(allowDeselect: true);
				}
			}
		}
		else if (CurrentPage > 0)
		{
			PickableBuildButton.DeselectLastSelected();
			CurrentPage--;
			int startIndex2 = firstFeaturedEntryIndex - GetCurrentEntrySlots().Length;
			UpdateFeaturedLevelView(startIndex2);
		}
	}

	public void OnClickNext()
	{
		if (featuredQuickInfoEnabled)
		{
			if (!(PickableBuildButton.SelectedEntry != null))
			{
				return;
			}
			SnapshotEntry component = PickableBuildButton.SelectedEntry.GetComponent<SnapshotEntry>();
			if (!(component != null) || component.indexOnCurrentPage == -1 || firstFeaturedEntryIndex + component.indexOnCurrentPage >= totalFeaturedEntries - 1)
			{
				return;
			}
			Transform[] currentEntrySlots = GetCurrentEntrySlots();
			int num = currentEntrySlots.Length;
			if (component.indexOnCurrentPage == num - 1)
			{
				if (CurrentPage + 1 < NumPages)
				{
					CurrentPage++;
					int startIndex = firstFeaturedEntryIndex + currentEntrySlots.Length;
					autoSelectIndexOnRefresh = 0;
					UpdateFeaturedLevelView(startIndex);
				}
			}
			else
			{
				Transform transform = currentEntrySlots[component.indexOnCurrentPage + 1];
				if (transform != null)
				{
					transform.GetComponentInChildren<PickableBuildButton>().Select(allowDeselect: true);
				}
			}
		}
		else if (CurrentPage + 1 < NumPages)
		{
			PickableBuildButton.DeselectLastSelected();
			CurrentPage++;
			int startIndex2 = firstFeaturedEntryIndex + GetCurrentEntrySlots().Length;
			UpdateFeaturedLevelView(startIndex2);
		}
	}

	private void ClampCurrentPageNumber()
	{
		int numPages = NumPages;
		if (numPages == 0)
		{
			CurrentPage = 0;
		}
		else
		{
			CurrentPage = Mathf.Clamp(CurrentPage, 0, numPages - 1);
		}
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(PickCursorClickedBackgroundEvent))
		{
			if (PickableButton.maskAll || PickableButton.allowedButtons.Count > 0)
			{
				if (Controller.InputFieldWasActiveRecently)
				{
					Controller.UnlockInputField();
					PickableButton.ResetMasks();
				}
			}
			else if (!DropdownMenu.dropdownDeployed)
			{
				if (FeaturedQuickInfoPane.localDeleteEnabled)
				{
					quickInfoPane.OnClickCancelDeleteFile();
				}
				else if (FeaturedQuickInfoPane.adminDeleteEnabled)
				{
					FeaturedQuickInfoPane.adminDeleteEnabled = false;
				}
			}
		}
		if (type == typeof(UndergroundComputerOpenedEvent))
		{
			if (neverOpened)
			{
				neverOpened = false;
				OnFirstOpened();
			}
			else
			{
				OnReopened();
			}
		}
		if (type == typeof(PlayerInGameRuleEvent) && e is PlayerInGameRuleEvent { Entered: false } && LevelSelectController.lastInstance.GameRuleBook.ActiveCursors == 0)
		{
			for (int i = 0; i < 3; i++)
			{
				BackOutOfSubmenu(autoClosing: true);
			}
		}
	}

	public void RenamedLocalLevel(PickableBuildButton button, PickCursor pickCursor)
	{
		quickInfoPane.localLevelNameText.interactable = true;
		quickInfoPane.ActivateLocalLevelRenameField(GetOnRenameCallback(PickableBuildButton.SelectedEntry), button, pickCursor);
		SteamDeck.OpenVirtualKeyboard(pickCursor);
	}

	public UnityAction<string> GetOnRenameCallback(PickableBuildButton button)
	{
		string originalName = quickInfoPane.localLevelNameText.text;
		SnapshotEntry snapshotEntry = button.GetComponent<SnapshotEntry>();
		quickInfoPane.localLevelNameText.onEndEdit.RemoveAllListeners();
		return delegate(string str)
		{
			QuickSaver.FindLocalSaveFilenameWithoutExt(originalName, delegate(string originalNameWithSuffix)
			{
				string localSaveExtraSuffix = QuickSaver.GetLocalSaveExtraSuffix(originalNameWithSuffix);
				PickableButton.ResetMasks();
				quickInfoPane.localLevelNameText.interactable = false;
				string trimmedName = str.Trim();
				string trimmedNameWithsuffix = trimmedName + localSaveExtraSuffix;
				UnityAction onRenameFail = delegate
				{
					quickInfoPane.localLevelNameText.text = originalName;
					quickInfoPane.localLevelNameText.interactable = false;
				};
				if (trimmedNameWithsuffix != originalNameWithSuffix)
				{
					if (IsFilenameValid(trimmedNameWithsuffix))
					{
						string text = QuickSaver.LocalSavesFolder + "/" + originalNameWithSuffix + ".snapshot";
						string text2 = QuickSaver.LocalSavesFolder + "/" + trimmedNameWithsuffix + ".snapshot";
						UnityAction onRenameComplete = delegate
						{
							SaveFileData saveFileData = StatTracker.Instance.GetSaveFileDataForMainUser();
							RenameSnapshotThumbnail(originalName, trimmedName);
							updateLevelPortalInfo(originalName, trimmedName, saveFileData);
							quickInfoPane.localLevelNameText.text = trimmedName;
							snapshotEntry.featuredLevelData.name = trimmedName;
							RefreshLocalSavesList(keepPage: true, delegate
							{
								if (snapshotEntry.Code.NullOrEmpty() && saveFileData.IsFavorite(originalName, null))
								{
									saveFileData.RemoveFavoriteSnapshotCode(originalName, null);
									saveFileData.AddFavoriteSnapshotCode(trimmedName, null);
								}
								else if (!snapshotEntry.Code.NullOrEmpty() && saveFileData.IsFavorite(originalName, snapshotEntry.Code))
								{
									saveFileData.RemoveFavoriteSnapshotCode(originalName, snapshotEntry.Code);
									saveFileData.AddFavoriteSnapshotCode(trimmedName, null);
								}
								if (!snapshotEntry.featuredLevelData.code.NullOrEmpty())
								{
									saveFileData.RemoveLocalSnapshotCodeAssociation(originalName);
									saveFileData.RemoveLocalSnapshotCodeAssociation(trimmedName);
								}
								PickableBuildButton.DeselectLastSelected();
								int pageNumberForFileEntry = GetPageNumberForFileEntry(trimmedNameWithsuffix);
								if (pageNumberForFileEntry != -1)
								{
									CurrentPage = pageNumberForFileEntry;
								}
								else
								{
									CurrentPage = 0;
								}
								RefreshCurrentPage();
								SnapshotEntry snapshotEntry2 = FindSnapshotInDisplayList(null, trimmedName);
								if (snapshotEntry2 != null)
								{
									PickableBuildButton component = snapshotEntry2.GetComponent<PickableBuildButton>();
									if (component != null)
									{
										component.Select(allowDeselect: false);
									}
								}
								else
								{
									Debug.LogError("Could not find snapshot with name " + trimmedName);
								}
							});
						};
						if (RamFS.PlatformUsesRamFS)
						{
							RamFS.AddRenameFileOperation(text, text2, delegate(RamFS.FSOperationReturnCode returnCode)
							{
								if (returnCode == RamFS.FSOperationReturnCode.OK)
								{
									onRenameComplete();
								}
								else
								{
									Debug.LogError("Error while renaming file: " + returnCode);
									UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ErrorWhileRenamingFile, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
									AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
									onRenameFail();
								}
							});
						}
						else if (!File.Exists(text2))
						{
							try
							{
								File.Move(text, text2);
							}
							catch (Exception ex)
							{
								Debug.LogError("Exception while renaming file: " + ex.Message);
								UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ErrorWhileRenamingFile, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
								AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
								onRenameFail();
								return;
							}
							onRenameComplete();
						}
						else
						{
							Debug.LogError("Supplied filename already exists.");
							UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.FileAlreadyExists, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
							AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
							onRenameFail();
						}
					}
					else
					{
						if (trimmedName.NullOrEmpty())
						{
							Debug.LogError("Supplied filename was empty.");
							UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.SpeficyFilename, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
							AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
						}
						else
						{
							Debug.LogError("Supplied filename was invalid.");
							UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.InvalidCharacters, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
							AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
						}
						onRenameFail();
					}
				}
				else
				{
					onRenameFail();
				}
			});
		};
	}

	private void RenameError(string logErrorMessage, string locText, UnityAction onRenameFail)
	{
		Debug.LogError(logErrorMessage);
		UserMessageManager.Instance.UserMessage(locText, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
		AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
		onRenameFail();
	}

	private void RenameSnapshotThumbnail(string originalName, string newName)
	{
		string thumbnailFilenameForLocalSave = QuickSaver.GetThumbnailFilenameForLocalSave(originalName);
		string thumbnailFilenameForLocalSave2 = QuickSaver.GetThumbnailFilenameForLocalSave(newName);
		if (File.Exists(thumbnailFilenameForLocalSave))
		{
			if (File.Exists(thumbnailFilenameForLocalSave2))
			{
				File.Delete(thumbnailFilenameForLocalSave2);
			}
			try
			{
				File.Move(thumbnailFilenameForLocalSave, thumbnailFilenameForLocalSave2);
			}
			catch (Exception ex)
			{
				Debug.LogError("Failed to rename level thumbnail: " + ex.Message);
			}
			LevelThumbnailCache.Instance.OnLocalThumbnailRenamed(thumbnailFilenameForLocalSave, thumbnailFilenameForLocalSave2);
		}
	}

	private void updateLevelPortalInfo(string originalName, string newName, SaveFileData saveFileData)
	{
		for (int i = 0; i < slotPortals.Length; i++)
		{
			CustomLevelPortal customLevelPortal = slotPortals[i];
			if (customLevelPortal.snapshotInfo != null && customLevelPortal.snapshotInfo.snapshotName == originalName && !GameSparksQuery.ValidateSnapshotCode(customLevelPortal.levelCode.text))
			{
				PickableBuildButton pickableBuildButton = ComputerSlots[i];
				pickableBuildButton.SetComputerSlotAppearance(newName, pickableBuildButton.LevelImage.sprite);
				pickableBuildButton.Select(allowDeselect: false);
				if (customLevelPortal.hasAuthority && customLevelPortal.snapshotInfo != null)
				{
					customLevelPortal.SetSnapshotInfo(customLevelPortal.snapshotInfo.targetLevel, customLevelPortal.snapshotInfo.code, newName, customLevelPortal.snapshotInfo.xml, customLevelPortal.snapshotInfo.levelType);
					saveFileData.SetPortalInfo(i, customLevelPortal.snapshotInfo);
				}
				customLevelPortal.SetContents(customLevelPortal.TargetLevel, newName, null, customLevelPortal.snapshotXml, customLevelPortal.levelImage.sprite, null);
				break;
			}
		}
	}

	private bool IsFilenameValid(string fileName)
	{
		if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
		{
			return false;
		}
		FileInfo fileInfo = null;
		try
		{
			fileInfo = new FileInfo(fileName);
		}
		catch (ArgumentException)
		{
		}
		catch (PathTooLongException)
		{
		}
		catch (NotSupportedException)
		{
		}
		return fileInfo != null;
	}

	private int GetPageNumberForFileEntry(string filenameWithoutExtension)
	{
		int num = -1;
		for (int i = 0; i < localSaveFilenames.Count; i++)
		{
			if (localSaveFilenames[i] == filenameWithoutExtension)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			Transform[] currentEntrySlots = GetCurrentSlotPageDef().GetCurrentEntrySlots();
			return num / currentEntrySlots.Length;
		}
		return -1;
	}

	public void OnClickConfirmDeleteLocalFile()
	{
		if (PickableBuildButton.SelectedEntry != null)
		{
			SnapshotEntry snapshotEntry = PickableBuildButton.SelectedEntry.GetComponent<SnapshotEntry>();
			string fileName = snapshotEntry.SnapshotName;
			QuickSaver.FindLocalSaveFilenameWithoutExt(fileName, delegate(string actualFilenameWithoutExt)
			{
				string pathToDelete = QuickSaver.LocalSavesFolder + "/" + actualFilenameWithoutExt + ".snapshot";
				SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
				if (snapshotEntry.Code.NullOrEmpty() && saveFileDataForMainUser.IsFavorite(fileName, null))
				{
					saveFileDataForMainUser.RemoveFavoriteSnapshotCode(fileName, null);
				}
				saveFileDataForMainUser.RemoveLocalSnapshotCodeAssociation(fileName);
				foreach (PickableBuildButton.LevelSlotEnum item in GetSlotsContainingSnapshot(null, snapshotEntry.SnapshotName))
				{
					OnClickClearCode(item);
				}
				string thumbnailPath = QuickSaver.GetThumbnailFilenameForLocalSave(fileName);
				if (RamFS.PlatformUsesRamFS)
				{
					RamFS.AddDeleteFileOperation(pathToDelete, delegate(RamFS.FSOperationReturnCode returnCode)
					{
						if (returnCode == RamFS.FSOperationReturnCode.OK)
						{
							RefreshLocalSavesList(keepPage: true, RefreshCurrentPage);
						}
						else
						{
							Debug.LogError("Could not delete snapshot file " + pathToDelete + ": " + returnCode);
							UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.CouldNotDelete, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
							AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
						}
					});
					RamFS.AddDeleteFileOperation(thumbnailPath, delegate(RamFS.FSOperationReturnCode returnCode)
					{
						if (returnCode == RamFS.FSOperationReturnCode.OK)
						{
							LevelThumbnailCache.Instance.OnLocalThumbnailDeleted(thumbnailPath);
						}
						else
						{
							Debug.LogError("Could not delete thumbnail file " + thumbnailPath + ": " + returnCode);
						}
					});
				}
				else
				{
					try
					{
						if (File.Exists(thumbnailPath))
						{
							File.Delete(thumbnailPath);
						}
						LevelThumbnailCache.Instance.OnLocalThumbnailDeleted(thumbnailPath);
						File.Delete(pathToDelete);
						RefreshLocalSavesList(keepPage: true, RefreshCurrentPage);
					}
					catch (Exception ex)
					{
						Debug.LogError("Could not delete snapshot file " + pathToDelete + ": " + ex.Message);
						UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.CouldNotDelete, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
						AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
					}
				}
				PickableBuildButton.DeselectLastSelected();
			});
		}
		quickInfoPane.OnClickCancelDeleteFile();
	}

	public void ActivateCodeEntryField(Cursor pickCusor)
	{
		Controller.LockInputField(codeEntryField.inputField, OnCodeEntryFieldSubmitted);
		SteamDeck.OpenVirtualKeyboard(pickCusor);
		PickableButton.maskAll = true;
		codeEntryField.inputField.onValueChanged.RemoveAllListeners();
		codeEntryField.inputField.onValueChanged.AddListener(OnCodeEntryFieldChanged);
	}

	public void OnCodeEntryFieldChanged(string str)
	{
		ResetCodeEntryStatus();
	}

	public void OnCodeEntryFieldSubmitted(string str)
	{
		PickableButton.ResetMasks();
		codeEntryField.inputField.onValueChanged.RemoveAllListeners();
		codeEntryField.inputField.onEndEdit.RemoveAllListeners();
		if (currentQuery == null && !currentCodeValidated && !str.NullOrEmpty())
		{
			currentCodeAssociatedXml = null;
			currentCodeAssociatedName = null;
			currentCodeAuthorInfo = null;
			currentCodeLevelType = FeaturedQuickFilter.LevelTypes.Any;
			codeStatusText.buttonText.text = ScriptLocalization.Snapshot.LookingUpCode;
			codeStatusCheckmark.buttonText.text = ScriptLocalization.Snapshot.Ellipsis;
			AddLoadingOperation();
			GameSparksQuery gameSparksQuery = GameSparksManager.Instance.CreateQuery();
			gameSparksQuery.GetXmlStringFromSnapshotCode(str);
			gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, GetCodeQueryReturnFunc(gameSparksQuery, str));
			currentQuery = gameSparksQuery;
		}
	}

	private void ResetCodeEntryStatus()
	{
		codeStatusCheckmark.buttonText.text = "";
		codeStatusText.buttonText.text = "";
		currentCodeValidated = false;
		currentCodeAssociatedXml = null;
		currentCodeAssociatedName = null;
		currentCodeAuthorInfo = null;
		currentCodeLevelType = FeaturedQuickFilter.LevelTypes.Any;
		currentQuery = null;
	}

	private UnityAction<GameSparksQuery> GetCodeQueryReturnFunc(GameSparksQuery query, string codeInput)
	{
		return delegate(GameSparksQuery q)
		{
			RemoveLoadingOperation();
			OnCodeQueryReturn(q, codeInput);
		};
	}

	private void OnCodeQueryReturn(GameSparksQuery query, string codeInput)
	{
		currentCodeValidated = false;
		currentCodeAssociatedXml = null;
		currentCodeAssociatedName = null;
		currentCodeAuthorInfo = null;
		currentCodeLevelType = FeaturedQuickFilter.LevelTypes.Any;
		if (query == currentQuery)
		{
			currentQuery = null;
			if (!query.HasError)
			{
				currentCodeValidated = true;
				byte[] data = query.ResultData["bytes"] as byte[];
				currentCodeAssociatedXml = QuickSaver.GetXmlStringFromBytes(data);
				currentCodeAssociatedName = query.ResultData["name"] as string;
				currentCodeLevelType = FeaturedQuickFilter.LevelTypes.Any;
				currentCodeAuthorInfo = null;
				if (query.ResultData.ContainsKey("authorInfo") && query.ResultData["authorInfo"] is GSData gSData)
				{
					string authorID = gSData.GetString("playerID");
					string authorDisplayName = gSData.GetString("authorDisplayName");
					GSData gSData2 = gSData.GetGSData("authorPlatformIds");
					currentCodeAuthorInfo = new CustomLevelPortal.AuthorInfo(authorID, authorDisplayName, gSData2);
				}
				currentCodeArchived = query.ResultData.ContainsKey("archived") && (bool)query.ResultData["archived"];
				codeEntryField.inputField.onValueChanged.RemoveAllListeners();
				codeEntryField.inputField.text = GameSparksQuery.GetFormattedSnapshotCode(codeInput);
				codeStatusCheckmark.buttonText.text = "<color=#ccff99>✓</color>";
				codeStatusText.buttonText.text = ScriptLocalization.Snapshot.LevelName + " " + currentCodeAssociatedName.ToString();
				PickableBuildButton[] computerSlots = ComputerSlots;
				for (int i = 0; i < computerSlots.Length; i++)
				{
					computerSlots[i].animator.SetBool("Visible", !currentCodeArchived && !CodeAlreadyLoaded(codeInput));
				}
				if (!currentCodeArchived)
				{
					StartCoroutine(OpenLevelInfoForRecentCode(codeInput));
				}
				else
				{
					UpdateVisibility();
				}
			}
			else
			{
				codeStatusCheckmark.buttonText.text = "<color=#ffaaaa>✗</color>";
				codeStatusText.buttonText.text = ScriptLocalization.Snapshot.InvalidCode;
				Debug.LogError("The code could not be validated: " + query.Error);
				UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.CodeNotValidated, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
				AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
			}
		}
		else
		{
			Debug.Log("Ignoring result for old query");
		}
	}

	private IEnumerator WaitForLoadingOperations()
	{
		while (loadingOperations > 0)
		{
			yield return null;
		}
	}

	private IEnumerator OpenLevelInfoForRecentCode(string codeInput)
	{
		OpenSpecialFilterPage(FeaturedSpecialFilter.SpecialFilterType.Recent);
		yield return WaitForLoadingOperations();
		SnapshotEntry snapshotEntry = FindSnapshotInDisplayList(GameSparksQuery.SanitizeSnapshotCode(codeInput), currentCodeAssociatedName);
		if (snapshotEntry != null)
		{
			snapshotEntry.GetComponent<PickableBuildButton>().OnAccept(null);
		}
	}

	public void LoadCurrentCodeIntoSlot(PickableBuildButton.LevelSlotEnum slot, bool selectEntry = true, Action<bool> OnFinish = null)
	{
		bool obj = false;
		if (currentCodeValidated && currentQuery == null)
		{
			if (slotPortals.Length > (int)slot)
			{
				CustomLevelPortal customLevelPortal = slotPortals[(int)slot];
				PickableBuildButton pickableBuildButton = ComputerSlots[(int)slot];
				XmlDocument xmlDocument = new XmlDocument();
				try
				{
					xmlDocument.LoadXml(currentCodeAssociatedXml);
				}
				catch (Exception ex)
				{
					Debug.LogError("Error loading XML: " + ex.Message);
					AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
					return;
				}
				string text = QuickSaver.ParseAttrStr(xmlDocument.DocumentElement, "levelSceneName");
				if (!text.NullOrEmpty())
				{
					GameState.LevelName levelNameEnumFromSceneName = LevelSelectController.GetLevelNameEnumFromSceneName(text);
					if (customLevelPortal.hasAuthority)
					{
						customLevelPortal.SetSnapshotInfo(levelNameEnumFromSceneName, codeEntryField.inputField.text, currentCodeAssociatedName, currentCodeAssociatedXml, currentCodeLevelType);
						StatTracker.Instance.GetSaveFileDataForMainUser().SetPortalInfo((int)slot, customLevelPortal.snapshotInfo);
					}
					customLevelPortal.SetContents(levelNameEnumFromSceneName, currentCodeAssociatedName, codeEntryField.inputField.text, currentCodeAssociatedXml, GetSpriteForLevel(levelNameEnumFromSceneName), currentCodeAuthorInfo);
					pickableBuildButton.SetComputerSlotAppearance(currentCodeAssociatedName, GetSpriteForLevel(levelNameEnumFromSceneName));
					if (selectEntry)
					{
						pickableBuildButton.Select(allowDeselect: false);
					}
					obj = true;
					AkSoundEngine.PostEvent("UI_Snapshot_LoadComplete", base.gameObject);
				}
				else
				{
					Debug.LogError("Could not find level scene name in XML payload.");
					AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
				}
			}
		}
		OnFinish?.Invoke(obj);
	}

	public Sprite GetSpriteForLevel(GameState.LevelName levelEnum)
	{
		LevelSignSprite[] array = levelSignSprites;
		foreach (LevelSignSprite levelSignSprite in array)
		{
			if (levelSignSprite.level == levelEnum)
			{
				return levelSignSprite.sprite;
			}
		}
		return null;
	}

	public void LoadCurrentLocalSaveIntoSlot(PickableBuildButton button, PickableBuildButton.LevelSlotEnum slot, bool selectEntry = true, Action<bool> OnFinish = null)
	{
		if (currentQuery != null)
		{
			return;
		}
		int portalIdx = (int)slot;
		if (slotPortals.Length <= portalIdx)
		{
			return;
		}
		CustomLevelPortal portal = slotPortals[portalIdx];
		PickableBuildButton SlotButton = ComputerSlots[portalIdx];
		SnapshotEntry snapshotEntry = button.GetComponent<SnapshotEntry>();
		QuickSaver.FindLocalSaveFilenameWithoutExt(snapshotEntry.SnapshotName, delegate(string actualFilenameWithoutExt)
		{
			FeaturedQuickFilter.LevelTypes levelType = QuickSaver.InferLevelTypeFromFilename(actualFilenameWithoutExt);
			string text = QuickSaver.LocalSavesFolder + "/" + actualFilenameWithoutExt + ".snapshot";
			UnityAction<XmlDocument> loadXmlIntoSlot = delegate(XmlDocument xmlDoc)
			{
				if (xmlDoc != null)
				{
					string text2 = QuickSaver.ParseAttrStr(xmlDoc.DocumentElement, "levelSceneName");
					if (!text2.NullOrEmpty())
					{
						string text3 = "";
						string code = null;
						if (snapshotEntry.codeText.enabled)
						{
							text3 = snapshotEntry.Code;
							code = snapshotEntry.Code;
							if (!GameSparksQuery.ValidateSnapshotCode(text3))
							{
								text3 = "";
							}
						}
						GameState.LevelName levelNameEnumFromSceneName = LevelSelectController.GetLevelNameEnumFromSceneName(text2);
						portal.SetContents(levelNameEnumFromSceneName, snapshotEntry.SnapshotName, text3, xmlDoc.OuterXml, GetSpriteForLevel(levelNameEnumFromSceneName), null);
						SlotButton.SetComputerSlotAppearance(snapshotEntry.SnapshotName, GetSpriteForLevel(levelNameEnumFromSceneName));
						if (selectEntry)
						{
							button.Select(allowDeselect: false);
						}
						if (portal.hasAuthority)
						{
							portal.SetSnapshotInfo(levelNameEnumFromSceneName, code, snapshotEntry.SnapshotName, xmlDoc.OuterXml, levelType);
							StatTracker.Instance.GetSaveFileDataForMainUser().SetPortalInfo(portalIdx, portal.snapshotInfo);
						}
						AkSoundEngine.PostEvent("UI_Snapshot_LoadComplete", base.gameObject);
						PickableBuildButton[] computerSlots = ComputerSlots;
						for (int i = 0; i < computerSlots.Length; i++)
						{
							computerSlots[i].animator.SetBool("Visible", value: false);
						}
						if (OnFinish != null)
						{
							OnFinish(obj: true);
						}
					}
					else
					{
						Debug.LogError("Could not find level scene name in XML payload.");
						if (OnFinish != null)
						{
							OnFinish(obj: false);
						}
					}
				}
				else
				{
					Debug.LogError("Failed to read snapshot file");
					UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ErrorLoadingFromDisk, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
					if (OnFinish != null)
					{
						OnFinish(obj: false);
					}
				}
			};
			if (RamFS.PlatformUsesRamFS)
			{
				RamFS.AddReadFileOperation(text, delegate(RamFS.FSOperationReturnCode returnCode, byte[] fileContents)
				{
					if (returnCode == RamFS.FSOperationReturnCode.OK)
					{
						XmlDocument xmlDocFromBytes = QuickSaver.GetXmlDocFromBytes(fileContents);
						loadXmlIntoSlot(xmlDocFromBytes);
					}
					else
					{
						Debug.LogError("Failed to read snapshot file (" + returnCode.ToString() + ")");
						UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ErrorLoadingFromDisk, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
						if (OnFinish != null)
						{
							OnFinish(obj: false);
						}
					}
				});
			}
			else
			{
				XmlDocument arg = QuickSaver.TryLoadSnapshotXMLFromPath(text);
				loadXmlIntoSlot(arg);
			}
		});
	}

	public void LoadCurrentRecentCodeIntoSlot(PickableBuildButton button, PickableBuildButton.LevelSlotEnum slot, bool selectEntry = true, Action<bool> OnFinish = null)
	{
		if (currentQuery == null)
		{
			SnapshotEntry component = button.GetComponent<SnapshotEntry>();
			if (component.featuredLevelData.archived)
			{
				return;
			}
			int portalIdx = (int)slot;
			if (slotPortals.Length > portalIdx)
			{
				PickableButton.maskAll = true;
				AddLoadingOperation();
				string code = component.Code;
				AkSoundEngine.PostEvent("UI_Snapshot_Load", base.gameObject);
				PickableBuildButton[] computerSlots = ComputerSlots;
				foreach (PickableBuildButton pickableBuildButton in computerSlots)
				{
					if (pickableBuildButton.ControlsLevelSlot == slot)
					{
						pickableBuildButton.animator.SetTrigger("Loading");
					}
					else
					{
						pickableBuildButton.animator.SetBool("Visible", value: false);
					}
				}
				GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
				query.GetXmlStringFromSnapshotCode(code);
				currentQuery = query;
				GameSparksQuery gameSparksQuery = query;
				gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate(GameSparksQuery response)
				{
					PickableButton.ResetMasks();
					bool obj = false;
					if (!response.HasError)
					{
						if (response.ResultData.ContainsKey("archived") && (bool)response.ResultData["archived"])
						{
							Debug.LogError("Could not load into slot: level is archived");
							UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.CodeNotValidated, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
							AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
						}
						else
						{
							string text = response.ResultData["name"] as string;
							byte[] data = response.ResultData["bytes"] as byte[];
							FeaturedQuickFilter.LevelTypes levelType = (FeaturedQuickFilter.LevelTypes)query.ResultData["levelType"];
							string xmlStringFromBytes = QuickSaver.GetXmlStringFromBytes(data);
							GSData authorInfo = response.ResultData["authorInfo"] as GSData;
							LoadSnapshotIntoPortalSlot(slot, xmlStringFromBytes, code, text, portalIdx, levelType, authorInfo, selectEntry);
							obj = true;
						}
					}
					else
					{
						Debug.LogError("The code could not be validated: " + query.Error);
						UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.CodeNotValidated, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
						AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
					}
					PickableBuildButton[] computerSlots2 = ComputerSlots;
					for (int j = 0; j < computerSlots2.Length; j++)
					{
						computerSlots2[j].animator.SetBool("Visible", value: false);
					}
					RemoveLoadingOperation();
					currentQuery = null;
					if (OnFinish != null)
					{
						OnFinish(obj);
					}
				});
			}
		}
		if (currentQuery == null && OnFinish != null)
		{
			OnFinish(obj: false);
		}
	}

	public void LoadCurrentFavoriteIntoPortalSlot(PickableBuildButton button, PickableBuildButton.LevelSlotEnum slot, bool selectEntry = true, Action<bool> OnFinish = null)
	{
		if (currentQuery != null)
		{
			return;
		}
		if (slotPortals.Length > (int)slot)
		{
			SnapshotEntry component = button.GetComponent<SnapshotEntry>();
			string text = component.Code;
			_ = component.SnapshotName;
			if (!GameSparksQuery.ValidateSnapshotCode(text))
			{
				text = null;
			}
			if (text.NullOrEmpty())
			{
				LoadCurrentLocalSaveIntoSlot(button, slot, selectEntry, OnFinish);
			}
			else
			{
				LoadCurrentRecentCodeIntoSlot(button, slot, selectEntry, OnFinish);
			}
		}
	}

	private void LoadSnapshotIntoPortalSlot(PickableBuildButton.LevelSlotEnum slot, string xml, string code, string name, int portalIdx, FeaturedQuickFilter.LevelTypes levelType, GSData authorInfo, bool selectEntry = true)
	{
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.LoadXml(xml);
		}
		catch (Exception ex)
		{
			Debug.LogError("Error loading XML: " + ex.Message);
			return;
		}
		string text = QuickSaver.ParseAttrStr(xmlDocument.DocumentElement, "levelSceneName");
		if (!text.NullOrEmpty())
		{
			CustomLevelPortal customLevelPortal = slotPortals[portalIdx];
			PickableBuildButton pickableBuildButton = ComputerSlots[portalIdx];
			GameState.LevelName levelNameEnumFromSceneName = LevelSelectController.GetLevelNameEnumFromSceneName(text);
			CustomLevelPortal.AuthorInfo authorInfo2 = null;
			if (authorInfo != null)
			{
				string authorID = authorInfo.GetString("playerID");
				string authorDisplayName = authorInfo.GetString("authorDisplayName");
				GSData gSData = authorInfo.GetGSData("authorPlatformIds");
				authorInfo2 = new CustomLevelPortal.AuthorInfo(authorID, authorDisplayName, gSData);
			}
			customLevelPortal.SetContents(levelNameEnumFromSceneName, name, code, xmlDocument.OuterXml, GetSpriteForLevel(levelNameEnumFromSceneName), authorInfo2);
			pickableBuildButton.SetComputerSlotAppearance(name, GetSpriteForLevel(levelNameEnumFromSceneName));
			if (selectEntry)
			{
				SnapshotEntry snapshotEntry = FindSnapshotInDisplayList(code, name);
				if (snapshotEntry != null)
				{
					snapshotEntry.GetComponent<PickableBuildButton>().Select(allowDeselect: false);
				}
				else
				{
					pickableBuildButton.Select(allowDeselect: false);
				}
			}
			if (customLevelPortal.hasAuthority)
			{
				customLevelPortal.SetSnapshotInfo(levelNameEnumFromSceneName, code, name, xmlDocument.OuterXml, levelType);
				if (authorInfo != null)
				{
					string authorID2 = authorInfo.GetString("playerID");
					string authorDisplayName2 = authorInfo.GetString("authorDisplayName");
					GSData gSData2 = authorInfo.GetGSData("authorPlatformIds");
					customLevelPortal.SetAuthorInfo(authorID2, authorDisplayName2, gSData2);
				}
				StatTracker.Instance.GetSaveFileDataForMainUser().SetPortalInfo(portalIdx, customLevelPortal.snapshotInfo);
			}
			AkSoundEngine.PostEvent("UI_Snapshot_LoadComplete", base.gameObject);
		}
		else
		{
			Debug.LogError("Could not find level scene name in XML payload.");
		}
	}

	public void GetCodeForLocalSave(PickableBuildButton button, UnityAction<bool> onFinish)
	{
		if (currentQuery != null)
		{
			return;
		}
		if (!NetworkConnectivityStatus.Connected)
		{
			Debug.LogError("Could not get code for local save: No network connection");
			TabletSaveAndShareScreen.ShowUploadError(TabletSaveAndShareScreen.UploadError.NoConnection);
			return;
		}
		if (PlatformFeatureRestrictions.IsNotConnected)
		{
			Debug.LogError("Could not get code for local save: Not signed in");
			TabletSaveAndShareScreen.ShowUploadError(TabletSaveAndShareScreen.UploadError.NotSignedIn);
			return;
		}
		SnapshotEntry snapshotEntry = button.GetComponent<SnapshotEntry>();
		if (!snapshotEntry.Code.NullOrEmpty())
		{
			return;
		}
		string filenameWithoutExt = snapshotEntry.SnapshotName;
		QuickSaver.FindLocalSaveFilenameWithoutExt(filenameWithoutExt, delegate(string actualFilenameWithoutExt)
		{
			string fullpath = QuickSaver.LocalSavesFolder + "/" + actualFilenameWithoutExt + ".snapshot";
			PickableButton.maskAll = true;
			UnityAction<XmlDocument> onGetLocalSaveXML = delegate(XmlDocument doc)
			{
				if (doc != null)
				{
					int num = QuickSaver.CalculateLevelFullnessFromXML(doc, metadataList);
					int levelFullnessScoreLimit = GameSettings.GetInstance().LevelFullnessScoreLimit;
					if (num <= levelFullnessScoreLimit)
					{
						UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.UploadingSnapshot, 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
						if (snapshotEntry.codeText != null)
						{
							snapshotEntry.codeText.text = "...";
						}
						AddLoadingOperation();
						bool hasMods = QuickSaver.CheckNonDefaultModsFromXML(doc);
						GameSparksQuery uploadQuery = GameSparksManager.Instance.CreateQuery();
						uploadQuery.UploadStringAsFile(doc.OuterXml, filenameWithoutExt, published: false, FeaturedQuickFilter.LevelTypes.Versus, hasMods);
						GameSparksQuery gameSparksQuery = uploadQuery;
						gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate(GameSparksQuery q)
						{
							RemoveLoadingOperation();
							if (q == currentQuery)
							{
								currentQuery = null;
								PickableButton.ResetMasks();
								if (!q.HasError)
								{
									string formattedSnapshotCode = GameSparksQuery.GetFormattedSnapshotCode(uploadQuery.ResultData["code"] as string);
									snapshotEntry.SetCode(formattedSnapshotCode, local: false);
									if (StatTracker.Instance != null)
									{
										SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
										saveFileDataForMainUser.AddRecentSnapshotCode(SaveFileData.RecentSnapshotEntry.SnapshotType.Uploaded, formattedSnapshotCode, uploadQuery.ResultData["name"] as string);
										saveFileDataForMainUser.AssociateLocalSnapshotCode(filenameWithoutExt, formattedSnapshotCode);
										if (saveFileDataForMainUser.IsFavorite(filenameWithoutExt, null))
										{
											saveFileDataForMainUser.AddCodeToLocalFavorite(filenameWithoutExt, formattedSnapshotCode);
										}
									}
									QuickSaver.CopyStringToClipboard(formattedSnapshotCode);
									UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ShareableCodeClipboard, 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
									AkSoundEngine.PostEvent("UI_Snapshot_GetCode", base.gameObject);
									if (onFinish != null)
									{
										onFinish(arg0: true);
									}
									return;
								}
								Debug.LogError("Error while uploading snapshot: " + q.Error);
								UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ErrorUploadingSnapshot, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
								AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
								snapshotEntry.SetCode(null, local: false);
							}
							else
							{
								Debug.Log("Ignoring result for old query");
							}
							if (onFinish != null)
							{
								onFinish(arg0: false);
							}
						});
						currentQuery = uploadQuery;
					}
					else
					{
						Debug.LogError("Could not upload snapshot: Too big!");
						UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ErrorTooBig, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
						PickableButton.ResetMasks();
						if (onFinish != null)
						{
							onFinish(arg0: false);
						}
					}
				}
				else
				{
					Debug.LogError("Could not load XML file!");
					UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ErrorLoadingFromDisk, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
					PickableButton.ResetMasks();
					if (onFinish != null)
					{
						onFinish(arg0: false);
					}
				}
			};
			if (RamFS.PlatformUsesRamFS)
			{
				AddLoadingOperation();
				RamFS.AddReadFileOperation(fullpath, delegate(RamFS.FSOperationReturnCode returnCode, byte[] resultData)
				{
					XmlDocument xmlDocFromBytes = QuickSaver.GetXmlDocFromBytes(resultData);
					RemoveLoadingOperation();
					onGetLocalSaveXML(xmlDocFromBytes);
				});
			}
			else
			{
				XmlDocument xmldoc = null;
				AddLoadingOperation();
				WorkerThreadManager.Instance.AddFileOpJob(delegate
				{
					xmldoc = QuickSaver.TryLoadSnapshotXMLFromPath(fullpath);
				}, delegate
				{
					RemoveLoadingOperation();
					onGetLocalSaveXML(xmldoc);
				});
			}
		});
	}

	private int GetPortalIdxFromSlot(PickableBuildButton.LevelSlotEnum slotEnum)
	{
		return (int)slotEnum;
	}

	public CustomLevelPortal GetPortalForSlot(PickableBuildButton.LevelSlotEnum slotEnum)
	{
		return slotPortals[GetPortalIdxFromSlot(slotEnum)];
	}

	public bool IsPortalPopulated(PickableBuildButton.LevelSlotEnum slotEnum)
	{
		return slotPortals[GetPortalIdxFromSlot(slotEnum)].populated;
	}

	public bool IsAnyPortalPopulated()
	{
		for (int i = 0; i < slotPortals.Length; i++)
		{
			if (slotPortals[i].populated)
			{
				return true;
			}
		}
		return false;
	}

	public string nameOfSnapShotInSlot(PickableBuildButton.LevelSlotEnum slotEnum)
	{
		if (slotPortals[GetPortalIdxFromSlot(slotEnum)].snapshotInfo != null)
		{
			return slotPortals[GetPortalIdxFromSlot(slotEnum)].snapshotInfo.snapshotName;
		}
		return null;
	}

	public List<PickableBuildButton.LevelSlotEnum> GetSlotsContainingSnapshot(string code, string levelName)
	{
		code = GameSparksQuery.SanitizeSnapshotCode(code);
		List<PickableBuildButton.LevelSlotEnum> list = new List<PickableBuildButton.LevelSlotEnum>();
		if (code.NullOrEmpty())
		{
			for (int i = 0; i < slotPortals.Length; i++)
			{
				if (slotPortals[i].snapshotInfo != null && slotPortals[i].snapshotInfo.snapshotName.Equals(levelName))
				{
					list.Add((PickableBuildButton.LevelSlotEnum)i);
				}
			}
		}
		else
		{
			for (int j = 0; j < slotPortals.Length; j++)
			{
				if (slotPortals[j].snapshotInfo != null && !slotPortals[j].snapshotInfo.code.NullOrEmpty() && slotPortals[j].snapshotInfo.code.Equals(code))
				{
					list.Add((PickableBuildButton.LevelSlotEnum)j);
				}
			}
		}
		return list;
	}

	public bool SnapshotAlreadyLoaded(PickableBuildButton entry)
	{
		SnapshotEntry component = entry.GetComponent<SnapshotEntry>();
		if (component.Code.NullOrEmpty())
		{
			for (int i = 0; i < slotPortals.Length; i++)
			{
				if (slotPortals[i].snapshotInfo != null && slotPortals[i].snapshotInfo.code.NullOrEmpty() && slotPortals[i].snapshotInfo.snapshotName.Equals(component.SnapshotName))
				{
					return true;
				}
			}
		}
		else
		{
			for (int j = 0; j < slotPortals.Length; j++)
			{
				if (slotPortals[j].snapshotInfo != null && !slotPortals[j].snapshotInfo.code.NullOrEmpty() && slotPortals[j].snapshotInfo.code.Equals(component.Code))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CodeAlreadyLoaded(string code)
	{
		for (int i = 0; i < slotPortals.Length; i++)
		{
			if (slotPortals[i].snapshotInfo != null && !slotPortals[i].snapshotInfo.code.NullOrEmpty() && slotPortals[i].snapshotInfo.code.Equals(code))
			{
				return true;
			}
		}
		return false;
	}

	public bool CodeAlreadyLoadedInSlot(string code, PickableBuildButton.LevelSlotEnum slotEnum)
	{
		CustomLevelPortal customLevelPortal = slotPortals[GetPortalIdxFromSlot(slotEnum)];
		if (customLevelPortal.snapshotInfo != null && !customLevelPortal.snapshotInfo.code.NullOrEmpty() && customLevelPortal.snapshotInfo.code.Equals(code))
		{
			return true;
		}
		return false;
	}

	public bool SnapshotAlreadyLoadedInSlot(PickableBuildButton entry, PickableBuildButton.LevelSlotEnum slotEnum)
	{
		SnapshotEntry component = entry.GetComponent<SnapshotEntry>();
		CustomLevelPortal customLevelPortal = slotPortals[GetPortalIdxFromSlot(slotEnum)];
		if (component.Code.NullOrEmpty())
		{
			if (customLevelPortal.snapshotInfo != null && customLevelPortal.snapshotInfo.code.NullOrEmpty() && customLevelPortal.snapshotInfo.snapshotName.Equals(component.SnapshotName))
			{
				return true;
			}
		}
		else if (customLevelPortal.snapshotInfo != null && !customLevelPortal.snapshotInfo.code.NullOrEmpty() && customLevelPortal.snapshotInfo.code.Equals(component.Code))
		{
			return true;
		}
		return false;
	}

	public bool CodeEntryMatchesLoadedCodeInSlot(PickableBuildButton.LevelSlotEnum slotEnum)
	{
		CustomLevelPortal customLevelPortal = slotPortals[GetPortalIdxFromSlot(slotEnum)];
		if (customLevelPortal.snapshotInfo != null && !customLevelPortal.snapshotInfo.code.NullOrEmpty() && customLevelPortal.snapshotInfo.code.Equals(codeEntryField.inputField.text))
		{
			return true;
		}
		return false;
	}

	public bool CodeEntryAlreadyLoaded()
	{
		for (int i = 0; i < slotPortals.Length; i++)
		{
			if (slotPortals[i].snapshotInfo != null && !slotPortals[i].snapshotInfo.code.NullOrEmpty() && slotPortals[i].snapshotInfo.code.Equals(codeEntryField.inputField.text))
			{
				return true;
			}
		}
		return false;
	}

	public void OnClickClearCode(PickableBuildButton.LevelSlotEnum slotEnum)
	{
		if (IsPortalPopulated(slotEnum))
		{
			int portalIdxFromSlot = GetPortalIdxFromSlot(slotEnum);
			ComputerSlots[portalIdxFromSlot].ClearComputerSlotContents();
			slotPortals[portalIdxFromSlot].ClearContents();
		}
	}

	public void ClearAllSlots()
	{
		for (int i = 0; i < slotPortals.Length; i++)
		{
			slotPortals[i].ClearContents();
			ComputerSlots[i].ClearComputerSlotContents();
		}
		StatTracker.Instance.GetSaveFileDataForMainUser().SetPortalInfo(new CustomLevelPortal.SnapshotInfo[0]);
	}

	public void DeleteRecentCode(PickableBuildButton button)
	{
		SnapshotEntry component = button.GetComponent<SnapshotEntry>();
		if (component != null)
		{
			string code = component.Code;
			StatTracker.Instance.GetSaveFileDataForMainUser().DeleteRecentSnapshotCode(GameSparksQuery.GetFormattedSnapshotCode(code));
			RefreshSearch(keepPage: true);
		}
	}

	public void RemoveFavorite(PickableBuildButton button)
	{
		SnapshotEntry component = button.GetComponent<SnapshotEntry>();
		if (component != null)
		{
			string text = component.inputField.text;
			string text2 = component.Code;
			if (!GameSparksQuery.ValidateSnapshotCode(text2))
			{
				text2 = null;
			}
			StatTracker.Instance.GetSaveFileDataForMainUser().RemoveFavoriteSnapshotCode(text, text2);
			RefreshSearch(keepPage: true);
		}
	}

	public void CreateLocalSaveFromRecentEntry(PickableBuildButton button)
	{
		if (currentQuery != null)
		{
			return;
		}
		PickableButton.maskAll = true;
		AddLoadingOperation();
		SnapshotEntry component = button.GetComponent<SnapshotEntry>();
		string code = component.Code;
		GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
		query.GetXmlStringFromSnapshotCode(code);
		currentQuery = query;
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate(GameSparksQuery response)
		{
			PickableButton.ResetMasks();
			RemoveLoadingOperation();
			currentQuery = null;
			if (!response.HasError)
			{
				string text = response.ResultData["name"] as string;
				byte[] xmlBytes = response.ResultData["bytes"] as byte[];
				FeaturedQuickFilter.LevelTypes levelType = (FeaturedQuickFilter.LevelTypes)response.ResultData["levelType"];
				SaveLocalCopy(text, code, xmlBytes, levelType);
			}
			else
			{
				Debug.LogError("The code could not be validated: " + query.Error);
				UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.CodeNotValidated, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
				AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
			}
		});
	}

	private void SaveLocalCopy(string name, string snapshotCode, object xmlBytes, FeaturedQuickFilter.LevelTypes levelType)
	{
		name = QuickSaver.SanitizePath(name);
		UnityAction OnNameFiltered = delegate
		{
			QuickSaver.RecountLocalSaves(delegate
			{
				int maxLocalSnapshots = GameSettings.GetInstance().maxLocalSnapshots;
				if (QuickSaver.numLocalSaves >= maxLocalSnapshots)
				{
					UserMessageManager.Instance.UserMessage(string.Format(LocalizationManager.GetTranslation("Snapshot/SaveShare/SnapshotLimitReached"), maxLocalSnapshots), 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
					Debug.LogError("Too many snapshots! (max: " + maxLocalSnapshots + " current: " + QuickSaver.numLocalSaves + ")");
				}
				else
				{
					XmlDocument xmlDoc = null;
					if (xmlBytes is string)
					{
						xmlDoc = QuickSaver.GetXmlDocFromString((string)xmlBytes);
					}
					else if (xmlBytes is byte[])
					{
						xmlDoc = QuickSaver.GetXmlDocFromBytes((byte[])xmlBytes);
					}
					if (xmlDoc != null)
					{
						QuickSaver.CheckSaveFolders();
						string suffix = QuickSaver.GetLocalSaveSuffixForLevelType(levelType);
						string tentativeFilename = QuickSaver.LocalSavesFolder + "/" + name;
						Action<IEnumerable<string>> action = delegate(IEnumerable<string> existingFilenames)
						{
							string actualFilename = QuickSaver.EnsureUniqueLocalLevelName(tentativeFilename, existingFilenames);
							if (actualFilename != null)
							{
								actualFilename = actualFilename + suffix + ".snapshot";
								byte[] compressedBytesFromXmlDoc = QuickSaver.GetCompressedBytesFromXmlDoc(xmlDoc);
								UnityAction onSaveComplete = delegate
								{
									string fileName = Path.GetFileName(actualFilename);
									if (RamFS.PlatformUsesRamFS)
									{
										UserMessageManager.Instance.UserMessage(LocalizationManager.GetTranslation("Snapshot/SavingFileAs") + " " + fileName, 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
										RamFS.PostUserMessageOnFlushToDisk(ScriptLocalization.Snapshot.SavedFileAs + " " + fileName);
									}
									else
									{
										UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.SavedFileAs + " " + fileName, 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
									}
									if (!snapshotCode.NullOrEmpty())
									{
										string snapshotNameWithoutSuffix = QuickSaver.GetSnapshotNameWithoutSuffix(Path.GetFileNameWithoutExtension(fileName));
										LevelThumbnailCache.Instance.DownloadThumbnailFromCloud(snapshotCode, QuickSaver.GetThumbnailFilenameForLocalSave(snapshotNameWithoutSuffix), delegate(Texture2D tex)
										{
											if (tex != null)
											{
												Debug.Log("Downloaded thumbnail for " + GameSparksQuery.GetFormattedSnapshotCode(snapshotCode));
											}
											else
											{
												Debug.Log("Failed to downloaded thumbnail for " + GameSparksQuery.GetFormattedSnapshotCode(snapshotCode));
											}
										}, 1);
									}
								};
								if (!RamFS.PlatformUsesRamFS)
								{
									try
									{
										FileStream fileStream = File.OpenWrite(actualFilename);
										fileStream.Write(compressedBytesFromXmlDoc, 0, compressedBytesFromXmlDoc.Length);
										fileStream.Close();
										onSaveComplete();
										return;
									}
									catch (Exception ex)
									{
										Debug.LogError("Error while saving file: " + ex.Message + "\n" + ex.StackTrace);
										UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ErrorSavingFile, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
										AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
										return;
									}
								}
								RamFS.AddAddFileOperation(actualFilename, compressedBytesFromXmlDoc, delegate(RamFS.FSOperationReturnCode returnCode)
								{
									if (returnCode == RamFS.FSOperationReturnCode.OK)
									{
										onSaveComplete();
									}
									else
									{
										Debug.LogError("Error while saving file: " + returnCode);
										UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ErrorSavingFile, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
										AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
									}
								});
							}
							else
							{
								Debug.LogError("Could not save the file!");
								UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ErrorSavingFile, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
								AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
							}
						};
						if (RamFS.PlatformUsesRamFS)
						{
							RamFS.AddGetExistingFilenamesOperation("/snapshots/", ".snapshot", ordered: false, action);
						}
						else
						{
							action(null);
						}
					}
					else
					{
						Debug.LogError("Downloaded XML could not be parsed!");
						UserMessageManager.Instance.UserMessage(LocalizationManager.GetTranslation("UndergroundComputer/ErrorParysingFile"), 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
						AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
					}
				}
			});
		};
		if (WordFilter.PlatformHasWordFilter)
		{
			WordFilter.FilterText(this, name, delegate(string filteredText)
			{
				name = filteredText.Replace('*', '-');
				OnNameFiltered();
			});
		}
		else
		{
			OnNameFiltered();
		}
	}

	public void CopyCodeToClipboard(PickableBuildButton button)
	{
		string code = button.GetComponent<SnapshotEntry>().Code;
		if (GameSparksQuery.ValidateSnapshotCode(code))
		{
			QuickSaver.CopyStringToClipboard(GameSparksQuery.GetFormattedSnapshotCode(code));
			UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ShareableCodeClipboard, 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
		}
	}

	public void OnSelectEntry(PickableBuildButton button)
	{
		if (button != null)
		{
			SnapshotEntry component = button.GetComponent<SnapshotEntry>();
			if (component != null)
			{
				string snapshotName = component.SnapshotName;
				string code = component.Code;
				bool favoriteButtonState = StatTracker.Instance.GetSaveFileDataForMainUser().IsFavorite(snapshotName, code);
				SetFavoriteButtonState(favoriteButtonState);
				if (PickableBuildButton.buildMenuCurrentState == PickableBuildButton.BuildScreenStates.FeaturedLevelTab)
				{
					ShowQuickLevelInfo(component);
				}
				bool flag = component.featuredLevelData != null && component.featuredLevelData.archived;
				PickableBuildButton[] computerSlots = ComputerSlots;
				for (int i = 0; i < computerSlots.Length; i++)
				{
					computerSlots[i].animator.SetBool("Visible", !flag && button.job != PickableBuildButton.BuildButtonJobs.LevelSlot && !SnapshotAlreadyLoaded(button));
				}
			}
		}
		else
		{
			ShowQuickLevelInfo(null);
			PickableBuildButton[] computerSlots = ComputerSlots;
			for (int i = 0; i < computerSlots.Length; i++)
			{
				computerSlots[i].animator.SetBool("Visible", value: false);
			}
		}
	}

	public void OnSelectLevelSlot(PickableBuildButton button)
	{
		if (button != null)
		{
			SnapshotEntry snapshotEntry = button.GetComponent<SnapshotEntry>();
			CustomLevelPortal customLevelPortal = slotPortals[GetPortalIdxFromSlot(button.ControlsLevelSlot)];
			if (customLevelPortal.snapshotInfo == null)
			{
				return;
			}
			SnapshotEntry snapshotEntry2 = FindSnapshotInDisplayList(customLevelPortal.snapshotInfo.code, customLevelPortal.snapshotInfo.snapshotName);
			if (snapshotEntry2 != null)
			{
				PickableBuildButton component = snapshotEntry2.GetComponent<PickableBuildButton>();
				component.Select(allowDeselect: false);
				snapshotEntry.SetInfoInternal(snapshotEntry2.Code, snapshotEntry2.SnapshotName, snapshotEntry2.Local);
				OnSelectEntry(component);
				return;
			}
			string text = GameSparksQuery.SanitizeSnapshotCode(customLevelPortal.snapshotInfo.code);
			if (text != null)
			{
				GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
				query.GetLevelInfo(new List<string> { text });
				GameSparksQuery gameSparksQuery = query;
				gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
				{
					if (!query.HasError)
					{
						if (query.ResultData["records"] is List<FeaturedLevelData> { Count: >0 } list)
						{
							FeaturedLevelData featuredLevelData = list[0];
							snapshotEntry.SetInfoInternal(featuredLevelData.code, featuredLevelData.name, local: false);
							snapshotEntry.featuredLevelData = featuredLevelData;
							OnSelectEntry(button);
						}
						else
						{
							Debug.LogError("Could not find level info in response");
							UserMessageManager.Instance.UserMessage(LocalizationManager.GetTranslation("UndergroundComputer/LevelNotFound"), 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
							AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
						}
					}
					else
					{
						Debug.LogError("Could not grab level info: " + query.Error);
						UserMessageManager.Instance.UserMessage(LocalizationManager.GetTranslation("UndergroundComputer/LevelNotFound"), 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
						AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
					}
				});
			}
			else
			{
				snapshotEntry.SetInfoInternal(null, customLevelPortal.snapshotInfo.snapshotName, local: true);
				OnSelectEntry(button);
			}
		}
		else
		{
			OnSelectEntry(null);
		}
	}

	public SnapshotEntry FindSnapshotInDisplayList(string code, string snapshotName)
	{
		if (GetCurrentEntrySlots() == null)
		{
			return null;
		}
		Transform[] currentEntrySlots = GetCurrentEntrySlots();
		for (int i = 0; i < currentEntrySlots.Length; i++)
		{
			SnapshotEntry componentInChildren = currentEntrySlots[i].GetComponentInChildren<SnapshotEntry>();
			if (componentInChildren != null && ((code.NullOrEmpty() && componentInChildren.Code.NullOrEmpty()) || componentInChildren.Code == code) && componentInChildren.SnapshotName == snapshotName)
			{
				return componentInChildren;
			}
		}
		return null;
	}

	private void SetFavoriteButtonState(bool isFavorite)
	{
		quickInfoPane.favoriteButtonImage.sprite = (isFavorite ? quickInfoPane.favoriteFilledImage : quickInfoPane.favoriteEmptyImage);
		quickInfoPane.localFavoriteButtonImage.sprite = (isFavorite ? quickInfoPane.favoriteFilledImage : quickInfoPane.favoriteEmptyImage);
		quickInfoPane.archivedFavoriteButtonImage.sprite = (isFavorite ? quickInfoPane.favoriteFilledImage : quickInfoPane.favoriteEmptyImage);
	}

	public void ToggleFavorite(PickableBuildButton button)
	{
		SnapshotEntry component = button.GetComponent<SnapshotEntry>();
		if (!(component != null))
		{
			return;
		}
		string snapshotName = component.SnapshotName;
		string text = component.Code;
		if (!GameSparksQuery.ValidateSnapshotCode(text))
		{
			text = null;
		}
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		bool flag = saveFileDataForMainUser.IsFavorite(snapshotName, text);
		if (flag)
		{
			saveFileDataForMainUser.RemoveFavoriteSnapshotCode(snapshotName, text);
		}
		else
		{
			saveFileDataForMainUser.AddFavoriteSnapshotCode(snapshotName, text);
		}
		PickableBuildButton selectedEntry = PickableBuildButton.SelectedEntry;
		if (selectedEntry != null)
		{
			SnapshotEntry component2 = selectedEntry.GetComponent<SnapshotEntry>();
			if (component2.faveStar != null)
			{
				component2.faveStar.enabled = !flag;
				component2.faveStarBack.enabled = !flag;
			}
		}
		SetFavoriteButtonState(!flag);
	}

	public PickCursor GetPickCursorForController(Controller sender)
	{
		int lastPlayerNumber = sender.GetLastPlayerNumber();
		if (lastPlayerNumber > 0)
		{
			return LevelSelectController.lastInstance.GameRuleBook.GetCursor(lastPlayerNumber);
		}
		return null;
	}

	public void ReceiveEvent(InputEvent e)
	{
		if (computerDisabled || (codeEntryField.inventoryBook != null && codeEntryField.InventoryBook.cursors.Count == 0))
		{
			return;
		}
		if (e.Valueb && e.Changed && e.Sender.GetFirstAssociatedCharacter() != Character.Animals.NONE)
		{
			switch (e.Key)
			{
			case InputEvent.InputKey.RotateLeft:
				if (e.Sender is KeyboardInput && Input.mouseScrollDelta.y != 0f)
				{
					PickCursor pickCursorForController2 = GetPickCursorForController(e.Sender);
					if (pickCursorForController2 != null)
					{
						switch (PickableBuildButton.buildMenuCurrentState)
						{
						case PickableBuildButton.BuildScreenStates.AdminPanelDialog:
							adminPanelDialog.OnScrollMinus(pickCursorForController2);
							return;
						case PickableBuildButton.BuildScreenStates.ViewReportsDialog:
							viewReportsDialog.scrollArrowContainer.OnPickCursorScrollMinus(pickCursorForController2);
							return;
						}
						if (featuredQuickInfoEnabled)
						{
							quickInfoPane.scrollArrowController.OnPickCursorScrollMinus(pickCursorForController2);
							return;
						}
					}
				}
				OnClickPrev();
				break;
			case InputEvent.InputKey.RotateRight:
				if (e.Sender is KeyboardInput && Input.mouseScrollDelta.y != 0f)
				{
					PickCursor pickCursorForController = GetPickCursorForController(e.Sender);
					if (pickCursorForController != null)
					{
						switch (PickableBuildButton.buildMenuCurrentState)
						{
						case PickableBuildButton.BuildScreenStates.AdminPanelDialog:
							adminPanelDialog.OnScrollPlus(pickCursorForController);
							return;
						case PickableBuildButton.BuildScreenStates.ViewReportsDialog:
							viewReportsDialog.scrollArrowContainer.OnPickCursorScrollPlus(pickCursorForController);
							return;
						}
						if (featuredQuickInfoEnabled)
						{
							quickInfoPane.scrollArrowController.OnPickCursorScrollPlus(pickCursorForController);
							return;
						}
					}
				}
				OnClickNext();
				break;
			}
		}
		if (!rightStickValues.ContainsKey(e.Sender))
		{
			rightStickValues.Add(e.Sender, new float[2]);
		}
		switch (e.Key)
		{
		case InputEvent.InputKey.Up2:
			rightStickValues[e.Sender][0] = 0f - e.Valuef;
			break;
		case InputEvent.InputKey.Down2:
			rightStickValues[e.Sender][1] = e.Valuef;
			break;
		}
	}

	private int GetCurrentTabIndex()
	{
		for (int i = 0; i < tabContainer.childCount; i++)
		{
			PickableBuildButton component = tabContainer.GetChild(i).GetComponent<PickableBuildButton>();
			if (component.JobToTabState(component.job) == PickableBuildButton.buildMenuCurrentState)
			{
				return i;
			}
		}
		return -1;
	}

	public void AddWaitingForFileOperation(PickableBuildButton.BuildScreenStates state)
	{
		waitingForFileOperation[state]++;
	}

	public void RemoveWaitingForFileOperation(PickableBuildButton.BuildScreenStates state)
	{
		waitingForFileOperation[state]--;
		if (waitingForFileOperation[state] < 0)
		{
			waitingForFileOperation[state] = 0;
		}
	}

	public void DescrambleFile(PickableBuildButton button)
	{
		SnapshotEntry component = button.GetComponent<SnapshotEntry>();
		if (!(component != null))
		{
			return;
		}
		QuickSaver.FindLocalSaveFilenameWithoutExt(component.SnapshotName, delegate(string actualFilenameWithoutExt)
		{
			string text = QuickSaver.LocalSavesFolder + "/" + actualFilenameWithoutExt + ".snapshot";
			XmlDocument xmlDocument = QuickSaver.TryLoadSnapshotXMLFromPath(text);
			if (xmlDocument != null)
			{
				xmlDocument.Save(text);
				Debug.Log("Saved unscrambled XML directly to " + text);
			}
		});
	}

	public static void ShareSnapshotCodeOnReddit(string snapshotName, string code, string imageURL)
	{
		string text = ScriptLocalization.Snapshot.RedditShareDescriptionP1 + " " + snapshotName + "\n\n" + ScriptLocalization.Snapshot.RedditShareTitleP2 + " " + code;
		if (!imageURL.NullOrEmpty())
		{
			text = text + "\n\n" + imageURL;
		}
		QuickSaver.CopyStringToClipboard(text);
		UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ShareableCodeClipboard, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
		GameState.GetInstance().StartCoroutine("OpenRedditUrlInASecond");
		AnalyticEvent.LevelSharedEvent(AnalyticEvent.ShareSite.Reddit);
	}

	public static void ShareSnapshotCodeOnImgur(string imageURL)
	{
		OpenURLWrapper.Open(imageURL);
	}

	public static void ShareSnapshotCodeOnTwitter(string snapshotName, string code, string imageURL)
	{
		string text = ScriptLocalization.Snapshot.TwitterShareP1 + " \"" + snapshotName + "\" " + ScriptLocalization.Snapshot.TwitterShareP2 + " " + code;
		if (!imageURL.NullOrEmpty())
		{
			int num = imageURL.IndexOf(".jpg");
			string text2 = imageURL;
			if (num >= 0)
			{
				text2 = imageURL.Remove(num, 4);
			}
			text = text + " " + text2;
		}
		OpenURLWrapper.Open("https://twitter.com/intent/tweet?text=" + WWW.EscapeURL(text));
		AnalyticEvent.LevelSharedEvent(AnalyticEvent.ShareSite.Reddit);
	}

	public void AddLoadingOperation()
	{
		loadingOperations++;
	}

	public void RemoveLoadingOperation()
	{
		loadingOperations--;
		if (loadingOperations < 0)
		{
			loadingOperations = 0;
		}
	}

	private void Update()
	{
		spinnyLoadingThing.enabled = loadingOperations > 0;
		if (GameSparksManager.Instance.MainUserIsAdmin && adminHideAcknowledgedCheckmark != null)
		{
			adminHideAcknowledgedCheckmark.enabled = adminHideAcknowledgedReports;
		}
		myLevelsQuickFilter.sortingFilter.restrictToUserId = GameSparksManager.Instance.MainUserGSID;
		if (advancedSearchBox.inputField.isFocused && !Controller.InputFieldIsActive)
		{
			ActivateAdvancedSearchField(null);
		}
		float num = 0f;
		foreach (KeyValuePair<Controller, float[]> rightStickValue in rightStickValues)
		{
			if (rightStickValue.Key != null && rightStickValue.Key.GetControlMask() != 0)
			{
				Character.Animals firstAssociatedCharacter = rightStickValue.Key.GetFirstAssociatedCharacter();
				Character characterFromAnimal = LevelSelectController.lastInstance.GetCharacterFromAnimal(firstAssociatedCharacter);
				if (characterFromAnimal != null && characterFromAnimal.InMenu)
				{
					num += rightStickValue.Value[0] + rightStickValue.Value[1];
				}
			}
		}
		quickInfoPane.scrollArrowController.autoScroll = num;
		if (PlatformFeatureRestrictions.HideOnlineContent != hiding_online_content)
		{
			hiding_online_content = PlatformFeatureRestrictions.HideOnlineContent;
			if (hiding_online_content)
			{
				for (int i = 0; i < slotPortals.Length; i++)
				{
					if (IsPortalPopulated((PickableBuildButton.LevelSlotEnum)i))
					{
						CustomLevelPortal customLevelPortal = slotPortals[i];
						if (customLevelPortal.snapshotInfo != null && !customLevelPortal.snapshotInfo.code.NullOrEmpty())
						{
							customLevelPortal.ClearContents();
						}
						ComputerSlots[i].ClearComputerSlotContents();
					}
				}
			}
			UpdateVisibility();
			TryRefreshCurrentScreen();
		}
		if (Input.GetKeyDown(KeyCode.F5))
		{
			TryRefreshCurrentScreen();
		}
	}

	private void TryRefreshCurrentScreen()
	{
		InventoryBook inventoryBook = codeEntryField.inventoryBook;
		if (currentQuery == null && loadingOperations == 0 && inventoryBook.ScreenMode && inventoryBook.CurrentScreenpage == inventoryBook.SecondScreenPage)
		{
			RefreshSearch(keepPage: true);
		}
	}

	public void PasteCodeFromClipboard()
	{
		string formattedSnapshotCode = GameSparksQuery.GetFormattedSnapshotCode(GUIUtility.systemCopyBuffer);
		if (!formattedSnapshotCode.NullOrEmpty())
		{
			ResetCodeEntryStatus();
			codeEntryField.inputField.text = formattedSnapshotCode;
			OnCodeEntryFieldSubmitted(formattedSnapshotCode);
		}
		else
		{
			UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.InvalidCode, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
			Debug.LogWarning("The clipboard doesn't seem to contain a valid code.");
		}
	}

	public void UpdateFeaturedLevelView(int startIndex)
	{
		Transform[] currentEntrySlots = GetCurrentEntrySlots();
		int numDisplaySlots = currentEntrySlots.Length;
		switch (currentFilter.filterType)
		{
		case FeaturedQuickFilter.FilterTypes.Local:
			ClearFeaturedLevelPage();
			CurrentPage = startIndex / numDisplaySlots;
			PopulateFeaturedLevelEntriesFromLocalSaves(startIndex);
			RefreshCurrentPage();
			break;
		case FeaturedQuickFilter.FilterTypes.Recent:
			ClearFeaturedLevelPage();
			CurrentPage = startIndex / numDisplaySlots;
			PopulateFeaturedLevelEntriesFromRecent(startIndex, RefreshCurrentPage);
			break;
		case FeaturedQuickFilter.FilterTypes.Favourites:
			ClearFeaturedLevelPage();
			CurrentPage = startIndex / numDisplaySlots;
			PopulateFeaturedLevelEntriesFromFavourites(startIndex, RefreshCurrentPage);
			break;
		case FeaturedQuickFilter.FilterTypes.Featured:
		case FeaturedQuickFilter.FilterTypes.Trending:
		case FeaturedQuickFilter.FilterTypes.Sorted:
		{
			if (loadingOperations != 0)
			{
				break;
			}
			MaskAllButTabs();
			AddLoadingOperation();
			ClearFeaturedLevelPage();
			GameSparksQuery searchQuery = GameSparksManager.Instance.CreateQuery();
			searchQuery.GetFeaturedLevelList(startIndex, numDisplaySlots, currentFilter);
			GameSparksQuery gameSparksQuery = searchQuery;
			gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate(GameSparksQuery q)
			{
				RemoveLoadingOperation();
				if (q == currentQuery)
				{
					currentQuery = null;
					PickableButton.ResetMasks();
					if (!q.HasError)
					{
						try
						{
							int num = (int)searchQuery.ResultData["totalEntries"];
							_ = (int)searchQuery.ResultData["returnedEntries"];
							int num2 = (int)searchQuery.ResultData["firstEntryIndex"];
							long num3 = (long)searchQuery.ResultData["date"];
							firstFeaturedEntryIndex = num2;
							totalFeaturedEntries = num;
							CurrentPage = firstFeaturedEntryIndex / numDisplaySlots;
							lastRefreshTimestamp = num3;
							List<FeaturedLevelData> list = searchQuery.ResultData["records"] as List<FeaturedLevelData>;
							featuredLevelEntries = list;
							RefreshCurrentPage();
							return;
						}
						catch (Exception ex)
						{
							Debug.LogError("Error while parsing response: " + ex.Message + "\n" + ex.StackTrace);
							UserMessageManager.Instance.UserMessage(LocalizationManager.GetTranslation("UndergroundComputer/ErrorWithSearch"), 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
							AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
							return;
						}
					}
					Debug.LogError("Error with search query: " + q.Error);
					UserMessageManager.Instance.UserMessage(LocalizationManager.GetTranslation("UndergroundComputer/ErrorWithSearch"), 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
					AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
				}
				else
				{
					Debug.Log("Ignoring result for old query");
				}
			});
			currentQuery = searchQuery;
			break;
		}
		}
		MemorizeBreadcrumbs();
	}

	public void OnClickQuickFilter(FeaturedQuickFilter.SortingFilter filter, int forcePage = 0)
	{
		bool mainUserIsAdmin = GameSparksManager.Instance.MainUserIsAdmin;
		FeaturedQuickFilter.SortingFilter sortingFilter = filter.Clone();
		sortingFilter.levelType = currentLevelType;
		sortingFilter.allowUnpublished = (mainUserIsAdmin ? allowUnpublished : (topPanelMode == FeaturedLevelTopPanelModes.MyLevels && allowUnpublished));
		sortingFilter.cutoffDays = cutoffDays;
		sortingFilter.lowerDifficultyBound = lowerDifficultyBound;
		sortingFilter.upperDifficultyBound = upperDifficultyBound;
		sortingFilter.showMods = showMods;
		UnityAction OnSearchTermsFiltered = delegate
		{
			currentFilter = sortingFilter;
			bool flag = PlatformFeatureRestrictions.IsUGCRestricted && currentFilter.filterType != FeaturedQuickFilter.FilterTypes.Local;
			if (!PlatformFeatureRestrictions.IsNotConnected && !PlatformFeatureRestrictions.MustHideAllUGC && !flag)
			{
				UpdateFeaturedLevelView(forcePage * GetCurrentEntrySlots().Length);
			}
		};
		string text = SanitizeSearchQuery(advancedSearchBox.inputField.text);
		if (lastFilteredSearchQuery == text)
		{
			sortingFilter.searchTerms = lastFilteredSearchQueryResult;
			OnSearchTermsFiltered();
		}
		else if (WordFilter.PlatformHasWordFilter)
		{
			lastFilteredSearchQuery = text;
			WordFilter.FilterText(this, text, delegate(string filteredText)
			{
				filteredText = filteredText.Replace('*', ' ');
				filteredText = SanitizeSearchQuery(filteredText);
				lastFilteredSearchQueryResult = filteredText;
				OnSearchTermsFiltered();
			});
		}
		else
		{
			lastFilteredSearchQuery = text;
			lastFilteredSearchQueryResult = text;
			OnSearchTermsFiltered();
		}
	}

	public void SetTopPanelMode(FeaturedLevelTopPanelModes mode)
	{
		topPanelMode = mode;
		if (mode == FeaturedLevelTopPanelModes.PlayerLevels || mode == FeaturedLevelTopPanelModes.MyLevels)
		{
			playerLevelsContainer.gameObject.SetActive(value: true);
			return;
		}
		playerLevelsContainer.gameObject.SetActive(value: false);
		lastDisplayedUserInfo = null;
	}

	public void RefreshSnapshotMetadata(SnapshotEntry snapshotEntry, UnityAction<bool> OnRefresh)
	{
		if (!(snapshotEntry != null) || snapshotEntry.Code.NullOrEmpty())
		{
			return;
		}
		GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
		query.GetLevelInfo(new List<string> { snapshotEntry.Code });
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			if (!query.HasError)
			{
				if (query.ResultData["records"] is List<FeaturedLevelData> { Count: >0 } list)
				{
					snapshotEntry.Initialize(list[0], ShouldShowPublishedMark && list[0].isPublished);
					OnRefresh(arg0: true);
				}
				else
				{
					Debug.LogError("No records returned...");
					OnRefresh(arg0: false);
				}
			}
			else
			{
				Debug.LogError("Could not get level info...");
				OnRefresh(arg0: false);
			}
		});
	}

	private void ShowQuickLevelInfo(SnapshotEntry snapshotEntry)
	{
		if (snapshotEntry != null)
		{
			featuredQuickInfoEnabled = true;
			if (snapshotEntry.featuredLevelData != null && !snapshotEntry.featuredLevelData.isLocal)
			{
				if (snapshotEntry.featuredLevelData.archived)
				{
					quickInfoPane.SetArchivedLevelInfo(snapshotEntry.featuredLevelData);
				}
				else
				{
					quickInfoPane.SetSnapshotInfo(snapshotEntry.featuredLevelData);
				}
			}
			else
			{
				quickInfoPane.SetLocalSaveInfo(snapshotEntry);
			}
		}
		else
		{
			featuredQuickInfoEnabled = false;
		}
		quickInfoPane.Show(featuredQuickInfoEnabled);
		quickInfoPane.removeRecentCodeContainer.gameObject.SetActive(IsTabHighlighted(PickableBuildButton.BuildButtonJobs.RecentTab));
		UpdateVisibility();
		MemorizeBreadcrumbs();
	}

	public void SetFeaturedLevelMode(FeaturedQuickFilter.LevelTypes levelType, bool refreshSearch)
	{
		currentLevelType = levelType;
		if (currentFilter == null)
		{
			return;
		}
		currentFilter.levelType = levelType;
		if (!refreshSearch)
		{
			return;
		}
		if (currentFilter.filterType == FeaturedQuickFilter.FilterTypes.Local)
		{
			RefreshLocalSavesList(keepPage: false, delegate
			{
				RefreshSearch(keepPage: false);
			});
		}
		else
		{
			RefreshSearch(keepPage: false);
		}
	}

	public void RefreshSearch(bool keepPage)
	{
		UpdateFeaturedLevelView(keepPage ? firstFeaturedEntryIndex : 0);
	}

	private void ClearFeaturedLevelPage()
	{
		if (PickableBuildButton.SelectedEntry != null)
		{
			PickableBuildButton.DeselectLastSelected();
		}
		Transform[] currentEntrySlots = GetCurrentEntrySlots();
		foreach (Transform obj in currentEntrySlots)
		{
			SnapshotEntry componentInChildren = obj.GetComponentInChildren<SnapshotEntry>();
			if (componentInChildren != null)
			{
				componentInChildren.ClearTip();
				if (componentInChildren.thumbnailImage.texture != null)
				{
					LevelThumbnailCache.Instance.RemoveTextureUser((Texture2D)componentInChildren.thumbnailImage.texture, componentInChildren);
					componentInChildren.thumbnailImage.texture = null;
					componentInChildren.thumbnailImage.enabled = false;
				}
			}
			obj.gameObject.SetActive(value: false);
		}
	}

	public void ShowLevelsByPlayer(string playerName, string playerPlatformId, FeaturedQuickFilter.SortingFilter filter, LobbyPlayer.SocialPlatform socialPlatform, bool isAnonymous, bool isMe, int startPage)
	{
		if (!isMe)
		{
			lastDisplayedUserInfo = new UserInfoPopup.UserInfo
			{
				username = playerName,
				platformID = playerPlatformId,
				platform = socialPlatform,
				GSID = filter.restrictToUserId,
				GSID_old = filter.restrictToGSID,
				shouldBeAnonymous = isAnonymous
			};
		}
		playerLevelsNameTag.Initialize(playerName, playerPlatformId, filter.restrictToUserId, socialPlatform, isAnonymous);
		previousFilter = currentFilter.Clone();
		previousPage = CurrentPage;
		previousPanelMode = topPanelMode;
		if (isMe)
		{
			SetTopPanelMode(FeaturedLevelTopPanelModes.MyLevels);
		}
		else
		{
			SetTopPanelMode(FeaturedLevelTopPanelModes.PlayerLevels);
		}
		OnClickQuickFilter(filter, startPage);
	}

	public void ShowMyLevels(FeaturedQuickFilter.SortingFilter sortingFilter)
	{
		LobbyPlayer lobbyPlayerByGSID = LobbyManager.instance.GetLobbyPlayerByGSID(sortingFilter.restrictToUserId);
		if (lobbyPlayerByGSID != null)
		{
			ShowLevelsByPlayer(lobbyPlayerByGSID.playerName, lobbyPlayerByGSID.platformUniqueID, sortingFilter, LobbyPlayer.LocalMachinePlatform, isAnonymous: false, isMe: true, 0);
		}
		else
		{
			Debug.LogError("Could not find LobbyPlayer for GSID " + sortingFilter.restrictToUserId);
		}
	}

	public void CloseLevelsByPlayer()
	{
		SetTopPanelMode(previousPanelMode);
		PickableBuildButton.DeselectLastSelected();
		if (previousFilter != null)
		{
			OnClickQuickFilter(previousFilter, previousPage);
			previousFilter = null;
			previousPanelMode = FeaturedLevelTopPanelModes.QuickFilters;
			previousPage = 0;
		}
		else
		{
			OnSelectFeaturedLevelTab(refreshCurrentPage: true, resetFilters: true);
		}
	}

	private DisplaySlotPageDef GetCurrentSlotPageDef()
	{
		DisplaySlotPageDef value = null;
		perPageDisplaySlotDefs.TryGetValue(PickableBuildButton.buildMenuCurrentState, out value);
		return value;
	}

	private Transform[] GetCurrentEntrySlots()
	{
		DisplaySlotPageDef currentSlotPageDef = GetCurrentSlotPageDef();
		if (currentSlotPageDef != null)
		{
			switch (currentViewMode)
			{
			case ViewModes.Grid:
				return currentSlotPageDef.gridViewSlotDef.slots;
			case ViewModes.List:
				return currentSlotPageDef.listViewSlotDef.slots;
			}
		}
		return null;
	}

	public void SetFeaturedViewMode(ViewModes viewMode, bool refreshSearch)
	{
		currentViewMode = viewMode;
		UpdateVisibility();
		if (refreshSearch)
		{
			RefreshSearch(keepPage: false);
		}
	}

	public void OpenShareDialog(PickableBuildButton button)
	{
		SnapshotEntry component = button.GetComponent<SnapshotEntry>();
		if (component != null)
		{
			previousTab = PickableBuildButton.buildMenuCurrentState;
			PickableBuildButton.buildMenuCurrentState = PickableBuildButton.BuildScreenStates.ShareDialog;
			UpdateVisibility();
			shareDialog.DisplayLevelData(component.Code, component.SnapshotName);
		}
	}

	public void CloseShareDialog()
	{
		PickableBuildButton.buildMenuCurrentState = previousTab;
		UpdateVisibility();
		shareDialog.OnClose();
		UpdateSingleEntry(PickableBuildButton.SelectedEntry);
	}

	public void OpenViewReportsDialog(PickableBuildButton button)
	{
		SnapshotEntry component = button.GetComponent<SnapshotEntry>();
		if (component != null)
		{
			previousTab = PickableBuildButton.buildMenuCurrentState;
			PickableBuildButton.buildMenuCurrentState = PickableBuildButton.BuildScreenStates.ViewReportsDialog;
			UpdateVisibility();
			viewReportsDialog.DisplayLevelData(component.Code, component.SnapshotName);
		}
	}

	public void CloseViewReportsDialog()
	{
		PickableBuildButton.buildMenuCurrentState = previousTab;
		UpdateVisibility();
		viewReportsDialog.OnClose();
	}

	public static string TimeToString(long totalSeconds)
	{
		if (totalSeconds < 60)
		{
			return LocalizationManager.GetTranslation("UndergroundComputer/Time/JustNow");
		}
		if (totalSeconds < 3600)
		{
			return PluralFromCount((int)(totalSeconds / 60), " " + LocalizationManager.GetTranslation("UndergroundComputer/Time/Min"), " " + LocalizationManager.GetTranslation("UndergroundComputer/Time/Mins"));
		}
		if (totalSeconds < 86400)
		{
			return PluralFromCount((int)(totalSeconds / 3600), " " + LocalizationManager.GetTranslation("UndergroundComputer/Time/Hr"), " " + LocalizationManager.GetTranslation("UndergroundComputer/Time/Hrs"));
		}
		return PluralFromCount((int)(totalSeconds / 86400), " " + LocalizationManager.GetTranslation("UndergroundComputer/Time/Day"), " " + LocalizationManager.GetTranslation("UndergroundComputer/Time/Days"));
	}

	public static string PluralFromCount(int count, string singular, string plural)
	{
		return count + ((count == 1) ? singular : plural);
	}

	public void OnClickUpvote()
	{
		CastVote(1);
	}

	public void OnClickDownvote()
	{
		CastVote(-1);
	}

	private void CastVote(int vote)
	{
		if (!(PickableBuildButton.SelectedEntry != null))
		{
			return;
		}
		SnapshotEntry snapshotEntry = PickableBuildButton.SelectedEntry.GetComponent<SnapshotEntry>();
		if (!(snapshotEntry != null) || snapshotEntry.featuredLevelData == null)
		{
			return;
		}
		AddLoadingOperation();
		PickableButton.maskAll = true;
		GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			RemoveLoadingOperation();
			PickableButton.ResetMasks();
			if (!query.HasError)
			{
				int num = (int)query.ResultData["newRating"];
				int myVote = (int)query.ResultData["myVote"];
				snapshotEntry.featuredLevelData.rating = num;
				snapshotEntry.featuredLevelData.myVote = myVote;
				quickInfoPane.SetVoteInfo(num, myVote);
			}
			else
			{
				Debug.LogError("Error casting vote: " + query.Error);
			}
		});
		if (snapshotEntry.featuredLevelData.myVote == vote)
		{
			query.CastLevelVote(snapshotEntry.featuredLevelData.code, 0);
		}
		else
		{
			query.CastLevelVote(snapshotEntry.featuredLevelData.code, vote);
		}
	}

	public void OnClickAllowUnpublishedToggle()
	{
		SetAllowUnpublishedToggle(!allowUnpublished, refreshSearch: true);
	}

	private void SetAllowUnpublishedToggle(bool value, bool refreshSearch)
	{
		allowUnpublished = value;
		if (currentFilter != null)
		{
			currentFilter.allowUnpublished = allowUnpublished;
		}
		if (allowUnpublished)
		{
			allowUnpublishedAdminText.color = new Color(allowUnpublishedAdminText.color.r, allowUnpublishedAdminText.color.g, allowUnpublishedAdminText.color.b, 0.5f);
			showPublishedYesImage.gameObject.SetActive(value: false);
		}
		else
		{
			allowUnpublishedAdminText.color = new Color(allowUnpublishedAdminText.color.r, allowUnpublishedAdminText.color.g, allowUnpublishedAdminText.color.b, 1f);
			showPublishedYesImage.gameObject.SetActive(value: true);
		}
		UpdateVisibility();
		if (refreshSearch)
		{
			RefreshSearch(keepPage: false);
		}
	}

	public void OnClickShowModsToggle()
	{
		int val = (showMods + 1) % 3;
		SetShowMods(val, refreshSearch: true);
	}

	private void SetShowMods(int val, bool refreshSearch)
	{
		showMods = val;
		if (currentFilter != null)
		{
			currentFilter.showMods = val;
		}
		switch (val)
		{
		case 0:
			showModsTipText.text = ScriptLocalization.UndergroundComputer.ShowAllLevels;
			showModsOverlayNoImage.gameObject.SetActive(value: false);
			showModsOverlayYesImage.gameObject.SetActive(value: false);
			break;
		case 1:
			showModsTipText.text = ScriptLocalization.UndergroundComputer.ShowOnlyModifiers;
			showModsOverlayYesImage.gameObject.SetActive(value: true);
			showModsOverlayNoImage.gameObject.SetActive(value: false);
			break;
		case 2:
			showModsTipText.text = ScriptLocalization.UndergroundComputer.ShowWithoutModifiers;
			showModsOverlayNoImage.gameObject.SetActive(value: true);
			showModsOverlayYesImage.gameObject.SetActive(value: false);
			break;
		}
		UpdateVisibility();
		if (refreshSearch)
		{
			RefreshSearch(keepPage: false);
		}
	}

	private void UpdateSingleEntry(PickableBuildButton selectedEntry)
	{
	}

	public void OpenReportDialog(PickableBuildButton button)
	{
		SnapshotEntry component = button.GetComponent<SnapshotEntry>();
		if (component != null)
		{
			previousTab = PickableBuildButton.buildMenuCurrentState;
			PickableBuildButton.buildMenuCurrentState = PickableBuildButton.BuildScreenStates.ReportDialog;
			UpdateVisibility();
			reportDialog.DisplayLevelInfo(component.Code, component.featuredLevelData.name);
		}
	}

	public void CloseReportDialog()
	{
		PickableBuildButton.buildMenuCurrentState = previousTab;
		UpdateVisibility();
		reportDialog.OnClose();
		UpdateSingleEntry(PickableBuildButton.SelectedEntry);
	}

	public void OpenDeleteDialog(PickableBuildButton button)
	{
		SnapshotEntry component = button.GetComponent<SnapshotEntry>();
		if (component != null)
		{
			previousTab = PickableBuildButton.buildMenuCurrentState;
			PickableBuildButton.buildMenuCurrentState = PickableBuildButton.BuildScreenStates.DeleteDialog;
			UpdateVisibility();
			deleteDialogue.DisplayLevelInfo(component.Code, component.featuredLevelData.name);
		}
	}

	public void CloseDeleteDialog()
	{
		PickableBuildButton.buildMenuCurrentState = previousTab;
		UpdateVisibility();
		deleteDialogue.OnClose();
		UpdateSingleEntry(PickableBuildButton.SelectedEntry);
	}

	public void OnClickDeleteConfirm()
	{
		CloseDeleteDialog();
		if (!(PickableBuildButton.SelectedEntry != null))
		{
			return;
		}
		SnapshotEntry component = PickableBuildButton.SelectedEntry.GetComponent<SnapshotEntry>();
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		if (saveFileDataForMainUser.IsFavorite(component.SnapshotName, component.Code))
		{
			saveFileDataForMainUser.RemoveFavoriteSnapshotCode(component.SnapshotName, component.Code);
		}
		foreach (PickableBuildButton.LevelSlotEnum item in GetSlotsContainingSnapshot(component.Code, component.SnapshotName))
		{
			OnClickClearCode(item);
		}
		PickableBuildButton.DeselectLastSelected();
		RefreshSearch(keepPage: true);
	}

	public void OnMainFilterDropdownValueChange()
	{
		RefreshMainFilter();
	}

	public void RefreshMainFilter(int gotoPage = -1)
	{
		int num = 0;
		bool keepPage = false;
		if (gotoPage != -1)
		{
			keepPage = true;
			CurrentPage = gotoPage;
			Transform[] currentEntrySlots = GetCurrentEntrySlots();
			num = gotoPage * currentEntrySlots.Length;
		}
		else
		{
			CurrentPage = 0;
		}
		if (adminFlaggedLevelControls != null)
		{
			adminFlaggedLevelControls.gameObject.SetActive(value: false);
		}
		if (!(mainFilterDropdown.selectedDropdownEntry != null))
		{
			return;
		}
		levelTypeDropdown.SetClickDisabled(disabled: false);
		dateCutoffDropdown.SetClickDisabled(disabled: false);
		difficultyDropdown.SetClickDisabled(disabled: false);
		FeaturedSpecialFilter component = mainFilterDropdown.selectedDropdownEntry.GetComponent<FeaturedSpecialFilter>();
		advancedSearchBox.gameObject.SetActive(component == null);
		FeaturedDisableDropdowns component2 = mainFilterDropdown.selectedDropdownEntry.GetComponent<FeaturedDisableDropdowns>();
		if (component2 != null && component2.dropdownsToDisable != null)
		{
			foreach (DropdownMenu item in component2.dropdownsToDisable)
			{
				item.SetClickDisabled(disabled: true);
				item.SelectEntryByIndex(0, triggerOnChangeEvent: false);
			}
		}
		FeaturedQuickFilter component3 = mainFilterDropdown.selectedDropdownEntry.GetComponent<FeaturedQuickFilter>();
		if (component3 != null)
		{
			if (component3.sortingFilter.restrictToUserId.NullOrEmpty())
			{
				SetTopPanelMode(FeaturedLevelTopPanelModes.QuickFilters);
				OnClickQuickFilter(component3.sortingFilter, CurrentPage);
			}
			else
			{
				ShowMyLevels(component3.sortingFilter);
			}
			return;
		}
		if (component != null)
		{
			FeaturedSpecialFilter.SpecialFilterType type = component.type;
			if ((uint)type <= 2u)
			{
				SetTopPanelMode(FeaturedLevelTopPanelModes.QuickFilters);
			}
		}
		FeaturedAdminFilter component4 = mainFilterDropdown.selectedDropdownEntry.GetComponent<FeaturedAdminFilter>();
		if (component4 != null)
		{
			SetTopPanelMode(FeaturedLevelTopPanelModes.QuickFilters);
			OnClickQuickFilter(component4.sortingFilter);
			if (adminFlaggedLevelControls != null)
			{
				adminFlaggedLevelControls.gameObject.SetActive(component4.filterType == FeaturedAdminFilter.FilterType.Flagged);
			}
			return;
		}
		if (component != null)
		{
			switch (component.type)
			{
			case FeaturedSpecialFilter.SpecialFilterType.Local:
				currentFilter = new FeaturedQuickFilter.SortingFilter
				{
					filterType = FeaturedQuickFilter.FilterTypes.Local,
					levelType = currentLevelType,
					infoLine1 = FeaturedQuickFilter.InfoLineTypes.LevelType,
					infoLine2 = FeaturedQuickFilter.InfoLineTypes.None
				};
				if (!PlatformFeatureRestrictions.MustHideAllUGC)
				{
					RefreshLocalSavesList(keepPage, RefreshCurrentPage);
				}
				break;
			case FeaturedSpecialFilter.SpecialFilterType.Recent:
				currentFilter = new FeaturedQuickFilter.SortingFilter
				{
					filterType = FeaturedQuickFilter.FilterTypes.Recent,
					levelType = currentLevelType
				};
				if (!PlatformFeatureRestrictions.IsNotConnected && !PlatformFeatureRestrictions.MustHideAllUGC)
				{
					PopulateFeaturedLevelEntriesFromRecent(num, RefreshCurrentPage);
				}
				break;
			case FeaturedSpecialFilter.SpecialFilterType.Favourites:
				currentFilter = new FeaturedQuickFilter.SortingFilter
				{
					filterType = FeaturedQuickFilter.FilterTypes.Favourites,
					levelType = currentLevelType
				};
				if (!PlatformFeatureRestrictions.IsNotConnected && !PlatformFeatureRestrictions.MustHideAllUGC)
				{
					PopulateFeaturedLevelEntriesFromFavourites(num, RefreshCurrentPage);
				}
				break;
			}
		}
		mainFilterDropdown.SetClickDisabled(currentFilter != null && currentFilter.IsSpecialFilterType);
	}

	public void UpdateLevelTypeFromDropdownValues(bool refreshPage)
	{
		DropdownEntry selectedDropdownEntry = levelTypeDropdown.selectedDropdownEntry;
		if (selectedDropdownEntry != null)
		{
			FeaturedLevelTypeDropdownData component = selectedDropdownEntry.GetComponent<FeaturedLevelTypeDropdownData>();
			if (component != null)
			{
				SetFeaturedLevelMode(component.levelType, refreshPage);
			}
		}
	}

	public void OnLevelTypeDropdownValueChange()
	{
		UpdateLevelTypeFromDropdownValues(refreshPage: true);
	}

	private void UpdateDateCutoffFromDropdownValues()
	{
		DropdownEntry selectedDropdownEntry = dateCutoffDropdown.selectedDropdownEntry;
		if (selectedDropdownEntry != null)
		{
			FeaturedDateCutoffDropdownData component = selectedDropdownEntry.GetComponent<FeaturedDateCutoffDropdownData>();
			if (component != null)
			{
				cutoffDays = component.days;
			}
			else
			{
				cutoffDays = 0;
			}
		}
		else
		{
			cutoffDays = 0;
		}
		currentFilter.cutoffDays = cutoffDays;
	}

	public void OnDateCutoffDropdownValueChange()
	{
		UpdateDateCutoffFromDropdownValues();
		RefreshSearch(keepPage: false);
	}

	private void UpdateDifficultyFromDropdownValues()
	{
		DropdownEntry selectedDropdownEntry = difficultyDropdown.selectedDropdownEntry;
		if (selectedDropdownEntry != null)
		{
			FeaturedDifficultyDropdownData component = selectedDropdownEntry.GetComponent<FeaturedDifficultyDropdownData>();
			lowerDifficultyBound = component.lowerLimit;
			upperDifficultyBound = component.upperLimit;
		}
		else
		{
			lowerDifficultyBound = 0f;
			upperDifficultyBound = 1f;
		}
		currentFilter.lowerDifficultyBound = lowerDifficultyBound;
		currentFilter.upperDifficultyBound = upperDifficultyBound;
	}

	public void OnDifficultyDropdownValueChange()
	{
		UpdateDifficultyFromDropdownValues();
		RefreshSearch(keepPage: false);
	}

	public void ActivateAdvancedSearchField(Cursor pickcursor)
	{
		Controller.LockInputField(advancedSearchBox.inputField, OnAdvancedSearchFieldSubmitted);
		PickableButton.AllowOnlyButtons(advancedSearchBox, advancedSearchButton);
		SteamDeck.OpenVirtualKeyboard(pickcursor);
	}

	private string SanitizeSearchQuery(string str)
	{
		str = str.CompactWhitespaces();
		if (str.NullOrEmpty() || str.Length <= 1)
		{
			str = "";
		}
		return str;
	}

	public void OnAdvancedSearchFieldSubmitted(string str)
	{
		UnityAction<string> unityAction = delegate(string filteredText)
		{
			filteredText = filteredText.Replace('*', ' ');
			lastFilteredSearchQueryResult = filteredText;
			PickableButton.ResetMasks();
			advancedSearchBox.inputField.onEndEdit.RemoveAllListeners();
			DoAdvancedSearch(filteredText);
		};
		str = SanitizeSearchQuery(str);
		lastFilteredSearchQuery = str;
		if (str.NullOrEmpty())
		{
			lastFilteredSearchQuery = "";
			lastFilteredSearchQueryResult = "";
			advancedSearchBox.inputField.text = "";
			DoAdvancedSearch("");
			PickableButton.ResetMasks();
		}
		else if (WordFilter.PlatformHasWordFilter)
		{
			WordFilter.FilterText(this, str, unityAction);
		}
		else
		{
			unityAction(str);
		}
	}

	public void OnClickAdvancedSearchErase()
	{
		advancedSearchBox.inputField.text = "";
		DoAdvancedSearch("");
	}

	public void DoAdvancedSearch(string str)
	{
		if (currentFilter != null)
		{
			if (currentFilter.searchTerms != str)
			{
				currentFilter = currentFilter.Clone();
				currentFilter.searchTerms = str;
				OnClickQuickFilter(currentFilter);
			}
		}
		else
		{
			Debug.LogError("Advanced search: no current filter???");
		}
	}

	public bool IsTabHighlighted(PickableBuildButton.BuildButtonJobs job)
	{
		if (PickableBuildButton.buildMenuCurrentState == PickableBuildButton.BuildScreenStates.FeaturedLevelTab)
		{
			FeaturedSpecialFilter component = mainFilterDropdown.selectedDropdownEntry.GetComponent<FeaturedSpecialFilter>();
			switch (job)
			{
			default:
				return component == null;
			case PickableBuildButton.BuildButtonJobs.RecentTab:
				if (component != null)
				{
					return component.type == FeaturedSpecialFilter.SpecialFilterType.Recent;
				}
				return false;
			case PickableBuildButton.BuildButtonJobs.FavoritesTab:
				if (component != null)
				{
					return component.type == FeaturedSpecialFilter.SpecialFilterType.Favourites;
				}
				return false;
			case PickableBuildButton.BuildButtonJobs.LocalSaves:
				if (component != null)
				{
					return component.type == FeaturedSpecialFilter.SpecialFilterType.Local;
				}
				return false;
			}
		}
		return false;
	}

	private void MaskAllButTabs()
	{
		PickableButton[] buttons = tabs;
		PickableButton.AllowOnlyButtons(buttons);
	}

	public void OnClickRetryConnectButton()
	{
		if (!GameSparksManager.Instance.Connecting)
		{
			GameSparksManager.Instance.ConnectNow();
		}
	}

	public void PopupNameOptions(List<UserInfoPopup.UserInfo> users)
	{
		Canvas componentInChildren = GetComponentInChildren<Canvas>();
		if (componentInChildren != null)
		{
			userInfoPopup = componentInChildren.gameObject.AddPrefabAsChild<UserInfoPopup>(userInfoPopupPrefab);
			userInfoPopup.Show(users, this);
		}
		else
		{
			Debug.LogError("PopupNameOptions: No Canvas element found...");
		}
	}

	public void BackOutOfSubmenu(bool autoClosing = false)
	{
		if (DropdownMenu.dropdownDeployed)
		{
			GameEventManager.SendEvent(new PickCursorClickedBackgroundEvent());
		}
		else if (userInfoPopup != null)
		{
			UnityEngine.Object.Destroy(userInfoPopup.gameObject);
		}
		else
		{
			if (!IsInSubmenu)
			{
				return;
			}
			switch (PickableBuildButton.buildMenuCurrentState)
			{
			case PickableBuildButton.BuildScreenStates.ViewReportsDialog:
				CloseViewReportsDialog();
				break;
			case PickableBuildButton.BuildScreenStates.ShareDialog:
				CloseShareDialog();
				break;
			case PickableBuildButton.BuildScreenStates.ReportDialog:
				CloseReportDialog();
				break;
			case PickableBuildButton.BuildScreenStates.DeleteDialog:
				CloseDeleteDialog();
				break;
			case PickableBuildButton.BuildScreenStates.AdminPanelDialog:
				if (adminPanelDialog.currentState.subDialog == AdminPanelDialog.SubDialog.IndexPage)
				{
					CloseAdminDialog();
				}
				else
				{
					adminPanelDialog.OnBack();
				}
				break;
			default:
				if (!autoClosing)
				{
					if (featuredQuickInfoEnabled)
					{
						PickableBuildButton.DeselectLastSelected();
					}
					else if (lastDisplayedUserInfo != null)
					{
						CloseLevelsByPlayer();
					}
				}
				break;
			}
			UpdatePageIndicator();
		}
	}

	private void CloseComputerAndWarpToPortal(PickCursor pickCursor, int portalIdx = 0)
	{
		pickCursor.InventoryBookMenu.ForceClose();
		if (pickCursor.LocalPlayer != null && pickCursor.LocalPlayer.PlayerCharacter != null)
		{
			Vector3 position = LevelSelectController.lastInstance.snapshotPortals[portalIdx].transform.position;
			position += new Vector3(0f, 2f, 0f);
			pickCursor.LocalPlayer.PlayerCharacter.transform.position = position;
		}
	}

	public void OnClickPlayNow(PickCursor pickCursor, bool suggested = false)
	{
		if (currentQuery != null || WaitingForFileOperationOnCurrentPage)
		{
			return;
		}
		Action<bool, int> action = delegate(bool success, int portalIDX)
		{
			if (success)
			{
				CloseComputerAndWarpToPortal(pickCursor, portalIDX);
			}
		};
		Action<bool> onFinish = delegate(bool success)
		{
			if (success)
			{
				CloseComputerAndWarpToPortal(pickCursor);
			}
		};
		if (PickableBuildButton.SelectedEntry != null)
		{
			SnapshotEntry component = PickableBuildButton.SelectedEntry.GetComponent<SnapshotEntry>();
			if (component != null)
			{
				if (suggested && GameSettings.GetInstance().GameMode != quickPlayMode)
				{
					MsgSwitchToMode msgSwitchToMode = new MsgSwitchToMode();
					msgSwitchToMode.toMode = quickPlayMode;
					NetworkServer.SendToAll(NetMsgTypes.SwitchToMode, msgSwitchToMode);
				}
				if (SnapshotAlreadyLoadedInSlot(PickableBuildButton.SelectedEntry, PickableBuildButton.LevelSlotEnum.A))
				{
					action(arg1: true, 0);
				}
				else if (SnapshotAlreadyLoadedInSlot(PickableBuildButton.SelectedEntry, PickableBuildButton.LevelSlotEnum.B))
				{
					action(arg1: true, 1);
				}
				else if (SnapshotAlreadyLoadedInSlot(PickableBuildButton.SelectedEntry, PickableBuildButton.LevelSlotEnum.C))
				{
					action(arg1: true, 2);
				}
				else if (SnapshotAlreadyLoadedInSlot(PickableBuildButton.SelectedEntry, PickableBuildButton.LevelSlotEnum.D))
				{
					action(arg1: true, 3);
				}
				else if (component.featuredLevelData != null && !component.featuredLevelData.isLocal)
				{
					LoadCurrentRecentCodeIntoSlot(PickableBuildButton.SelectedEntry, PickableBuildButton.LevelSlotEnum.A, selectEntry: false, onFinish);
				}
				else
				{
					LoadCurrentLocalSaveIntoSlot(PickableBuildButton.SelectedEntry, PickableBuildButton.LevelSlotEnum.A, selectEntry: false, onFinish);
				}
			}
		}
		else if (currentCodeValidated)
		{
			if (CodeAlreadyLoadedInSlot(GameSparksQuery.SanitizeSnapshotCode(codeEntryField.inputField.text), PickableBuildButton.LevelSlotEnum.A))
			{
				action(arg1: true, 0);
			}
			else
			{
				LoadCurrentCodeIntoSlot(PickableBuildButton.LevelSlotEnum.A, selectEntry: false, onFinish);
			}
		}
	}

	public void OnClickSaveArchivedLevel()
	{
		if (currentCodeValidated && currentCodeArchived && !currentCodeAssociatedXml.NullOrEmpty())
		{
			SaveLocalCopy(currentCodeAssociatedName, codeEntryField.inputField.text, currentCodeAssociatedXml, FeaturedQuickFilter.LevelTypes.Any);
		}
	}

	public void OnClickOpenAdminDialog()
	{
		if (adminPanelDialog != null && GameSparksManager.Instance.MainUserIsAdmin && PickableBuildButton.buildMenuCurrentState != PickableBuildButton.BuildScreenStates.AdminPanelDialog)
		{
			previousTab = PickableBuildButton.buildMenuCurrentState;
			PickableBuildButton.buildMenuCurrentState = PickableBuildButton.BuildScreenStates.AdminPanelDialog;
			UpdateVisibility();
			adminPanelDialog.Initialize(this);
		}
	}

	public void CloseAdminDialog()
	{
		if (adminPanelDialog != null)
		{
			PickableBuildButton.buildMenuCurrentState = previousTab;
			UpdateVisibility();
			adminPanelDialog.OnClose();
		}
	}

	public static void SuggestQuickPlayMode(GameState.GameMode mode)
	{
		if (GameState.GetInstance().UsingHotSeat)
		{
			quickPlayMode = GameState.GameMode.CREATIVE;
		}
		else
		{
			quickPlayMode = mode;
		}
	}

	public static void NextQuickPlayMode()
	{
		if (GameState.GetInstance().UsingHotSeat)
		{
			quickPlayMode = GameState.GameMode.CREATIVE;
			return;
		}
		switch (quickPlayMode)
		{
		case GameState.GameMode.FREEPLAY:
			quickPlayMode = GameState.GameMode.PARTY;
			break;
		case GameState.GameMode.PARTY:
			quickPlayMode = GameState.GameMode.CREATIVE;
			break;
		case GameState.GameMode.CREATIVE:
			quickPlayMode = GameState.GameMode.CHALLENGE;
			break;
		case GameState.GameMode.CHALLENGE:
			quickPlayMode = GameState.GameMode.FREEPLAY;
			break;
		}
	}

	public static void PrevQuickPlayMode()
	{
		if (GameState.GetInstance().UsingHotSeat)
		{
			quickPlayMode = GameState.GameMode.CREATIVE;
			return;
		}
		switch (quickPlayMode)
		{
		case GameState.GameMode.FREEPLAY:
			quickPlayMode = GameState.GameMode.CHALLENGE;
			break;
		case GameState.GameMode.CHALLENGE:
			quickPlayMode = GameState.GameMode.CREATIVE;
			break;
		case GameState.GameMode.CREATIVE:
			quickPlayMode = GameState.GameMode.PARTY;
			break;
		case GameState.GameMode.PARTY:
			quickPlayMode = GameState.GameMode.FREEPLAY;
			break;
		}
	}

	public void OnAdminApprovalStatusDropdownValueChange()
	{
		currentFilter.approvalStatusFilter = adminApprovalStatusDropdown.selectedDropdownEntry.EntryValue;
		RefreshSearch(keepPage: true);
	}

	public void OnClickAdminHideAcknowledged(PickCursor pickCursor)
	{
		adminHideAcknowledgedReports = !adminHideAcknowledgedReports;
		if (currentFilter != null)
		{
			currentFilter.hideAcknowledged = adminHideAcknowledgedReports;
		}
		RefreshSearch(keepPage: true);
	}

	public void MemorizeBreadcrumbs()
	{
		Breadcrumbs breadcrumbs = new Breadcrumbs();
		breadcrumbs.selectedTab = PickableBuildButton.buildMenuCurrentState;
		breadcrumbs.pageNumber = CurrentPage;
		breadcrumbs.leaderboardNumPlayers = FeaturedQuickInfoPane.lastLeaderboardNumPlayers;
		breadcrumbs.mainDropdownIndex = mainFilterDropdown.GetSelectedEntryIndex();
		breadcrumbs.dateCutoffDropdownIndex = dateCutoffDropdown.GetSelectedEntryIndex();
		breadcrumbs.difficultyDropdownIndex = difficultyDropdown.GetSelectedEntryIndex();
		breadcrumbs.levelTypeDropdownIndex = levelTypeDropdown.GetSelectedEntryIndex();
		breadcrumbs.showMods = showMods;
		if (breadcrumbs.selectedTab == PickableBuildButton.BuildScreenStates.LevelCodesTab)
		{
			string text = codeEntryField.inputField.text;
			breadcrumbs.levelCode = GameSparksQuery.SanitizeSnapshotCode(text);
		}
		else
		{
			if (lastDisplayedUserInfo != null)
			{
				breadcrumbs.playerInfo = lastDisplayedUserInfo;
			}
			if (featuredQuickInfoEnabled && PickableBuildButton.SelectedEntry != null)
			{
				breadcrumbs.showingInfoPane = true;
				SnapshotEntry component = PickableBuildButton.SelectedEntry.GetComponent<SnapshotEntry>();
				if (component != null && component.featuredLevelData != null)
				{
					breadcrumbs.levelCode = GameSparksQuery.SanitizeSnapshotCode(component.featuredLevelData.code);
					breadcrumbs.localFilename = component.featuredLevelData.name;
				}
			}
		}
		lastComputerState = breadcrumbs;
	}
}
