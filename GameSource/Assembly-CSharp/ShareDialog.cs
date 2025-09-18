using System;
using GameEvent;
using I2.Loc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ShareDialog : MonoBehaviour, IGameEventListener
{
	public UndergroundComputer undergroundComputer;

	public Transform codeAreaContainer;

	public Transform publishAreaContainer;

	public Transform publishMessageContainer;

	public Transform alreadyPublishedContainer;

	public Text levelNameText;

	public Text codeText;

	public ThumbnailDisplaySlot thumbnailDisplaySlot;

	public Text loadingDataText;

	public Text codeInfoText;

	public Text generateCodeButtonText;

	public Text publishMessageText;

	public FeaturedQuickFilter.LevelTypes selectedLevelType;

	private void Awake()
	{
		ChangeListener(adding: true);
		HideAllPanes();
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<PlayerInGameRuleEvent>(this, adding);
	}

	public void DisplayLevelData(string code, string levelName)
	{
		HideAllPanes();
		if (code.NullOrEmpty())
		{
			codeAreaContainer.gameObject.SetActive(value: true);
			levelNameText.text = levelName;
			codeText.text = "";
			loadingDataText.enabled = false;
			thumbnailDisplaySlot.HideImage();
			UpdateCodeInfoText(codeGenerated: false);
			thumbnailDisplaySlot.LoadThumbnail(code, levelName);
			return;
		}
		loadingDataText.enabled = true;
		GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
		query.GetLevelPublishStatus(code);
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			if (query.HasError)
			{
				Debug.LogError("Error with query: " + query.Error);
			}
			else
			{
				codeAreaContainer.gameObject.SetActive(value: true);
				loadingDataText.enabled = false;
				levelNameText.text = levelName;
				codeText.text = GameSparksQuery.GetFormattedSnapshotCode(code);
				UpdateCodeInfoText(codeGenerated: true);
				bool published = (int)query.ResultData["published"] == 1;
				bool isOwner = (int)query.ResultData["isOwner"] == 1;
				bool isAnonymous = (int)query.ResultData["isAnonymous"] == 1;
				string category = query.ResultData["category"] as string;
				ShowPublishArea(published, isOwner, isAnonymous, category);
				thumbnailDisplaySlot.LoadThumbnail(code, levelName);
			}
		});
	}

	private void ShowPublishArea(bool published, bool isOwner, bool isAnonymous, string category)
	{
		bool num = !category.NullOrEmpty();
		FeaturedQuickFilter.LevelTypes levelType = FeaturedQuickFilter.LevelTypes.Versus;
		if (num)
		{
			try
			{
				levelType = (FeaturedQuickFilter.LevelTypes)Enum.Parse(typeof(FeaturedQuickFilter.LevelTypes), category);
			}
			catch (Exception)
			{
			}
		}
		if (isOwner)
		{
			if (!published)
			{
				publishAreaContainer.gameObject.SetActive(value: true);
				SelectLevelType(levelType);
			}
			else
			{
				alreadyPublishedContainer.gameObject.SetActive(value: true);
			}
		}
		else
		{
			publishMessageContainer.gameObject.SetActive(value: true);
			if (isAnonymous)
			{
				publishMessageText.text = LocalizationManager.GetTranslation("Snapshot/AnonymousUserCreateNew");
			}
			else
			{
				publishMessageText.text = LocalizationManager.GetTranslation("Snapshot/AnonymousUserMessage");
			}
		}
	}

	public void SelectLevelType(FeaturedQuickFilter.LevelTypes levelType)
	{
		selectedLevelType = levelType;
	}

	private void HideAllPanes()
	{
		codeAreaContainer.gameObject.SetActive(value: false);
		publishAreaContainer.gameObject.SetActive(value: false);
		publishMessageContainer.gameObject.SetActive(value: false);
		loadingDataText.gameObject.SetActive(value: false);
		alreadyPublishedContainer.gameObject.SetActive(value: false);
	}

	public void OnClose()
	{
		HideAllPanes();
	}

	private void UpdateCodeInfoText(bool codeGenerated)
	{
		if (codeGenerated)
		{
			codeInfoText.text = LocalizationManager.GetTranslation("Snapshot/CodeInstructions");
			generateCodeButtonText.text = LocalizationManager.GetTranslation("Snapshot/CopyCode");
		}
		else
		{
			codeInfoText.text = LocalizationManager.GetTranslation("Snapshot/GetCodeinstructions");
			generateCodeButtonText.text = LocalizationManager.GetTranslation("Snapshot/GetCode");
		}
	}

	public void OnClickGenerateCode()
	{
		if (PickableBuildButton.SelectedEntryHasCode)
		{
			undergroundComputer.CopyCodeToClipboard(PickableBuildButton.SelectedEntry);
			return;
		}
		codeText.text = "...";
		undergroundComputer.GetCodeForLocalSave(PickableBuildButton.SelectedEntry, delegate(bool success)
		{
			if (success)
			{
				SnapshotEntry component = PickableBuildButton.SelectedEntry.GetComponent<SnapshotEntry>();
				codeText.text = GameSparksQuery.GetFormattedSnapshotCode(component.Code);
				ShowPublishArea(published: false, isOwner: true, isAnonymous: false, null);
			}
			else
			{
				codeText.text = "";
			}
			UpdateCodeInfoText(success);
		});
	}

	public void OnClickShareReddit()
	{
		PickableBuildButton selectedEntry = PickableBuildButton.SelectedEntry;
		SnapshotEntry snapshotEntry = selectedEntry.GetComponent<SnapshotEntry>();
		if (!(snapshotEntry != null))
		{
			return;
		}
		if (!snapshotEntry.Code.NullOrEmpty())
		{
			UndergroundComputer.ShareSnapshotCodeOnReddit(snapshotEntry.SnapshotName, snapshotEntry.Code, null);
			return;
		}
		codeText.text = "...";
		undergroundComputer.GetCodeForLocalSave(selectedEntry, delegate(bool success)
		{
			if (success)
			{
				UndergroundComputer.ShareSnapshotCodeOnReddit(snapshotEntry.SnapshotName, snapshotEntry.Code, null);
				codeText.text = GameSparksQuery.GetFormattedSnapshotCode(snapshotEntry.Code);
			}
			else
			{
				Debug.LogError("Did not share code on Reddit - upload not successful!");
				codeText.text = "";
			}
		});
	}

	public void OnClickShareTwitter()
	{
		PickableBuildButton selectedEntry = PickableBuildButton.SelectedEntry;
		SnapshotEntry snapshotEntry = selectedEntry.GetComponent<SnapshotEntry>();
		if (!(snapshotEntry != null))
		{
			return;
		}
		if (!snapshotEntry.Code.NullOrEmpty())
		{
			UndergroundComputer.ShareSnapshotCodeOnTwitter(snapshotEntry.SnapshotName, snapshotEntry.Code, null);
			return;
		}
		codeText.text = "...";
		undergroundComputer.GetCodeForLocalSave(selectedEntry, delegate(bool success)
		{
			if (success)
			{
				UndergroundComputer.ShareSnapshotCodeOnTwitter(snapshotEntry.SnapshotName, snapshotEntry.Code, null);
				codeText.text = GameSparksQuery.GetFormattedSnapshotCode(snapshotEntry.Code);
			}
			else
			{
				Debug.LogError("Did not share code on Twitter - upload not successful!");
				codeText.text = "";
			}
		});
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e.GetType() == typeof(PlayerInGameRuleEvent) && (e as PlayerInGameRuleEvent).Entered && PickableBuildButton.buildMenuCurrentState == PickableBuildButton.BuildScreenStates.ShareDialog)
		{
			if (PickableBuildButton.SelectedEntry != null)
			{
				SnapshotEntry component = PickableBuildButton.SelectedEntry.GetComponent<SnapshotEntry>();
				DisplayLevelData(component.Code, component.SnapshotName);
			}
			else
			{
				undergroundComputer.CloseShareDialog();
			}
		}
	}

	public void OnClickPublish()
	{
		if (!(PickableBuildButton.SelectedEntry != null))
		{
			return;
		}
		SnapshotEntry snapshotEntry = PickableBuildButton.SelectedEntry.GetComponent<SnapshotEntry>();
		if (!(snapshotEntry != null) || snapshotEntry.Code.NullOrEmpty())
		{
			return;
		}
		PickableButton.maskAll = true;
		undergroundComputer.AddLoadingOperation();
		GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
		query.SetLevelPublishStatus(GameSparksQuery.SanitizeSnapshotCode(snapshotEntry.Code), published: true, selectedLevelType);
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			undergroundComputer.RemoveLoadingOperation();
			PickableButton.ResetMasks();
			if (query.HasError)
			{
				UserMessageManager.Instance.UserMessage(LocalizationManager.GetTranslation("Snapshot/RequestError"), 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
				Debug.LogError("There was an error with the request: " + query.Error);
			}
			else
			{
				publishAreaContainer.gameObject.SetActive(value: false);
				ShowPublishArea(published: true, isOwner: true, isAnonymous: false, selectedLevelType.ToString());
				undergroundComputer.RefreshSnapshotMetadata(snapshotEntry, delegate(bool success)
				{
					if (success)
					{
						undergroundComputer.quickInfoPane.SetSnapshotInfo(snapshotEntry.featuredLevelData);
					}
				});
			}
		});
	}

	public void OnClickUnpublish()
	{
		if (!(PickableBuildButton.SelectedEntry != null))
		{
			return;
		}
		SnapshotEntry snapshotEntry = PickableBuildButton.SelectedEntry.GetComponent<SnapshotEntry>();
		if (!(snapshotEntry != null) || snapshotEntry.Code.NullOrEmpty())
		{
			return;
		}
		PickableButton.maskAll = true;
		undergroundComputer.AddLoadingOperation();
		GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
		query.SetLevelPublishStatus(GameSparksQuery.SanitizeSnapshotCode(snapshotEntry.Code), published: false, selectedLevelType);
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			undergroundComputer.RemoveLoadingOperation();
			PickableButton.ResetMasks();
			if (query.HasError)
			{
				UserMessageManager.Instance.UserMessage(LocalizationManager.GetTranslation("Snapshot/RequestError"), 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
				Debug.LogError("There was an error with the request: " + query.Error);
			}
			else
			{
				alreadyPublishedContainer.gameObject.SetActive(value: false);
				ShowPublishArea(published: false, isOwner: true, isAnonymous: false, selectedLevelType.ToString());
				undergroundComputer.RefreshSnapshotMetadata(snapshotEntry, delegate(bool success)
				{
					if (success)
					{
						undergroundComputer.quickInfoPane.SetSnapshotInfo(snapshotEntry.featuredLevelData);
					}
				});
			}
		});
	}
}
