using GameEvent;
using I2.Loc;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PickableBuildButton : PickableButton
{
	public enum BuildButtonJobs
	{
		LevelSlot = 0,
		LoadSlot = 200,
		LevelCodes = 10,
		LocalSaves = 11,
		FeaturedLevels = 12,
		RecentTab = 13,
		FavoritesTab = 14,
		CodeInputField = 20,
		DownloadedLevel = 21,
		LevelCodeElement = 22,
		LoadLevelApply = 23,
		RecentCode = 24,
		CodeStatusText = 25,
		CodeStatusCheckmark = 26,
		ClearCode = 27,
		PasteCode = 28,
		ArchivedCodeSaveLocally = 29,
		ArchivedCodeNotification = 100030,
		RecentElement = 30,
		DeleteRecentElement = 31,
		SaveCopy = 32,
		CopyCodeToClipboard = 33,
		LocalSaveElement = 40,
		LoadSelectedLevel = 43,
		LevelSave = 44,
		Delete = 45,
		DeleteYes = 46,
		DeleteSlash = 47,
		DeleteNo = 48,
		GetCode = 49,
		Rename = 50,
		ContextualButtonBackground = 51,
		ToggleFavorite = 52,
		Descramble = 53,
		PublishToggle = 54,
		PublishLevelType = 55,
		FeaturedLevelSave = 60,
		FeaturedLevelElement = 61,
		FeaturedModeToggle = 62,
		FeaturedSearchField = 63,
		FeaturedSearchErase = 64,
		FeaturedQuickFilterButton = 66,
		FeaturedOpenInfoPage = 67,
		FeaturedQuickInfoUsername = 68,
		FeaturedPlayerLevelsBack = 69,
		FeaturedViewToggle = 80,
		FeaturedShareButton = 81,
		FeaturedUpvoteButton = 82,
		FeaturedDownvoteButton = 83,
		FeaturedAllowUnpublishedToggle = 84,
		FeaturedReportButton = 85,
		FeaturedViewReportsDialogButton = 86,
		FeaturedAdminRenameField = 87,
		FeaturedAdminRenameButton = 88,
		FeaturedAdminDeleteForever = 89,
		FeaturedAdminDeleteYes = 90,
		FeaturedAdminDeleteNo = 91,
		FeaturedPlayNow = 92,
		FeaturedPlayNowSuggested = 93,
		FeaturedNextMode = 94,
		FeaturedPrevMode = 95,
		FeaturedQuickPlayModeIndicator = 96,
		FeaturedAdminAddToBatch = 97,
		FeaturedAcknowledgeReportsButton = 98,
		FeaturedIgnoreReportsButton = 99,
		FeaturedShowMods = 10100,
		FavoriteEntry = 70,
		RemoveFavorite = 71,
		Close = 101,
		Next = 102,
		Prev = 103,
		PageIndicator = 104,
		EmptyPage = 105,
		TerminalLocked = 106,
		Share = 107,
		ShareTwitter = 108,
		ShareReddit = 109,
		CloseShareDialog = 110,
		ShareDialogElement = 111,
		ShareDialogGetCode = 112,
		ShareDialogPublish = 113,
		ShareDialogUnpublish = 114,
		CloseReportDialog = 120,
		ReportDialogElement = 121,
		ReportDialogReason = 122,
		ReportDialogSubmit = 123,
		ReportDialogCustomReason = 124,
		ReportDialogDelete = 125,
		CloseViewReportsDialog = 130,
		ViewReportsDialogElement = 131,
		CloseDeleteDialog = 132,
		DeleteLevelButton = 133,
		DeleteLevelConfirmButton = 134,
		AdminPanelElement = 140,
		CloseAdminPanel = 141
	}

	public enum BuildScreenStates
	{
		LevelCodesTab,
		LocalSavesTab,
		FeaturedLevelTab,
		AlwaysOn,
		RecentTab,
		FavoritesTab,
		ShareDialog,
		ReportDialog,
		ViewReportsDialog,
		AdminPanelDialog,
		DeleteDialog
	}

	public enum LevelSlotEnum
	{
		A,
		B,
		C,
		D,
		E,
		F,
		G
	}

	public BuildButtonJobs job;

	public BuildScreenStates buildPage;

	public LevelSlotEnum ControlsLevelSlot;

	public static UndergroundComputer undergroundComputer;

	public static BuildScreenStates buildMenuCurrentState;

	public Text[] highlightedTexts;

	public Image[] highlightedImages;

	public Image LevelImage;

	public Text SlotText;

	protected bool SlotHasLevel;

	public string LevelCode;

	public InputField inputField;

	public bool Selected;

	public static PickableBuildButton SelectedEntry;

	public bool needsnetworkIdentity;

	public GameObject connectButton;

	public static bool SelectedEntryHasCode
	{
		get
		{
			if (SelectedEntry != null)
			{
				SnapshotEntry component = SelectedEntry.GetComponent<SnapshotEntry>();
				if (component != null && !component.Code.NullOrEmpty())
				{
					return true;
				}
			}
			return false;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (needsnetworkIdentity && !GetComponent<NetworkIdentity>())
		{
			base.gameObject.AddComponent<NetworkIdentity>();
		}
	}

	protected override void Start()
	{
		base.Start();
		if (job != BuildButtonJobs.LevelSlot)
		{
			_ = 10;
		}
	}

	public override void OnAccept(PickCursor pickCursor)
	{
		switch (job)
		{
		case BuildButtonJobs.LoadLevelApply:
			LoadIntoSlot(ControlsLevelSlot);
			break;
		case BuildButtonJobs.LevelCodes:
			if (buildMenuCurrentState != BuildScreenStates.LevelCodesTab && !undergroundComputer.CurrentlyLoading)
			{
				buildMenuCurrentState = BuildScreenStates.LevelCodesTab;
				DeselectLastSelected();
				undergroundComputer.OnSelectLevelCodesTab();
			}
			break;
		case BuildButtonJobs.LocalSaves:
			if (!undergroundComputer.IsTabHighlighted(job) && !undergroundComputer.CurrentlyLoading)
			{
				undergroundComputer.OpenSpecialFilterPage(FeaturedSpecialFilter.SpecialFilterType.Local);
			}
			break;
		case BuildButtonJobs.FeaturedLevels:
			if (!undergroundComputer.IsTabHighlighted(job) && !undergroundComputer.CurrentlyLoading)
			{
				undergroundComputer.OnSelectFeaturedLevelTab(refreshCurrentPage: true, resetFilters: true);
			}
			break;
		case BuildButtonJobs.RecentTab:
			if (!undergroundComputer.IsTabHighlighted(job) && !undergroundComputer.CurrentlyLoading)
			{
				undergroundComputer.OpenSpecialFilterPage(FeaturedSpecialFilter.SpecialFilterType.Recent);
			}
			break;
		case BuildButtonJobs.FavoritesTab:
			if (!undergroundComputer.IsTabHighlighted(job) && !undergroundComputer.CurrentlyLoading)
			{
				undergroundComputer.OpenSpecialFilterPage(FeaturedSpecialFilter.SpecialFilterType.Favourites);
			}
			break;
		case BuildButtonJobs.LevelSlot:
		{
			BuildScreenStates buildScreenStates = buildMenuCurrentState;
			if ((uint)(buildScreenStates - 6) <= 3u)
			{
				break;
			}
			if (undergroundComputer.IsPortalPopulated(ControlsLevelSlot))
			{
				CustomLevelPortal portalForSlot = undergroundComputer.GetPortalForSlot(ControlsLevelSlot);
				if (portalForSlot != null)
				{
					SnapshotEntry snapshotEntry = undergroundComputer.FindSnapshotInDisplayList(portalForSlot.snapshotInfo.code, portalForSlot.snapshotInfo.snapshotName);
					if ((bool)snapshotEntry)
					{
						snapshotEntry.GetComponent<PickableBuildButton>().Select(allowDeselect: true);
					}
					else
					{
						Select(allowDeselect: true);
					}
				}
				else
				{
					Select(allowDeselect: true);
				}
			}
			else if (SelectedEntry != null && SelectedEntry.job != BuildButtonJobs.LevelSlot && !undergroundComputer.SnapshotAlreadyLoaded(SelectedEntry))
			{
				LoadIntoSlot(ControlsLevelSlot);
			}
			break;
		}
		case BuildButtonJobs.DownloadedLevel:
		case BuildButtonJobs.RecentCode:
		case BuildButtonJobs.LocalSaveElement:
		case BuildButtonJobs.FeaturedLevelSave:
		case BuildButtonJobs.FavoriteEntry:
		{
			BuildScreenStates buildScreenStates = buildMenuCurrentState;
			if ((uint)(buildScreenStates - 6) > 3u)
			{
				Select(allowDeselect: true);
			}
			break;
		}
		case BuildButtonJobs.DeleteRecentElement:
			if (SelectedEntry != null)
			{
				undergroundComputer.DeleteRecentCode(SelectedEntry);
			}
			break;
		case BuildButtonJobs.CodeInputField:
			undergroundComputer.ActivateCodeEntryField(pickCursor);
			break;
		case BuildButtonJobs.PasteCode:
			undergroundComputer.PasteCodeFromClipboard();
			break;
		case BuildButtonJobs.Delete:
			if (SelectedEntry != null)
			{
				undergroundComputer.quickInfoPane.OnClickDeleteFile();
			}
			break;
		case BuildButtonJobs.DeleteYes:
			undergroundComputer.OnClickConfirmDeleteLocalFile();
			break;
		case BuildButtonJobs.DeleteNo:
			undergroundComputer.quickInfoPane.OnClickCancelDeleteFile();
			break;
		case BuildButtonJobs.Next:
			undergroundComputer.OnClickNext();
			break;
		case BuildButtonJobs.Prev:
			undergroundComputer.OnClickPrev();
			break;
		case BuildButtonJobs.Rename:
			if (SelectedEntry != null)
			{
				undergroundComputer.RenamedLocalLevel(this, pickCursor);
			}
			break;
		case BuildButtonJobs.GetCode:
			if (SelectedEntry != null)
			{
				undergroundComputer.GetCodeForLocalSave(SelectedEntry, null);
			}
			break;
		case BuildButtonJobs.ClearCode:
			undergroundComputer.ClearAllSlots();
			break;
		case BuildButtonJobs.SaveCopy:
			if (SelectedEntry != null)
			{
				undergroundComputer.CreateLocalSaveFromRecentEntry(SelectedEntry);
			}
			break;
		case BuildButtonJobs.ToggleFavorite:
			if (SelectedEntry != null)
			{
				undergroundComputer.ToggleFavorite(SelectedEntry);
			}
			break;
		case BuildButtonJobs.Descramble:
			if (SelectedEntry != null)
			{
				undergroundComputer.DescrambleFile(SelectedEntry);
			}
			break;
		case BuildButtonJobs.CopyCodeToClipboard:
			if (SelectedEntry != null)
			{
				undergroundComputer.CopyCodeToClipboard(SelectedEntry);
			}
			break;
		case BuildButtonJobs.FeaturedQuickFilterButton:
		{
			FeaturedQuickFilter component2 = GetComponent<FeaturedQuickFilter>();
			if (component2 != null)
			{
				if (component2.sortingFilter.restrictToUserId.NullOrEmpty())
				{
					undergroundComputer.OnClickQuickFilter(component2.sortingFilter);
				}
				else
				{
					undergroundComputer.ShowMyLevels(component2.sortingFilter);
				}
			}
			else
			{
				Debug.LogError("Missing quickfilter component!", this);
			}
			break;
		}
		case BuildButtonJobs.FeaturedModeToggle:
		{
			FeaturedQuickFilter.LevelTypes siblingIndex2 = (FeaturedQuickFilter.LevelTypes)base.transform.GetSiblingIndex();
			if (undergroundComputer.currentLevelType != siblingIndex2)
			{
				undergroundComputer.SetFeaturedLevelMode(siblingIndex2, refreshSearch: true);
			}
			break;
		}
		case BuildButtonJobs.FeaturedViewToggle:
		{
			UndergroundComputer.ViewModes siblingIndex = (UndergroundComputer.ViewModes)base.transform.GetSiblingIndex();
			if (UndergroundComputer.currentViewMode != siblingIndex)
			{
				undergroundComputer.SetFeaturedViewMode(siblingIndex, refreshSearch: true);
			}
			break;
		}
		case BuildButtonJobs.FeaturedQuickInfoUsername:
		{
			UGCNameTag component = GetComponent<UGCNameTag>();
			if (component != null)
			{
				component.OnClick(undergroundComputer);
			}
			else
			{
				Debug.LogError("No UGCNameTag found on Featured Quick Info Username");
			}
			break;
		}
		case BuildButtonJobs.FeaturedPlayerLevelsBack:
			undergroundComputer.CloseLevelsByPlayer();
			break;
		case BuildButtonJobs.FeaturedAdminAddToBatch:
			undergroundComputer.quickInfoPane.OnClickAdminAddToBatch();
			break;
		case BuildButtonJobs.FeaturedShareButton:
		case BuildButtonJobs.Share:
			if (SelectedEntry != null)
			{
				undergroundComputer.OpenShareDialog(SelectedEntry);
			}
			break;
		case BuildButtonJobs.CloseShareDialog:
			undergroundComputer.CloseShareDialog();
			break;
		case BuildButtonJobs.FeaturedReportButton:
			if (SelectedEntry != null)
			{
				undergroundComputer.OpenReportDialog(SelectedEntry);
			}
			break;
		case BuildButtonJobs.CloseReportDialog:
			undergroundComputer.CloseReportDialog();
			break;
		case BuildButtonJobs.FeaturedViewReportsDialogButton:
			if (SelectedEntry != null)
			{
				undergroundComputer.OpenViewReportsDialog(SelectedEntry);
			}
			break;
		case BuildButtonJobs.FeaturedAcknowledgeReportsButton:
			if (SelectedEntry != null)
			{
				undergroundComputer.quickInfoPane.ToggleAcknowledgeReports(SelectedEntry);
			}
			break;
		case BuildButtonJobs.FeaturedIgnoreReportsButton:
			if (SelectedEntry != null)
			{
				undergroundComputer.quickInfoPane.ToggleIgnoreReports(SelectedEntry);
			}
			break;
		case BuildButtonJobs.CloseViewReportsDialog:
			undergroundComputer.CloseViewReportsDialog();
			break;
		case BuildButtonJobs.CloseAdminPanel:
			undergroundComputer.BackOutOfSubmenu();
			break;
		case BuildButtonJobs.ShareDialogGetCode:
			undergroundComputer.shareDialog.OnClickGenerateCode();
			break;
		case BuildButtonJobs.ShareReddit:
			undergroundComputer.shareDialog.OnClickShareReddit();
			break;
		case BuildButtonJobs.ShareTwitter:
			undergroundComputer.shareDialog.OnClickShareTwitter();
			break;
		case BuildButtonJobs.PublishLevelType:
			undergroundComputer.shareDialog.SelectLevelType((FeaturedQuickFilter.LevelTypes)base.transform.GetSiblingIndex());
			break;
		case BuildButtonJobs.ShareDialogPublish:
			undergroundComputer.shareDialog.OnClickPublish();
			break;
		case BuildButtonJobs.ShareDialogUnpublish:
			undergroundComputer.shareDialog.OnClickUnpublish();
			break;
		case BuildButtonJobs.FeaturedUpvoteButton:
			undergroundComputer.OnClickUpvote();
			break;
		case BuildButtonJobs.FeaturedDownvoteButton:
			undergroundComputer.OnClickDownvote();
			break;
		case BuildButtonJobs.FeaturedAllowUnpublishedToggle:
			undergroundComputer.OnClickAllowUnpublishedToggle();
			break;
		case BuildButtonJobs.FeaturedShowMods:
			undergroundComputer.OnClickShowModsToggle();
			break;
		case BuildButtonJobs.ReportDialogReason:
			if (SelectedEntry != null)
			{
				ReportReason componentInParent = GetComponentInParent<ReportReason>();
				if (componentInParent != null)
				{
					undergroundComputer.reportDialog.OnClickReason(componentInParent);
				}
			}
			break;
		case BuildButtonJobs.ReportDialogSubmit:
			undergroundComputer.reportDialog.OnClickSubmit(delete: false);
			break;
		case BuildButtonJobs.ReportDialogDelete:
			undergroundComputer.reportDialog.OnClickDelete();
			break;
		case BuildButtonJobs.ReportDialogCustomReason:
			undergroundComputer.reportDialog.ActivateCommentInputField(pickCursor);
			break;
		case BuildButtonJobs.FeaturedSearchField:
			undergroundComputer.ActivateAdvancedSearchField(pickCursor);
			break;
		case BuildButtonJobs.FeaturedSearchErase:
			undergroundComputer.OnClickAdvancedSearchErase();
			break;
		case BuildButtonJobs.FeaturedAdminRenameField:
			undergroundComputer.quickInfoPane.ActivateAdminRenameField();
			break;
		case BuildButtonJobs.FeaturedAdminRenameButton:
			undergroundComputer.quickInfoPane.OnClickApplyAdminRename();
			break;
		case BuildButtonJobs.FeaturedAdminDeleteForever:
			undergroundComputer.quickInfoPane.OnClickAdminDelete();
			break;
		case BuildButtonJobs.FeaturedAdminDeleteYes:
			undergroundComputer.quickInfoPane.OnClickAdminConfirmDelete();
			break;
		case BuildButtonJobs.FeaturedAdminDeleteNo:
			undergroundComputer.quickInfoPane.OnClickAdminCancelDelete();
			break;
		case BuildButtonJobs.FeaturedPlayNow:
			undergroundComputer.OnClickPlayNow(pickCursor);
			break;
		case BuildButtonJobs.FeaturedPlayNowSuggested:
			undergroundComputer.OnClickPlayNow(pickCursor, suggested: true);
			break;
		case BuildButtonJobs.FeaturedPrevMode:
			UndergroundComputer.PrevQuickPlayMode();
			break;
		case BuildButtonJobs.FeaturedNextMode:
			UndergroundComputer.NextQuickPlayMode();
			break;
		case BuildButtonJobs.ArchivedCodeSaveLocally:
			undergroundComputer.OnClickSaveArchivedLevel();
			break;
		case BuildButtonJobs.CloseDeleteDialog:
			undergroundComputer.CloseDeleteDialog();
			break;
		case BuildButtonJobs.DeleteLevelButton:
			undergroundComputer.OpenDeleteDialog(SelectedEntry);
			break;
		case BuildButtonJobs.DeleteLevelConfirmButton:
			undergroundComputer.deleteDialogue.OnClickSubmit();
			break;
		}
		base.OnAccept(pickCursor);
	}

	protected override void Update()
	{
		base.Update();
		if (!Visible || !initialized)
		{
			return;
		}
		bool flag = false;
		switch (job)
		{
		case BuildButtonJobs.LevelSlot:
		{
			bool flag2 = true;
			if (!undergroundComputer.IsPortalPopulated(ControlsLevelSlot))
			{
				flag2 = !(SelectedEntry == null) && SelectedEntry.job != BuildButtonJobs.LevelSlot && !undergroundComputer.SnapshotAlreadyLoaded(SelectedEntry);
			}
			Show(show: true, updateColliders: false);
			flag = true;
			Collider2D[] pickColliders = PickColliders;
			foreach (Collider2D collider2D in pickColliders)
			{
				if (collider2D.enabled != flag2)
				{
					collider2D.enabled = flag2;
				}
			}
			break;
		}
		case BuildButtonJobs.LoadLevelApply:
			switch (buildMenuCurrentState)
			{
			case BuildScreenStates.LevelCodesTab:
				Show(undergroundComputer.currentCodeValidated && !undergroundComputer.CodeEntryAlreadyLoaded());
				flag = true;
				break;
			case BuildScreenStates.LocalSavesTab:
			case BuildScreenStates.FeaturedLevelTab:
			case BuildScreenStates.RecentTab:
			case BuildScreenStates.FavoritesTab:
				Show(SelectedEntry != null && !undergroundComputer.SnapshotAlreadyLoaded(SelectedEntry));
				flag = true;
				break;
			}
			break;
		case BuildButtonJobs.ClearCode:
			Show(undergroundComputer.IsAnyPortalPopulated());
			flag = true;
			break;
		case BuildButtonJobs.Next:
			Show(undergroundComputer.ShouldShowNext);
			flag = true;
			break;
		case BuildButtonJobs.Prev:
			Show(undergroundComputer.ShouldShowPrev);
			flag = true;
			break;
		case BuildButtonJobs.DeleteYes:
		case BuildButtonJobs.DeleteSlash:
		case BuildButtonJobs.DeleteNo:
			Show(FeaturedQuickInfoPane.localDeleteEnabled);
			flag = true;
			break;
		case BuildButtonJobs.FeaturedAdminDeleteYes:
		case BuildButtonJobs.FeaturedAdminDeleteNo:
			Show(FeaturedQuickInfoPane.adminDeleteEnabled);
			flag = true;
			break;
		case BuildButtonJobs.FeaturedShareButton:
		case BuildButtonJobs.Share:
			Show(SelectedEntry != null);
			flag = true;
			break;
		case BuildButtonJobs.SaveCopy:
		case BuildButtonJobs.Delete:
		case BuildButtonJobs.Rename:
		case BuildButtonJobs.ContextualButtonBackground:
		case BuildButtonJobs.ToggleFavorite:
		case BuildButtonJobs.FeaturedUpvoteButton:
		case BuildButtonJobs.FeaturedDownvoteButton:
		case BuildButtonJobs.ShareTwitter:
		case BuildButtonJobs.ShareReddit:
			Show(SelectedEntry != null);
			flag = true;
			break;
		case BuildButtonJobs.GetCode:
			Show(SelectedEntry != null && !SelectedEntryHasCode);
			flag = true;
			break;
		case BuildButtonJobs.CopyCodeToClipboard:
			Show(SelectedEntry != null && SelectedEntryHasCode);
			flag = true;
			break;
		case BuildButtonJobs.Descramble:
			Show(SelectedEntry != null && GameState.DebugMode);
			flag = true;
			break;
		case BuildButtonJobs.PageIndicator:
			Show(undergroundComputer.ShouldShowPageNumber);
			flag = true;
			break;
		case BuildButtonJobs.FeaturedAllowUnpublishedToggle:
			if (ControlsLevelSlot != LevelSlotEnum.A)
			{
				Show(show: false);
			}
			else if (GameSparksManager.Instance.MainUserIsAdmin)
			{
				Show(show: true);
				flag = true;
			}
			else
			{
				Show(undergroundComputer.topPanelMode == UndergroundComputer.FeaturedLevelTopPanelModes.MyLevels);
				flag = true;
			}
			break;
		case BuildButtonJobs.DeleteRecentElement:
			Show(undergroundComputer.IsTabHighlighted(BuildButtonJobs.RecentTab));
			flag = true;
			break;
		case BuildButtonJobs.FeaturedPlayNow:
		case BuildButtonJobs.FeaturedPlayNowSuggested:
		case BuildButtonJobs.FeaturedNextMode:
		case BuildButtonJobs.FeaturedPrevMode:
			switch (buildMenuCurrentState)
			{
			case BuildScreenStates.LevelCodesTab:
				Show(undergroundComputer.currentCodeValidated);
				flag = true;
				break;
			case BuildScreenStates.LocalSavesTab:
			case BuildScreenStates.FeaturedLevelTab:
			case BuildScreenStates.RecentTab:
			case BuildScreenStates.FavoritesTab:
				Show(SelectedEntry != null);
				flag = true;
				break;
			}
			break;
		case BuildButtonJobs.FeaturedQuickPlayModeIndicator:
			switch (buildMenuCurrentState)
			{
			case BuildScreenStates.LevelCodesTab:
				Show(undergroundComputer.currentCodeValidated);
				flag = true;
				break;
			case BuildScreenStates.LocalSavesTab:
			case BuildScreenStates.FeaturedLevelTab:
			case BuildScreenStates.RecentTab:
			case BuildScreenStates.FavoritesTab:
				Show(SelectedEntry != null);
				flag = true;
				break;
			}
			buttonText.text = GameState.GetLocalizedGameModeName(UndergroundComputer.quickPlayMode);
			break;
		case BuildButtonJobs.ArchivedCodeSaveLocally:
		case BuildButtonJobs.ArchivedCodeNotification:
			Show(undergroundComputer.currentCodeArchived);
			flag = true;
			break;
		case BuildButtonJobs.FeaturedSearchErase:
			Show(undergroundComputer.currentFilter == null || !undergroundComputer.currentFilter.searchTerms.NullOrEmpty());
			flag = true;
			break;
		}
		if (deactivatedInBook)
		{
			if (overrideDeactivatedAlphaBool)
			{
				SetAlpha(overrideDeactivatedAlphafloat);
			}
			else
			{
				SetAlpha(0.5f);
			}
		}
		else
		{
			SetAlpha(1f);
		}
		if (!flag)
		{
			Show(show: true);
		}
	}

	public static void DeselectLastSelected()
	{
		if (SelectedEntry != null)
		{
			SelectedEntry.Selected = false;
		}
		SelectedEntry = null;
		undergroundComputer.OnSelectEntry(null);
	}

	protected bool LoadIntoSlot(LevelSlotEnum levelSlot)
	{
		switch (buildMenuCurrentState)
		{
		case BuildScreenStates.FeaturedLevelTab:
		{
			if (!(SelectedEntry != null))
			{
				break;
			}
			SnapshotEntry component = SelectedEntry.GetComponent<SnapshotEntry>();
			if (component.featuredLevelData != null && !component.featuredLevelData.isLocal)
			{
				if (!component.featuredLevelData.archived)
				{
					undergroundComputer.LoadCurrentRecentCodeIntoSlot(SelectedEntry, levelSlot);
				}
			}
			else
			{
				undergroundComputer.LoadCurrentLocalSaveIntoSlot(SelectedEntry, levelSlot);
			}
			return true;
		}
		case BuildScreenStates.LevelCodesTab:
			if (undergroundComputer.currentCodeValidated && !undergroundComputer.currentCodeArchived)
			{
				undergroundComputer.LoadCurrentCodeIntoSlot(levelSlot);
				return true;
			}
			break;
		case BuildScreenStates.LocalSavesTab:
			if (SelectedEntry != null)
			{
				undergroundComputer.LoadCurrentLocalSaveIntoSlot(SelectedEntry, levelSlot);
				return true;
			}
			break;
		case BuildScreenStates.RecentTab:
			if (SelectedEntry != null)
			{
				undergroundComputer.LoadCurrentRecentCodeIntoSlot(SelectedEntry, levelSlot);
				return true;
			}
			break;
		case BuildScreenStates.FavoritesTab:
			if (SelectedEntry != null)
			{
				undergroundComputer.LoadCurrentFavoriteIntoPortalSlot(SelectedEntry, levelSlot);
				return true;
			}
			break;
		}
		return false;
	}

	protected void Show(bool show, bool updateColliders = true)
	{
		if (show)
		{
			if (buildMenuCurrentState == BuildScreenStates.AdminPanelDialog && buildPage != BuildScreenStates.AdminPanelDialog)
			{
				Show(show: false);
				return;
			}
			if (buildMenuCurrentState == BuildScreenStates.ShareDialog && buildPage != BuildScreenStates.ShareDialog)
			{
				Show(show: false);
				return;
			}
			if (buildMenuCurrentState == BuildScreenStates.ReportDialog && buildPage != BuildScreenStates.ReportDialog)
			{
				Show(show: false);
				return;
			}
			if (buildMenuCurrentState == BuildScreenStates.LevelCodesTab && buildPage == BuildScreenStates.LevelCodesTab && PlatformFeatureRestrictions.HideOnlineContent)
			{
				Show(show: false);
				return;
			}
			if (buildPage != buildMenuCurrentState && buildPage != BuildScreenStates.AlwaysOn)
			{
				Show(show: false);
				return;
			}
			if (!LobbyManager.instance.IsHost)
			{
				if (job != BuildButtonJobs.TerminalLocked)
				{
					Show(show: false);
					return;
				}
			}
			else if (job == BuildButtonJobs.TerminalLocked)
			{
				Show(show: false);
				return;
			}
			if (job == BuildButtonJobs.EmptyPage)
			{
				BuildScreenStates buildScreenStates = buildMenuCurrentState;
				if (((uint)(buildScreenStates - 1) <= 1u || (uint)(buildScreenStates - 4) <= 1u) && !undergroundComputer.ShowingEmptyPage)
				{
					Show(show: false);
					return;
				}
			}
		}
		if ((bool)buttonText && buttonText.enabled != show)
		{
			buttonText.enabled = show;
		}
		if (updateColliders)
		{
			Collider2D[] pickColliders = PickColliders;
			foreach (Collider2D collider2D in pickColliders)
			{
				if (collider2D.enabled != show)
				{
					collider2D.enabled = show;
				}
			}
		}
		if ((bool)inputField)
		{
			if (inputField.gameObject.activeSelf != show)
			{
				inputField.gameObject.SetActive(show);
			}
			bool flag = inputField.text.NullOrEmpty();
			if (show && inputField.placeholder != null && inputField.placeholder.gameObject.activeSelf != flag)
			{
				inputField.placeholder.gameObject.SetActive(flag);
			}
		}
		bool flag2 = false;
		if ((bool)base.image)
		{
			if (base.image.enabled != show)
			{
				base.image.enabled = show;
			}
			if (show && buildMenuCurrentState == BuildScreenStates.ShareDialog && job == BuildButtonJobs.PublishLevelType)
			{
				flag2 = ((undergroundComputer.shareDialog.selectedLevelType == (FeaturedQuickFilter.LevelTypes)base.transform.GetSiblingIndex()) ? true : false);
			}
		}
		Image[] array = additionalImages;
		foreach (Image image in array)
		{
			if (image != null)
			{
				image.enabled = show;
			}
		}
		switch (job)
		{
		case BuildButtonJobs.LevelSlot:
			if (SelectedEntry == this)
			{
				flag2 = true;
			}
			else if (buildMenuCurrentState == BuildScreenStates.LevelCodesTab)
			{
				if (undergroundComputer.CodeEntryMatchesLoadedCodeInSlot(ControlsLevelSlot))
				{
					flag2 = true;
				}
			}
			else if (SelectedEntry != null && undergroundComputer.SnapshotAlreadyLoadedInSlot(SelectedEntry, ControlsLevelSlot))
			{
				flag2 = true;
			}
			break;
		case BuildButtonJobs.LevelCodes:
			if (buildMenuCurrentState == BuildScreenStates.LevelCodesTab)
			{
				flag2 = true;
			}
			break;
		case BuildButtonJobs.LocalSaves:
		case BuildButtonJobs.FeaturedLevels:
		case BuildButtonJobs.RecentTab:
		case BuildButtonJobs.FavoritesTab:
			if (buildMenuCurrentState == BuildScreenStates.FeaturedLevelTab)
			{
				flag2 = undergroundComputer.IsTabHighlighted(job);
			}
			break;
		case BuildButtonJobs.DownloadedLevel:
		case BuildButtonJobs.RecentCode:
		case BuildButtonJobs.LocalSaveElement:
		case BuildButtonJobs.FeaturedLevelSave:
		case BuildButtonJobs.FavoriteEntry:
			if (Selected)
			{
				flag2 = true;
			}
			break;
		case BuildButtonJobs.FeaturedModeToggle:
			if (undergroundComputer.currentLevelType == (FeaturedQuickFilter.LevelTypes)base.transform.GetSiblingIndex())
			{
				flag2 = true;
			}
			break;
		case BuildButtonJobs.FeaturedViewToggle:
			if (UndergroundComputer.currentViewMode == (UndergroundComputer.ViewModes)base.transform.GetSiblingIndex())
			{
				flag2 = true;
			}
			break;
		}
		bool flag3 = flag2 && show;
		array = highlightedImages;
		foreach (Image image2 in array)
		{
			if (image2 != null && image2.enabled != flag3)
			{
				image2.enabled = flag3;
			}
		}
		Text[] array2 = highlightedTexts;
		foreach (Text text in array2)
		{
			if (text != null && text.enabled != flag3)
			{
				text.enabled = flag3;
			}
		}
		if (LevelImage != null && LevelImage.enabled != show)
		{
			LevelImage.enabled = show;
		}
		if (SlotText != null && SlotText.enabled != show)
		{
			SlotText.enabled = show;
		}
	}

	public override void Enable(bool onOff = true)
	{
		base.Enable(onOff);
		if (inputField != null && inputField.enabled != onOff)
		{
			inputField.enabled = onOff;
		}
	}

	public override void SetAlpha(float newAlpha)
	{
		if ((bool)buttonText)
		{
			buttonText.color = new Color(buttonText.color.r, buttonText.color.g, buttonText.color.b, newAlpha);
		}
		if ((bool)image)
		{
			image.color = new Color(image.color.r, image.color.g, image.color.b, newAlpha);
		}
	}

	public void SetTextAlpha(Text textItem, float newAlpha)
	{
		if (textItem != null)
		{
			textItem.color = new Color(textItem.color.r, textItem.color.g, textItem.color.b, newAlpha);
		}
	}

	public void SetComputerSlotAppearance(string levelName, Sprite levelImageSprite)
	{
		SlotText.text = levelName;
		LevelImage.sprite = levelImageSprite;
		LevelImage.color = new Color(1f, 1f, 1f, 0.75f);
		SlotHasLevel = true;
	}

	public void ClearComputerSlotContents()
	{
		SlotText.text = ScriptLocalization.Snapshot.Empty;
		LevelImage.color = new Color(1f, 1f, 1f, 0f);
		if (SelectedEntry == this)
		{
			DeselectLastSelected();
		}
		SlotHasLevel = false;
	}

	public BuildScreenStates JobToTabState(BuildButtonJobs job)
	{
		return job switch
		{
			BuildButtonJobs.LevelCodes => BuildScreenStates.LevelCodesTab, 
			BuildButtonJobs.FeaturedLevels => BuildScreenStates.FeaturedLevelTab, 
			BuildButtonJobs.LocalSaves => BuildScreenStates.LocalSavesTab, 
			BuildButtonJobs.RecentTab => BuildScreenStates.RecentTab, 
			BuildButtonJobs.FavoritesTab => BuildScreenStates.FavoritesTab, 
			_ => BuildScreenStates.AlwaysOn, 
		};
	}

	private bool SlotEntryIsSelectedInOtherPane()
	{
		if (job == BuildButtonJobs.LevelSlot && SelectedEntry != this && SelectedEntry != null)
		{
			SnapshotEntry component = SelectedEntry.GetComponent<SnapshotEntry>();
			SnapshotEntry component2 = GetComponent<SnapshotEntry>();
			if (component.SnapshotName == component2.SnapshotName && ((component.Code.NullOrEmpty() && component2.Code.NullOrEmpty()) || (!component.Code.NullOrEmpty() && component.Code == component2.Code)))
			{
				return true;
			}
		}
		return false;
	}

	public void Select(bool allowDeselect)
	{
		if (allowDeselect && SlotEntryIsSelectedInOtherPane())
		{
			DeselectLastSelected();
			return;
		}
		if (allowDeselect && SelectedEntry == this)
		{
			DeselectLastSelected();
			return;
		}
		if (SelectedEntry != null)
		{
			SelectedEntry.Selected = false;
		}
		SelectedEntry = this;
		Selected = true;
		if (job == BuildButtonJobs.LevelSlot)
		{
			undergroundComputer.OnSelectLevelSlot(this);
		}
		else
		{
			undergroundComputer.OnSelectEntry(this);
		}
	}

	public override void ChangeListener(bool adding)
	{
		base.ChangeListener(adding);
		GameEventManager.ChangeListener<LanguageChangeEvent>(this, adding);
	}

	public override void handleEvent(global::GameEvent.GameEvent e)
	{
		base.handleEvent(e);
		if (e.GetType() == typeof(LanguageChangeEvent) && !SlotHasLevel && SlotText != null)
		{
			SlotText.text = ScriptLocalization.Snapshot.Empty;
		}
	}
}
