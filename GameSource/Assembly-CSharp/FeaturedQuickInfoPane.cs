using System;
using System.Collections.Generic;
using GameSparks.Core;
using I2.Loc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FeaturedQuickInfoPane : MonoBehaviour
{
	public Text levelName;

	public Image favoriteButtonImage;

	public Sprite favoriteFilledImage;

	public Sprite favoriteEmptyImage;

	public Text levelTypeText;

	public Text levelModsText;

	public Text completedText;

	public Text attemptsText;

	public Text successesText;

	public Text PlayCount;

	public Text PublishedDate;

	public UGCNameTag authorNametag;

	public Text LevelRating;

	public PickableBuildButton UpvoteButton;

	public PickableBuildButton DownvoteButton;

	public PickableBuildButton CodeButton;

	public Color upvoteActiveColor;

	public Color downvoteActiveColor;

	public Color voteArrowInactiveColor;

	public Transform scrollContainer;

	public ScrollArrowController scrollArrowController;

	public UnityEngine.Object highScoreDisplayEntryPrefab;

	public Transform noCoinsScoresSection;

	public Transform noCoinsScoresContainer;

	public Transform allCoinsScoresSection;

	public Transform allCoinsScoresContainer;

	public Text noCoinsYourBestTitleText;

	public HighscoreDisplayEntry noCoinsHighscoreDisplayEntry;

	public Text allCoinsYourBestTitleText;

	public HighscoreDisplayEntry allCoinsHighscoreDisplayEntry;

	public Color highlightedScoreColor;

	public GenericButton[] numPlayersButtons;

	private int numPlayersSelected;

	public Color activeLabelColor;

	public Color dimmedLabelColor;

	private bool showScoreLoadingSpinner;

	public Image scoreLoadingSpinner;

	public Transform firstClearBox;

	public HighscoreDisplayEntry firstClearHighScoreDisplayEntry;

	public InputField localLevelNameText;

	public Image localFavoriteButtonImage;

	public Text localLevelTypeText;

	public Transform levelInfoColumn;

	public Transform archivedLevelInfoColumn;

	public Transform localLevelInfoColumn;

	public Transform highScoreColumn;

	public Transform adminControlsColumn;

	public Transform additionalInfoColumn;

	public Transform archivedNoticeColumn;

	public Text adminReportsText;

	public DropdownMenu adminApprovalStatusDropdown;

	public DropdownMenu adminFeaturedBatchDropdown;

	public PickableButton adminFeaturedBatchAddButton;

	public UnityEngine.Object featuredBatchDropdownEntryPrefab;

	public PickableButton adminAcknowledgeReportsButton;

	public PickableButton adminIgnoreReportsButton;

	public PickableBuildButton adminRenameField;

	public PickableBuildButton adminRenameButton;

	public static bool adminDeleteEnabled = false;

	public static bool localDeleteEnabled = false;

	public PickableBuildButton localDeleteYes;

	public PickableBuildButton localDeleteNo;

	public PickableBuildButton recentDeleteYes;

	public PickableBuildButton recentDeleteNo;

	public Transform deleteLocalContainer;

	public Transform removeRecentCodeContainer;

	public RawImage localLevelImage;

	public RawImage onlineLevelImage;

	public Text archivedLevelText;

	public Image archivedFavoriteButtonImage;

	public GameObject reportButton;

	public GameObject deleteButton;

	public static int lastLeaderboardNumPlayers = -1;

	public void SetSnapshotInfo(UndergroundComputer.FeaturedLevelData featuredLevelData)
	{
		levelInfoColumn.gameObject.SetActive(value: true);
		archivedLevelInfoColumn.gameObject.SetActive(value: false);
		archivedNoticeColumn.gameObject.SetActive(value: false);
		additionalInfoColumn.gameObject.SetActive(value: true);
		localLevelInfoColumn.gameObject.SetActive(value: false);
		highScoreColumn.gameObject.SetActive(value: true);
		adminControlsColumn.gameObject.SetActive(GameSparksManager.Instance.MainUserIsAdmin);
		if (GameSparksManager.Instance.MainUserIsAdmin)
		{
			adminReportsText.text = "Reports: " + featuredLevelData.numReports;
			DropdownEntry entry = adminApprovalStatusDropdown.FindFirstDropdownEntryWithCriteria((DropdownEntry dropdownEntry) => dropdownEntry.EntryValue == featuredLevelData.approvalStatus);
			adminApprovalStatusDropdown.OnClickDropdownEntry(entry, triggerOnChangeEvent: false);
			UpdateAcknowledgeReportsButton(featuredLevelData.hasNewReports);
			UpdateIgnoreReportsButton(featuredLevelData.ignoreReports);
		}
		else
		{
			adminReportsText.text = "";
			adminApprovalStatusDropdown.SelectEntryByIndex(0, triggerOnChangeEvent: false);
		}
		adminRenameField.inputField.text = "";
		adminDeleteEnabled = false;
		levelName.text = featuredLevelData.name;
		if (featuredLevelData.attempts > 0)
		{
			completedText.text = string.Format(LocalizationManager.GetTranslation("UndergroundComputer/Stats/CompletedBy"), featuredLevelData.CompletionPercentage);
			attemptsText.text = string.Format(LocalizationManager.GetTranslation("UndergroundComputer/Stats/Attempts"), featuredLevelData.attempts);
			successesText.text = string.Format(LocalizationManager.GetTranslation("UndergroundComputer/Stats/Successes"), featuredLevelData.successes, featuredLevelData.SuccessFailurePercentage);
			attemptsText.gameObject.SetActive(value: true);
			successesText.gameObject.SetActive(value: true);
		}
		else
		{
			completedText.text = LocalizationManager.GetTranslation("UndergroundComputer/Stats/CompletedNoData");
			attemptsText.gameObject.SetActive(value: false);
			successesText.gameObject.SetActive(value: false);
		}
		CodeButton.buttonText.text = GameSparksQuery.GetFormattedSnapshotCode(featuredLevelData.code);
		authorNametag.InitializeAsync(featuredLevelData);
		PlayCount.text = LocalizationManager.GetTranslation("UndergroundComputer/Stats/Played") + " " + featuredLevelData.playCount + " " + ((featuredLevelData.playCount == 1) ? LocalizationManager.GetTranslation("UndergroundComputer/Stats/PlayedTime") : LocalizationManager.GetTranslation("UndergroundComputer/Stats/PlayedTimes"));
		if (!featuredLevelData.isPublished)
		{
			PublishedDate.text = LocalizationManager.GetTranslation("UndergroundComputer/Stats/Unpublished");
		}
		else if (featuredLevelData.timestamp > 0)
		{
			long num = UndergroundComputer.lastRefreshTimestamp - featuredLevelData.timestamp;
			PublishedDate.text = LocalizationManager.GetTranslation("UndergroundComputer/Stats/Published") + " " + UndergroundComputer.TimeToString((int)(num / 1000));
		}
		else
		{
			PublishedDate.text = LocalizationManager.GetTranslation("UndergroundComputer/Stats/Published") + " ???";
		}
		SetVoteInfo(featuredLevelData.rating, featuredLevelData.myVote);
		if (lastLeaderboardNumPlayers == -1)
		{
			numPlayersSelected = 1;
		}
		else
		{
			numPlayersSelected = lastLeaderboardNumPlayers;
		}
		UpdateNumPlayerButtonsState();
		LoadRecords(numPlayersSelected, 5);
		scrollArrowController.ResetScrolling();
		switch (featuredLevelData.levelType)
		{
		case FeaturedQuickFilter.LevelTypes.Challenge:
			levelTypeText.text = LocalizationManager.GetTranslation("InLobby/ChallengeModeButtonText");
			UndergroundComputer.SuggestQuickPlayMode(GameState.GameMode.CHALLENGE);
			break;
		case FeaturedQuickFilter.LevelTypes.Versus:
			levelTypeText.text = LocalizationManager.GetTranslation("InLobby/PartyText");
			UndergroundComputer.SuggestQuickPlayMode(GameState.GameMode.PARTY);
			break;
		default:
			levelTypeText.text = LocalizationManager.GetTranslation("Inventory/None");
			UndergroundComputer.SuggestQuickPlayMode(UndergroundComputer.quickPlayMode);
			break;
		}
		if (featuredLevelData.hasMods)
		{
			levelModsText.text = ScriptLocalization.UndergroundComputer.ModifiersEnabledYes;
		}
		else
		{
			levelModsText.text = ScriptLocalization.UndergroundComputer.ModifiersEnabledNo;
		}
		if (featuredLevelData.authorId == GameSparksManager.Instance.MainUserGSID)
		{
			reportButton.SetActive(value: false);
			deleteButton.SetActive(value: true);
		}
		else
		{
			reportButton.SetActive(value: true);
			deleteButton.SetActive(value: false);
		}
	}

	private void UpdateNumPlayerButtonsState()
	{
		for (int i = 0; i < numPlayersButtons.Length; i++)
		{
			numPlayersButtons[i].buttonText.color = ((i == numPlayersSelected - 1) ? activeLabelColor : dimmedLabelColor);
		}
	}

	public void SetArchivedLevelInfo(UndergroundComputer.FeaturedLevelData featuredLevelData)
	{
		levelInfoColumn.gameObject.SetActive(value: false);
		archivedLevelInfoColumn.gameObject.SetActive(value: true);
		archivedNoticeColumn.gameObject.SetActive(value: true);
		additionalInfoColumn.gameObject.SetActive(value: false);
		localLevelInfoColumn.gameObject.SetActive(value: false);
		highScoreColumn.gameObject.SetActive(value: false);
		adminControlsColumn.gameObject.SetActive(value: false);
		scrollArrowController.ResetScrolling();
		archivedLevelText.text = featuredLevelData.name;
	}

	public void SetLocalSaveInfo(SnapshotEntry localSave)
	{
		levelInfoColumn.gameObject.SetActive(value: false);
		archivedLevelInfoColumn.gameObject.SetActive(value: false);
		archivedNoticeColumn.gameObject.SetActive(value: false);
		additionalInfoColumn.gameObject.SetActive(value: false);
		localLevelInfoColumn.gameObject.SetActive(value: true);
		highScoreColumn.gameObject.SetActive(value: false);
		adminControlsColumn.gameObject.SetActive(value: false);
		adminRenameField.inputField.text = "";
		adminDeleteEnabled = false;
		QuickSaver.FindLocalSaveFilenameWithoutExt(localSave.UncensoredName, delegate(string filename)
		{
			switch (QuickSaver.InferLevelTypeFromFilename(filename))
			{
			case FeaturedQuickFilter.LevelTypes.Challenge:
				localLevelTypeText.text = LocalizationManager.GetTranslation("InLobby/ChallengeModeButtonText");
				UndergroundComputer.SuggestQuickPlayMode(GameState.GameMode.CHALLENGE);
				break;
			case FeaturedQuickFilter.LevelTypes.Versus:
				localLevelTypeText.text = LocalizationManager.GetTranslation("InLobby/PartyText");
				UndergroundComputer.SuggestQuickPlayMode(GameState.GameMode.PARTY);
				break;
			default:
				localLevelTypeText.text = LocalizationManager.GetTranslation("Inventory/None");
				UndergroundComputer.SuggestQuickPlayMode(UndergroundComputer.quickPlayMode);
				break;
			}
		});
		localLevelNameText.text = localSave.SnapshotName;
		scrollArrowController.ResetScrolling();
	}

	public void SetVoteInfo(int newRating, int myVote)
	{
		LevelRating.text = newRating.ToString();
		UpvoteButton.image.color = ((myVote == 1) ? upvoteActiveColor : voteArrowInactiveColor);
	}

	public void OnClickNumPlayersButton(int num)
	{
		lastLeaderboardNumPlayers = num;
		if (numPlayersSelected != num)
		{
			numPlayersSelected = num;
			LoadRecords(num, 5);
		}
		UpdateNumPlayerButtonsState();
	}

	public void Show(bool onOff)
	{
		scrollContainer.gameObject.SetActive(onOff);
		scrollArrowController.gameObject.SetActive(onOff);
		if (localDeleteEnabled)
		{
			OnClickCancelDeleteFile();
		}
		adminDeleteEnabled = false;
	}

	public void LoadRecords(int numPlayers, int maxRecords)
	{
		PickableBuildButton selectedEntry = PickableBuildButton.SelectedEntry;
		if (!(selectedEntry != null))
		{
			return;
		}
		SnapshotEntry component = selectedEntry.GetComponent<SnapshotEntry>();
		if (!(component != null))
		{
			return;
		}
		HideAllRankings();
		showScoreLoadingSpinner = true;
		GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
		query.GetChallengeTimes(component.Code, numPlayersSelected, 0, 5);
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			if (selectedEntry == PickableBuildButton.SelectedEntry)
			{
				showScoreLoadingSpinner = false;
				if (query.HasError)
				{
					Debug.LogError("Error with query: " + query.Error);
				}
				else
				{
					List<ChallengeScoreboard.ChallengeTimeData> recordsList = query.ResultData["recordsNoCoins"] as List<ChallengeScoreboard.ChallengeTimeData>;
					List<ChallengeScoreboard.ChallengeTimeData> list = query.ResultData["recordsAllCoins"] as List<ChallengeScoreboard.ChallengeTimeData>;
					ChallengeScoreboard.ChallengeTimeData personalRecord = query.ResultData["personalRecordNoCoins"] as ChallengeScoreboard.ChallengeTimeData;
					ChallengeScoreboard.ChallengeTimeData personalRecord2 = query.ResultData["personalRecordAllCoins"] as ChallengeScoreboard.ChallengeTimeData;
					PopulateScoreEntries(recordsList, personalRecord, noCoinsScoresSection, noCoinsScoresContainer, noCoinsYourBestTitleText, noCoinsHighscoreDisplayEntry);
					PopulateScoreEntries(list, personalRecord2, allCoinsScoresSection, allCoinsScoresContainer, allCoinsYourBestTitleText, allCoinsHighscoreDisplayEntry);
					allCoinsScoresSection.gameObject.SetActive(list.Count != 0);
					List<string> list2 = query.ResultData["firstClearNameList"] as List<string>;
					List<string> firstClearIdList = query.ResultData["firstClearIdList"] as List<string>;
					List<GSData> platformIds = query.ResultData["firstClearPlatformIdList"] as List<GSData>;
					if (list2 != null && firstClearIdList != null)
					{
						List<UserInfoPopup.UserInfo> userList = UserInfoPopup.GetUserList(list2, firstClearIdList, platformIds);
						((UnityAction)delegate
						{
							firstClearBox.gameObject.SetActive(value: true);
							Color textColor = (firstClearIdList.Contains(GameSparksManager.Instance.MainUserGSID) ? highlightedScoreColor : activeLabelColor);
							firstClearHighScoreDisplayEntry.Initialize(0, "", textColor, userList, shownInComputer: true);
						})();
					}
				}
			}
		});
	}

	private void PopulateScoreEntries(List<ChallengeScoreboard.ChallengeTimeData> recordsList, ChallengeScoreboard.ChallengeTimeData personalRecord, Transform scoresSection, Transform scoresContainer, Text bestTitleText, HighscoreDisplayEntry highscoreDisplayEntry)
	{
		if (recordsList != null && recordsList.Count > 0)
		{
			scoresSection.gameObject.SetActive(value: true);
			scoresContainer.DestroyAllChildren();
			HighscoreDisplayEntry[] entries = new HighscoreDisplayEntry[5];
			for (int i = 0; i != 5; i++)
			{
				entries[i] = scoresContainer.gameObject.AddPrefabAsChild<HighscoreDisplayEntry>(highScoreDisplayEntryPrefab);
			}
			int num = 0;
			foreach (ChallengeScoreboard.ChallengeTimeData record in recordsList)
			{
				List<string> playerNames = record.playerNames;
				List<bool> playerAnonymousFlags = new List<bool>();
				foreach (string item in playerNames)
				{
					_ = item;
					playerAnonymousFlags.Add(item: false);
				}
				((UnityAction<int>)delegate(int ind)
				{
					string timeString2 = HighscoreDisplayEntry.GetTimeString(record.time);
					bool flag = record.playerIds.Contains(GameSparksManager.Instance.MainUserGSID);
					List<UserInfoPopup.UserInfo> userListFromChallengeTimeData = UserInfoPopup.GetUserListFromChallengeTimeData(record);
					for (int j = 0; j < playerNames.Count; j++)
					{
						userListFromChallengeTimeData[j].shouldBeAnonymous = playerAnonymousFlags[j];
					}
					entries[ind].Initialize(ind + 1, timeString2, flag ? highlightedScoreColor : activeLabelColor, userListFromChallengeTimeData, shownInComputer: true);
				})(num);
				num++;
			}
			for (; num < 5; num++)
			{
				entries[num].Initialize(num + 1, "--:--.--", activeLabelColor, null, shownInComputer: true);
			}
		}
		if (personalRecord != null && recordsList.Count > 0)
		{
			string timeString = HighscoreDisplayEntry.GetTimeString(personalRecord.time);
			highscoreDisplayEntry.Initialize(0, timeString, highlightedScoreColor, UserInfoPopup.GetUserListFromChallengeTimeData(personalRecord), shownInComputer: true);
		}
		else
		{
			highscoreDisplayEntry.Initialize(0, "--:--.--", activeLabelColor, null, shownInComputer: true);
		}
	}

	private void HideAllRankings()
	{
		firstClearHighScoreDisplayEntry.Initialize(0, "", dimmedLabelColor, null, shownInComputer: true);
		foreach (Transform item in new List<Transform> { noCoinsScoresContainer, allCoinsScoresContainer })
		{
			item.DestroyAllChildren();
			for (int i = 0; i < 5; i++)
			{
				item.gameObject.AddPrefabAsChild<HighscoreDisplayEntry>(highScoreDisplayEntryPrefab).Initialize(i + 1, "--:--.--", activeLabelColor, null, shownInComputer: true);
			}
		}
		allCoinsHighscoreDisplayEntry.Initialize(0, "--:--.--", activeLabelColor, null, shownInComputer: true);
		noCoinsHighscoreDisplayEntry.Initialize(0, "--:--.--", activeLabelColor, null, shownInComputer: true);
	}

	public void OnAdminApprovalStatusDropdownValueChanged()
	{
		if (!GameSparksManager.Instance.MainUserIsAdmin || !(PickableBuildButton.SelectedEntry != null))
		{
			return;
		}
		SnapshotEntry snapshotEntry = PickableBuildButton.SelectedEntry.GetComponent<SnapshotEntry>();
		if (!(snapshotEntry != null))
		{
			return;
		}
		PickableButton.maskAll = true;
		int approvalStatus = adminApprovalStatusDropdown.selectedDropdownEntry.EntryValue;
		GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
		query.SetLevelApprovalStatus(snapshotEntry.Code, approvalStatus);
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			if (!query.HasError)
			{
				snapshotEntry.featuredLevelData.approvalStatus = approvalStatus;
			}
			PickableButton.ResetMasks();
		});
	}

	public void ActivateAdminRenameField()
	{
		Controller.LockInputField(adminRenameField.inputField, null);
	}

	public void ActivateLocalLevelRenameField(UnityAction<string> onEndEdit, PickableBuildButton button, PickCursor pickCursor)
	{
		localLevelNameText.interactable = true;
		Controller.LockInputField(localLevelNameText, onEndEdit);
	}

	public void OnClickApplyAdminRename()
	{
		PickableBuildButton selectedEntry = PickableBuildButton.SelectedEntry;
		if (!(selectedEntry != null))
		{
			return;
		}
		SnapshotEntry component = selectedEntry.GetComponent<SnapshotEntry>();
		if (!(component != null) || component.Code.NullOrEmpty())
		{
			return;
		}
		string text = adminRenameField.inputField.text;
		GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
		query.SendSimpleRequest("adminRenameLevel", new Dictionary<string, object>
		{
			{ "code", component.Code },
			{ "newName", text }
		}, returnScriptData: false);
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			if (query.HasError)
			{
				UserMessageManager.Instance.UserMessage(LocalizationManager.GetTranslation("UndergroundComputer/RenameFailed"), 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
				AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
			}
			else
			{
				UserMessageManager.Instance.UserMessage(LocalizationManager.GetTranslation("UndergroundComputer/LevelRenamed"), 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
				adminRenameField.inputField.text = "";
			}
		});
	}

	public void OnClickAdminDelete()
	{
		adminDeleteEnabled = true;
	}

	public void OnClickAdminConfirmDelete()
	{
		adminDeleteEnabled = false;
		PickableBuildButton selectedEntry = PickableBuildButton.SelectedEntry;
		if (!(selectedEntry != null))
		{
			return;
		}
		SnapshotEntry component = selectedEntry.GetComponent<SnapshotEntry>();
		if (!(component != null) || component.Code.NullOrEmpty())
		{
			return;
		}
		GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
		query.SendSimpleRequest("adminDeleteLevel", new Dictionary<string, object> { { "code", component.Code } }, returnScriptData: false);
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			if (query.HasError)
			{
				UserMessageManager.Instance.UserMessage(LocalizationManager.GetTranslation("UndergroundComputer/DeleteFailed"), 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
				AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
			}
			else
			{
				UserMessageManager.Instance.UserMessage(LocalizationManager.GetTranslation("UndergroundComputer/LevelDeleted"), 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
			}
		});
	}

	public void OnClickAdminCancelDelete()
	{
		adminDeleteEnabled = false;
	}

	public void OnClickDeleteFile()
	{
		localDeleteEnabled = true;
		PickableButton.AllowOnlyButtons(localDeleteYes, localDeleteNo);
	}

	public void OnClickCancelDeleteFile()
	{
		localDeleteEnabled = false;
		PickableButton.ResetMasks();
	}

	private void Update()
	{
		scoreLoadingSpinner.enabled = showScoreLoadingSpinner;
		PickableBuildButton selectedEntry = PickableBuildButton.SelectedEntry;
		if (selectedEntry != null)
		{
			SnapshotEntry component = selectedEntry.GetComponent<SnapshotEntry>();
			if (component != null)
			{
				if (component.thumbnailImage != null && component.thumbnailImage.texture != null)
				{
					SetLevelImageTexture(component.thumbnailImage.texture);
				}
				else if (selectedEntry.job == PickableBuildButton.BuildButtonJobs.LevelSlot && PickableBuildButton.undergroundComputer != null)
				{
					CustomLevelPortal portalForSlot = PickableBuildButton.undergroundComputer.GetPortalForSlot(selectedEntry.ControlsLevelSlot);
					if (portalForSlot != null && portalForSlot.thumbnailRawImage != null && portalForSlot.thumbnailRawImage.texture != null)
					{
						SetLevelImageTexture(portalForSlot.thumbnailRawImage.texture);
					}
					else
					{
						SetLevelImageTexture(null);
					}
				}
				else
				{
					SetLevelImageTexture(null);
				}
			}
			else
			{
				SetLevelImageTexture(null);
			}
		}
		else
		{
			SetLevelImageTexture(null);
		}
	}

	private void SetLevelImageTexture(Texture texture)
	{
		if (texture != null)
		{
			localLevelImage.enabled = true;
			localLevelImage.texture = texture;
			onlineLevelImage.enabled = true;
			onlineLevelImage.texture = texture;
		}
		else
		{
			localLevelImage.enabled = false;
			localLevelImage.texture = null;
			onlineLevelImage.enabled = false;
			onlineLevelImage.texture = null;
		}
	}

	public void RefreshAdminBatchList()
	{
		GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
		query.SendSimpleRequest("adminGetBatchList", new Dictionary<string, object>(), returnScriptData: true);
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			OnGetAdminBatchListResult(query);
		});
	}

	public void OnGetAdminBatchListResult(GameSparksQuery query)
	{
		adminFeaturedBatchDropdown.ClearEntries();
		foreach (AdminBatchManagementDialog.BatchListResult item in AdminBatchManagementDialog.ProcessGetBatchListResult(query))
		{
			DropdownEntry dropdownEntry = adminFeaturedBatchDropdown.popupBox.gameObject.AddPrefabAsChild<DropdownEntry>(featuredBatchDropdownEntryPrefab);
			dropdownEntry.labelText.text = item.batchName;
			dropdownEntry.EntryData = item.batchID;
			adminFeaturedBatchDropdown.AddEntry(dropdownEntry.gameObject);
		}
	}

	public void OnClickAdminAddToBatch()
	{
		PickableBuildButton selectedEntry = PickableBuildButton.SelectedEntry;
		if (!(selectedEntry != null))
		{
			return;
		}
		SnapshotEntry component = selectedEntry.GetComponent<SnapshotEntry>();
		if (!(component != null) || !GameSparksManager.Instance.MainUserIsAdmin || !(adminFeaturedBatchDropdown.selectedDropdownEntry != null))
		{
			return;
		}
		if (adminFeaturedBatchDropdown.selectedDropdownEntry.EntryData is string value)
		{
			GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
			query.SendSimpleRequest("adminAddLevelsToBatch", new Dictionary<string, object>
			{
				{ "batchID", value },
				{
					"codeList",
					new List<string> { GameSparksQuery.SanitizeSnapshotCode(component.Code) }
				}
			}, returnScriptData: true);
			GameSparksQuery gameSparksQuery = query;
			gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
			{
				if (query.HasError)
				{
					Debug.LogError("Error adding level to batch: " + query.Error);
					UserMessageManager.Instance.UserMessage("Error adding level to batch");
				}
				else
				{
					UserMessageManager.Instance.UserMessage("Level added to batch");
				}
			});
		}
		else
		{
			Debug.LogError("Batch has no batch ID");
		}
	}

	public void ToggleAcknowledgeReports(PickableBuildButton selectedEntry)
	{
		SnapshotEntry snapshotEntry = selectedEntry.GetComponent<SnapshotEntry>();
		if (!(snapshotEntry != null))
		{
			return;
		}
		bool newValue = !snapshotEntry.featuredLevelData.hasNewReports;
		GameSparksQuery gameSparksQuery = GameSparksManager.Instance.CreateQuery();
		gameSparksQuery.SendSimpleRequest("adminMarkLevelReportsAcknowledged", new Dictionary<string, object>
		{
			{
				"code",
				GameSparksQuery.SanitizeSnapshotCode(snapshotEntry.Code)
			},
			{ "hasNewReports", newValue }
		}, returnScriptData: true);
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate(GameSparksQuery q)
		{
			if (q.HasError)
			{
				Debug.LogError("Error acknowledging level reports");
				UserMessageManager.Instance.UserMessage("Error acknowledging level reports");
			}
			else
			{
				snapshotEntry.featuredLevelData.hasNewReports = newValue;
				UpdateAcknowledgeReportsButton(newValue);
			}
		});
	}

	public void ToggleIgnoreReports(PickableBuildButton selectedEntry)
	{
		SnapshotEntry snapshotEntry = selectedEntry.GetComponent<SnapshotEntry>();
		if (!(snapshotEntry != null))
		{
			return;
		}
		bool newValue = !snapshotEntry.featuredLevelData.ignoreReports;
		GameSparksQuery gameSparksQuery = GameSparksManager.Instance.CreateQuery();
		gameSparksQuery.SendSimpleRequest("adminIgnoreLevelReports", new Dictionary<string, object>
		{
			{
				"code",
				GameSparksQuery.SanitizeSnapshotCode(snapshotEntry.Code)
			},
			{ "ignoreReports", newValue }
		}, returnScriptData: true);
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate(GameSparksQuery q)
		{
			if (q.HasError)
			{
				Debug.LogError("Error ignoring level reports: " + q.Error);
				UserMessageManager.Instance.UserMessage("Error ignoring level reports");
			}
			else
			{
				snapshotEntry.featuredLevelData.ignoreReports = newValue;
				UpdateIgnoreReportsButton(newValue);
			}
		});
	}

	private void UpdateAcknowledgeReportsButton(bool hasNewReports)
	{
		adminAcknowledgeReportsButton.buttonText.text = (hasNewReports ? "Acknowledge Reports" : "De-acknowledge Reports");
	}

	private void UpdateIgnoreReportsButton(bool ignoreReports)
	{
		adminIgnoreReportsButton.buttonText.text = (ignoreReports ? "Un-ignore Reports" : "Ignore Future Reports");
	}
}
